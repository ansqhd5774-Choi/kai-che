using System.Collections;
using KaiChe.Gameplay.Survivor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Presentation
{
    public sealed class KaiCheRuntimeFlow : MonoBehaviour
    {
        private static bool _installed;
        private bool _loading;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (_installed) return;
            _installed = true;

            var root = new GameObject("KaiCheRuntimeFlow");
            DontDestroyOnLoad(root);
            root.AddComponent<KaiCheRuntimeFlow>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _loading = false;

            if (scene.name == "Boot")
            {
                StartCoroutine(AdvanceFromBoot());
                return;
            }

            if (scene.name == "Game")
                SetupSurvivorWorld();
        }

        private IEnumerator AdvanceFromBoot()
        {
            if (_loading) yield break;
            _loading = true;
            yield return null;
            SceneManager.LoadScene("Main");
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (_titleStyle == null) return;

            string scene = SceneManager.GetActiveScene().name;
            if (scene == "Main")
                DrawMain();
        }
        private void DrawMain()
        {
            float width = Mathf.Min(Screen.width * 0.84f, 960f);
            float height = Mathf.Min(Screen.height * 0.52f, 760f);
            var panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x + 32f, panel.y + 44f, panel.width - 64f, 90f), "KaiChe", _titleStyle);
            GUI.Label(new Rect(panel.x + 32f, panel.y + 150f, panel.width - 64f, 120f),
                "판타지 전투 프로토타입\n공용 RPG 데이터 / 코어", _bodyStyle);

            float buttonWidth = Mathf.Min(panel.width - 64f, 560f);
            var button = new Rect(panel.center.x - buttonWidth * 0.5f, panel.yMax - 170f, buttonWidth, 96f);

            if (GUI.Button(button, "게임 시작", _buttonStyle))
                SceneManager.LoadScene("Game");

            GUI.Label(new Rect(panel.x + 32f, panel.yMax - 58f, panel.width - 64f, 34f),
                "전투 프로토타입 → 향후 3D 판타지 RPG", _smallStyle);
        }

        private static void SetupSurvivorWorld()
        {
            if (GameObject.Find("KaiCheSurvivorPrototype") != null) return;

            Material template = Resources.Load<Material>("KaiCheRuntimeLit");
            if (template == null)
            {
                Debug.LogError("KAI_CHE_RUNTIME_MATERIAL_NOT_FOUND");
                return;
            }

            var root = new GameObject("KaiCheSurvivorPrototype");
            var arena = root.AddComponent<SurvivorArenaController>();
            arena.Initialize(template);

            var hud = root.AddComponent<SurvivorHudController>();
            hud.Bind(arena);
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            GUISkin skin = GUI.skin;
            if (skin == null || skin.label == null || skin.button == null)
                return;

            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 26f, 28f, 54f));
            _titleStyle = new GUIStyle(skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize + 18,
                fontStyle = FontStyle.Bold
            };
            _bodyStyle = new GUIStyle(skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _smallStyle = new GUIStyle(skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(22, baseSize - 8)
            };
            _buttonStyle = new GUIStyle(skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(26, baseSize - 2),
                fontStyle = FontStyle.Bold
            };
        }
    }
}