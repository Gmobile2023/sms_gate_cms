﻿using System.Runtime.Serialization;
using ServiceStack;
using SmsGate.Shared.Common;

namespace SmsGate.Shared.Contract.Grpc.Balance
{
    [DataContract]
    [Route("/api/v1/wallet/charging", "POST")]
    public class BalanceChargingRequest : IPost, IReturn<ResponseMessageBase<BalanceResponse>>
    {
        [DataMember(Order = 1)] public string AccountCode { get; set; }
        [DataMember(Order = 2)] public string CurrencyCode { get; set; }
        [DataMember(Order = 3)] public decimal Amount { get; set; }
        [DataMember(Order = 4)] public string TransRef { get; set; }
        [DataMember(Order = 5)] public string Description { get; set; }
        [DataMember(Order = 6)] public string TransNote { get; set; }
        [DataMember(Order = 7)] public TransactionType TransactionType { get; set; }
        [DataMember(Order = 8)] public string ExtraInfo { get; set; }
    }
}
