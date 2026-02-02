using FPVPulse.LocalHost.Components;
using FPVPulse.LocalHost.Injest;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost
{
    public static class StartUp
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            {
                // Add services to the container.
                builder.Services.AddRazorComponents()
                    .AddInteractiveWebAssemblyComponents();

                builder.Services.AddSignalR();

                builder.Services.AddDbContext<InjestDbContext>();

                builder.Services.AddSingleton<ChangeSignaler>();

                builder.Services.AddSingleton<InjestData>();
                builder.Services.AddSingleton<InjestQueue>();
                builder.Services.AddHostedService<InjestProcessor>();

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<HttpClient>(sp =>
                {
                    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
                    var request = accessor.HttpContext?.Request;
                    var baseUri = $"{request?.Scheme}://{request?.Host}/";
                    return new HttpClient { BaseAddress = new Uri(baseUri) };
                });

                var mvcBuilder = builder.Services.AddControllers();

                mvcBuilder.AddNewtonsoftJson(mvcBuilderOptions =>
                {
                    mvcBuilderOptions.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseWebAssemblyDebugging();
                }
                else
                {
                    app.UseExceptionHandler("/Error", createScopeForErrors: true);
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }

                app.UseHttpsRedirection();

                app.UseAntiforgery();

                app.MapStaticAssets();
                app.MapRazorComponents<App>()
                    .AddInteractiveWebAssemblyRenderMode()
                    .AddAdditionalAssemblies(typeof(FPVPulse.LocalHost.Client._Imports).Assembly);

                app.MapControllers();
                app.MapHub<ChangeHub>("/hubs/change");

                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();
                    db.Database.EnsureCreated();
                    db.Database.Migrate();
                }

                app.Run();
            }
        }
    }
}