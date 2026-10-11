using System;
using MessagePipe;
using VContainer;
using uPools;

namespace KaiChe.Infrastructure
{
    /// <summary>
    /// Concrete adapters for the currently selected third-party backends.
    /// Gameplay code stays coupled to Kai-Che contracts instead of package APIs.
    /// </summary>
    public sealed class UPoolsGamePoolAdapter<T> : IGamePoolAdapter<T>
        where T : class
    {
        private readonly ObjectPool<T> _pool;

        public UPoolsGamePoolAdapter(
            Func<T> factory,
            Action<T> onRent = null,
            Action<T> onReturn = null,
            Action<T> onDestroy = null)
        {
            _pool = new ObjectPool<T>(factory, onRent, onReturn, onDestroy);
        }

        public int Count => _pool.Count;

        public T Rent()
        {
            return _pool.Rent();
        }

        public void Return(T instance)
        {
            if (instance == null)
                return;

            _pool.Return(instance);
        }

        public void Prewarm(int count)
        {
            if (count <= 0)
                return;

            _pool.Prewarm(count);
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }

    /// <summary>
    /// Strongly typed MessagePipe channel used by systems that know their message type.
    /// This avoids reflection and keeps generic broker registration explicit.
    /// </summary>
    public sealed class MessagePipeChannel<T>
    {
        private readonly IPublisher<T> _publisher;
        private readonly ISubscriber<T> _subscriber;

        public MessagePipeChannel(IPublisher<T> publisher, ISubscriber<T> subscriber)
        {
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _subscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));
        }

        public void Publish(T message)
        {
            _publisher.Publish(message);
        }

        public IDisposable Subscribe(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return _subscriber.Subscribe(handler);
        }
    }

    /// <summary>
    /// Shared composition helpers for MessagePipe + VContainer. Each game event type
    /// remains explicit so linker/AOT behavior on Android is deterministic.
    /// </summary>
    public static class KaiCheContainerRegistration
    {
        public static MessagePipeOptions RegisterMessaging(IContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.RegisterMessagePipe();
        }

        public static void RegisterMessage<T>(IContainerBuilder builder, MessagePipeOptions options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            builder.RegisterMessageBroker<T>(options);
            builder.Register<MessagePipeChannel<T>>(Lifetime.Singleton);
        }
    }
}
