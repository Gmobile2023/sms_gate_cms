using ServiceStack;
using ServiceStack.DataAnnotations;

namespace SmsGateCms.ServiceModel;

[Description("Số dư")]
[Notes("Quản lý lịch sử giao dịch")]
public class TransactionHistory : AuditBase
{
    [AutoIncrement] public long Id { get; set; }
    [StringLength(100)] public string TransactionCode { get; set; }
    [StringLength(10)] public string CurrencyCode { get; set; }
    [StringLength(100)] public string TransactionType { get; set; }
    [StringLength(200)] public string ChangeReason { get; set; }
    public decimal? OldAmount { get; set; }
    public decimal? NewAmount { get; set; }
    public TransactionHistoryStatus Status { get; set; }
    [Ref(Model = nameof(Transaction), RefId = nameof(Transaction.Id))]
    [References(typeof(Transaction))]
    public long TransactionId { get; set; }
}

[EnumAsInt]
public enum TransactionHistoryStatus : byte
{
    [Description("Khởi tạo")] Initial = 0,
    [Description("Thành công")] Sent = 1,
    [Description("Đang xử lý")] Delivered = 2,
    [Description("Thất bại")] Failed = 3
}