using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SmsGateCms.ServiceModel;

[Description("Quản lý nhà cung cấp")]
public class Provider : AuditBase
{
    [AutoIncrement] public int Id { get; set; }
    [StringLength(50)] public string ProviderCode { get; set; }
    [StringLength(150)] public string ProviderName { get; set; }
    [StringLength(150)] public string EmailAddress { get; set; }
    [StringLength(15)] public string PhoneNumber { get; set; }
    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(250)] public string ApiUrl { get; set; }
    public ProviderStatus Status { get; set; } = 0;
}

[EnumAsInt]
public enum ProviderStatus : byte
{
    [Description("Hoạt động")] Active = 1,
    [Description("Ngưng hoạt động")] Inactive = 0
}

[Tag("provider"), Description("Danh sách nhà cung cấp")]
[Route("/providers", "GET")]
[Route("/provider/{Id}", "GET")]
[ValidateHasRole(Roles.Manager)]
public class GetProviders : QueryDb<Provider>
{
    public int? Id { get; set; }
}

[Tag("provider"), Description("Tạo mới nhà cung cấp")]
[Route("/provider", "POST")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditCreate)]
public class CreateProvider : ICreateDb<Provider>, IReturn<IdResponse>
{
    [StringLength(50)] public string ProviderCode { get; set; }
    [StringLength(150)] public string ProviderName { get; set; }
    public string EmailAddress { get; set; }

    [ValidateRegularExpression("^[0-9]{9,10}$")]
    [StringLength(15)]
    public string PhoneNumber { get; set; }

    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(250)] public string ApiUrl { get; set; }
    public ProviderStatus Status { get; set; } = 0;
}

[Tag("provider"), Description("Cập nhật nhà cung cấp")]
[Route("/provider/{Id}", "PATCH")]
[ValidateHasRole(Roles.Manager)]
[AutoApply(Behavior.AuditModify)]
public class UpdateProvider : IPatchDb<Provider>, IReturn<IdResponse>
{
    public int Id { get; set; }
    [StringLength(50)] public string ProviderCode { get; set; }
    [StringLength(150)] public string ProviderName { get; set; }
    public string EmailAddress { get; set; }

    [ValidateRegularExpression("^[0-9]{9,10}$")]
    [StringLength(15)]
    public string PhoneNumber { get; set; }

    [StringLength(50)] public string ApiKey { get; set; }
    [StringLength(50)] public string UserName { get; set; }
    [StringLength(50)] public string Password { get; set; }
    [StringLength(250)] public string ApiUrl { get; set; }
    public ProviderStatus Status { get; set; } = 0;
}

[Tag("provider"), Description("Xóa nhà cung cấp")]
[Route("/provider/{Id}", "DELETE")]
[ValidateHasRole(Roles.Manager)]
public class DeleteProvider : IDeleteDb<Provider>, IReturnVoid
{
    public int Id { get; set; }
}