namespace koi.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Imazen.Common.Storage;
    using Microsoft.Extensions.DependencyInjection;

    public static class CustomBlobServiceExtensions
    {
        public static IServiceCollection AddImageflowCustomBlobService(this IServiceCollection services, CustomBlobServiceOptions options)
        {
            services.AddSingleton<IBlobProvider>((container) =>
            {
                return new CustomBlobService(options);
            });

            return services;
        }
    }

    public class CustomBlobServiceOptions
    {
        private readonly string connectionString = string.Empty;

        public CustomBlobServiceOptions(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public BlobClientOptions BlobClientOptions { get; set; } = new BlobClientOptions();

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        /// <summary>
        /// Can block container/key pairs by returning null
        /// </summary>
        public Func<string, string, Tuple<string, string>> ContainerKeyFilterFunction { get; set; }
            = Tuple.Create;

        public bool IgnorePrefixCase { get; set; } = true;

        public const string Prefix = "/b/";
    }

    public class CustomBlobService : IBlobProvider
    {
        private readonly BlobServiceClient client;
        private readonly CustomBlobServiceOptions options;

        public CustomBlobService(CustomBlobServiceOptions options)
        {
            this.options = options;
            client = new BlobServiceClient(options.ConnectionString, options.BlobClientOptions);
        }

        public IEnumerable<string> GetPrefixes()
        {
            return Enumerable.Repeat(CustomBlobServiceOptions.Prefix, 1);
        }

        public bool SupportsPath(string virtualPath)
            => virtualPath.StartsWith(CustomBlobServiceOptions.Prefix,
                options.IgnorePrefixCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        public async Task<IBlobData> Fetch(string virtualPath)
        {
            if (!SupportsPath(virtualPath))
            {
                return null;
            }

            var path = virtualPath[CustomBlobServiceOptions.Prefix.Length..].TrimStart('/');
            var indexOfSlash = path.IndexOf('/');
            if (indexOfSlash < 1) return null;

            var container = path[..indexOfSlash];
            var blobKey = path[(indexOfSlash + 1)..];

            var filtered = options.ContainerKeyFilterFunction(container, blobKey);

            if (filtered == null)
            {
                return null;
            }

            container = filtered.Item1;
            blobKey = filtered.Item2;

            try
            {
                var blobClient = client.GetBlobContainerClient(container).GetBlobClient(blobKey);

                var s = await blobClient.DownloadAsync();
                return new CustomAzureBlob(s);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 404)
                {
                    throw new BlobMissingException($"Azure blob \"{blobKey}\" not found.", e);
                }

                throw;
            }
        }
    }

    internal class CustomAzureBlob : IBlobData, IDisposable
    {
        private readonly Response<BlobDownloadInfo> response;

        internal CustomAzureBlob(Response<BlobDownloadInfo> r)
        {
            response = r;
        }

        public bool? Exists => true;
        public DateTime? LastModifiedDateUtc => response.Value.Details.LastModified.UtcDateTime;
        public Stream OpenRead()
        {
            return response.Value.Content;
        }

        public void Dispose()
        {
            response?.Value?.Dispose();
        }
    }
}