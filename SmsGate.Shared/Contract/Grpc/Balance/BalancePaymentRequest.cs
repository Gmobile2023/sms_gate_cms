
using ServiceStack;
using ServiceStack.DataAnnotations;
using System.Runtime.Serialization;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;

[DataContract]
[Route("/api/v1/wallet/payment", "POST")]
public class BalancePaymentRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
{
    [DataMember(Order = 1)] [Required] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public decimal PaymentAmount { get; set; }
    [DataMember(Order = 3)] public string CurrencyCode { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string TransCode { get; set; }
    [DataMember(Order = 6)] public string Description { get; set; }
    [DataMember(Order = 7)] public string MerchantCode { get; set; }
    [DataMember(Order = 8)] public string TransNote { get; set; }
    [DataMember(Order = 9)] public decimal Fee { get; set; }
}