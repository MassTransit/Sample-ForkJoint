namespace ForkJoint.Components.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Util;


    class FutureResponsePipe<T> :
        IPipe<SendContext<T>>
        where T : class
    {
        readonly Guid _requestId;

        public FutureResponsePipe(Guid requestId)
        {
            _requestId = requestId;
        }

        public Task Send(SendContext<T> context)
        {
            context.RequestId = _requestId;

            // TODO add future location header
            return TaskUtil.Completed;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope(nameof(FutureResponsePipe<T>));
        }
    }
}