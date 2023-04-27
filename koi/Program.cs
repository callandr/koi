using Imageflow.Server;

namespace koi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

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