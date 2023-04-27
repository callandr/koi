using Imageflow.Fluent;
using Imageflow.Server;
using Imageflow.Server.DiskCache;
using koi.Services;

namespace koi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllersWithViews();

            string azureAccount = builder.Configuration.GetValue<string>(nameof(azureAccount)) ?? string.Empty;
            string azureKey = builder.Configuration.GetValue<string>(nameof(azureKey)) ?? string.Empty;
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=" + azureAccount + ";AccountKey=" + azureKey + ";EndpointSuffix=core.windows.net";

            ////var options = new AzureBlobServiceOptions(
            ////            connectionString,
            ////            new BlobClientOptions()).MapPrefix("", "");

            ////builder.Services.AddImageflowAzureBlobService(options);
            var options = new CustomBlobServiceOptions(connectionString);
            builder.Services.AddImageflowCustomBlobService(options);

            builder.Services.AddImageflowDiskCache(
                new DiskCacheOptions("Cache")
                {
                    AutoClean = true
                });

            var app = builder.Build();

            var ifoptions = new ImageflowMiddlewareOptions()
            {
                JobSecurityOptions = new SecurityOptions()
                {
                    MaxFrameSize = new FrameSizeLimit(12900, 12900, 201)
                }
            }.SetMapWebRoot(true)
            .SetMyOpenSourceProjectUrl("https://github.com/callandr/koi");

            app.UseImageflow(ifoptions);

            app.UseRouting();

            ////app.UseEndpoints(endpoints =>
            ////{
            ////    endpoints.MapGet("/", async context =>
            ////    {
            ////        context.Response.ContentType = "text/html";
            ////        await context.Response.WriteAsync("<img src=\"vegme.jpg?width=420\" />");
            ////    });
            ////});

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Run();
        }
    }
}