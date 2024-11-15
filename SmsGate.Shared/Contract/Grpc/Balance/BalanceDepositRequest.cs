using System.Runtime.Serialization;
using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;

[DataContract]
[Route("/api/v1/wallet/deposit", "POST")]
[Route("/api/v1/wallet/deposit/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}", "POST")]
[Route("/api/v1/wallet/deposit/{AccountCode}/{CurrencyCode}/{Amount}/{TransRef}/{Description}", "POST")]
public class BalanceDepositRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] public decimal Amount { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string Description { get; set; }
    [DataMember(Order = 6)] public string TransNote { get; set; }
    [DataMember(Order = 7)] public string ExtraInfo { get; set; }
    [DataMember(Order = 8)] public decimal Fee { get; set; }
}