using Azure.Storage.Blobs;
using Imageflow.Server;
using Imageflow.Server.Storage.AzureBlob;

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

            builder.Services.AddImageflowAzureBlobService(
                new AzureBlobServiceOptions(
                        connectionString,
                        new BlobClientOptions())
                    .MapPrefix("/u", "u62"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                ////app.UseHsts();
            }

            app.UseImageflow(new ImageflowMiddlewareOptions()
                .SetMapWebRoot(true)
                .SetMyOpenSourceProjectUrl("https://github.com/callandr/koi"));

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<img src=\"vegme.jpg?width=420\" />");
                });
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.Run();
        }
    }
}