using SmsGate.Balance.Hosting.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack.Redis;

[assembly: HostingStartup(typeof(ConfigureRedis))]

namespace SmsGate.Balance.Hosting.Configurations;

public class ConfigureRedis : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            var redisHost = context.Configuration.GetConnectionString("Redis");
            services.AddSingleton<IRedisClientsManager>(new RedisManagerPool(redisHost));
        });
    }
}