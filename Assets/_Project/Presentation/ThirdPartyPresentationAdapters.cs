using Cysharp.Text;
using UnityScreenNavigator.Runtime.Core.Page;

namespace KaiChe.Presentation
{
    /// <summary>
    /// Thin navigation facade. Existing combat HUD remains authoritative; new menu/modal
    /// flows can opt into UnityScreenNavigator without coupling game logic to it.
    /// </summary>
    internal static class KaiCheScreenNavigator
    {
        internal static bool TryPushPage(
            string containerName,
            string resourceKey,
            bool playAnimation = true,
            bool stack = true)
        {
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(resourceKey))
                return false;

            PageContainer container = PageContainer.Find(containerName);
            if (container == null)
                return false;

            container.Push(resourceKey, playAnimation, stack);
            return true;
        }

        internal static bool TryPopPage(
            string containerName,
            bool playAnimation = true,
            int popCount = 1)
        {
            if (string.IsNullOrEmpty(containerName) || popCount <= 0)
                return false;

            PageContainer container = PageContainer.Find(containerName);
            if (container == null)
                return false;

            container.Pop(playAnimation, popCount);
            return true;
        }
    }

    /// <summary>
    /// Central low-allocation formatter for hot HUD strings.
    /// </summary>
    internal static class KaiCheHudText
    {
        internal static string Level(int level)
        {
            return ZString.Format("LV {0}", level);
        }

        internal static string Wave(int wave)
        {
            return ZString.Format("WAVE {0}", wave);
        }

        internal static string Experience(long current, long required)
        {
            return ZString.Format("XP {0}/{1}", current, required);
        }

        internal static string Health(float current, float max)
        {
            return ZString.Format("HP {0:0}/{1:0}", current, max);
        }

        internal static string Kills(int kills)
        {
            return ZString.Format("KILL {0}", kills);
        }
    }
}
