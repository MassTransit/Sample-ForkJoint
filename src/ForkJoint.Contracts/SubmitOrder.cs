namespace ForkJoint.Contracts
{
    using System;


    public interface SubmitOrder
    {
        Guid OrderId { get; }

        Burger[] Burgers { get; }
    }
}