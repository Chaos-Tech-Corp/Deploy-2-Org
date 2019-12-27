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
                options.ClientId = Configuration["AppSettings:GitHub_ClientId"];
                options.ClientSecret = Configuration["AppSettings:GitHub_ClientSecret"];
                options.SaveTokens = true;
            })
            //.AddSalesforce("SalesforceProduction", options =>
            //{
            //    options.ClientId = Configuration["AppSettings:Salesforce_ClientId"];
            //    options.ClientSecret = Configuration["AppSettings:Salesforce_ClientSecret"];
            //    options.SaveTokens = true;
            //    options.CallbackPath = "/signin-salesforceproduction";
            //    options.Environment = AspNet.Security.OAuth.Salesforce.SalesforceAuthenticationEnvironment.Production;
            //})
            .AddSalesforce("SalesforceTest", options =>
            {
                options.ClientId = Configuration["AppSettings:Salesforce_ClientId"];
                options.ClientSecret = Configuration["AppSettings:Salesforce_ClientSecret"];
                options.SaveTokens = true;
                options.CallbackPath = "/signin-salesforcetest";
                options.Environment = AspNet.Security.OAuth.Salesforce.SalesforceAuthenticationEnvironment.Test;
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
