using SmsGate.Balance.Domain;
using SmsGate.Balance.Domain.Entities;
using SmsGate.Balance.Hosting.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;

[assembly: HostingStartup(typeof(ConfigureDb))]

namespace SmsGate.Balance.Hosting.Configurations;

public class ConfigureDb : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            // services.AddSingleton<IKppConnectionFactory>(new KppConnectionFactory(
            //     Configuration.GetConnectionString("Kpp"),
            //     SqliteOrmLiteDialectProvider.Instance));

            services.AddSingleton<IBalanceConnectionFactory>(new BalanceConnectionFactory(
                context.Configuration.GetConnectionString("Balance"),
                PostgreSqlDialectProvider.Instance));

            // services.AddSingleton<IKppConnectionFactory>(new KppConnectionFactory(
            //     Configuration.GetConnectionString("KppSql"),
            //     SqlServer2014OrmLiteDialectProvider.Instance));
        }).ConfigureAppHost(appHost =>
        {
            var scriptMethods = appHost.GetPlugin<SharpPagesFeature>()?.ScriptMethods;
            scriptMethods?.Add(new DbScriptsAsync());

            using var db = appHost.Resolve<IBalanceConnectionFactory>().Open();
            db.CreateTableIfNotExists<AccountBalance>();
            db.CreateTableIfNotExists<Currency>();
            db.CreateTableIfNotExists<Transaction>();
            db.CreateTableIfNotExists<Settlement>();

            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            OrmLiteConfig.DialectProvider.GetStringConverter().UseUnicode = true;
        });
    }
}