using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SmsGateCms.ServiceModel;

[Description("Số dư")]
[Notes("Quản giao dịch")]
public class Transaction : AuditBase
{
    [AutoIncrement] public long Id { get; set; }
    [StringLength(100)] public string TransactionCode { get; set; }

    [Ref(Model = nameof(Partner), RefId = nameof(Partner.Id), RefLabel = nameof(Partner.PartnerCode))]
    [References(typeof(Partner))]
    public int PartnerId { get; set; }

    [StringLength(10)] public string CurrencyCode { get; set; }
    [StringLength(100)] public string TransactionType { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    [StringLength(50)] public TransactionStatus Status { get; set; }
    public string Description { get; set; }
    public ICollection<TransactionHistory> TransactionHistories { get; set; }
}

[EnumAsInt]
public enum TransactionStatus : byte
{
    [Description("Khởi tạo")] Initial = 0,
    [Description("Thành công")] Sent = 1,
    [Description("Đang xử lý")] Delivered = 2,
    [Description("Thất bại")] Failed = 3
}