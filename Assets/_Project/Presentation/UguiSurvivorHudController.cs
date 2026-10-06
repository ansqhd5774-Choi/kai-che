using System.Collections.Generic;
using KaiChe.Core.Progression;
using KaiChe.Gameplay.Survivor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KaiChe.Presentation
{
    public sealed class UguiSurvivorHudController : MonoBehaviour
    {
        private const string BuildLabel = "R30";
        private static readonly Color PanelColor = new Color(0.045f, 0.040f, 0.034f, 0.94f);
        private static readonly Color GoldColor = new Color(0.94f, 0.72f, 0.22f, 1f);
        private static readonly Color ParchmentColor = new Color(0.93f, 0.88f, 0.75f, 1f);
        private static readonly Color HealthColor = new Color(0.72f, 0.10f, 0.08f, 1f);
        private static readonly Color ExpColor = new Color(0.18f, 0.42f, 0.92f, 1f);
        private static readonly Color BossColor = new Color(0.62f, 0.18f, 0.84f, 1f);

        private SurvivorArenaController _arena;
        private Canvas _canvas;
        private RectTransform _safeRoot;
        private Font _font;
        private Text _levelText;
        private Text _waveText;
        private Text _skillText;
        private Text _buildText;
        private Image _healthFill;
        private Text _healthText;
        private Image _expFill;
        private Text _expText;
        private GameObject _bossPanel;
        private Image _bossFill;
        private Text _bossText;
        private Button _exitButton;
        private Button _potionButton;
        private Text _potionButtonText;

        private GameObject _modalDim;
        private GameObject _modalPanel;
        private SurvivorHudController _legacyHud;
        private Text _modalTitle;
        private Text _modalSubtitle;
        private readonly List<Button> _choiceButtons = new List<Button>();
        private readonly List<Text> _choiceTexts = new List<Text>();
        private readonly List<Button> _attributeButtons = new List<Button>();
        private readonly List<Text> _attributeTexts = new List<Text>();
        private int _lastScreenWidth;
        private int _lastScreenHeight;
        private Rect _lastSafeArea;

        public void Bind(SurvivorArenaController arena)
        {
            _arena = arena;
        }

        private void Awake()
        {
            _legacyHud = GetComponent<SurvivorHudController>();
            BuildUi();
            UpdateLegacyModalFallback();
        }

        private void Update()
        {
            if (_arena == null || _canvas == null)
                return;

            EnsureEventSystem();
            UpdateSafeArea();
            UpdateHud();
            UpdateModal();
            UpdateLegacyModalFallback();
        }

        private void BuildUi()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new GameObject("R14_UguiCanvas");
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 30;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EnsureEventSystem();

            _safeRoot = CreateRect(
                "SafeArea",
                canvasObject.transform,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);

            RectTransform statusPanel = CreatePanel(
                "StatusPanel",
                _safeRoot,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(34f, -34f),
                new Vector2(980f, 224f),
                PanelColor);

            _levelText = CreateText(
                "LevelText",
                statusPanel,
                "전사  LV 1",
                38,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                GoldColor,
                new Vector2(28f, -18f),
                new Vector2(410f, 54f));

            _waveText = CreateText(
                "WaveText",
                statusPanel,
                "WAVE 1",
                27,
                FontStyle.Bold,
                TextAnchor.UpperRight,
                ParchmentColor,
                new Vector2(420f, -18f),
                new Vector2(520f, 48f));

            CreateBar(
                statusPanel,
                "Health",
                new Vector2(28f, -78f),
                new Vector2(924f, 42f),
                HealthColor,
                out _healthFill,
                out _healthText);

            CreateBar(
                statusPanel,
                "Experience",
                new Vector2(28f, -128f),
                new Vector2(924f, 34f),
                ExpColor,
                out _expFill,
                out _expText);

            _skillText = CreateText(
                "SkillText",
                statusPanel,
                string.Empty,
                25,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                ParchmentColor,
                new Vector2(28f, -174f),
                new Vector2(924f, 42f));

            RectTransform buildPanel = CreatePanel(
                "BuildBadge",
                _safeRoot,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-330f, -112f),
                new Vector2(296f, 76f),
                new Color(0.10f, 0.075f, 0.025f, 0.95f));

            _buildText = CreateText(
                "BuildText",
                buildPanel,
                "BUILD " + BuildLabel,
                34,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                GoldColor,
                Vector2.zero,
                new Vector2(296f, 76f));

            RectTransform exitRoot = CreateRect(
                "Exit",
                _safeRoot,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-218f, -34f),
                new Vector2(184f, 66f));
            _exitButton = CreateButton(exitRoot, "나가기", 28);
            _exitButton.onClick.AddListener(() => SceneManager.LoadScene("Main"));

            RectTransform potionRoot = CreateRect(
                "Potion",
                _safeRoot,
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(-34f, 34f),
                new Vector2(300f, 112f));
            _potionButton = CreateButton(
                potionRoot,
                "물약 x0\n회복 35%",
                28);
            _potionButtonText =
                _potionButton.GetComponentInChildren<Text>();
            _potionButton.onClick.AddListener(
                () => _arena.TryUsePotion());

            RectTransform bossRoot = CreatePanel(
                "BossPanel",
                _safeRoot,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-620f, -278f),
                new Vector2(1240f, 78f),
                new Color(0.035f, 0.020f, 0.050f, 0.96f));
            _bossPanel = bossRoot.gameObject;
            CreateBar(
                bossRoot,
                "Boss",
                new Vector2(22f, -18f),
                new Vector2(1196f, 44f),
                BossColor,
                out _bossFill,
                out _bossText);

            BuildModal();
            UpdateSafeArea();
        }

        private void BuildModal()
        {
            _modalDim = CreatePanel(
                "ModalDim",
                _safeRoot,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.01f, 0.008f, 0.006f, 0.72f)).gameObject;

            RectTransform modal = CreatePanel(
                "ModalPanel",
                _safeRoot,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(1240f, 820f),
                new Color(0.055f, 0.045f, 0.034f, 0.985f));
            _modalPanel = modal.gameObject;

            _modalTitle = CreateText(
                "ModalTitle",
                modal,
                string.Empty,
                46,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                GoldColor,
                new Vector2(40f, -34f),
                new Vector2(1160f, 78f));

            _modalSubtitle = CreateText(
                "ModalSubtitle",
                modal,
                string.Empty,
                27,
                FontStyle.Normal,
                TextAnchor.MiddleCenter,
                ParchmentColor,
                new Vector2(54f, -114f),
                new Vector2(1132f, 66f));

            for (int i = 0; i < 3; i++)
            {
                RectTransform buttonRoot = CreateRect(
                    "Choice_" + i,
                    modal,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -214f - i * 176f),
                    new Vector2(1080f, 142f));
                Button button = CreateButton(buttonRoot, string.Empty, 30);
                _choiceButtons.Add(button);
                _choiceTexts.Add(button.GetComponentInChildren<Text>());
            }

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

            for (int i = 0; i < attributes.Length; i++)
            {
                int row = i / 2;
                int col = i % 2;
                RectTransform buttonRoot = CreateRect(
                    "Attribute_" + i,
                    modal,
                    new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 1f),
                    new Vector2(-283f + col * 566f, -230f - row * 130f),
                    new Vector2(534f, 102f));
                Button button = CreateButton(buttonRoot, string.Empty, 27);
                CharacterAttribute captured = attributes[i];
                button.onClick.AddListener(() => _arena.TrySpendAttributePoint(captured));
                _attributeButtons.Add(button);
                _attributeTexts.Add(button.GetComponentInChildren<Text>());
            }

            _modalDim.SetActive(false);
            _modalPanel.SetActive(false);
        }

        private void UpdateHud()
        {
            float maxHealth = Mathf.Max(1f, _arena.MaxPlayerHealth);
            _healthFill.fillAmount = Mathf.Clamp01(_arena.PlayerHealth / maxHealth);
            _healthText.text =
                "HP  " + Mathf.CeilToInt(_arena.PlayerHealth)
                + " / " + Mathf.CeilToInt(maxHealth);

            float xpMax = Mathf.Max(1f, _arena.ExperienceRequiredForNextLevel);
            _expFill.fillAmount = Mathf.Clamp01(_arena.LevelExperience / xpMax);
            _expText.text =
                "EXP  " + _arena.LevelExperience
                + " / " + _arena.ExperienceRequiredForNextLevel;

            _levelText.text =
                "전사  LV " + _arena.Level
                + "   공격 +" + Mathf.RoundToInt(_arena.LevelDamageBonusPercent) + "%";

            _waveText.text =
                "WAVE " + _arena.Wave
                + "   " + Mathf.FloorToInt(_arena.SurvivalTime / 60f).ToString("00")
                + ":" + Mathf.FloorToInt(_arena.SurvivalTime % 60f).ToString("00")
                + (_arena.IsPressureWave ? "   공세" : string.Empty)
                + "   처치 " + _arena.Kills
                + (_arena.IsFrenzyActive
                    ? "   폭주 " + _arena.FrenzyTimeRemaining.ToString("0.0") + "s"
                    : _arena.ComboCount > 1
                        ? "   콤보 x" + _arena.ComboCount
                        : string.Empty)
                + "   적 " + _arena.EnemyCount
                + (_arena.EliteCount > 0 ? "   엘리트 " + _arena.EliteCount : string.Empty);

            _skillText.text =
                "기본기 "
                + _arena.ActiveBaseSkillCount
                + "/"
                + _arena.MaxActiveBaseSkillCount
                + "   "
                + _arena.ActiveBaseSkillProgressSummary
                + "   숙련 F" + _arena.ActiveSkillMastery
                + "/I" + _arena.IceSkillMastery
                + (_arena.IsThermalShockUnlocked ? "   열충격" : string.Empty)
                + (_arena.IsArcaneChainUnlocked ? "   비전연쇄" : string.Empty);

            _buildText.text = "BUILD " + BuildLabel;

            if (_potionButton != null)
            {
                _potionButton.interactable = _arena.CanUsePotion;
                if (_potionButtonText != null)
                {
                    _potionButtonText.text =
                        "물약 x" + _arena.PotionCount
                        + "\n회복 35%";
                }
            }

            _bossPanel.SetActive(_arena.HasBossAlive);
            if (_arena.HasBossAlive)
            {
                _bossFill.fillAmount = _arena.BossHealthNormalized;
                _bossText.text =
                    "BOSS · " + _arena.BossName
                    + (_arena.IsBossPhaseTwo ? " · PHASE II" : string.Empty);
            }
        }

        private void UpdateModal()
        {
            bool show = _arena.IsDead
                || _arena.IsAttributeAllocationPending
                || _arena.IsSkillChoicePending
                || _arena.IsBossRewardPending;

            _modalDim.SetActive(show);
            _modalPanel.SetActive(show);
            if (!show)
                return;

            SetChoiceButtonsVisible(false);
            SetAttributeButtonsVisible(false);

            if (_arena.IsDead)
            {
                _modalTitle.text = "전투 불능";
                _modalSubtitle.text =
                    "생존 " + Mathf.FloorToInt(_arena.SurvivalTime)
                    + "초 · 처치 " + _arena.Kills;

                SetChoiceButtonsVisible(true);
                ConfigureChoiceButton(
                    0,
                    "다시 도전",
                    () =>
                    {
                        _arena.PrepareRetryAfterDefeat();
                        SceneManager.LoadScene("Game");
                    });
                ConfigureChoiceButton(
                    1,
                    "메인으로",
                    () =>
                    {
                        _arena.PrepareRetryAfterDefeat();
                        SceneManager.LoadScene("Main");
                    });
                _choiceButtons[2].gameObject.SetActive(false);
                return;
            }

            if (_arena.IsAttributeAllocationPending)
            {
                _modalTitle.text = "레벨 상승 · 능력치 강화";
                _modalSubtitle.text =
                    "남은 포인트 " + _arena.UnspentAttributePoints
                    + " · 선택 즉시 전투 성능에 반영됩니다.";
                SetAttributeButtonsVisible(true);
                UpdateAttributeLabels();
                return;
            }

            if (_arena.IsSkillChoicePending)
            {
                _modalTitle.text = "기본기 선택";
                _modalSubtitle.text =
                    "남은 선택 " + _arena.PendingSkillChoices
                    + " · 신규 습득 또는 보유 기본기를 강화합니다.";
                SetChoiceButtonsVisible(true);
                for (int i = 0; i < 3; i++)
                {
                    int captured = i;
                    ConfigureChoiceButton(
                        i,
                        _arena.GetSkillUpgradeTitle(i)
                        + "\n" + _arena.GetSkillUpgradeDescription(i),
                        () => _arena.TryChooseSkillUpgrade(captured));
                }
                return;
            }

            if (_arena.IsBossRewardPending)
            {
                _modalTitle.text = "균열 군주 전리품";
                _modalSubtitle.text =
                    "남은 보상 " + _arena.PendingBossRewards
                    + " · 이번 원정에 영구 적용됩니다.";
                SetChoiceButtonsVisible(true);
                for (int i = 0; i < 3; i++)
                {
                    int captured = i;
                    ConfigureChoiceButton(
                        i,
                        _arena.GetBossRewardTitle(i)
                        + "\n" + _arena.GetBossRewardDescription(i),
                        () => _arena.TryChooseBossReward(captured));
                }
            }
        }

        private void UpdateAttributeLabels()
        {
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

            for (int i = 0; i < attributes.Length; i++)
            {
                CharacterAttribute attribute = attributes[i];
                _attributeTexts[i].text =
                    AttributeName(attribute)
                    + "  " + _arena.GetAttributeValue(attribute)
                    + "\n+1";
            }
        }

        private static string AttributeName(CharacterAttribute attribute)
        {
            switch (attribute)
            {
                case CharacterAttribute.Body: return "육체 · 최대 체력";
                case CharacterAttribute.Agility: return "민첩 · 이동 속도";
                case CharacterAttribute.Perception: return "감각 · 사거리/탄속";
                case CharacterAttribute.Intelligence: return "지능 · 스킬 피해";
                case CharacterAttribute.Wisdom: return "지혜 · 재사용";
                case CharacterAttribute.Will: return "의지 · 피해 감소";
                case CharacterAttribute.Good: return "선 · 냉기 강화";
                case CharacterAttribute.Evil: return "악 · 화염 강화";
                default: return attribute.ToString();
            }
        }

        private void ConfigureChoiceButton(int index, string label, UnityEngine.Events.UnityAction action)
        {
            Button button = _choiceButtons[index];
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            _choiceTexts[index].text = label;
        }

        private void SetChoiceButtonsVisible(bool visible)
        {
            for (int i = 0; i < _choiceButtons.Count; i++)
                _choiceButtons[i].gameObject.SetActive(visible);
        }

        private void SetAttributeButtonsVisible(bool visible)
        {
            for (int i = 0; i < _attributeButtons.Count; i++)
                _attributeButtons[i].gameObject.SetActive(visible);
        }

        private void UpdateSafeArea()
        {
            Rect safe = Screen.safeArea;
            if (_lastScreenWidth == Screen.width
                && _lastScreenHeight == Screen.height
                && _lastSafeArea == safe)
                return;

            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;
            _lastSafeArea = safe;

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Mathf.Max(1f, Screen.width);
            min.y /= Mathf.Max(1f, Screen.height);
            max.x /= Mathf.Max(1f, Screen.width);
            max.y /= Mathf.Max(1f, Screen.height);

            _safeRoot.anchorMin = min;
            _safeRoot.anchorMax = max;
            _safeRoot.offsetMin = Vector2.zero;
            _safeRoot.offsetMax = Vector2.zero;
        }

        private void EnsureEventSystem()
        {
            EventSystem current = EventSystem.current;
            if (current == null)
            {
                GameObject eventObject = new GameObject("R16_EventSystem");
                eventObject.transform.SetParent(transform, false);
                current = eventObject.AddComponent<EventSystem>();
            }

            BaseInputModule[] modules =
                current.GetComponents<BaseInputModule>();
            bool hasEnabledModule = false;
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null && modules[i].enabled)
                {
                    hasEnabledModule = true;
                    break;
                }
            }

            if (!hasEnabledModule)
                current.gameObject.AddComponent<StandaloneInputModule>();
        }

        private void UpdateLegacyModalFallback()
        {
            if (_legacyHud == null)
                return;

            _legacyHud.SetModalFallbackEnabled(!IsPrimaryUiReady());
        }

        private bool IsPrimaryUiReady()
        {
            if (_canvas == null
                || !_canvas.isActiveAndEnabled
                || _safeRoot == null
                || _modalDim == null
                || _modalPanel == null
                || _modalTitle == null
                || _modalSubtitle == null
                || _choiceButtons.Count != 3
                || _attributeButtons.Count != 8)
                return false;

            EventSystem current = EventSystem.current;
            if (current == null)
                return false;

            BaseInputModule[] modules =
                current.GetComponents<BaseInputModule>();
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i] != null && modules[i].enabled)
                    return true;
            }

            return false;
        }

        private RectTransform CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            Color color)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                anchorMin,
                anchorMax,
                anchoredPosition,
                size);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private RectTransform CreateRect(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin == anchorMax
                ? new Vector2(
                    anchorMin.x <= 0.01f ? 0f : anchorMin.x >= 0.99f ? 1f : 0.5f,
                    anchorMin.y <= 0.01f ? 0f : anchorMin.y >= 0.99f ? 1f : 0.5f)
                : new Vector2(0.5f, 0.5f);

            if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
            {
                rect.offsetMin = anchoredPosition;
                rect.offsetMax = size;
            }
            else
            {
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
            }
            return rect;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            Color color,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            RectTransform rect = CreateRect(
                name,
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                anchoredPosition,
                size);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Button CreateButton(RectTransform root, string label, int fontSize)
        {
            Image image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.10f, 0.075f, 0.98f);

            Button button = root.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.92f, 0.72f, 1f);
            colors.pressedColor = new Color(0.82f, 0.66f, 0.34f, 1f);
            button.colors = colors;

            RectTransform textRect = CreateRect(
                "Label",
                root,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            Text text = textRect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = ParchmentColor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return button;
        }

        private void CreateBar(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            Vector2 size,
            Color fillColor,
            out Image fill,
            out Text label)
        {
            RectTransform back = CreateRect(
                name + "Back",
                parent,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                anchoredPosition,
                size);
            Image backImage = back.gameObject.AddComponent<Image>();
            backImage.color = new Color(0.015f, 0.015f, 0.015f, 0.94f);

            RectTransform fillRect = CreateRect(
                name + "Fill",
                back,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            fill = fillRect.gameObject.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 1f;

            RectTransform labelRect = CreateRect(
                name + "Label",
                back,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero);
            label = labelRect.gameObject.AddComponent<Text>();
            label.font = _font;
            label.fontSize = 24;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
        }
    }
}
