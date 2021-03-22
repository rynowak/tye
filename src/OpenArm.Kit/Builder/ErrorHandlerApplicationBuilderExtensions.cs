using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

namespace OpenArm
{
    public static class ErrorHandlerApplicationBuilderExtensions
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        public static IApplicationBuilder UseArmExceptionHandler(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
            app.UseExceptionHandler(options =>
            {
                options.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature.Error;

                    int httpStatusCode;
                    ExtendedError error;
                    if (exception is ErrorResponseException errorResponseException)
                    {
                        httpStatusCode = errorResponseException.HttpStatusCode;
                        error = errorResponseException.Error;
                    }
                    else
                    {
                        var message = environment.IsDevelopment() ? exception.Message : "An internal error occurred";

                        httpStatusCode = 500;
                        error = new ExtendedError() { Code = "InternalServerError", Message = message, };
                    }

                    // all errors should obey the ARM error contract
                    context.Response.StatusCode = httpStatusCode;

                    context.Response.Headers[HeaderNames.ContentType] = "application/json";
                    await JsonSerializer.SerializeAsync(context.Response.Body, new ErrorResponse(){ Error = error}, jsonSerializerOptions);
                });
            });

            return app;
        }
    }
}
