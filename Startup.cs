using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using System;

namespace deploy2.org.com
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";
            })
            .AddGitHub(options =>
            {
                options.ClientId = "Iv1.4712cd1251e02649";
                options.ClientSecret = "76c106eab538718d136354afb6f0a41effb2927c";
                options.SaveTokens = true;
            })
            .AddSalesforce(options =>
            {
                options.ClientId = "3MVG9GYWKbMgBvbw5uycR_9sQUwY1YIcLEyP.wruTS5Qw2rH5d1IB.n3a085pbZfql7s22OwjHOEYpKURQFja";
                options.ClientSecret = "2D189C480A0BBE49FA6EA9B6A9CB7D1226769FD8E2C9C24445D8F7505F1AF6B6";
                options.SaveTokens = true;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".deploy2.org";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Required to serve files with no extension in the .well-known folder
            var options = new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
            };
            app.UseStaticFiles(options);

            app.UseSession();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "deploy",
                    pattern: "deploy/{action}/{id?}",
                    defaults: new { controller = "Deploy", action = "Index" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });

        }
    }
}
