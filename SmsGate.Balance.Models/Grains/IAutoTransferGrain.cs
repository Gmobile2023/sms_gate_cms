using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace SmsGate.Balance.Models.Grains;

public interface IAutoTransferGrain : IGrainWithStringKey, IRemindable
{
    Task Start();
}