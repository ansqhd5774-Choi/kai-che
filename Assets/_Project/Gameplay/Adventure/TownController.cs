using KaiChe.Gameplay;
using KaiChe.Gameplay.Survivor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Gameplay.Adventure
{
    public sealed class TownController : MonoBehaviour
    {
        private static readonly Vector2 InnPosition = new Vector2(3.2f, 1.8f);
        private static readonly Vector2 ApothecaryPosition = new Vector2(-3.2f, 1.8f);
        private static readonly Vector2 WorkshopPosition = new Vector2(-3.2f, -2.2f);
        private static readonly Vector2 HallPosition = new Vector2(3.2f, -2.2f);
        private static readonly Vector2 FieldGatePosition = new Vector2(0f, 6.3f);
        private const float PerspectiveCameraHeight = 11.5f;
        private const float PerspectiveCameraBackDistance = 12f;
        private const float PerspectiveCameraSideOffset = -2.4f;
        private const float PerspectiveCameraFieldOfView = 58f;

        private Material _materialTemplate;
        private GameObject _player;
        private RuntimeNoviceBodyAnimator _playerNoviceBodyAnimator;
        private Vector2 _playerPosition = new Vector2(0f, -4.6f);
        private Vector2 _moveInput;
        private int _activeFingerId = -1;
        private Vector2 _joystickOrigin;
        private bool _joystickVisible;
        private string _message = "마을에 도착했습니다.";
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;

        public float Health => PrototypeAdventureState.Health;
        public float MaxHealth => PrototypeAdventureState.MaxHealth;
        public int PotionCount => PrototypeAdventureState.PotionCount;
        public void Initialize(Material materialTemplate)
        {
            _materialTemplate = materialTemplate;
            PrototypeAdventureState.EnsureLoaded();
            ConfigureCamera();
            RuntimeTownEnvironment.Build(
                transform,
                _materialTemplate,
                InnPosition,
                ApothecaryPosition,
                WorkshopPosition,
                HallPosition,
                FieldGatePosition);
            _player = CreatePlayerView();
            _playerNoviceBodyAnimator =
                _player != null
                    ? _player.GetComponent<RuntimeNoviceBodyAnimator>()
                    : null;
            _player.transform.position =
                RuntimeWorldSpace.ToWorld(
                    _playerPosition,
                    RuntimeWorldSpace.PlayerRootHeight);
        }

        private static void ConfigureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            camera.orthographic = false;
            camera.fieldOfView = PerspectiveCameraFieldOfView;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;

            Vector3 target = RuntimeWorldSpace.ToWorld(
                Vector2.zero,
                RuntimeWorldSpace.CameraTargetHeight);
            camera.transform.position = new Vector3(
                PerspectiveCameraSideOffset,
                PerspectiveCameraHeight,
                -PerspectiveCameraBackDistance);
            camera.transform.LookAt(target, Vector3.up);
            camera.backgroundColor = new Color(0.055f, 0.070f, 0.050f);
            Debug.Log("KAI_CHE_R19_TOWN_PERSPECTIVE_CAMERA_ACTIVE");
            Debug.Log("KAI_CHE_R20_TOWN_XZ_WORLD_ACTIVE");
        }

        private void Update()
        {
            ReadMovementInput();
            UpdateMovement(Time.deltaTime);
            UpdateFieldGate();
        }
        private void ReadMovementInput()
        {
            Vector2 input = Vector2.zero;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (_activeFingerId < 0 && touch.phase == TouchPhase.Began)
                {
                    _activeFingerId = touch.fingerId;
                    _joystickOrigin = touch.position;
                    _joystickVisible = true;
                }

                if (touch.fingerId != _activeFingerId) continue;

                if (touch.phase == TouchPhase.Ended
                    || touch.phase == TouchPhase.Canceled)
                {
                    _activeFingerId = -1;
                    _joystickVisible = false;
                    break;
                }

                input = Vector2.ClampMagnitude(
                    (touch.position - _joystickOrigin) / JoystickRadius,
                    1f);
            }
            if (_activeFingerId >= 0 && Input.touchCount == 0)
            {
                _activeFingerId = -1;
                _joystickVisible = false;
            }

            if (Input.touchCount == 0)
            {
                float x = 0f;
                float y = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
                input = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            }

            _moveInput = input;
        }

        private void UpdateMovement(float deltaTime)
        {
            _playerPosition += _moveInput * 4.5f * deltaTime;
            _playerPosition.x = Mathf.Clamp(_playerPosition.x, -5.6f, 5.6f);
            _playerPosition.y = Mathf.Clamp(_playerPosition.y, -6.0f, 6.6f);

            if (_player != null)
            {
                _player.transform.position =
                    RuntimeWorldSpace.ToWorld(
                        _playerPosition,
                        RuntimeWorldSpace.PlayerRootHeight);
                _playerNoviceBodyAnimator?.SetMovement(
                    _moveInput,
                    deltaTime);
            }
        }
        private void UpdateFieldGate()
        {
            if ((_playerPosition - FieldGatePosition).sqrMagnitude > 0.55f)
                return;

            SceneManager.LoadScene("Game");
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (_titleStyle == null) return;

            GUI.Box(new Rect(20f, 20f, Mathf.Min(Screen.width - 40f, 820f), 150f), GUIContent.none);
            GUI.Label(new Rect(44f, 28f, 400f, 54f), "마을", _titleStyle);
            GUI.Label(
                new Rect(44f, 82f, 700f, 38f),
                $"체력 {Mathf.CeilToInt(Health)} / {Mathf.CeilToInt(MaxHealth)}   ·   물약 {PotionCount}",
                _bodyStyle);
            GUI.Label(new Rect(44f, 120f, 720f, 36f), _message, _bodyStyle);

            DrawWorldLabels();
            DrawJoystick();
            DrawContextAction();
        }

        private void DrawWorldLabels()
        {
            DrawWorldLabel(InnPosition + Vector2.up * 1.35f, "여관");
            DrawWorldLabel(ApothecaryPosition + Vector2.up * 1.35f, "약제상");
            DrawWorldLabel(WorkshopPosition + Vector2.up * 1.35f, "작업장");
            DrawWorldLabel(HallPosition + Vector2.up * 1.35f, "회관");
            DrawWorldLabel(FieldGatePosition + Vector2.down * 0.7f, "사냥터 출구");
        }

        private void DrawWorldLabel(Vector2 world, string label)
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            Vector3 screen = camera.WorldToScreenPoint(
                RuntimeWorldSpace.ToWorld(
                    world,
                    RuntimeWorldSpace.LabelHeight));
            var rect = new Rect(
                screen.x - 90f,
                Screen.height - screen.y - 24f,
                180f,
                48f);
            GUI.Label(rect, label, _bodyStyle);
        }

        private void DrawContextAction()
        {
            float nearInn = (_playerPosition - InnPosition).sqrMagnitude;
            float nearApothecary = (_playerPosition - ApothecaryPosition).sqrMagnitude;
            Rect action = new Rect(Screen.width - 330f, Screen.height - 160f, 290f, 100f);
            if (nearInn <= 1.45f)
            {
                if (GUI.Button(action, "여관에서 휴식", _buttonStyle))
                {
                    PrototypeAdventureState.Rest();
                    _message = "휴식 완료 · 체력 회복";
                }
                return;
            }

            if (nearApothecary <= 1.45f
                && GUI.Button(action, "물약 보급", _buttonStyle))
            {
                int added = PrototypeAdventureState.RefillStarterPotions();
                _message = added > 0
                    ? "물약 보급 +" + added + " · 현재 " + PotionCount + "개"
                    : "물약은 이미 " + PotionCount + "개 보유 중입니다.";
                Debug.Log(
                    "KAI_CHE_R29_TOWN_POTION_REFILL_ADDED_"
                    + added
                    + "_COUNT_"
                    + PotionCount);
            }
        }

        private void DrawJoystick()
        {
            if (!_joystickVisible) return;

            float radius = JoystickRadius;
            Vector2 center = _joystickOrigin;
            Rect outer = ScreenRect(center, radius);
            Rect inner = ScreenRect(center + _moveInput * radius * 0.62f, radius * 0.40f);
            GUI.Box(outer, GUIContent.none);
            GUI.Box(inner, GUIContent.none);
        }
        private void CreateTownLayout()
        {
            GameObject floor = CreateQuad(
                "TownGround",
                new Color(0.19f, 0.20f, 0.14f),
                new Vector3(0f, 0f, 2.5f),
                1f);
            floor.transform.localScale = new Vector3(14f, 17f, 1f);

            GameObject plaza = CreateQuad(
                "TownPlaza",
                new Color(0.34f, 0.31f, 0.23f),
                new Vector3(0f, 0f, 2.2f),
                1f);
            plaza.transform.localScale = new Vector3(6.5f, 5.2f, 1f);

            CreateBuilding("Inn", InnPosition, new Color(0.48f, 0.25f, 0.12f));
            CreateBuilding("Apothecary", ApothecaryPosition, new Color(0.20f, 0.38f, 0.22f));
            CreateBuilding("Workshop", new Vector2(-3.2f, -2.2f), new Color(0.30f, 0.30f, 0.32f));
            CreateBuilding("Hall", new Vector2(3.2f, -2.2f), new Color(0.36f, 0.28f, 0.16f));
            CreateNpc("Innkeeper", InnPosition + new Vector2(0f, -1.25f), new Color(0.70f, 0.52f, 0.28f));
            CreateNpc("ApothecaryNpc", ApothecaryPosition + new Vector2(0f, -1.25f), new Color(0.34f, 0.62f, 0.36f));
            CreateGate();
        }

        private void CreateNpc(string name, Vector2 position, Color color)
        {
            GameObject body = CreateQuad(
                name,
                color,
                new Vector3(position.x, position.y, 1.7f),
                1f);
            body.transform.localScale = new Vector3(0.36f, 0.58f, 1f);

            GameObject head = CreateQuad(
                name + "Head",
                new Color(0.74f, 0.62f, 0.48f),
                new Vector3(position.x, position.y + 0.42f, 1.65f),
                1f);
            head.transform.localScale = new Vector3(0.30f, 0.30f, 1f);
        }

        private void CreateBuilding(string name, Vector2 position, Color color)
        {
            GameObject body = CreateQuad(name, color, new Vector3(position.x, position.y, 1.9f), 1f);
            body.transform.localScale = new Vector3(2.2f, 1.6f, 1f);

            GameObject roof = CreateQuad(
                name + "Roof",
                new Color(color.r * 0.65f, color.g * 0.65f, color.b * 0.65f),
                new Vector3(position.x, position.y + 0.92f, 1.8f),
                1f);
            roof.transform.localScale = new Vector3(2.6f, 0.45f, 1f);
        }
        private void CreateGate()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject post = CreateQuad(
                    side < 0 ? "FieldGateLeft" : "FieldGateRight",
                    new Color(0.42f, 0.31f, 0.16f),
                    new Vector3(side * 1.0f, FieldGatePosition.y, 1.8f),
                    1f);
                post.transform.localScale = new Vector3(0.28f, 2.0f, 1f);
            }

            GameObject road = CreateQuad(
                "FieldRoad",
                new Color(0.35f, 0.30f, 0.21f),
                new Vector3(0f, 4.4f, 2.1f),
                1f);
            road.transform.localScale = new Vector3(2.0f, 4.4f, 1f);
        }

        private GameObject CreatePlayerView()
        {
            GameObject runtime3D = RuntimeCombatVisualFactory.CreateNoviceHuman(
                transform,
                _materialTemplate,
                0.66f,
                out _);
            if (runtime3D != null)
            {
                runtime3D.name = "TownPlayer";
                return runtime3D;
            }

            var root = new GameObject("TownPlayer");
            root.transform.SetParent(transform, false);
            root.transform.localScale = Vector3.one * 0.66f;

            GameObject body = CreateQuad("PlayerBody", new Color(0.22f, 0.24f, 0.25f), Vector3.zero, 0.9f);
            body.transform.SetParent(root.transform, false);
            GameObject chest = CreateQuad("PlayerChest", new Color(0.60f, 0.44f, 0.18f), new Vector3(0f, 0.02f, -0.03f), 0.54f);
            chest.transform.SetParent(root.transform, false);
            GameObject helmet = CreateQuad("PlayerHelmet", new Color(0.35f, 0.37f, 0.36f), new Vector3(0f, 0.50f, -0.04f), 0.42f);
            helmet.transform.SetParent(root.transform, false);
            return root;
        }

        private GameObject CreateQuad(string name, Color color, Vector3 position, float scale)
        {
            var view = GameObject.CreatePrimitive(PrimitiveType.Quad);
            view.name = name;
            view.transform.SetParent(transform, false);
            view.transform.position = position;
            view.transform.localScale = Vector3.one * scale;

            Collider collider = view.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            Renderer renderer = view.GetComponent<Renderer>();
            if (renderer != null && _materialTemplate != null)
            {
                var material = new Material(_materialTemplate);
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                else material.color = color;
                renderer.sharedMaterial = material;
            }
            return view;
        }
        private void EnsureStyles()
        {
            if (_titleStyle != null) return;
            GUISkin skin = GUI.skin;
            if (skin == null) return;

            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 30f, 26f, 48f));
            _titleStyle = new GUIStyle(skin.label)
            {
                fontSize = baseSize + 8,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            _bodyStyle = new GUIStyle(skin.label)
            {
                fontSize = Mathf.Max(22, baseSize - 4),
                alignment = TextAnchor.MiddleLeft
            };
            _buttonStyle = new GUIStyle(skin.button)
            {
                fontSize = Mathf.Max(24, baseSize - 2),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private float JoystickRadius =>
            Mathf.Clamp(Screen.width * 0.125f, 132f, 210f);
        private static Rect ScreenRect(Vector2 screenCenter, float radius)
        {
            return new Rect(
                screenCenter.x - radius,
                Screen.height - screenCenter.y - radius,
                radius * 2f,
                radius * 2f);
        }
    }
}
