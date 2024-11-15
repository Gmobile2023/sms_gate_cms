using System.Runtime.Serialization;

using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Balance.Models.Requests.ApiAppRequests;

[DataContract]
[Tag(Name = "Api app")]
[Route("/api/v1/wallet/account/transfer", Summary = "Chuyển tiền tài khoản", Verbs = "POST")]
public class BalanceTransferAccountRequest : IPost, IReturn<object>
{
    [ApiMember(Description = "Số tiền cần chuyển", IsRequired = true)]
    [DataMember(Order = 1)]
    public decimal Amount { get; set; }

    [ApiMember(Description = "CurrencyCode loại tiền cần chuyển", IsRequired = true)]
    [DataMember(Order = 2)]
    public string CurrencyCode { get; set; }

    [ApiMember(Description = "Mã tài khoản nhận", IsRequired = true)]
    [DataMember(Order = 3)]
    public string DesAccount { get; set; }

    [ApiMember(Description = "Ghi chú chuyển tiền", IsRequired = false)]
    [DataMember(Order = 4)]
    public string Description { get; set; }

    [ApiMember(
        Description =
            "Chuyển tiền vào mục đích gì. Nếu k truyền thông tin này thì mặc định là chuyển tiền. Để tặng GSTAR thì truyền Give = 30 ",
        IsRequired = false)]
    [ApiAllowableValues("TransactionType", typeof(TransactionType))]
    [DataMember(Order = 5)]
    public TransactionType TransactionType { get; set; }

    [IgnoreDataMember]
    [DataMember(Order = 6)]
    public string SrcAccount { get; set; }

    [IgnoreDataMember]
    [DataMember(Order = 7)]
    public decimal Fee { get; set; }

    [DataMember(Order = 8)]
    public string TransNote { get; set; }    
}