// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Tye.BuildServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddSingleton<ContainerBuilder>();
            services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ContainerBuilder builder, JsonSerializerOptions options)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");

                endpoints.MapPost("/build/container", async context =>
                {
                    var request = await JsonSerializer.DeserializeAsync<BuildContainerRequest>(context.Request.Body, options);

                    try
                    {
                        var response = await builder.BuildAsync(request);
                        await JsonSerializer.SerializeAsync(context.Response.Body, response, options);
                    }
                    catch (CommandException ex)
                    {
                        context.Response.StatusCode = 400;
                        await JsonSerializer.SerializeAsync(context.Response.Body, new { message = ex.Message, });
                    }
                });
            });
        }
    }
}
