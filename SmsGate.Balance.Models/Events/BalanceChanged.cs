using HLS.Paygate.Balance.Models.Dtos;
using HLS.Paygate.Shared;
using HLS.Paygate.Shared.Messages.Events;

namespace HLS.Paygate.Balance.Models.Events;

public interface BalanceChanged : IEvent
{
    string AccountCode { get; }
    decimal Amount { get; }
    string TransRef { get; }
    string TransCode { get; }
    string TransNote { get;}
    TransactionType TransactionType { get; }
}