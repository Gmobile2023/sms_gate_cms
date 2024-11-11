using SmsGate.Balance.Models.Dtos;
using SmsGate.Balance.Models.Enums;

using ServiceStack;
using System.Runtime.Serialization;

namespace SmsGate.Balance.Models.Requests;

// [DataContract]
// [Route("/api/v1/wallet/status", "PUT")]
// public class AccountUpdateStatusRequest : IPut, IReturn<object>
// {
//     [DataMember(Order = 1)] public string AccountCode { get; set; }
//     [DataMember(Order = 2)] public string CurrencyCode { get; set; }
//     [DataMember(Order = 3)] public BalanceStatus Status { get; set; }
// }
//
// [DataContract]
// [Route("/api/v1/wallet/checkBalanceInfo", "GET")]
// public class AccountBalanceInfoCheckRequest : IGet, IReturn<ResponseMessageBase<AccountBalanceInfo>>
// {
//     [DataMember(Order = 1)] public string AccountCode { get; set; }
//     [DataMember(Order = 2)] public string CurrencyCode { get; set; }
// }