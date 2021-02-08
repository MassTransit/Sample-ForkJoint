namespace ForkJoint.Contracts
{
    using System;


    public interface CookFry
    {
        Guid OrderId { get; }
        Guid OrderLineId { get; }

        Size Size { get; }
    }


    public class CookFryRequest :
        CookFry
    {
        public CookFryRequest(Guid orderId, Guid orderLineId, Size size)
        {
            OrderId = orderId;
            OrderLineId = orderLineId;
            Size = size;
        }

        public Guid OrderId { get; }
        public Guid OrderLineId { get; }
        public Size Size { get; }
    }
}