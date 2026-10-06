using KaiChe.Core.Progression;
using KaiChe.Gameplay.Survivor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.Presentation
{
    public sealed class SurvivorHudController : MonoBehaviour
    {
        private const string BuildLabel = "R30";
        private SurvivorArenaController _arena;
        private int _activeFingerId = -1;
        private Vector2 _joystickInput;
        private Vector2 _joystickOrigin;
        private bool _joystickVisible;
        private Texture2D _joystickBase;
        private Texture2D _joystickKnob;
        private Texture2D _panelOuter;
        private Texture2D _panelInner;
        private Texture2D _healthFill;
        private Texture2D _experienceFill;
        private Texture2D _barBack;
        private Texture2D _buttonBack;
        private Texture2D _buttonPressed;
        private Texture2D _modalDim;
        private Texture2D _damageFlash;
        private GUIStyle _hudStyle;
        private GUIStyle _smallStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _deathStyle;
        private GUIStyle _buildStyle;
        private bool _visualsEnabled = true;
        private bool _modalFallbackEnabled;

        public void Bind(SurvivorArenaController arena)
        {
            _arena = arena;
        }

        public void SetVisualsEnabled(bool enabled)
        {
            _visualsEnabled = enabled;
        }

        public void SetModalFallbackEnabled(bool enabled)
        {
            _modalFallbackEnabled = enabled;
        }

        private void Awake()
        {
            _joystickBase = CreateCircleTexture(
                128,
                new Color(0.82f, 0.78f, 0.68f, 0.18f));
            _joystickKnob = CreateCircleTexture(
                96,
                new Color(0.74f, 0.56f, 0.20f, 0.90f));

            _panelOuter = CreateSolidTexture(new Color(0.48f, 0.34f, 0.12f, 0.96f));
            _panelInner = CreateSolidTexture(new Color(0.055f, 0.050f, 0.043f, 0.94f));
            _healthFill = CreateSolidTexture(new Color(0.62f, 0.10f, 0.075f, 0.98f));
            _experienceFill = CreateSolidTexture(new Color(0.16f, 0.32f, 0.64f, 0.98f));
            _barBack = CreateSolidTexture(new Color(0.02f, 0.02f, 0.02f, 0.90f));
            _buttonBack = CreateSolidTexture(
                new Color(0.12f, 0.105f, 0.08f, 0.98f));
            _buttonPressed = CreateSolidTexture(
                new Color(0.30f, 0.22f, 0.10f, 0.98f));
            _modalDim = CreateSolidTexture(
                new Color(0.015f, 0.012f, 0.008f, 0.58f));
            _damageFlash = CreateSolidTexture(
                new Color(0.70f, 0.03f, 0.02f, 0.16f));
        }

        private void Update()
        {
            if (_arena == null) return;

            if (_arena.IsAttributeAllocationPending
                || _arena.IsSkillChoicePending
                || _arena.IsBossRewardPending)
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

                if (_activeFingerId < 0
                    && touch.phase == TouchPhase.Began)
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
        }

        private void OnGUI()
        {
            if (_arena == null) return;

            if (!_visualsEnabled)
            {
                bool fallbackModalOpen = _arena.IsDead
                    || _arena.IsAttributeAllocationPending
                    || _arena.IsSkillChoicePending
                    || _arena.IsBossRewardPending;

                if (fallbackModalOpen && _modalFallbackEnabled)
                {
                    EnsureStyles();
                    if (_hudStyle == null)
                        return;

                    if (_arena.IsDead)
                        DrawDeathOverlay();
                    else if (_arena.IsAttributeAllocationPending)
                        DrawAttributeAllocationOverlay();
                    else if (_arena.IsBossRewardPending)
                        DrawBossRewardOverlay();
                    else if (_arena.IsSkillChoicePending)
                        DrawSkillChoiceOverlay();

                    return;
                }

                if (!fallbackModalOpen)
                {
                    DrawJoystick();
                    DrawPotionButton();
                }
                return;
            }

            EnsureStyles();
            if (_hudStyle == null) return;

            Rect safe = SafeGuiRect;
            if (_arena.IsPlayerHitFlashActive)
                GUI.DrawTexture(
                    safe,
                    _damageFlash,
                    ScaleMode.StretchToFill,
                    true);

            float margin = Mathf.Max(18f, safe.width * 0.018f);
            float hudWidth = Mathf.Min(safe.width * 0.76f, 1080f);
            Rect hudPanel = new Rect(
                safe.x + margin,
                safe.y + margin,
                hudWidth,
                180f);
            DrawFantasyPanel(hudPanel);

            GUI.Label(
                new Rect(hudPanel.x + 24f, hudPanel.y + 10f, 360f, 44f),
                $"전사  LV {_arena.Level}",
                _hudStyle);
            GUI.Label(
                new Rect(hudPanel.xMax - 330f, hudPanel.y + 12f, 302f, 40f),
                $"WAVE {_arena.Wave}"
                + $"   {Mathf.FloorToInt(_arena.SurvivalTime / 60f):00}"
                + $":{Mathf.FloorToInt(_arena.SurvivalTime % 60f):00}"
                + (_arena.IsPressureWave ? "   공세" : string.Empty)
                + $"   ·   처치 {_arena.Kills}"
                + (_arena.IsFrenzyActive
                    ? $"   폭주 {_arena.FrenzyTimeRemaining:0.0}s"
                    : _arena.ComboCount > 1
                        ? $"   콤보 x{_arena.ComboCount}"
                        : string.Empty)
                + $"   적 {_arena.EnemyCount}",
                _smallStyle);

            Rect hpRect = new Rect(
                hudPanel.x + 24f,
                hudPanel.y + 56f,
                hudPanel.width - 48f,
                30f);
            DrawProgressBar(
                hpRect,
                Mathf.Clamp01(
                    _arena.PlayerHealth
                    / Mathf.Max(1f, _arena.MaxPlayerHealth)),
                _healthFill,
                $"체력 {Mathf.CeilToInt(_arena.PlayerHealth)}"
                + $" / {Mathf.CeilToInt(_arena.MaxPlayerHealth)}");

            float xpMax = Mathf.Max(1f, _arena.ExperienceRequiredForNextLevel);
            Rect xpRect = new Rect(
                hudPanel.x + 24f,
                hudPanel.y + 94f,
                hudPanel.width - 48f,
                28f);
            DrawProgressBar(
                xpRect,
                Mathf.Clamp01(_arena.LevelExperience / xpMax),
                _experienceFill,
                $"경험치 {_arena.LevelExperience} / {_arena.ExperienceRequiredForNextLevel}");

            GUI.Label(
                new Rect(
                    hudPanel.x + 24f,
                    hudPanel.y + 132f,
                    hudPanel.width - 48f,
                    32f),
                $"기본기 {_arena.ActiveBaseSkillCount}/{_arena.MaxActiveBaseSkillCount}"
                + $"   ·   {_arena.ActiveBaseSkillProgressSummary}"
                + $"   ·   숙련 F{_arena.ActiveSkillMastery}/I{_arena.IceSkillMastery}"
                + (_arena.IsThermalShockUnlocked ? "   ·   열충격" : string.Empty)
                + (_arena.IsArcaneChainUnlocked ? "   ·   비전연쇄" : string.Empty)
                + (_arena.EliteCount > 0 ? $"   ·   엘리트 {_arena.EliteCount}" : string.Empty),
                _smallStyle);

            bool modalOpen = _arena.IsDead
                || _arena.IsAttributeAllocationPending
                || _arena.IsSkillChoicePending
                || _arena.IsBossRewardPending;
            if (!modalOpen)
            {
                DrawJoystick();
                DrawPotionButton();
            }

            if (_arena.HasBossAlive)
            {
                Rect bossRect = new Rect(
                    safe.x + safe.width * 0.14f,
                    hudPanel.yMax + 18f,
                    safe.width * 0.72f,
                    64f);
                DrawProgressBar(
                    bossRect,
                    _arena.BossHealthNormalized,
                    _experienceFill,
                    "BOSS · " + _arena.BossName
                    + (_arena.IsBossPhaseTwo ? " · PHASE II" : string.Empty));
            }

            Rect buildRect = new Rect(
                safe.xMax - margin - 300f,
                safe.y + margin + 82f,
                300f,
                88f);
            DrawFantasyPanel(buildRect);
            GUI.Label(
                buildRect,
                "BUILD " + BuildLabel,
                _buildStyle);

            Rect exitRect = new Rect(
                safe.xMax - margin - 168f,
                safe.y + margin,
                168f,
                64f);
            if (GUI.Button(exitRect, "나가기", _buttonStyle))
                SceneManager.LoadScene("Main");

            if (_arena.IsDead)
                DrawDeathOverlay();
            else if (_arena.IsAttributeAllocationPending)
                DrawAttributeAllocationOverlay();
            else if (_arena.IsBossRewardPending)
                DrawBossRewardOverlay();
            else if (_arena.IsSkillChoicePending)
                DrawSkillChoiceOverlay();
        }

        private void DrawPotionButton()
        {
            EnsureStyles();
            if (_buttonStyle == null || _arena == null)
                return;

            Rect safe = SafeGuiRect;
            Rect potionRect = new Rect(
                safe.xMax - 310f,
                safe.yMax - 138f,
                270f,
                96f);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = _arena.CanUsePotion;
            if (GUI.Button(
                    potionRect,
                    "물약 x" + _arena.PotionCount + "\n회복 35%",
                    _buttonStyle))
            {
                _arena.TryUsePotion();
            }
            GUI.enabled = previousEnabled;
        }

        private void DrawBossRewardOverlay()
        {
            Rect safe = SafeGuiRect;
            float width = Mathf.Min(safe.width * 0.90f, 1100f);
            float height = Mathf.Min(safe.height * 0.68f, 1040f);
            Rect panel = new Rect(
                safe.x + (safe.width - width) * 0.5f,
                safe.y + (safe.height - height) * 0.5f,
                width,
                height);

            GUI.DrawTexture(safe, _modalDim, ScaleMode.StretchToFill, true);
            DrawFantasyPanel(panel);

            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 34f, panel.width - 60f, 86f),
                "보스 전리품 선택",
                _deathStyle);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 118f, panel.width - 60f, 62f),
                $"남은 전리품 {_arena.PendingBossRewards} · 이번 전투의 방향을 강화합니다.",
                _smallStyle);

            const float gap = 22f;
            float top = panel.y + 208f;
            float buttonHeight = (panel.height - 250f - gap * 2f) / 3f;

            for (int i = 0; i < 3; i++)
            {
                Rect buttonRect = new Rect(
                    panel.x + 30f,
                    top + i * (buttonHeight + gap),
                    panel.width - 60f,
                    buttonHeight);

                string label = _arena.GetBossRewardTitle(i)
                    + "\n"
                    + _arena.GetBossRewardDescription(i);

                if (GUI.Button(buttonRect, label, _buttonStyle))
                    _arena.TryChooseBossReward(i);
            }
        }

        private void DrawSkillChoiceOverlay()
        {
            Rect safe = SafeGuiRect;
            float width = Mathf.Min(safe.width * 0.92f, 1120f);
            float height = Mathf.Min(safe.height * 0.72f, 1120f);
            Rect panel = new Rect(
                safe.x + (safe.width - width) * 0.5f,
                safe.y + (safe.height - height) * 0.5f,
                width,
                height);

            GUI.DrawTexture(safe, _modalDim, ScaleMode.StretchToFill, true);
            DrawFantasyPanel(panel);

            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 34f, panel.width - 60f, 86f),
                "기본기 선택",
                _deathStyle);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 116f, panel.width - 60f, 58f),
                $"남은 선택 {_arena.PendingSkillChoices} · 신규 습득 또는 보유 기본기를 강화합니다.",
                _smallStyle);

            const float gap = 22f;
            float top = panel.y + 196f;
            float buttonHeight = (panel.height - 242f - gap * 2f) / 3f;

            for (int i = 0; i < 3; i++)
            {
                Rect buttonRect = new Rect(
                    panel.x + 30f,
                    top + i * (buttonHeight + gap),
                    panel.width - 60f,
                    buttonHeight);

                string label = _arena.GetSkillUpgradeTitle(i)
                    + "\n"
                    + _arena.GetSkillUpgradeDescription(i);

                if (GUI.Button(buttonRect, label, _buttonStyle))
                    _arena.TryChooseSkillUpgrade(i);
            }
        }

        private void DrawJoystick()
        {
            if (!_joystickVisible) return;

            float radius = JoystickRadius;
            Vector2 center = _joystickOrigin;

            Rect baseRect = ScreenRect(center, radius);
            GUI.DrawTexture(baseRect, _joystickBase, ScaleMode.StretchToFill, true);

            Vector2 knobCenter = center + _joystickInput * radius * 0.62f;
            Rect knobRect = ScreenRect(knobCenter, radius * 0.40f);
            GUI.DrawTexture(knobRect, _joystickKnob, ScaleMode.StretchToFill, true);
        }

        private void DrawAttributeAllocationOverlay()
        {
            Rect safe = SafeGuiRect;
            float width = Mathf.Min(safe.width * 0.90f, 1120f);
            float height = Mathf.Min(safe.height * 0.58f, 1200f);
            Rect panel = new Rect(
                safe.x + (safe.width - width) * 0.5f,
                safe.y + (safe.height - height) * 0.5f,
                width,
                height);

            GUI.DrawTexture(safe, _modalDim, ScaleMode.StretchToFill, true);
            DrawFantasyPanel(panel);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 44f, panel.width - 60f, 92f),
                $"레벨 상승  레벨 {_arena.Level}",
                _deathStyle);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 142f, panel.width - 60f, 70f),
                $"능력치 포인트 {_arena.UnspentAttributePoints}",
                _hudStyle);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 198f, panel.width - 60f, 72f),
                $"레벨 기본 강화: 스킬 피해 +{Mathf.RoundToInt(_arena.LevelDamageBonusPercent)}%"
                + " · 최대체력 증가 · 체력 30% 회복\n"
                + "능력치 포인트를 배분하면 즉시 전투 성능에 반영됩니다.",
                _smallStyle);

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
            float gridTop = panel.y + 276f;
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
            string label =
                $"{GetAttributeLabel(attribute)}  {value}\n"
                + GetAttributeEffectLabel(attribute)
                + "\n+1";

            if (GUI.Button(rect, label, _buttonStyle))
                _arena.TrySpendAttributePoint(attribute);
        }

        private static string GetAttributeEffectLabel(
            CharacterAttribute attribute)
        {
            switch (attribute)
            {
                case CharacterAttribute.Body:
                    return "최대체력 +20";
                case CharacterAttribute.Agility:
                    return "이동속도 +5%";
                case CharacterAttribute.Perception:
                    return "사거리·탄속 +6%";
                case CharacterAttribute.Intelligence:
                    return "스킬 피해 +8%";
                case CharacterAttribute.Wisdom:
                    return "재사용시간 -5%";
                case CharacterAttribute.Will:
                    return "받는 피해 -6%";
                case CharacterAttribute.Good:
                    return "냉기 피해 +10%";
                case CharacterAttribute.Evil:
                    return "화염 피해 +10%";
                default:
                    return string.Empty;
            }
        }

        private static string GetAttributeLabel(CharacterAttribute attribute)
        {
            switch (attribute)
            {
                case CharacterAttribute.Body: return "육체";
                case CharacterAttribute.Agility: return "민첩";
                case CharacterAttribute.Perception: return "감각";
                case CharacterAttribute.Intelligence: return "지능";
                case CharacterAttribute.Wisdom: return "지혜";
                case CharacterAttribute.Will: return "의지";
                case CharacterAttribute.Good: return "선";
                case CharacterAttribute.Evil: return "악";
                default: return attribute.ToString();
            }
        }

        private void DrawDeathOverlay()
        {
            Rect safe = SafeGuiRect;
            float width = Mathf.Min(safe.width * 0.82f, 900f);
            float height = Mathf.Min(420f, safe.height * 0.52f);
            Rect panel = new Rect(
                safe.x + (safe.width - width) * 0.5f,
                safe.y + (safe.height - height) * 0.5f,
                width,
                height);

            DrawFantasyPanel(panel);
            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 50f, panel.width - 60f, 100f),
                "전투 불능",
                _deathStyle);            GUI.Label(
                new Rect(panel.x + 30f, panel.y + 160f, panel.width - 60f, 60f),
                $"처치 {_arena.Kills}   총 경험치 {_arena.Experience}",
                _hudStyle);

            if (GUI.Button(
                new Rect(panel.center.x - 180f, panel.yMax - 130f, 360f, 86f),
                "다시 시작",
                _buttonStyle))
            {
                _arena.PrepareRetryAfterDefeat();
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

            _buildStyle = new GUIStyle(skin.label)
            {
                fontSize = Mathf.Max(42, baseSize + 12),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Color parchment = new Color(0.92f, 0.84f, 0.65f);
            _hudStyle.normal.textColor = parchment;
            _smallStyle.normal.textColor = new Color(0.78f, 0.72f, 0.58f);
            _deathStyle.normal.textColor = new Color(0.96f, 0.80f, 0.36f);
            _buildStyle.normal.textColor = new Color(1f, 0.88f, 0.34f);
            _buttonStyle.normal.textColor = parchment;
            _buttonStyle.hover.textColor = parchment;
            _buttonStyle.active.textColor = Color.white;
            _buttonStyle.normal.background = _buttonBack;
            _buttonStyle.hover.background = _buttonPressed;
            _buttonStyle.active.background = _buttonPressed;
        }

        private void DrawFantasyPanel(Rect rect)
        {
            GUI.DrawTexture(rect, _panelOuter, ScaleMode.StretchToFill, true);
            Rect inner = new Rect(
                rect.x + 4f,
                rect.y + 4f,
                rect.width - 8f,
                rect.height - 8f);
            GUI.DrawTexture(inner, _panelInner, ScaleMode.StretchToFill, true);
        }

        private void DrawProgressBar(
            Rect rect,
            float normalized,
            Texture2D fill,
            string label)
        {
            GUI.DrawTexture(rect, _panelOuter, ScaleMode.StretchToFill, true);
            Rect inner = new Rect(
                rect.x + 3f,
                rect.y + 3f,
                rect.width - 6f,
                rect.height - 6f);
            GUI.DrawTexture(inner, _barBack, ScaleMode.StretchToFill, true);

            Rect fillRect = new Rect(
                inner.x,
                inner.y,
                inner.width * Mathf.Clamp01(normalized),
                inner.height);
            GUI.DrawTexture(fillRect, fill, ScaleMode.StretchToFill, true);

            GUI.Label(rect, label, _smallStyle);
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            return texture;
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
            Mathf.Clamp(Screen.width * 0.125f, 132f, 210f);

        private static Rect SafeGuiRect
        {
            get
            {
                Rect safe = Screen.safeArea;
                return new Rect(
                    safe.x,
                    Screen.height - safe.yMax,
                    safe.width,
                    safe.height);
            }
        }

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