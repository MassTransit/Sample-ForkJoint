namespace ForkJoint.Components.Pipeline
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using MassTransit;
    using MassTransit.Util;


    class FutureCommandPipe<T> :
        IPipe<SendContext<T>>
        where T : class
    {
        readonly Uri _responseAddress;
        readonly Guid _requestId;

        public FutureCommandPipe(Uri responseAddress, Guid requestId)
        {
            _responseAddress = responseAddress;
            _requestId = requestId;
        }

        public Task Send(SendContext<T> context)
        {
            context.ResponseAddress = _responseAddress;
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