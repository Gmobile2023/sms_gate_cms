using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SmsGateCms.ServiceModel;

[Description("Quản lý đối tác")]
public class Partner : AuditBase
{
    [AutoIncrement] public int Id { get; set; }
    [StringLength(50)] public string PartnerCode { get; set; }
    [StringLength(150)] public string PartnerName { get; set; }
    [StringLength(150)] public string EmailAddress { get; set; }
    [StringLength(15)] public string PhoneNumber { get; set; }
    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(200)] public string Ips { get; set; }
    public PartnerStatus Status { get; set; } = 0;
}

[EnumAsInt]
public enum PartnerStatus : byte
{
    [Description("Hoạt động")] Active = 1,
    [Description("Ngưng hoạt động")] Inactive = 0
}

[Tag("partner"), Description("Danh sách đối tác")]
[Route("/partners", "GET")]
[Route("/partner/{Id}", "GET")]
[ValidateHasRole(Roles.Manager)]
public class GetPartners : QueryDb<Partner>
{
    public int? Id { get; set; }
}

[Tag("partner"), Description("Tạo mới đối tác")]
[Route("/partner", "POST")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditCreate)]
public class CreatePartner : ICreateDb<Partner>, IReturn<IdResponse>
{
    [StringLength(50)] public string PartnerCode { get; set; }
    [StringLength(150)] public string PartnerName { get; set; }
    [StringLength(150)] public string EmailAddress { get; set; }
    [StringLength(15)] public string PhoneNumber { get; set; }
    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(200)] public string Ips { get; set; }
    public PartnerStatus Status { get; set; } = 0;
}

[Tag("partner"), Description("Cập nhật đối tác")]
[Route("/partner/{Id}", "PATCH")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditModify)]
public class UpdatePartner : IPatchDb<Partner>, IReturn<IdResponse>
{
    public int Id { get; set; }
    [StringLength(50)] public string PartnerCode { get; set; }
    [StringLength(150)] public string PartnerName { get; set; }
    [StringLength(150)] public string EmailAddress { get; set; }
    [StringLength(15)] public string PhoneNumber { get; set; }
    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(200)] public string Ips { get; set; }
    public PartnerStatus Status { get; set; } = 0;
}

[Tag("partner"), Description("Xóa đối tác")]
[Route("/partner/{Id}", "DELETE")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditSoftDelete)]
public class DeletePartner : IDeleteDb<Partner>, IReturnVoid
{
    public int Id { get; set; }
}