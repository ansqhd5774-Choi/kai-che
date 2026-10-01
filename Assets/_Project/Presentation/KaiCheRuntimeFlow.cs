using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Presentation
{
    public sealed class KaiCheRuntimeFlow : MonoBehaviour
    {
        private static bool _installed;
        private bool _loading;
        private GameObject _demoCube;
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
                SetupDemoWorld();
        }

        private IEnumerator AdvanceFromBoot()
        {
            if (_loading) yield break;
            _loading = true;
            yield return null;
            SceneManager.LoadScene("Main");
        }

        private void Update()
        {
            if (_demoCube != null)
                _demoCube.transform.Rotate(0f, 35f * Time.deltaTime, 0f, Space.World);
        }

        private void OnGUI()
        {
            EnsureStyles();

            string scene = SceneManager.GetActiveScene().name;
            if (scene == "Main")
                DrawMain();
            else if (scene == "Game")
                DrawGameOverlay();
        }
        private void DrawMain()
        {
            float width = Mathf.Min(Screen.width * 0.84f, 960f);
            float height = Mathf.Min(Screen.height * 0.52f, 760f);
            var panel = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);

            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x + 32f, panel.y + 44f, panel.width - 64f, 90f), "KaiChe", _titleStyle);
            GUI.Label(new Rect(panel.x + 32f, panel.y + 150f, panel.width - 64f, 120f),
                "Android Prototype 0.1.0\nBoot -> Main transition OK", _bodyStyle);

            float buttonWidth = Mathf.Min(panel.width - 64f, 560f);
            var button = new Rect(panel.center.x - buttonWidth * 0.5f, panel.yMax - 170f, buttonWidth, 96f);

            if (GUI.Button(button, "ENTER 3D TEST", _buttonStyle))
                SceneManager.LoadScene("Game");

            GUI.Label(new Rect(panel.x + 32f, panel.yMax - 58f, panel.width - 64f, 34f),
                "KaiChe mobile RPG / first device test", _smallStyle);
        }

        private void DrawGameOverlay()
        {
            GUI.Box(new Rect(24f, 24f, Screen.width - 48f, 150f), GUIContent.none);
            GUI.Label(new Rect(52f, 42f, Screen.width - 104f, 52f), "3D RENDER TEST", _bodyStyle);
            GUI.Label(new Rect(52f, 96f, Screen.width - 104f, 42f),
                "Rotating cube + sphere + capsule", _smallStyle);

            if (GUI.Button(new Rect(52f, Screen.height - 132f, 320f, 82f), "BACK", _buttonStyle))
                SceneManager.LoadScene("Main");
        }
        private void SetupDemoWorld()
        {
            if (GameObject.Find("KaiCheDemoCube") != null) return;

            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0f, 2.4f, -9f);
                camera.transform.LookAt(new Vector3(0f, 0.1f, 4.5f));
                camera.backgroundColor = new Color(0.025f, 0.035f, 0.065f);
            }

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "KaiCheDemoGround";
            ground.transform.position = new Vector3(0f, -1f, 5f);
            ground.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            SetColor(ground, new Color(0.12f, 0.15f, 0.2f));

            _demoCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _demoCube.name = "KaiCheDemoCube";
            _demoCube.transform.position = new Vector3(0f, 0.2f, 4f);
            _demoCube.transform.localScale = Vector3.one * 1.8f;
            SetColor(_demoCube, new Color(0.22f, 0.65f, 1f));

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "KaiCheDemoSphere";
            sphere.transform.position = new Vector3(-2.6f, -0.1f, 5f);
            sphere.transform.localScale = Vector3.one * 1.5f;
            SetColor(sphere, new Color(0.35f, 0.85f, 0.45f));

            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "KaiCheDemoCapsule";
            capsule.transform.position = new Vector3(2.6f, 0f, 5f);
            SetColor(capsule, new Color(1f, 0.55f, 0.22f));
        }
        private static void SetColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;

            Material template = Resources.Load<Material>("KaiCheRuntimeLit");
            if (template == null)
            {
                Debug.LogError("KAI_CHE_RUNTIME_MATERIAL_NOT_FOUND");
                return;
            }

            var material = new Material(template);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else
                material.color = color;

            renderer.sharedMaterial = material;
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 26f, 28f, 54f));
            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize + 18,
                fontStyle = FontStyle.Bold
            };
            _bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(22, baseSize - 8)
            };
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(26, baseSize - 2),
                fontStyle = FontStyle.Bold
            };
        }
    }
}