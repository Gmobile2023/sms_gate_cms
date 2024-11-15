using System.Runtime.Serialization;
using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance
{
    [DataContract]
    [Route("/api/v1/wallet/fee/{SrcAccount}/{CurrencyCode}/{Fee}", "POST")]
    public class BalanceChargeFeeRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
    {
        [DataMember(Order = 1)] public string SrcAccount { get; set; }
        [DataMember(Order = 2)] public string DesAccount { get; set; }
        [DataMember(Order = 3)] public string CurrencyCode { get; set; }
        [DataMember(Order = 4)] public decimal Fee { get; set; }
        [DataMember(Order = 5)] public string TransRef { get; set; }
        [DataMember(Order = 6)] public string Description { get; set; }
        [DataMember(Order = 7)] public string TransNote { get; set; }
        [DataMember(Order = 8)] public TransactionType TransactionType { get; set; }
        [DataMember(Order = 9)] public string ExtraInfo { get; set; }
    }
}