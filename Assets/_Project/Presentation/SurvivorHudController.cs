using KaiChe.Gameplay.Survivor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Presentation
{
    public sealed class SurvivorHudController : MonoBehaviour
    {
        private SurvivorArenaController _arena;
        private int _activeFingerId = -1;
        private Vector2 _joystickInput;
        private Texture2D _joystickBase;
        private Texture2D _joystickKnob;
        private GUIStyle _hudStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _deathStyle;

        public void Bind(SurvivorArenaController arena)
        {
            _arena = arena;
        }

        private void Awake()
        {
            _joystickBase = CreateCircleTexture(
                128,
                new Color(1f, 1f, 1f, 0.17f));
            _joystickKnob = CreateCircleTexture(
                96,
                new Color(0.25f, 0.7f, 1f, 0.72f));
        }        private void Update()
        {
            if (_arena == null) return;

            Vector2 input = ReadTouchInput();
            if (_activeFingerId < 0 && Input.touchCount == 0)
                input = ReadFallbackInput();

            _joystickInput = input;
            _arena.SetMoveInput(input);
        }

        private Vector2 ReadTouchInput()
        {
            float radius = JoystickRadius;
            Vector2 center = JoystickCenter;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (_activeFingerId < 0
                    && touch.phase == TouchPhase.Began
                    && Vector2.Distance(touch.position, center) <= radius * 1.45f)
                {
                    _activeFingerId = touch.fingerId;
                }

                if (touch.fingerId != _activeFingerId) continue;

                if (touch.phase == TouchPhase.Ended
                    || touch.phase == TouchPhase.Canceled)
                {                    _activeFingerId = -1;
                    return Vector2.zero;
                }

                Vector2 delta = touch.position - center;
                return Vector2.ClampMagnitude(delta / radius, 1f);
            }

            if (_activeFingerId >= 0)
                _activeFingerId = -1;

            return Vector2.zero;
        }

        private static Vector2 ReadFallbackInput()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                horizontal -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                horizontal += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                vertical -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                vertical += 1f;

            return Vector2.ClampMagnitude(
                new Vector2(horizontal, vertical),
                1f);
        }        private void OnGUI()
        {
            if (_arena == null) return;
            EnsureStyles();

            float margin = Mathf.Max(24f, Screen.width * 0.025f);
            GUI.Box(
                new Rect(margin, margin, Screen.width - margin * 2f, 160f),
                GUIContent.none);

            string status =
                $"HP {Mathf.CeilToInt(_arena.PlayerHealth)}/100   "
                + $"KILLS {_arena.Kills}   XP {_arena.Experience}   "
                + $"MOBS {_arena.EnemyCount}";

            GUI.Label(
                new Rect(margin + 24f, margin + 18f, Screen.width - margin * 2f - 48f, 58f),
                status,
                _hudStyle);

            GUI.Label(
                new Rect(margin + 24f, margin + 82f, Screen.width - margin * 2f - 48f, 48f),
                $"{_arena.ActiveSkillId}  /  {_arena.ActiveMonsterId}",
                _smallStyle);

            DrawJoystick();

            if (GUI.Button(
                new Rect(Screen.width - 250f, Screen.height - 112f, 220f, 80f),
                "EXIT",
                _buttonStyle))
            {                SceneManager.LoadScene("Main");
            }

            if (_arena.IsDead)
                DrawDeathOverlay();
        }

        private void DrawJoystick()
        {
            float radius = JoystickRadius;
            Vector2 center = JoystickCenter;

            Rect baseRect = ScreenRect(center, radius);
            GUI.DrawTexture(baseRect, _joystickBase, ScaleMode.StretchToFill, true);

            Vector2 knobCenter = center + _joystickInput * radius * 0.62f;
            Rect knobRect = ScreenRect(knobCenter, radius * 0.42f);
            GUI.DrawTexture(knobRect, _joystickKnob, ScaleMode.StretchToFill, true);
        }

        private void DrawDeathOverlay()
        {
            float width = Mathf.Min(Screen.width * 0.82f, 900f);
            float height = 420f;
            Rect panel = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUI.Box(panel, GUIContent.none);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 50f, panel.width - 60f, 100f),
                "GAME OVER",
                _deathStyle);            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 160f, panel.width - 60f, 60f),
                $"Kills {_arena.Kills}   XP {_arena.Experience}",
                _hudStyle);

            if (GUI.Button(
                new Rect(panel.center.x - 180f, panel.yMax - 130f, 360f, 86f),
                "RESTART",
                _buttonStyle))
            {
                SceneManager.LoadScene("Game");
            }
        }

        private void EnsureStyles()
        {
            if (_hudStyle != null) return;

            int baseSize = Mathf.RoundToInt(
                Mathf.Clamp(Screen.width / 28f, 30f, 54f));

            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            _smallStyle = new GUIStyle(GUI.skin.label)
            {                fontSize = Mathf.Max(22, baseSize - 10),
                alignment = TextAnchor.MiddleLeft
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.Max(26, baseSize - 4),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _deathStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize + 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static Texture2D CreateCircleTexture(int size, Color color)
        {
            var texture = new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false);

            float center = (size - 1) * 0.5f;
            float radius = center;
            var pixels = new Color[size * size];            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    bool inside = dx * dx + dy * dy <= radius * radius;
                    pixels[y * size + x] = inside ? color : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private float JoystickRadius =>
            Mathf.Clamp(Screen.width * 0.145f, 150f, 230f);

        private Vector2 JoystickCenter =>
            new Vector2(
                Screen.width * 0.22f,
                Screen.height * 0.16f);

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