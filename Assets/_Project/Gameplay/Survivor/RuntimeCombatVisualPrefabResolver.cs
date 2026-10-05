using KaiChe.Core.Content;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    internal static class RuntimeCombatVisualPrefabResolver
    {
        private const string Root = "KaiChe/CombatVisuals/";

        public static GameObject LoadPlayerPrefab()
        {
            return Load("PlayerWarrior", "PLAYER_WARRIOR");
        }

        public static GameObject LoadMonsterPrefab(string contentId)
        {
            string assetName;
            switch (contentId)
            {
                case ContentIds.StarterMonster:
                    assetName = "StarterMonster";
                    break;
                case ContentIds.RunnerMonster:
                    assetName = "RunnerMonster";
                    break;
                case ContentIds.BruteMonster:
                    assetName = "BruteMonster";
                    break;
                case ContentIds.BossMonster:
                    assetName = "BossMonster";
                    break;
                default:
                    return null;
            }

            return Load(assetName, contentId);
        }

        private static GameObject Load(string assetName, string contentId)
        {
            GameObject prefab =
                Resources.Load<GameObject>(Root + assetName);

            if (prefab != null)
                Debug.Log("KAI_CHE_R16_PREFAB_READY_" + contentId);

            return prefab;
        }
    }
}
