using SmsGate.Balance.Hosting.Configurations;
using iZota.Core.Shared.Logging;
using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ConfigureLog))]

namespace SmsGate.Balance.Hosting.Configurations;

public class ConfigureLog : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) => { services.RegisterLogging(context.Configuration); });
    }
}