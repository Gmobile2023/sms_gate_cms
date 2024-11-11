using System.Threading.Tasks;
using Orleans.Concurrency;
using Orleans.Placement;

namespace Orleans.Sagas;

[StatelessWorker]
public class SagaCancellationGrain : Grain<SagaCancellationGrainState>, ISagaCancellationGrain
{
    public async Task RequestAbort()
    {
        if (!State.AbortRequested)
        {
            State.AbortRequested = true;
            await WriteStateAsync();
        }
    }

    public Task<bool> HasAbortBeenRequested()
    {
        return Task.FromResult(State.AbortRequested);
    }

    public Task Dispose()
    {
        DeactivateOnIdle();
        return Task.CompletedTask;
    }
}