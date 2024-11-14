using Microsoft.Extensions.Logging;
using ServiceStack;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface;

public class MyServices : Service
{
    private readonly IMessageService _messageService;
    private readonly ILogger<MyServices> _logger;
    public IAutoQueryDb AutoQuery;
    public MyServices(ILogger<MyServices> logger, IMessageService messageService, IAutoQueryDb autoQuery)
    {
        _logger = logger;
        _messageService = messageService;
        this.AutoQuery = autoQuery;
    }

    public async Task<object> PostAsync(CreateSendMessageRequest request)
    {
        var result = await _messageService.CreateSendMessage(request);
        return result;
    }
    public object Any(QueryMessages request)
    {
        var userSession = base.SessionAs<AuthUserSession>();

        using var db = AutoQuery.GetDb(request, base.Request);
        
        // Tạo truy vấn từ Request DTO với AutoQuery
        var query = AutoQuery.CreateQuery(request, base.Request, db);
        
        if (userSession?.Roles.Contains("admin") == true)
        {
            query.And(x => x.CreatedBy == "admin");
        }

        // Thực thi truy vấn và trả về kết quả
        return AutoQuery.Execute(request, query, base.Request, db);
    }
}