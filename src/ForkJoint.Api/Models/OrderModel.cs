namespace ForkJoint.Api.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;


    public class OrderModel
    {
        [Required]
        public Guid OrderId { get; init; }

        public bool Lettuce { get; init; }
    }
}