// Complete declarative AutoQuery services for Bookings CRUD example:
// https://docs.servicestack.net/autoquery-crud-bookings

using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace SmsGateCms.ServiceModel;

[Icon(Svg = Icons.Message)]
[Description("Mẫu tin nhắn")]
[Notes("Quản lý nội dung tin nhắn của đối tác")]
public class MessageTemplate : AuditBase
{
    [AutoIncrement]
    public int Id { get; set; }
    [StringLength(500)]
    public string Content { get; set; }
    public MessageTemplateStatus Status { get; set; } = MessageTemplateStatus.Initial;
    [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
    [References(typeof(Partner))]
    public int PartnerId { get; set; }

    [Reference] public Partner PartnerName { get; set; }
}

[EnumAsInt]
public enum MessageTemplateStatus : byte
{
    [Description("Khởi tạo")]
    Initial = 0,
    [Description("Đã duyệt")]
    Active = 1,
    [Description("Hủy")]
    Cancel = 2,
    [Description("Khóa")]
    Locked = 3,
}

[Tag("messages"), Description("Find Message template")]
[Notes("Find Message template")]
[Route("/messages-templates", "GET")]
[Route("/messages-templates/{Id}", "GET")]
[AutoApply(Behavior.AuditQuery)]
public class QueryMessageTemplates : QueryDb<MessageTemplate>
{
    public int? Id { get; set; }
}

[Route("/messages-templates-by-status", "GET")]
[AutoApply(Behavior.AuditQuery)]
[Route("/messages-templates-by-status/{Status}", "GET")]
[AutoApply(Behavior.AuditQuery)]
[ValidateHasRole(Roles.Partner)]
public class QueryMessageTemplatesByStatus : QueryDb<MessageTemplate>
{
    public required MessageTemplateStatus Status { get; set; }
}

// Uncomment below to enable DeletedBookings API to view deleted bookings:
// [Route("/bookings/deleted")]
// [AutoFilter(QueryTerm.Ensure, nameof(AuditBase.DeletedDate), Template = SqlTemplate.IsNotNull)]
// public class DeletedBookings : QueryDb<Booking> {}

[Tag("messages"), Description("Tạo mới mẫu tin nhắn")]
[Notes("Tạo mới mẫu tin nhắn của bạn")]
[LocodeCss(Field = "col-span-12 sm:col-span-6", Fieldset = "grid grid-cols-8 gap-2", Form = "border overflow-hidden max-w-screen-lg")]
[ExplorerCss(Field = "col-span-12 sm:col-span-6", Fieldset = "grid grid-cols-6 gap-8", Form = "border border-indigo-500 overflow-hidden max-w-screen-lg")]
[Route("/message-templates", "POST")]
[ValidateHasRole(Roles.Partner)]
[AutoApply(Behavior.AuditCreate)]
public class CreateMessageTemplate : ICreateDb<MessageTemplate>, IReturn<IdResponse>
{
    [Input(Type = "textarea")]
    [Description("Missing template"), ValidateNotEmpty]
    public required string? Content { get; set; }
    [AutoDefault(Value = MessageTemplateStatus.Initial)]
    public MessageTemplateStatus Status { get; set; }
    [Reference]
    public int? PartnerName { get; set; }
}

[Tag("messages"), Description("Chỉnh sửa mẫu tin nhắn")]
[Notes("Chỉnh sửa mẫu tin nhắn của bạn")]
[Route("/message-templates/{Id}", "PATCH")]
// [ValidateHasRole(Roles.Partner)]
[AutoApply(Behavior.AuditModify)]
public class UpdateMessageTemplate : IPatchDb<MessageTemplate>, IReturn<IdResponse>
{
    public required int Id { get; set; }
    [Input(Type = "textarea")]
    [Description("Missing template"), ValidateNotEmpty]
    public string Content { get; set; }
    public MessageTemplateStatus Status { get; set; }
    // public Partner Partner { get; set; }
}

[Tag("messages"), Description("Delete a Message")]
[Route("/message-templates/{Id}", "DELETE")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditSoftDelete)]
public class DeleteMessageTemplate : IDeleteDb<MessageTemplate>, IReturnVoid
{
    public required int Id { get; set; }
}


public class MessageTemplateResponse
{
    public int Id { get; set; }
    public string Result { get; set; }
}

[Icon(Svg = Icons.Message)]
[Description("Mẫu tin nhắn chi tiết")]
[Notes("Mẫu tin nhắn chi tiết")]
public class MessageTemplateDetail : IHasId<int>
{
    public int Id { get; set; }
    [References(typeof(MessageTemplate))]
    public int MessageTemplateId { get; set; }
    [Reference] public MessageTemplate MessageTemplate { get; set; }
    public string Content { get; set; }
}

[Tag("messages")] [Description("Find Message template detail")]
[Notes("Find Message template detail")]
[Route("/messages-templates-detail", "GET")]
[Route("/messages-templates-detail/{Id}", "GET")]
public class QueryMessageTemplateDetails : QueryDb<MessageTemplateDetail>
{
    public int? Id { get; set; }
}

// [Tag("messages"), Description("Tạo mới mẫu tin nhắn chi tiết")]
// [Notes("Tạo mới mẫu tin nhắn chi tiết của bạn")]
// [Route("/message-templates-detail", "POST")]
// [ValidateHasRole(Roles.Partner)]
// public class CreateMessageTemplateDetail : ICreateDb<MessageTemplateDetail>, IReturn<IdResponse>
// {
//     public int MessageTemplateId { get; set; }
//     [Reference]
//     public Partner Provider { get; set; }
//     public string Content { get; set; }
// }

[Tag("messages"), Description("Chỉnh sửa mẫu tin nhắn chi tiết")]
[Notes("Chỉnh sửa mẫu tin nhắn chi tiết của bạn")]
[Route("/message-templates-detail/{Id}", "PATCH")]
// [ValidateHasRole(Roles.Partner)]
[AutoApply(Behavior.AuditModify)]
public class UpdateMessageTemplateDetail : IPatchDb<MessageTemplateDetail>, IReturn<IdResponse>
{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public string Content { get; set; }
}

[Tag("messages"), Description("Delete a Message detail")]
[Route("/message-templates-detail/{Id}", "DELETE")]
[ValidateHasRole(Roles.Manager)]
public class DeleteMessageTemplateDetail : IDeleteDb<MessageTemplateDetail>, IReturnVoid
{
    public int Id { get; set; }
}