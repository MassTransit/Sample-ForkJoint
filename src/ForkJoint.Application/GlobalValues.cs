using GreenPipes;
using GreenPipes.Configurators;
using System;

namespace ForkJoint.Application
{
    public static class GlobalValues
    {
        public readonly static int? PrefetchCount = null;

        public readonly static int? ConcurrentMessageLimit = null;

        public readonly static bool UseLazyQueues = true;

        public readonly static bool PreOrderOnionRings = true;

        public readonly static bool UseQuorumQueues = true;

        public static void RetryPolicy(IRetryConfigurator retryConfigurator)
        {
            retryConfigurator.Exponential(3, TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(150));
        }
    }
}
