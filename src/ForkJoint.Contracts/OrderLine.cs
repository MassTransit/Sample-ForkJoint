namespace ForkJoint.Contracts;

using System;
using MassTransit;


[ExcludeFromTopology]
public interface OrderLine
{
    Guid OrderId { get; }
    Guid OrderLineId { get; }
}