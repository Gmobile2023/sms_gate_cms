using System.Runtime.Serialization;

using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance;

[DataContract]
[Route("/api/v1/wallet/revert", "POST")]
[Route("/api/v1/wallet/revert/{TransactionCode}/{TransRef}", "POST")]
[Route("/api/v1/wallet/revert/{TransactionCode}/{Reason}/{TransRef}", "POST")]
public class BalanceRevertRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
{
    [DataMember(Order = 1)] public string TransactionCode { get; set; }
    [DataMember(Order = 2)] public string Reason { get; set; }
    [DataMember(Order = 3)] public decimal RevertAmount { get; set; }
    [DataMember(Order = 4)] public string TransRef { get; set; }
    [DataMember(Order = 5)] public string TransNote { get; set; }
}