using MassTransit;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using SmsGate.Shared.Common;
using SmsGate.Shared.Contract.Command;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface.BusinessService;

public class MessageService : IMessageService
{
    private readonly ILogger<MessageService> _logger;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IBaseConnector _baseConnector;
    private readonly IRequestClient<IPaymentCancelCommand> _paymentRequestClient;
    private readonly IRequestClient<ICheckBalanceCommand> _getBalanceRequestClient;

    public MessageService(ILogger<MessageService> logger, IDbConnectionFactory connectionFactory,
        IBaseConnector baseConnector, IRequestClient<IPaymentCancelCommand> paymentRequestClient,
        IRequestClient<ICheckBalanceCommand> getBalanceRequestClient)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _baseConnector = baseConnector;
        _paymentRequestClient = paymentRequestClient;
        _getBalanceRequestClient = getBalanceRequestClient;
    }

    public async Task<ResponseMessageBase<object>> SendMessageRequest(SendMessageRequest request)
    {
        using var db = _connectionFactory.OpenDbConnection();
        try
        {
            _logger.LogInformation($"Create Send Message Request {request.ToJson()}");
            var checkMessageTemplate = await db.SingleAsync<MessageTemplate>(x => x.Content == request.Sms);
            if (checkMessageTemplate is not { Status: MessageTemplateStatus.Active })
            {
                return ResponseMessageBase<object>.Error(ResponseCodeConst.MessageTemplateNotFound,
                    "Message Template does not exist");
            }

            var messageId = Guid.NewGuid().ToString();
            decimal amount = 1000;
            //check số dư khả dụng
            var checkBalance = await _getBalanceRequestClient.GetResponse<ResponseMessageBase<decimal>>(new
            {
                AccountCode = request.PartnerCode,
                CurrencyCode = CurrencyCode.VND.ToString("G")
            });
            if (checkBalance.Message.ResponseStatus.ErrorCode != ResponseCodeConst.Success)
            {
                return ResponseMessageBase<object>.Error("Yêu cầu không thành công. Vui lòng thực hiện lại sau");
            }

            if (checkBalance.Message.Results < amount)
            {
                return ResponseMessageBase<object>.Error(ResponseCodeConst.BalanceNotEnough,
                    "Balance not enough");
            }

            var newMessage = request.ConvertTo<Message>();
            newMessage.Status = MessageStatus.Initial;
            newMessage.MessageTemplateId = checkMessageTemplate.Id;
            newMessage.RequestDate = new DateTime();
            newMessage.ResponseMassage = "";
            var message = await db.InsertAsync(newMessage);
            if (message <= 0)
                return ResponseMessageBase<object>.Error("Giao dịch không thành công");
            //trừ tiền
            var payment = await _paymentRequestClient.GetResponse<ResponseMessageBase<object>>(new
            {
                TransCode = request.RequestId,
                AccountCode = request.PartnerCode,
                TransNote = $"Thanh toán cho giao dịch gửi sms có mã giao dịch {request.RequestId}",
                CurrencyCode = CurrencyCode.VND.ToString("G")
            });
            if (payment.Message.ResponseStatus.ErrorCode != ResponseCodeConst.Success)
            {
                newMessage.Status = MessageStatus.Failed;
                await db.UpdateAsync(message);
                return ResponseMessageBase<object>.Error("Yêu cầu không thành công. Vui lòng thực hiện lại sau");
            }
            newMessage.Status = MessageStatus.Payed;
            _logger.LogInformation("Send Request To Provider");
            var sendMessageToProvider = request.ConvertTo<SendMessageToProviderRequest>();
            await _baseConnector.SendSmsAsync(sendMessageToProvider);

            return ResponseMessageBase<object>.Success();
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private async Task<double> SubtractMoney(double currentAmount, double amountDeducted)
    {
        var finalBalance = currentAmount - amountDeducted;
        if (finalBalance < 0)
        {
            return -1;
        }

        return finalBalance;
    }
}