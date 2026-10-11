using System;
using System.Collections.Generic;

namespace KaiChe.Infrastructure
{
    /// <summary>
    /// Stable seams for third-party integrations. Competing libraries can be evaluated
    /// without coupling gameplay code directly to a concrete package.
    /// </summary>
    public static class ThirdPartyIntegrationPolicy
    {
        public const PoolBackend ActivePoolBackend = PoolBackend.UPools;
        public const EventBackend ActiveEventBackend = EventBackend.MessagePipe;
        public const AiBackend ActiveAiBackend = AiBackend.UnityHFSM;
        public const NetworkBackend ActiveNetworkBackend = NetworkBackend.None;
        public const CloudBackend ActiveCloudBackend = CloudBackend.None;

        public static bool IsOnlineEnabled => ActiveNetworkBackend != NetworkBackend.None;
        public static bool IsCloudEnabled => ActiveCloudBackend != CloudBackend.None;
    }

    public enum PoolBackend
    {
        UPools = 0,
        KPooling = 1,
        XPool = 2
    }

    public enum EventBackend
    {
        MessagePipe = 0,
        UnityEventBus = 1,
        R3 = 2
    }

    public enum AiBackend
    {
        UnityHFSM = 0,
        Behaviac = 1,
        Morpeh = 2
    }

    public enum NetworkBackend
    {
        None = 0,
        Mirror = 1,
        LiteNetLib = 2,
        MagicOnion = 3
    }

    public enum CloudBackend
    {
        None = 0,
        PlayFab = 1,
        Supabase = 2
    }

    public interface IGamePoolAdapter<T> where T : class
    {
        T Rent();
        void Return(T instance);
        void Prewarm(int count);
        void Clear();
    }

    public interface IGameEventBus
    {
        IDisposable Subscribe<T>(Action<T> handler);
        void Publish<T>(T message);
    }

    public interface IStateSerializer
    {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(ReadOnlyMemory<byte> payload);
    }

    public interface IRealtimeTransport : IDisposable
    {
        bool IsConnected { get; }
        event Action Connected;
        event Action Disconnected;
        event Action<ReadOnlyMemory<byte>> PayloadReceived;
        void Connect(string endpoint);
        void Send(ReadOnlyMemory<byte> payload);
        void Tick();
        void Disconnect();
    }

    public interface IRemoteProfileStore
    {
        bool IsAuthenticated { get; }
        void Authenticate(string playerId, Action<bool> completed);
        void Load(string key, Action<bool, string> completed);
        void Save(string key, string value, Action<bool> completed);
    }

    public interface IEnemyDecisionBackend
    {
        void Tick(float deltaTime, in EnemyDecisionInput input, ref EnemyDecisionOutput output);
    }

    public readonly struct EnemyDecisionInput
    {
        public EnemyDecisionInput(float distanceToPlayer, float healthNormalized, bool canAttack)
        {
            DistanceToPlayer = distanceToPlayer;
            HealthNormalized = healthNormalized;
            CanAttack = canAttack;
        }

        public float DistanceToPlayer { get; }
        public float HealthNormalized { get; }
        public bool CanAttack { get; }
    }

    public struct EnemyDecisionOutput
    {
        public bool ShouldChase;
        public bool ShouldAttack;
        public bool ShouldRetreat;
    }

    public sealed class InMemoryEventBus : IGameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers =
            new Dictionary<Type, List<Delegate>>();

        public IDisposable Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type type = typeof(T);
            if (!_handlers.TryGetValue(type, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(type, handlers);
            }

            handlers.Add(handler);
            return new Subscription(() => handlers.Remove(handler));
        }

        public void Publish<T>(T message)
        {
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> handlers))
                return;

            Delegate[] snapshot = handlers.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i] is Action<T> callback)
                    callback(message);
            }
        }

        private sealed class Subscription : IDisposable
        {
            private Action _dispose;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                Action dispose = _dispose;
                _dispose = null;
                dispose?.Invoke();
            }
        }
    }
}
