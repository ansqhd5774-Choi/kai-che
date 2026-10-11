using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Survivor
{
    [DefaultExecutionOrder(10000)]
    internal sealed class RuntimeTopDownShooterCamera : MonoBehaviour
    {
        private const string GameplaySceneName = "Game";
        private const float CameraHeight = 18f;
        private const float BaseOrthographicSize = 7.25f;
        private const float MinimumHorizontalHalfExtent = 4.5f;

        private SurvivorArenaController _arena;
        private bool _loggedActive;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneHook()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForInitialScene()
        {
            InstallForScene(SceneManager.GetActiveScene());
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InstallForScene(scene);
        }

        private static void InstallForScene(Scene scene)
        {
            if (!scene.IsValid() || scene.name != GameplaySceneName)
                return;

            GameObject root = GameObject.Find("GameRoot");
            if (root == null)
            {
                Debug.LogWarning("KAI_CHE_TOPDOWN_CAMERA_GAME_ROOT_MISSING");
                return;
            }

            if (root.GetComponent<RuntimeTopDownShooterCamera>() == null)
                root.AddComponent<RuntimeTopDownShooterCamera>();
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            if (_arena == null)
                _arena = GetComponent<SurvivorArenaController>();

            Vector2 focus = _arena != null
                ? _arena.PlayerWorldPosition
                : Vector2.zero;

            float aspect = Mathf.Max(0.1f, camera.aspect);
            float orthographicSize = Mathf.Max(
                BaseOrthographicSize,
                MinimumHorizontalHalfExtent / aspect);

            camera.orthographic = true;
            camera.orthographicSize = orthographicSize;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.transform.position = new Vector3(
                focus.x,
                CameraHeight,
                focus.y);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (!_loggedActive)
            {
                _loggedActive = true;
                Debug.Log(
                    "KAI_CHE_TOPDOWN_SHOOTER_CAMERA_ACTIVE"
                    + " size=" + orthographicSize.ToString("0.00")
                    + " aspect=" + aspect.ToString("0.00"));
            }
        }
    }
}
