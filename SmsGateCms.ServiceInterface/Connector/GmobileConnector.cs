using System.Runtime.Serialization;
using System.Text;
using Microsoft.Extensions.Logging;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Sms.ServiceInterface.Utils;
using SmsGateCms.ServiceModel;

namespace SmsGateCms.ServiceInterface.Connector;

public class GmobileConnector : IBaseConnector
{
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(120) };
    private readonly ILogger<GmobileConnector> _logger;
    private readonly IDbConnectionFactory _connectionFactory;
    public GmobileConnector(ILogger<GmobileConnector> logger, IDbConnectionFactory connectionFactory)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
    }
    
    private string GenerateChecksum(string userName, string password, string channel, string sender, string brandName, string receiver, string content, string transId, string typeMsg, string report, string apiKey)
    {
        string rawData = $"{userName}{password}{channel}{sender}{brandName}{receiver}{content}{transId}{typeMsg}{report}{apiKey}";
        return EncryptUtils.Sha256(rawData);
    }
    
    public async Task<string> SendSmsAsync(SendMessageToProviderRequest sendRequest)
    {
        using var db = _connectionFactory.OpenDbConnection();

       var provider =  await db.SingleByIdAsync<Provider>(sendRequest.providerId);
       if (provider == null)
       {
           _logger.LogError("Provider not found for code: {ProviderId}", sendRequest.providerId);
           return "0|Provider not found";
       }

       var request = new
        {
            user_name = provider.UserName,
            password = provider.Password, // Assuming password is already hashed using MD5
            channel = "MT",
            sendRequest.Sender,
            brand_name = sendRequest.BrandName,
            sendRequest.Receiver,
            sendRequest.Sms,
            trans_id = sendRequest.RequestId,
            type_msg = sendRequest.TypeMsg,
            sendRequest.Report,
            check_sum = GenerateChecksum(provider.UserName, provider.Password, "MT", sendRequest.Sender, sendRequest.BrandName, sendRequest.Receiver, sendRequest.Sms, sendRequest.RequestId,
                sendRequest.TypeMsg, sendRequest.Report, provider.ApiKey)
        };

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, provider.ApiUrl)
        {
            Content = new StringContent(request.ToJson(), Encoding.UTF8, "application/json")
        };
        httpRequestMessage.Headers.Add("token", provider.Password);

        try
        {
            var response = await HttpClient.SendAsync(httpRequestMessage);
        
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"Response: {sendRequest.Sender}|{sendRequest.Receiver}:{sendRequest.Sms} - response: {responseContent}");

            if (response.IsSuccessStatusCode)
            {
                // Parse successful response
                var successResponse = responseContent.FromJson<ProviderSmsResponse>();
                return string.Join("|", successResponse.Status, successResponse.Msg, successResponse.MessageId);
            }
            else
            {
                _logger.LogError("Error sending SMS with Status {Status}", response.StatusCode);
                return string.Join("|", "0", response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending SMS");
            return string.Join("|", "0", e.Message);
        }
    }

    [DataContract]
    private class ProviderSmsResponse
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }
        [DataMember(Name = "msg")]
        public string Msg { get; set; }
        [DataMember(Name = "req_id")]
        public string ReqId { get; set; }
        [DataMember(Name = "message_id")]
        public string MessageId { get; set; }
    }
}