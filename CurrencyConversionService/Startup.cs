using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using System;
using System.IO;
using System.Reflection;

namespace CurrencyConversionService;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        _ = services.AddMemoryCache();
        _ = services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>();
        _ = services.AddSingleton(Configuration);
        _ = services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });
        _ = services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Conversion API", Version = "v1" });

            // Include XML comments
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            _ = app.UseDeveloperExceptionPage();
        }

        _ = app.UseSwagger();
        _ = app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            c.RoutePrefix = string.Empty;
        });

        _ = app.UseRouting();
        _ = app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllers();
        });
    }
}
