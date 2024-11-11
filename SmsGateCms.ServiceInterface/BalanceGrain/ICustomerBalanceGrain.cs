using Orleans;

namespace SmsGateCms.ServiceInterface.BalanceGrain;

public interface ICustomerBalanceGrain : IGrainWithStringKey
{
    Task<decimal> GetBalanceAsync();
    Task<bool> DepositAsync(decimal amount);
    Task<bool> WithdrawAsync(decimal amount);
}