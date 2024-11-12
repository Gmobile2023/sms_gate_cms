using Microsoft.EntityFrameworkCore;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SmsGateCms.Data;
using ServiceStack;
using SmsGateCms.ServiceInterface;
using SmsGateCms.ServiceInterface.Connector;

[assembly: HostingStartup(typeof(SmsGateCms.ConfigureDb))]

namespace SmsGateCms;

public class ConfigureDb : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) => {
            var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
            
            services.AddSingleton<IDbConnectionFactory>(new OrmLiteConnectionFactory(
                connectionString, PostgreSqlDialect.Provider));

            // $ dotnet ef migrations add CreateIdentitySchema
            // $ dotnet ef database update
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString, b => b.MigrationsAssembly(nameof(SmsGateCms))));
            
            // Enable built-in Database Admin UI at /admin-ui/database
            services.AddPlugin(new AdminDatabaseFeature());
            services.AddScoped<IMessageService,MessageService>();
            services.AddScoped<IBaseConnector, GmobileConnector>();
        });
}