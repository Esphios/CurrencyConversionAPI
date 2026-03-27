using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConversionService.Helpers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCurrencyConversionService(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddMemoryCache();
        _ = services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>();
        _ = services.AddSingleton<ICurrencyConverterService, CurrencyConverterService>();
        _ = services.AddSingleton(configuration);
        return services;
    }
}
