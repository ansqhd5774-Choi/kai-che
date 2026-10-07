using System.Collections.Generic;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class RuntimeMageSpriteAnimator : MonoBehaviour
    {
        private enum Clip
        {
            Idle = 0,
            Run = 1,
            Damage = 2,
            Attack = 3,
            Dead = 4
        }

        private const string ResourcePath = "Characters/Mage/mage_sheet";
        private const int CellSize = 25;
        private const int CellGap = 2;
        private const int CellStride = CellSize + CellGap;
        private const float PixelsPerUnit = 8f;

        private static readonly int[] PreferredFrameCounts =
        {
            4, // Idle
            6, // Run
            3, // Damage
            6, // Attack
            6  // Dead
        };

        private readonly Dictionary<Clip, Sprite[]> _clips =
            new Dictionary<Clip, Sprite[]>();

        private SpriteRenderer _renderer;
        private Clip _clip;
        private int _frameIndex;
        private float _frameTimer;
        private Vector2 _moveInput;
        private bool _oneShot;
        private bool _dead;

        public static GameObject TryCreate(
            Transform parent,
            out RuntimeMageSpriteAnimator animator)
        {
            animator = null;
            Texture2D texture = Resources.Load<Texture2D>(ResourcePath);
            if (texture == null)
            {
                Debug.LogWarning(
                    "KAI_CHE_R31_2D_MAGE_RESOURCE_MISSING_FALLBACK_3D");
                return null;
            }

            var root = new GameObject("PlayerMage2D");
            root.transform.SetParent(parent, false);

            var spriteObject = new GameObject("MageSprite");
            spriteObject.transform.SetParent(root.transform, false);
            spriteObject.transform.localPosition =
                new Vector3(0f, 0.02f, 0f);

            var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 40;
            spriteRenderer.color = Color.white;

            animator = root.AddComponent<RuntimeMageSpriteAnimator>();
            if (!animator.Configure(texture, spriteRenderer))
            {
                Object.Destroy(root);
                animator = null;
                Debug.LogWarning(
                    "KAI_CHE_R31_2D_MAGE_LAYOUT_INVALID_FALLBACK_3D");
                return null;
            }

            Debug.Log("KAI_CHE_R31_2D_MAGE_ACTIVE");
            Debug.Log(
                "KAI_CHE_R31_2D_MAGE_TEXTURE_"
                + texture.width
                + "X"
                + texture.height);
            return root;
        }

        public void SetMovement(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);

            if (_renderer != null && Mathf.Abs(_moveInput.x) > 0.05f)
                _renderer.flipX = _moveInput.x < 0f;

            if (_dead || _oneShot)
                return;

            SetClip(
                _moveInput.sqrMagnitude > 0.01f
                    ? Clip.Run
                    : Clip.Idle,
                false);
        }

        public void TriggerAttack()
        {
            if (_dead)
                return;

            SetClip(Clip.Attack, true);
        }

        public void TriggerHit()
        {
            if (_dead)
                return;

            SetClip(Clip.Damage, true);
        }

        public void SetDead(bool dead)
        {
            _dead = dead;
            if (dead)
                SetClip(Clip.Dead, false);
            else
                SetClip(Clip.Idle, false);
        }

        private bool Configure(
            Texture2D texture,
            SpriteRenderer spriteRenderer)
        {
            if (texture == null || spriteRenderer == null)
                return false;

            _renderer = spriteRenderer;

            int columns = Mathf.Max(
                1,
                (texture.width + CellGap) / CellStride);
            int rows = Mathf.Max(
                1,
                (texture.height + CellGap) / CellStride);

            if (rows < 5)
                return false;

            for (int row = 0; row < 5; row++)
            {
                Clip clip = (Clip)row;
                int frameCount = Mathf.Clamp(
                    PreferredFrameCounts[row],
                    1,
                    columns);
                var frames = new Sprite[frameCount];

                for (int column = 0; column < frameCount; column++)
                {
                    float x = column * CellStride;
                    float y =
                        texture.height
                        - CellSize
                        - row * CellStride;

                    if (x + CellSize > texture.width
                        || y < 0f
                        || y + CellSize > texture.height)
                    {
                        return false;
                    }

                    Sprite frame = Sprite.Create(
                        texture,
                        new Rect(x, y, CellSize, CellSize),
                        new Vector2(0.5f, 0.08f),
                        PixelsPerUnit,
                        0,
                        SpriteMeshType.FullRect);
                    frame.name =
                        "Mage_"
                        + clip
                        + "_"
                        + column;
                    frames[column] = frame;
                }

                _clips[clip] = frames;
            }

            SetClip(Clip.Idle, false);
            Debug.Log("KAI_CHE_R31_2D_MAGE_ANIMATION_READY");
            return true;
        }

        private void Update()
        {
            if (_renderer == null)
                return;

            Sprite[] frames = GetFrames(_clip);
            if (frames == null || frames.Length == 0)
                return;

            _frameTimer += Time.deltaTime;
            float frameDuration = GetFrameDuration(_clip);
            if (_frameTimer < frameDuration)
                return;

            _frameTimer -= frameDuration;

            if (_frameIndex < frames.Length - 1)
            {
                _frameIndex += 1;
            }
            else if (_clip == Clip.Dead)
            {
                _frameIndex = frames.Length - 1;
            }
            else if (_oneShot)
            {
                _oneShot = false;
                SetClip(
                    _moveInput.sqrMagnitude > 0.01f
                        ? Clip.Run
                        : Clip.Idle,
                    false);
                return;
            }
            else
            {
                _frameIndex = 0;
            }

            ApplyFrame();
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            Vector3 forward =
                transform.position - camera.transform.position;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                transform.rotation =
                    Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private void SetClip(Clip clip, bool oneShot)
        {
            if (_clip == clip && _oneShot == oneShot)
                return;

            _clip = clip;
            _oneShot = oneShot;
            _frameIndex = 0;
            _frameTimer = 0f;
            ApplyFrame();
        }

        private void ApplyFrame()
        {
            Sprite[] frames = GetFrames(_clip);
            if (_renderer == null
                || frames == null
                || frames.Length == 0)
                return;

            _frameIndex = Mathf.Clamp(
                _frameIndex,
                0,
                frames.Length - 1);
            _renderer.sprite = frames[_frameIndex];
        }

        private Sprite[] GetFrames(Clip clip)
        {
            return _clips.TryGetValue(
                clip,
                out Sprite[] frames)
                ? frames
                : null;
        }

        private static float GetFrameDuration(Clip clip)
        {
            switch (clip)
            {
                case Clip.Run:
                    return 0.095f;
                case Clip.Attack:
                    return 0.080f;
                case Clip.Damage:
                    return 0.110f;
                case Clip.Dead:
                    return 0.135f;
                default:
                    return 0.180f;
            }
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<Clip, Sprite[]> pair in _clips)
            {
                Sprite[] frames = pair.Value;
                if (frames == null)
                    continue;

                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i] != null)
                        Destroy(frames[i]);
                }
            }

            _clips.Clear();
        }
    }
}
