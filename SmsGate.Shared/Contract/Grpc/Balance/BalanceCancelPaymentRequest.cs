
using ServiceStack;
using System.Runtime.Serialization;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;

[DataContract]
[Route("/api/v1/wallet/cancel_payment", "POST")]
public class BalanceCancelPaymentRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
{
    [DataMember(Order = 1)] public string TransactionCode { get; set; }
    [DataMember(Order = 2)] public decimal RevertAmount { get; set; }
    [DataMember(Order = 3)] public string TransRef { get; set; }
    [DataMember(Order = 4)] public string TransNote { get; set; }
    [DataMember(Order = 5)] public string Description { get; set; }
    [DataMember(Order = 6)] public string CurrencyCode { get; set; }
    [DataMember(Order = 7)] public string AccountCode { get; set; }
}