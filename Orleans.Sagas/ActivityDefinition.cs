using System;

namespace Orleans.Sagas
{
    [GenerateSerializer]
    public class ActivityDefinition(Type type, ISagaPropertyBag properties)
    {
        [Id(0)]public Type Type { get; } = type;
        [Id(1)]public ISagaPropertyBag Properties { get; } = properties;
    }
}
