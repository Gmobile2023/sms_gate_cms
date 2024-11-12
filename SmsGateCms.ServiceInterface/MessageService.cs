using System.Runtime.Serialization;
using System.Text;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Sms.ServiceInterface.Utils;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface;

public class MessageService : IMessageService
{
    private readonly ILogger<MessageService> _logger;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IBaseConnector _baseConnector;
    public MessageService(ILogger<MessageService> logger, IDbConnectionFactory connectionFactory, IBaseConnector baseConnector)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _baseConnector = baseConnector;
    }

    public async Task<bool> CreateSendMessage(CreateSendMessageRequest request)
    {
        using var db = _connectionFactory.OpenDbConnection();
        using var trans = db.OpenTransaction();
        try
        {
            _logger.LogInformation($"Create Send Message Request {request.ToJson()}");
            var checkMessageTemplate = await db.SingleAsync<MessageTemplate>(x => request.Sms.Contains(x.Content));
           
            if (!(checkMessageTemplate != null && checkMessageTemplate.Status == MessageTemplateStatus.Active))
            {
                return false;
            }
            if (request.Telco.Length != 10 && request.Receiver.Length != 10)
            {
                return false;
            }
            var subtractMoney = await SubtractMoney(10,1);
            if (subtractMoney.Equals(-1))
            {
                return false;
            }
            _logger.LogInformation("Send Request To Provider");
            var sendMessageToProvider = request.ConvertTo<SendMessageToProviderRequest>();
            await _baseConnector.SendSmsAsync(sendMessageToProvider);
            var newMessage = request.ConvertTo<Message>();
            newMessage.Status = MessageStatus.Initial;
            newMessage.MessageTemplateId = checkMessageTemplate.Id;
            newMessage.RequestDate = new DateTime();
            newMessage.ResponseMassage = "";
            await db.InsertAsync<Message>(newMessage);
            trans.Commit();
            
            
            return true;

        }
        catch(Exception ex)
        {
            trans.Rollback();
            return false;
        }
    }

    private async Task<double> SubtractMoney(double currentAmount , double amountDeducted  )
    {
        var finalBalance = currentAmount - amountDeducted;
        if (finalBalance < 0)
        {
            return -1;
        }

        return finalBalance;
    }
    
}