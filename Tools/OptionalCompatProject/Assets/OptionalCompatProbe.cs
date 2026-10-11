using MemoryPack;
using R3;
using UnityEngine;

internal static class OptionalCompatProbe
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void Verify()
    {
        byte[] payload = MemoryPackSerializer.Serialize(new[] { 1, 2, 3 });
        int[] restored = MemoryPackSerializer.Deserialize<int[]>(payload);
        Observable<int> signal = Observable.Return(restored != null ? restored.Length : 0);
        Debug.Log($"KAI_CHE_OPTIONAL_COMPAT_R3_MEMORYPACK_READY type={signal.GetType().Name}");
    }
}
