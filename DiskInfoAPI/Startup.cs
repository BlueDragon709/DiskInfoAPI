﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiskInfoAPI.Services;
using Hangfire;
using Hangfire.MySql.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiskInfoAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DiskService>();
            services.AddHangfire(x => x
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(
                Configuration.GetConnectionString("Hangfire"),
                new MySqlStorageOptions() {
                    TablePrefix = "hangfire"
                }
            )
            ));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IBackgroundJobClient backgroundJobs, IHostingEnvironment env)
        {
            var config = Configuration;
            NotificationService notificationService = new NotificationService(config);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfire");

            RecurringJob.AddOrUpdate("notificationService", () => notificationService.SendNotification(), Cron.Weekly(DayOfWeek.Sunday));

            app.UseMvc();
            
        }
    }
}
