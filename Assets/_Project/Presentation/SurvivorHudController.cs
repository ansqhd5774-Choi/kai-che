using KaiChe.Core.Progression;
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
        private Vector2 _joystickOrigin;
        private bool _joystickVisible;
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

            if (_arena.IsAttributeAllocationPending)
            {
                ResetJoystick();
                _arena.SetMoveInput(Vector2.zero);
                return;
            }

            Vector2 input = ReadTouchInput();
            if (_activeFingerId < 0 && Input.touchCount == 0)
                input = ReadFallbackInput();

            _joystickInput = input;
            _arena.SetMoveInput(input);
        }

        private Vector2 ReadTouchInput()
        {
            float radius = JoystickRadius;

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
                    return Vector2.zero;
                }

                Vector2 delta = touch.position - _joystickOrigin;
                return Vector2.ClampMagnitude(delta / radius, 1f);
            }

            if (_activeFingerId >= 0)
            {
                _activeFingerId = -1;
                _joystickVisible = false;
            }

            return Vector2.zero;
        }

        private void ResetJoystick()
        {
            _activeFingerId = -1;
            _joystickInput = Vector2.zero;
            _joystickVisible = false;
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
            if (_hudStyle == null) return;

            float margin = Mathf.Max(24f, Screen.width * 0.025f);
            GUI.Box(
                new Rect(margin, margin, Screen.width - margin * 2f, 160f),
                GUIContent.none);

            string status =
                $"LV {_arena.Level}   HP {Mathf.CeilToInt(_arena.PlayerHealth)}/100   "
                + $"KILLS {_arena.Kills}   "
                + $"XP {_arena.LevelExperience}/{_arena.ExperienceRequiredForNextLevel}   "
                + $"MOBS {_arena.EnemyCount}";

            GUI.Label(
                new Rect(margin + 24f, margin + 18f, Screen.width - margin * 2f - 48f, 58f),
                status,
                _hudStyle);

            Vector2 worldPosition = _arena.PlayerWorldPosition;
            GUI.Label(
                new Rect(margin + 24f, margin + 82f, Screen.width - margin * 2f - 48f, 48f),
                $"{_arena.ActiveSkillId} MASTERY {_arena.ActiveSkillMastery}   "
                + $"UNLOCKED {_arena.UnlockedSkillCount}   "
                + $"POS {worldPosition.x:0.0},{worldPosition.y:0.0}",
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
            else if (_arena.IsAttributeAllocationPending)
                DrawAttributeAllocationOverlay();
        }

        private void DrawJoystick()
        {
            if (!_joystickVisible) return;

            float radius = JoystickRadius;
            Vector2 center = _joystickOrigin;

            Rect baseRect = ScreenRect(center, radius);
            GUI.DrawTexture(baseRect, _joystickBase, ScaleMode.StretchToFill, true);

            Vector2 knobCenter = center + _joystickInput * radius * 0.62f;
            Rect knobRect = ScreenRect(knobCenter, radius * 0.42f);
            GUI.DrawTexture(knobRect, _joystickKnob, ScaleMode.StretchToFill, true);
        }

        private void DrawAttributeAllocationOverlay()
        {
            float width = Mathf.Min(Screen.width * 0.90f, 1120f);
            float height = Mathf.Min(Screen.height * 0.72f, 1480f);
            Rect panel = new Rect(
                (Screen.width - width) * 0.5f,
                (Screen.height - height) * 0.5f,
                width,
                height);

            GUI.Box(panel, GUIContent.none);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 44f, panel.width - 60f, 92f),
                $"LEVEL UP  LV {_arena.Level}",
                _deathStyle);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 142f, panel.width - 60f, 70f),
                $"ATTRIBUTE POINTS {_arena.UnspentAttributePoints}  PROTOTYPE",
                _hudStyle);

            CharacterAttribute[] attributes =
            {
                CharacterAttribute.Body,
                CharacterAttribute.Agility,
                CharacterAttribute.Perception,
                CharacterAttribute.Intelligence,
                CharacterAttribute.Wisdom,
                CharacterAttribute.Will,
                CharacterAttribute.Good,
                CharacterAttribute.Evil
            };

            float gap = 24f;
            float gridTop = panel.y + 244f;
            float buttonWidth = (panel.width - 60f - gap) * 0.5f;
            float buttonHeight = Mathf.Min(
                190f,
                (panel.height - 330f - gap * 3f) * 0.25f);

            for (int i = 0; i < attributes.Length; i++)
            {
                int row = i / 2;
                int column = i % 2;
                Rect buttonRect = new Rect(
                    panel.x + 30f + column * (buttonWidth + gap),
                    gridTop + row * (buttonHeight + gap),
                    buttonWidth,
                    buttonHeight);

                DrawAttributeButton(buttonRect, attributes[i]);
            }
        }

        private void DrawAttributeButton(
            Rect rect,
            CharacterAttribute attribute)
        {
            int value = _arena.GetAttributeValue(attribute);
            string label = $"{GetAttributeLabel(attribute)}  {value}\n+1";

            if (GUI.Button(rect, label, _buttonStyle))
                _arena.TrySpendAttributePoint(attribute);
        }

        private static string GetAttributeLabel(CharacterAttribute attribute)
        {
            switch (attribute)
            {
                case CharacterAttribute.Body: return "BODY";
                case CharacterAttribute.Agility: return "AGILITY";
                case CharacterAttribute.Perception: return "PERCEPTION";
                case CharacterAttribute.Intelligence: return "INTELLIGENCE";
                case CharacterAttribute.Wisdom: return "WISDOM";
                case CharacterAttribute.Will: return "WILL";
                case CharacterAttribute.Good: return "GOOD";
                case CharacterAttribute.Evil: return "EVIL";
                default: return attribute.ToString().ToUpperInvariant();
            }
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

            GUISkin skin = GUI.skin;
            if (skin == null || skin.label == null || skin.button == null)
                return;

            int baseSize = Mathf.RoundToInt(
                Mathf.Clamp(Screen.width / 28f, 30f, 54f));

            _hudStyle = new GUIStyle(skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            _smallStyle = new GUIStyle(skin.label)
            {                fontSize = Mathf.Max(22, baseSize - 10),
                alignment = TextAnchor.MiddleLeft
            };

            _buttonStyle = new GUIStyle(skin.button)
            {
                fontSize = Mathf.Max(26, baseSize - 4),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _deathStyle = new GUIStyle(skin.label)
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