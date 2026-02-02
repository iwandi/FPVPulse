using FPVPulse.LocalHost.Components;
using FPVPulse.LocalHost.Injest;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;

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

                builder.Services.AddSingleton<InjestDbContext>();
                builder.Services.AddSingleton<InjestProcessor>();
                builder.Services.AddSingleton<InjestData>();
                builder.Services.AddSingleton<InjestQueue>();

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

                app.Run();
            }
        }
    }
}