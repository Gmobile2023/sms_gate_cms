using Orleans;
using Orleans.Runtime;
using SmsGateCms.ServiceInterface.BalanceState;

namespace SmsGateCms.ServiceInterface.BalanceGrain;

public class CustomerBalanceGrain : Grain, ICustomerBalanceGrain
{
    private readonly IPersistentState<CustomerBalanceState> _balanceState;

    public CustomerBalanceGrain(
        [PersistentState("customerBalance", "balanceStorage")]
        IPersistentState<CustomerBalanceState> balanceState)
    {
        _balanceState = balanceState;
    }

    public Task<decimal> GetBalanceAsync()
    {
        return Task.FromResult(_balanceState.State.Balance);
    }

    public async Task<bool> DepositAsync(decimal amount)
    {
        _balanceState.State.Balance += amount;
        await _balanceState.WriteStateAsync();
        return true;
    }

    public async Task<bool> WithdrawAsync(decimal amount)
    {
        if (_balanceState.State.Balance >= amount)
        {
            _balanceState.State.Balance -= amount;
            await _balanceState.WriteStateAsync();
            return true;
        }
        else
        {
            throw new InvalidOperationException("Insufficient funds.");
        }
    }
}