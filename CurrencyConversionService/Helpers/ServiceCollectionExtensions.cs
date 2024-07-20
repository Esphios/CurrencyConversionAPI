using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConversionService.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCurrencyConversionService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>();
            services.AddSingleton(configuration);

            return services;
        }
    }
}
