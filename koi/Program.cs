using Imageflow.Fluent;
using Imageflow.Server;
using Imageflow.Server.HybridCache;
using koi.Services;

namespace koi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            string azureAccount = builder.Configuration.GetValue<string>(nameof(azureAccount)) ?? string.Empty;
            string azureKey = builder.Configuration.GetValue<string>(nameof(azureKey)) ?? string.Empty;
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=" + azureAccount + ";AccountKey=" + azureKey + ";EndpointSuffix=core.windows.net";

            var options = new CustomBlobServiceOptions(connectionString);
            builder.Services.AddImageflowCustomBlobService(options);

            builder.Services.AddImageflowHybridCache(new HybridCacheOptions("C:\\imgresizercache\\")
            {
                CacheSizeLimitInBytes = 1173741800L,
            });

            var app = builder.Build();

            var ifoptions = new ImageflowMiddlewareOptions()
            {
                JobSecurityOptions = new SecurityOptions()
                {
                    MaxFrameSize = new FrameSizeLimit(17400, 15100, 225)
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