using System;
using MassTransit;

namespace SmsGate.Shared.Contract
{
    public interface ICommand : CorrelatedBy<Guid>
    {
        DateTime TimeStamp { get; set; }
    }
}