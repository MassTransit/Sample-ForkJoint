namespace ForkJoint.Components
{
    using System;
    using System.Collections.Generic;
    using MassTransit.Courier;
    using MassTransit.JobService.Components;
    using MassTransit.Util;


    public class FutureMessage
    {
        public FutureMessage(Guid resultTypeId, IDictionary<string, object> result)
        {
            ResultTypeId = resultTypeId;
            Result = result;
        }

        public IDictionary<string, object> Result { get; }
        public Guid ResultTypeId { get; }

        public T ToObject<T>()
            where T : class
        {
            return ObjectTypeDeserializer.Deserialize<T>(Result);
        }
    }


    public class FutureMessage<T> :
        FutureMessage
        where T : class
    {
        public FutureMessage(T result)
            : base(JobMetadataCache<T>.JobTypeId, SerializerCache.GetObjectAsDictionary(result))
        {
            // TODO change to capture entire message from ConsumeContext with SupportedMessageTypes
        }
    }
}