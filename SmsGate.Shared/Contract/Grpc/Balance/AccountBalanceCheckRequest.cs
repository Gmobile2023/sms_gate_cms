using System.Runtime.Serialization;
using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;

[DataContract]
[Route("/api/v1/wallet/check", "GET")]
public class AccountBalanceCheckRequest : IGet, IReturn<ResponseMessageBase<decimal>>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
}

[DataContract]
[Route("/api/v1/wallet/check-block", "GET")]
public class AccountBlockCheckRequest : IGet, IReturn<ResponseMessageBase<decimal>>
{
    [DataMember(Order = 1)] public string AccountCode { get; set; }
    [DataMember(Order = 2)] public string CurrencyCode { get; set; }
}