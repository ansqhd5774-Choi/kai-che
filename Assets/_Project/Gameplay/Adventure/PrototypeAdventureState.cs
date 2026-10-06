using UnityEngine;

namespace KaiChe.Gameplay.Adventure
{
    public static class PrototypeAdventureState
    {
        private const string Prefix = "KaiChe.Adventure.";
        private const int HealthStateVersion = 1;
        private const int InventoryStateVersion = 1;
        public const int StarterPotionCount = 3;
        public const float BaseMaxHealth = 100f;

        private static bool _loaded;

        public static int PotionCount { get; private set; }
        public static float Health { get; private set; } = BaseMaxHealth;
        public static float MaxHealth { get; private set; } = BaseMaxHealth;

        public static void EnsureLoaded()
        {
            if (_loaded) return;

            PotionCount = Mathf.Max(
                0,
                PlayerPrefs.GetInt(Prefix + "PotionCount", 0));
            MaxHealth = Mathf.Max(
                BaseMaxHealth,
                PlayerPrefs.GetFloat(
                    Prefix + "MaxHealth",
                    BaseMaxHealth));
            Health = Mathf.Clamp(
                PlayerPrefs.GetFloat(Prefix + "Health", BaseMaxHealth),
                0f,
                MaxHealth);
            _loaded = true;

            if (PlayerPrefs.GetInt(
                    Prefix + "HealthState.Version",
                    0) < HealthStateVersion)
            {
                SaveHealthState();
                Debug.Log("KAI_CHE_R27_HEALTH_STATE_MIGRATED");
            }

            if (PlayerPrefs.GetInt(
                    Prefix + "InventoryState.Version",
                    0) < InventoryStateVersion)
            {
                PotionCount = Mathf.Max(
                    PotionCount,
                    StarterPotionCount);
                SaveInventory();
                Debug.Log(
                    "KAI_CHE_R29_POTION_INVENTORY_MIGRATED_COUNT_"
                    + PotionCount);
            }
        }

        public static void SetHealth(float health)
        {
            EnsureLoaded();
            SetHealth(health, MaxHealth);
        }

        public static void SetHealth(float health, float maxHealth)
        {
            EnsureLoaded();

            MaxHealth = Mathf.Max(BaseMaxHealth, maxHealth);
            Health = Mathf.Clamp(health, 0f, MaxHealth);
            SaveHealthState();
        }

        public static void PrepareCombatRetry(float maxHealth)
        {
            EnsureLoaded();

            MaxHealth = Mathf.Max(BaseMaxHealth, maxHealth);
            Health = MaxHealth;
            SaveHealthState();
        }

        public static void AddPotion(int amount = 1)
        {
            if (amount <= 0) return;
            EnsureLoaded();

            PotionCount += amount;
            SaveInventory();
        }

        public static int RefillStarterPotions()
        {
            EnsureLoaded();

            int previous = PotionCount;
            PotionCount = Mathf.Max(
                PotionCount,
                StarterPotionCount);
            SaveInventory();
            return PotionCount - previous;
        }

        public static bool TryConsumePotionAndSetHealth(
            float health,
            float maxHealth)
        {
            EnsureLoaded();
            if (PotionCount <= 0)
                return false;

            PotionCount -= 1;
            MaxHealth = Mathf.Max(BaseMaxHealth, maxHealth);
            Health = Mathf.Clamp(health, 0f, MaxHealth);

            SaveInventoryAndHealth();
            return true;
        }

        public static void Rest()
        {
            EnsureLoaded();
            SetHealth(MaxHealth, MaxHealth);
        }

        private static void SaveHealthState()
        {
            PlayerPrefs.SetInt(
                Prefix + "HealthState.Version",
                HealthStateVersion);
            PlayerPrefs.SetFloat(
                Prefix + "Health",
                Health);
            PlayerPrefs.SetFloat(
                Prefix + "MaxHealth",
                MaxHealth);
            PlayerPrefs.Save();
        }

        private static void SaveInventory()
        {
            PlayerPrefs.SetInt(
                Prefix + "InventoryState.Version",
                InventoryStateVersion);
            PlayerPrefs.SetInt(
                Prefix + "PotionCount",
                Mathf.Max(0, PotionCount));
            PlayerPrefs.Save();
        }

        private static void SaveInventoryAndHealth()
        {
            PlayerPrefs.SetInt(
                Prefix + "InventoryState.Version",
                InventoryStateVersion);
            PlayerPrefs.SetInt(
                Prefix + "PotionCount",
                Mathf.Max(0, PotionCount));
            PlayerPrefs.SetInt(
                Prefix + "HealthState.Version",
                HealthStateVersion);
            PlayerPrefs.SetFloat(
                Prefix + "Health",
                Health);
            PlayerPrefs.SetFloat(
                Prefix + "MaxHealth",
                MaxHealth);
            PlayerPrefs.Save();
        }
    }
}
