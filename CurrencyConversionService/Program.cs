using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CurrencyConversionService;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                _ = webBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(logging =>
            {
                _ = logging.ClearProviders();
                _ = logging.AddConsole();
            });
    }
}
