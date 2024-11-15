using System.Runtime.Serialization;

using ServiceStack;
using ServiceStack.DataAnnotations;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;
[DataContract]
[Route("/api/v1/wallet/unblock", "POST")]
public class UnBlockBalanceRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
{
    [DataMember(Order = 1)] [Required] public string AccountCode { get; set; }
    [DataMember(Order = 2)] [Required] public string CurrencyCode { get; set; }
    [DataMember(Order = 3)] [Required] public decimal UnBlockAmount { get; set; }
    [DataMember(Order = 4)] [Required] public string TransRef { get; set; }
    [DataMember(Order = 5)] [Required] public string TransNote { get; set; }
    [DataMember(Order = 6)] public bool IsUnBlockAllBalance { get; set; }
}
