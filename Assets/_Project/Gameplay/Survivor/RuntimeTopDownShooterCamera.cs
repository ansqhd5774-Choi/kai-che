using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Survivor
{
    [DefaultExecutionOrder(10000)]
    internal sealed class RuntimeTopDownShooterCamera : MonoBehaviour
    {
        private const string GameplaySceneName = "Game";
        private const string RuntimeArenaObjectName = "KaiCheSurvivorPrototype";
        private const float CameraHeight = 18f;
        private const float BaseOrthographicSize = 7.25f;
        private const float MinimumHorizontalHalfExtent = 4.5f;
        private const float PlayerHitShakeMagnitude = 0.12f;
        private const float BossDefeatShakeMagnitude = 0.28f;
        private const float ImpactDuration = 0.22f;

        private SurvivorArenaController _arena;
        private MotionHandle _impactMotion;
        private Vector3 _impactOffset;
        private bool _loggedActive;
        private bool _wasPlayerHitFlashActive;
        private bool _hadBossAlive;
        private int _impactSequence;

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

            ResolveArena();
            UpdateImpactFeedback();

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
                focus.x + _impactOffset.x,
                CameraHeight,
                focus.y + _impactOffset.z);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            if (!_loggedActive && _arena != null)
            {
                _loggedActive = true;
                Debug.Log(
                    "KAI_CHE_TOPDOWN_SHOOTER_CAMERA_ACTIVE"
                    + " size=" + orthographicSize.ToString("0.00")
                    + " aspect=" + aspect.ToString("0.00"));
            }
        }

        private void OnDestroy()
        {
            if (_impactMotion.IsActive())
                _impactMotion.Cancel();
        }

        private void ResolveArena()
        {
            if (_arena != null)
                return;

            GameObject arenaObject = GameObject.Find(RuntimeArenaObjectName);
            if (arenaObject != null)
                _arena = arenaObject.GetComponent<SurvivorArenaController>();
        }

        private void UpdateImpactFeedback()
        {
            if (_arena == null)
                return;

            bool isPlayerHit = _arena.IsPlayerHitFlashActive;
            if (isPlayerHit && !_wasPlayerHitFlashActive)
                PlayImpact(PlayerHitShakeMagnitude);
            _wasPlayerHitFlashActive = isPlayerHit;

            bool hasBossAlive = _arena.HasBossAlive;
            if (_hadBossAlive && !hasBossAlive)
                PlayImpact(BossDefeatShakeMagnitude);
            _hadBossAlive = hasBossAlive;
        }

        private void PlayImpact(float magnitude)
        {
            if (_impactMotion.IsActive())
                _impactMotion.Cancel();

            _impactOffset = Vector3.zero;
            _impactSequence = (_impactSequence + 1) & 7;

            Vector2 direction = GetImpactDirection(_impactSequence);
            Vector3 amplitude = new Vector3(
                direction.x * magnitude,
                0f,
                direction.y * magnitude);

            _impactMotion = LMotion.Punch
                .Create(Vector3.zero, amplitude, ImpactDuration)
                .WithFrequency(12)
                .WithDampingRatio(0.68f)
                .Bind(value => _impactOffset = value)
                .AddTo(gameObject);
        }

        private static Vector2 GetImpactDirection(int sequence)
        {
            switch (sequence & 7)
            {
                case 0:
                    return Vector2.right;
                case 1:
                    return new Vector2(1f, 1f).normalized;
                case 2:
                    return Vector2.up;
                case 3:
                    return new Vector2(-1f, 1f).normalized;
                case 4:
                    return Vector2.left;
                case 5:
                    return new Vector2(-1f, -1f).normalized;
                case 6:
                    return Vector2.down;
                default:
                    return new Vector2(1f, -1f).normalized;
            }
        }
    }
}
