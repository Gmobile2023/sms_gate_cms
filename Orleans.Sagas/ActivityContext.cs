using Orleans.Runtime;
using System;
using System.Collections.Generic;

namespace Orleans.Sagas
{
    public class ActivityContext(
        Guid sagaId,
        IGrainFactory grainFactory,
        IGrainContextAccessor grainContextAccessor,
        Dictionary<string, string> existingProperties)
        : IActivityContext
    {
        public Guid SagaId { get; } = sagaId;
        public IGrainFactory GrainFactory { get; } = grainFactory;
        public ISagaPropertyBag SagaProperties { get; } = new SagaPropertyBag(existingProperties);
        private IGrainContextAccessor GrainContextAccessor { get; } = grainContextAccessor;
        public IGrainContext GrainContext => GrainContextAccessor.GrainContext;

        public string GetSagaError()
        {
            if (!SagaProperties.ContainsKey(SagaPropertyBagKeys.ActivityErrorPropertyKey))
            {
                return null;
            }

            return SagaProperties.Get<string>(SagaPropertyBagKeys.ActivityErrorPropertyKey);
        }
    }
}