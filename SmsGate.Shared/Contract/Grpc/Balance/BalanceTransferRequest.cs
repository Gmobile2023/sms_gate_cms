using System.Runtime.Serialization;
using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance
{
    [DataContract]
    [Route("/api/v1/wallet/transfer", "POST")]
    [Route("/api/v1/wallet/transfer/{SrcAccount}/{DesAccount}/{CurrencyCode}/{Amount}", "POST")]
    [Route("/api/v1/wallet/transfer/{SrcAccount}/{DesAccount}/{CurrencyCode}/{Amount}/{Description}", "POST")]
    public class BalanceTransferRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
    {
        [ApiMember(Description = "Số tiền cần chuyển", IsRequired = true)]
        [DataMember(Order = 1)] public decimal Amount { get; set; }
        [ApiMember(Description = "Tài khoản chuyển tiền", IsRequired = true)]
        [DataMember(Order = 2)] public string SrcAccount { get; set; }
        [ApiMember(Description = "CurrencyCode loại tiền cần chuyển", IsRequired = true)]
        [DataMember(Order = 3)] public string CurrencyCode { get; set; }
        [ApiMember(Description = "Mã tài khoản nhận", IsRequired = true)]
        [DataMember(Order = 4)] public string DesAccount { get; set; }
        [ApiMember(Description = "Mã giao dịch tham chiếu (APP không cần chuyển)", IsRequired = false)]
        [DataMember(Order = 5)] public string TransRef { get; set; }
        [ApiMember(Description = "Ghi chú chuyển tiền", IsRequired = false)]
        [DataMember(Order = 6)] public string Description { get; set; }
        [DataMember(Order = 7)] public string TransNote { get; set; }
        [ApiMember(Description = "Chuyển tiền vào mục đích gì. Nếu k truyền thông tin này thì mặc định là chuyển tiền. Để tặng GSTAR thì truyền GiveGSTAR = 30 ", IsRequired = false)]
        [ApiAllowableValues("TransactionType", typeof(TransactionType))]
        [DataMember(Order = 8)] public TransactionType TransactionType { get; set; }
        [ApiMember(Description = "Phí chuyển tiên", IsRequired = true)]
        [DataMember(Order = 9)] public decimal Fee { get; set; }
        [DataMember(Order = 10)] public FeeType FeeType { get; set; }
    }
}
