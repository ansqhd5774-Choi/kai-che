using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Survivor
{
    internal static class TopDownRoguelikeDeviceProbeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryInstall(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstall(scene);
        }

        private static void TryInstall(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || scene.name != "Game")
                return;

            if (Object.FindFirstObjectByType<TopDownRoguelikeDeviceProbe>() != null)
                return;

            GameObject host = new GameObject("KaiChe.TopDownRoguelikeDeviceProbe");
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<TopDownRoguelikeDeviceProbe>();
        }
    }

    public sealed class TopDownRoguelikeDeviceProbe : MonoBehaviour
    {
        private TopDownRoguelikeRuntime _runtime;
        private Transform _player;
        private Vector3 _initialPlayerPosition;
        private bool _initialPositionCaptured;
        private bool _movementLogged;
        private bool _killLogged;
        private bool _xpLogged;
        private bool _levelLogged;
        private float _nextSearchTime;

        private void Update()
        {
            if (_runtime == null)
                _runtime = Object.FindFirstObjectByType<TopDownRoguelikeRuntime>();

            if (_player == null && Time.unscaledTime >= _nextSearchTime)
            {
                _nextSearchTime = Time.unscaledTime + 0.25f;
                GameObject playerObject = GameObject.Find("TopDownPlayer");
                if (playerObject != null)
                    _player = playerObject.transform;
            }

            if (_player != null)
                ObserveMovement();

            if (_runtime == null)
                return;

            if (!_killLogged && _runtime.Kills > 0)
            {
                _killLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_DEVICE_KILL_CONFIRMED kills=" + _runtime.Kills);
            }

            if (!_xpLogged && _runtime.Experience > 0)
            {
                _xpLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_DEVICE_XP_CONFIRMED xp=" + _runtime.Experience);
            }

            if (!_levelLogged && _runtime.Level > 1)
            {
                _levelLogged = true;
                Debug.Log("KAI_CHE_TOPDOWN_DEVICE_LEVELUP_CONFIRMED level=" + _runtime.Level);
            }
        }

        private void ObserveMovement()
        {
            Vector3 current = _player.position;
            if (!_initialPositionCaptured)
            {
                _initialPlayerPosition = current;
                _initialPositionCaptured = true;
                Debug.Log("KAI_CHE_TOPDOWN_DEVICE_PLAYER_POSITION_READY x=" + current.x.ToString("F2") + " z=" + current.z.ToString("F2"));
                return;
            }

            if (_movementLogged)
                return;

            Vector3 delta = current - _initialPlayerPosition;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.04f)
                return;

            _movementLogged = true;
            Debug.Log("KAI_CHE_TOPDOWN_DEVICE_MOVEMENT_CONFIRMED distance=" + delta.magnitude.ToString("F2"));
        }
    }
}
