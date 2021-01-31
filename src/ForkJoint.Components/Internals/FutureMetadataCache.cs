namespace ForkJoint.Components.Internals
{
    using System;
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using GreenPipes.Internals.Extensions;


    public static class FutureMetadataCache
    {
        static class Cached
        {
            internal static readonly ConcurrentDictionary<Type, CachedType> Instance = new();
        }


        interface CachedType
        {
            Guid TypeId { get; }
        }


        class CachedType<T> :
            CachedType
            where T : class
        {
            public Guid TypeId => FutureMetadataCache<T>.TypeId;
        }
    }


    interface IFutureMetadataCache<T>
        where T : class
    {
        Guid TypeId { get; }
    }


    public class FutureMetadataCache<T> :
        IFutureMetadataCache<T>
        where T : class
    {
        readonly Guid _typeId;

        FutureMetadataCache()
        {
            _typeId = GenerateTypeId();
        }

        public static Guid TypeId => Cached.Metadata.Value.TypeId;

        Guid IFutureMetadataCache<T>.TypeId => _typeId;

        static Guid GenerateTypeId()
        {
            var shortName = TypeCache<T>.ShortName;

            using var hasher = MD5.Create();

            var data = hasher.ComputeHash(Encoding.UTF8.GetBytes(shortName));

            return new Guid(data);
        }


        static class Cached
        {
            internal static readonly Lazy<IFutureMetadataCache<T>> Metadata = new(() => new FutureMetadataCache<T>(), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}