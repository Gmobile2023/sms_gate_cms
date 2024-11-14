// Complete declarative AutoQuery services for Bookings CRUD example:
// https://docs.servicestack.net/autoquery-crud-bookings

using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SmsGateCms.ServiceModel;

#region base
       
[Icon(Svg = Icons.Message)]
[Description("Tin nhắn")]
[Notes("Danh sách tin nhắn")]

public class Message : AuditBase
{
    [AutoIncrement] public long Id { get; set; }
    [StringLength(500)] public string Sms { get; set; }
    public MessageStatus Status { get; set; } = 0;
    [StringLength(15)] public string Receiver { get; set; }
    [Reference] public MessageTemplate MessageTemplate { get; set; }
    [Ref(Model = nameof(MessageTemplate), RefId = nameof(MessageTemplate.Id),
        RefLabel = nameof(MessageTemplate.Content))]
    [References(typeof(MessageTemplate))]
    public int MessageTemplateId { get; set; }

    [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
    [References(typeof(Partner))]
    public int PartnerId { get; set; }
    [Index(true)] // Đảm bảo RequestId và PartnerId là duy nhất
    public string RequestId { get; set; }

    [Reference] public Partner Partner { get; set; }

    [Ref(Model = nameof(Provider), RefId = nameof(Provider.Id), RefLabel = nameof(Provider.ProviderCode))]
    [References(typeof(Provider))]
    public int ProviderId { get; set; }
    
    [Reference] public Provider Provider { get; set; }

    public DateTime RequestDate { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? ResponseDate { get; set; }
    [StringLength(30)] public string Telco { get; set; }
    [StringLength(255)] public string ResponseMassage { get; set; }
    [StringLength(250)] [Index] public string MessageId { get; set; }
    
}

[EnumAsInt]
public enum MessageStatus : byte
{
    [Description("Khởi tạo")] Initial = 0,
    [Description("Đã gửi")] Sent = 1,
    [Description("Đã nhận")] Delivered = 2,
    [Description("Thất bại")] Failed = 3
}

[Tag("messages"), Description("Find Message")]
[Route("/messages", "GET")]
[Route("/messages/{Id}", "GET")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditQuery)]
public class QueryMessages : QueryDb<Message>
{
    public long? Id { get; set; }
    public MessageStatus? Status { get; set; }
    public string Receiver { get; set; }
    public string Telco { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string CreatedBy { get; set; }
}

#endregion

#region api cho đối tác
[Tag("messages"), Description("Create Message")]
[Route("/api/v1/messages","POST")]
[AutoApply(Behavior.AuditCreate)]
public class CreateSendMessageRequest
{
    public long Id { get; set; }
    public string Sms { get; set; }
    public string Receiver { get; set; }
    
    public string Telco { get; set; }
    
    public int ProviderId { get; set; }
    
    public string PartnerCode { get; set; }
    
    public string RequestId { get; set; }
    
}

public class SendMessageToProviderRequest
{
   public  int providerId { get; set; }
   public string PassWord { get; set; }
   public string Sender { get; set; }
   public  string BrandName { get; set; }
    public string Receiver { get; set; }
    public string Sms { get; set; }
   public  string RequestId { get; set; }
    public string TypeMsg { get; set; }
    public string Report { get; set; }
}

#endregion