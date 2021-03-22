// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Tye.Dashboard.Models;
using Microsoft.Tye.Hosting;
using Microsoft.Tye.Resources.Containers;
using OpenArm.Repositories;
using OpenArm.Resources;

namespace Microsoft.Tye
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DefaultRuntime>();
            services.AddSingleton<Runtime>(services =>
            {
                return services.GetRequiredService<DefaultRuntime>();
            });
            services.AddHostedService<DefaultRuntime>(services =>
            {
                return services.GetRequiredService<DefaultRuntime>();
            });

            services.AddSingleton<DefaultEventBus>();
            services.AddSingleton<EventBus>(services =>
            {
                return services.GetRequiredService<DefaultEventBus>();
            });
            services.AddHostedService<DefaultEventBus>(services =>
            {
                return services.GetRequiredService<DefaultEventBus>();
            });

            services.AddSingleton<RuntimeEventSink>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<EventSink, RuntimeEventSink>(services =>
            {
                return services.GetRequiredService<RuntimeEventSink>();
            }));
            
            services.AddHealthChecks();

            services.AddControllers()
                .AddResourceIdModelBinder()
                .AddResourceTypeModelBinder()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddRazorPages(options =>
            {
                options.RootDirectory = "/Dashboard/Pages";
            });
            
            services.AddServerSideBlazor();

            services
                .AddOptions<StaticFileOptions>()
                .PostConfigure(o =>
                {
                    // serve static files from embedded manifest
                    var fileProvider = new ManifestEmbeddedFileProvider(typeof(Startup).Assembly, "wwwroot");

                    // Make sure we don't remove the existing file providers (blazor needs this)
                    o.FileProvider = new CompositeFileProvider(o.FileProvider, fileProvider);
                });

            OpenArmRepositories.RegisterJsonType(typeof(ResourceProperties), new JsonSerializerOptions(JsonSerializerDefaults.Web));
            OpenArmRepositories.RegisterJsonType(typeof(ContainerProperties), new JsonSerializerOptions(JsonSerializerDefaults.Web));

            var databasePath = Configuration["Data:DatabasePath"];
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new InvalidOperationException("Database path must be set via the Data:DatabasePath configuration key.");
            }
            services.AddSingleton<Func<IDbConnection>>(sp => () => new SqliteConnection($"Data Source={databasePath}"));

            services.AddRepository<IResourceRepository, ResourceRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapControllers();

                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
