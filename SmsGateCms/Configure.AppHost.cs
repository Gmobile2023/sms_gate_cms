using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using ServiceStack;
using ServiceStack.IO;

[assembly: HostingStartup(typeof(SmsGateCms.AppHost))]

namespace SmsGateCms;

public class AppHost() : AppHostBase("SmsGateCms"), IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context,services) => {
            // Configure ASP.NET Core IOC Dependencies
            context.Configuration.GetSection(nameof(AppConfig)).Bind(AppConfig.Instance);
            services.AddSingleton(AppConfig.Instance);
            //ConfigureKestrel(services);
        });

    // Configure your AppHost with the necessary configuration and dependencies your App needs
    public override void Configure()
    {
        SetConfig(new HostConfig {
        });
        
        //Allow Referencing in #Script expressions, e.g. [Input(EvalAllowableEntries)]
        ScriptContext.Args[nameof(AppData)] = AppData.Instance;
        AppConfig.Instance.GitPagesBaseUrl ??= ResolveGitBlobBaseUrl(ContentRootDirectory);
    }
    
    private string? ResolveGitBlobBaseUrl(IVirtualDirectory contentDir)
    {
        var srcDir = new DirectoryInfo(contentDir.RealPath);
        var gitConfig = new FileInfo(Path.Combine(srcDir.Parent!.FullName, ".git", "config"));
        if (gitConfig.Exists)
        {
            var txt = gitConfig.ReadAllText();
            var pos = txt.IndexOf("url = ", StringComparison.Ordinal);
            if (pos >= 0)
            {
                var url = txt[(pos + "url = ".Length)..].LeftPart(".git").LeftPart('\n').Trim();
                var gitBaseUrl = url.CombineWith($"blob/main/{srcDir.Name}");
                return gitBaseUrl;
            }
        }
        return null;
    }
    private void ConfigureKestrel(IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Listen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 443),
                listenOptions =>
                {
                    var certPassword = "";//_appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Password");
                    var certPath = "gmobile.pfx";//_appConfiguration.GetValue<string>("Kestrel:Certificates:Default:Path");
                    var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(certPath,
                        certPassword);
                    listenOptions.UseHttps(new HttpsConnectionAdapterOptions()
                    {
                        ServerCertificate = cert
                    });
                });
        });
    }
}

public class AppConfig
{
    public static AppConfig Instance { get; } = new();
    public string LocalBaseUrl { get; set; }
    public string PublicBaseUrl { get; set; }
    public string? GitPagesBaseUrl { get; set; }
}

public static class HtmlHelpers
{
    public static string ToAbsoluteContentUrl(string? relativePath) => HostContext.DebugMode 
        ? AppConfig.Instance.LocalBaseUrl.CombineWith(relativePath)
        : AppConfig.Instance.PublicBaseUrl.CombineWith(relativePath);
    public static string ToAbsoluteApiUrl(string? relativePath) => HostContext.DebugMode 
        ? AppConfig.Instance.LocalBaseUrl.CombineWith(relativePath)
        : AppConfig.Instance.PublicBaseUrl.CombineWith(relativePath);

    public static string ContentUrl(this IHtmlHelper html, string? relativePath) => ToAbsoluteContentUrl(relativePath); 
    public static string ApiUrl(this IHtmlHelper html, string? relativePath) => ToAbsoluteApiUrl(relativePath);
}


// Shared App Data
public class AppData
{
    public static readonly AppData Instance = new();
}

