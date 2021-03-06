﻿using System;
using MusicStoreUI.Services;
using MusicStoreUI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pivotal.Discovery.Client;
using Polly;

#if USE_REDIS_CACHE
using Microsoft.AspNetCore.DataProtection;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Security.DataProtection;
#endif

using Steeltoe.Common.Discovery;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.CircuitBreaker.Hystrix;
using Command = MusicStoreUI.Services.HystrixCommands;

namespace MusicStoreUI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            // Add framework services.
#if USE_REDIS_CACHE
            services.AddRedisConnectionMultiplexer(Configuration);
            services.AddDataProtection()
                .PersistKeysToRedis()
                .SetApplicationName("MusicStoreUI");
            services.AddDistributedRedisCache(Configuration);
#else
            services.AddDistributedMemoryCache();
#endif
            // Add custom health check contributor
            services.AddScoped<IHealthContributor, MySqlHealthContributor>();

            // Add managment endpoint services
            services.AddCloudFoundryActuators(Configuration);

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddDbContext<AccountsContext>(options => options.UseMySql(Configuration));
            services.ConfigureApplicationCookie(options => options.AccessDeniedPath = "/Home/AccessDenied");
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<AccountsContext>()
                    .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            services.AddDiscoveryClient(Configuration);

            services.AddHystrixCommand<Command.GetTopAlbums>("MusicStore", Configuration);
            services.AddHystrixCommand<Command.GetGenres>("MusicStore", Configuration);
            services.AddHystrixCommand<Command.GetGenre>("MusicStore", Configuration);
            services.AddHystrixCommand<Command.GetAlbum>("MusicStore", Configuration);

            services.AddTransient<DiscoveryHttpMessageHandler>();

            services.AddHttpClient("store", c =>
            {
                c.BaseAddress = new Uri("http://musicstore/api/Store/");
            })
            .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
            .AddTypedClient<IMusicStore, MusicStoreService>();

            services.AddHttpClient("orders", c =>
            {
                c.BaseAddress = new Uri("http://orderprocessing/api/Order/");
            })
            .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
            .AddTypedClient<IOrderProcessing, OrderProcessingService>();

            services.AddHttpClient("cart", c =>
            {
                c.BaseAddress = new Uri("http://shoppingcart/api/ShoppingCart/");
            })
            .AddHttpMessageHandler<DiscoveryHttpMessageHandler>()
            .AddTypedClient<IShoppingCart, ShoppingCartService>();

            services.AddMvc()
                    .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            // Add memory cache services
            services.AddMemoryCache();

            // Add session related services.

            // Use call below if you want sticky Sessions on Cloud Foundry
            // services.AddSession((options) => options.CookieName = "JSESSIONID");

            services.AddSession();

            // Configure Auth
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "ManageStore",
                    authBuilder =>
                    {
                        authBuilder.RequireClaim("ManageStore", "Allowed");
                        authBuilder.RequireAuthenticatedUser();
                    });
            });

            // Add Hystrix metrics stream to enable monitoring 
            services.AddHystrixMetricsStream(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add Hystrix Metrics context to pipeline
            app.UseHystrixRequestContext();

            // Add management endpoints into pipeline
            app.UseCloudFoundryActuators();

            app.UseSession();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseDiscoveryClient();

            // Startup Hystrix metrics stream
            app.UseHystrixMetricsStream();
        }
    }
}
