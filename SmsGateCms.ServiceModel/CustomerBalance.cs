using ServiceStack;
using ServiceStack.DataAnnotations;
using SmsGateCms.ServiceModel.Utils;

namespace SmsGateCms.ServiceModel;

[Description("Số dư")]
[Notes("Quản lý số dư khách hàng")]
public class CustomerBalance : AuditBase
{
    [AutoIncrement] public long Id { get; set; }
    [Computed] [Ignore] public decimal? AvailableBalance => Balance + LimitOverDraft - MinBalance - BlockedMoney;
    public decimal LimitOverDraft { get; set; } = 0;
    public decimal MinBalance { get; set; } = 0;
    public decimal BlockedMoney { get; set; } = 0;
    public string CurrencyCode { get; set; }
    public CustomerBalanceStatus Status { get; set; }
    [StringLength(100)] public string LastTransCode { get; set; }
    public DateTime? LastUpdated { get; set; } = DateTime.Now;
    public decimal Balance { get; set; } = 0;

    [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
    [References(typeof(Partner))]
    public int PartnerId { get; set; }

    [Reference] public Partner Partner { get; set; }

    public string ToCheckSum()
    {
        var plantText =
            $"{PartnerId}{CurrencyCode}{Balance:0.0000}{LastTransCode}000071a8-77e8-54e4-b4a5-08dd0094f02d";
        return plantText.Sha256();
    }
}

[EnumAsInt]
public enum CustomerBalanceStatus : byte
{
    [Description("Hoạt động")] Active = 1,
    [Description("Ngưng hoạt động")] Inactive = 0
}