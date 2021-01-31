namespace ForkJoint.Components
{
    using System;
    using System.Collections.Generic;
    using Internals;
    using MassTransit.Courier;
    using MassTransit.Util;


    public class FutureMessage
    {
        public FutureMessage(Guid typeId, IDictionary<string, object> message)
        {
            TypeId = typeId;
            Message = message;
        }

        public IDictionary<string, object> Message { get; private set; }
        public Guid TypeId { get; private set; }

        public T ToObject<T>()
            where T : class
        {
            return ObjectTypeDeserializer.Deserialize<T>(Message);
        }
    }


    public class FutureMessage<T> :
        FutureMessage
        where T : class
    {
        public FutureMessage(T result)
            : base(FutureMetadataCache<T>.TypeId, SerializerCache.GetObjectAsDictionary(result))
        {
            // TODO change to capture entire message from ConsumeContext with SupportedMessageTypes
        }
    }
}