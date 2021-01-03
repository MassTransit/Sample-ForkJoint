namespace ForkJoint.Api.Models
{
    using System;
    using Contracts;


    public class OrderResponseModel
    {
        public Guid OrderId { get; init; }

        public Burger Burger { get; init; }
    }
}