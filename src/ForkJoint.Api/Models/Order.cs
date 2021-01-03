namespace ForkJoint.Api.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Contracts;


    public class Order
    {
        [Required]
        public Guid OrderId { get; init; }

        public Burger[] Burgers { get; init; }
    }
}