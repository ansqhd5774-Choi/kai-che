using System;
using System.Collections.Generic;
using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class RuntimeMageSpriteAnimator : MonoBehaviour
    {
        private enum Clip
        {
            Idle = 0,
            Walk = 1,
            Attack = 2,
            Hit = 3,
            Death = 4
        }

        private const string FoundryResourceRoot = "Characters/SpriteFoundryMage";
        private const string PicoResourceRoot = "Characters/PicoMage";
        private const float MoveDeadZoneSqr = 0.01f;
        private const float FoundryVisualScale = 1.62f;

        private static readonly RuntimeDirection8[] Directions =
        {
            RuntimeDirection8.South,
            RuntimeDirection8.SouthEast,
            RuntimeDirection8.East,
            RuntimeDirection8.NorthEast,
            RuntimeDirection8.North,
            RuntimeDirection8.NorthWest,
            RuntimeDirection8.West,
            RuntimeDirection8.SouthWest
        };

        private readonly Dictionary<Clip, Dictionary<RuntimeDirection8, Sprite[]>> _clips =
            new Dictionary<Clip, Dictionary<RuntimeDirection8, Sprite[]>>();
        private readonly Dictionary<RuntimeDirection8, Sprite> _foundrySprites =
            new Dictionary<RuntimeDirection8, Sprite>();

        private SpriteRenderer _renderer;
        private Transform _spriteTransform;
        private Clip _clip = Clip.Idle;
        private RuntimeDirection8 _direction = RuntimeDirection8.South;
        private Vector2 _moveInput;
        private int _frameIndex;
        private float _frameTimer;
        private float _poseTime;
        private bool _oneShot;
        private bool _dead;
        private bool _usesSpriteFoundry;
        private Vector3 _spriteBasePosition;

        public RuntimeDirection8 Direction => _direction;
        public bool UsesSpriteFoundry => _usesSpriteFoundry;

        public static GameObject TryCreate(
            Transform parent,
            out RuntimeMageSpriteAnimator animator)
        {
            animator = null;

            var root = new GameObject("PlayerMage2D");
            root.transform.SetParent(parent, false);

            var spriteObject = new GameObject("MageSprite");
            spriteObject.transform.SetParent(root.transform, false);
            spriteObject.transform.localPosition = new Vector3(0f, 0.02f, 0f);

            var spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 40;
            spriteRenderer.color = Color.white;
            spriteRenderer.spriteSortPoint = SpriteSortPoint.Pivot;

            animator = root.AddComponent<RuntimeMageSpriteAnimator>();
            if (!animator.Configure(spriteRenderer))
            {
                Destroy(root);
                animator = null;
                Debug.LogWarning(
                    "KAI_CHE_2D_8DIR_MAGE_RESOURCE_INCOMPLETE_FALLBACK_3D");
                return null;
            }

            if (animator.UsesSpriteFoundry)
                Debug.Log("KAI_CHE_SPRITE_FOUNDRY_MAGE_ACTIVE");
            else
                Debug.Log("KAI_CHE_2D_8DIR_MAGE_ACTIVE");
            return root;
        }

        public void SetMovement(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
            if (_moveInput.sqrMagnitude > MoveDeadZoneSqr)
            {
                _direction = RuntimeDirection8Utility.FromVector(
                    _moveInput,
                    _direction);
                ApplyFrame();
            }

            if (_dead || _oneShot)
                return;

            SetClip(
                _moveInput.sqrMagnitude > MoveDeadZoneSqr
                    ? Clip.Walk
                    : Clip.Idle,
                false);
        }

        public void TriggerAttack()
        {
            TriggerAttack(Vector2.zero);
        }

        public void TriggerAttack(Vector2 aimDirection)
        {
            if (_dead)
                return;

            if (aimDirection.sqrMagnitude > MoveDeadZoneSqr)
            {
                _direction = RuntimeDirection8Utility.FromVector(
                    aimDirection,
                    _direction);
            }

            SetClip(Clip.Attack, true);
        }

        public void TriggerHit()
        {
            if (_dead)
                return;

            SetClip(Clip.Hit, true);
        }

        public void SetDead(bool dead)
        {
            _dead = dead;
            SetClip(dead ? Clip.Death : Clip.Idle, false);
        }

        private bool Configure(SpriteRenderer spriteRenderer)
        {
            if (spriteRenderer == null)
                return false;

            _renderer = spriteRenderer;
            _spriteTransform = spriteRenderer.transform;
            _spriteBasePosition = _spriteTransform.localPosition;
            _clips.Clear();
            _foundrySprites.Clear();

            if (TryConfigureSpriteFoundry())
            {
                _usesSpriteFoundry = true;
                SetClip(Clip.Idle, false);
                Debug.Log("KAI_CHE_SPRITE_FOUNDRY_MAGE_READY_8DIR");
                return true;
            }

            _usesSpriteFoundry = false;
            foreach (Clip clip in Enum.GetValues(typeof(Clip)))
            {
                string stateToken = GetStateToken(clip);
                var directionalFrames =
                    new Dictionary<RuntimeDirection8, Sprite[]>();

                for (int i = 0; i < Directions.Length; i++)
                {
                    RuntimeDirection8 direction = Directions[i];
                    string path = PicoResourceRoot
                        + "/"
                        + stateToken
                        + "/"
                        + RuntimeDirection8Utility.ToResourceToken(direction);

                    Sprite[] frames = Resources.LoadAll<Sprite>(path);
                    if (frames == null || frames.Length == 0)
                    {
                        Debug.LogWarning(
                            "KAI_CHE_2D_8DIR_MAGE_MISSING_"
                            + stateToken.ToUpperInvariant()
                            + "_"
                            + RuntimeDirection8Utility
                                .ToResourceToken(direction)
                                .ToUpperInvariant());
                        _clips.Clear();
                        return false;
                    }

                    Array.Sort(
                        frames,
                        (left, right) => string.CompareOrdinal(
                            left != null ? left.name : string.Empty,
                            right != null ? right.name : string.Empty));
                    directionalFrames[direction] = frames;
                }

                _clips[clip] = directionalFrames;
            }

            SetClip(Clip.Idle, false);
            Debug.Log("KAI_CHE_2D_8DIR_MAGE_ANIMATION_READY_5X8");
            return true;
        }

        private bool TryConfigureSpriteFoundry()
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                RuntimeDirection8 direction = Directions[i];
                string token = ToFoundryToken(direction);
                Texture2D texture = Resources.Load<Texture2D>(
                    FoundryResourceRoot + "/" + token);
                if (texture == null)
                {
                    DestroyFoundrySprites();
                    return false;
                }

                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.08f),
                    Mathf.Max(1f, texture.height),
                    0,
                    SpriteMeshType.FullRect);
                sprite.name = "SpriteFoundryMage_" + token;
                _foundrySprites[direction] = sprite;
            }

            return _foundrySprites.Count == Directions.Length;
        }

        private void Update()
        {
            if (_renderer == null)
                return;

            if (_usesSpriteFoundry)
            {
                UpdateSpriteFoundryPose();
                return;
            }

            Sprite[] frames = GetFrames(_clip, _direction);
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
            else if (_clip == Clip.Death)
            {
                _frameIndex = frames.Length - 1;
            }
            else if (_oneShot)
            {
                _oneShot = false;
                SetClip(
                    _moveInput.sqrMagnitude > MoveDeadZoneSqr
                        ? Clip.Walk
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

        private void UpdateSpriteFoundryPose()
        {
            float deltaTime = Time.deltaTime;
            _poseTime += deltaTime;
            _frameTimer += deltaTime;

            if (_oneShot)
            {
                float duration = GetFoundryOneShotDuration(_clip);
                if (_frameTimer >= duration)
                {
                    _oneShot = false;
                    SetClip(
                        _moveInput.sqrMagnitude > MoveDeadZoneSqr
                            ? Clip.Walk
                            : Clip.Idle,
                        false);
                    return;
                }
            }

            ApplyFoundryPose();
        }

        private void ApplyFoundryPose()
        {
            if (_renderer == null || _spriteTransform == null)
                return;

            Vector3 localPosition = _spriteBasePosition;
            Vector3 localScale = Vector3.one * FoundryVisualScale;
            float rotationZ = 0f;
            Color tint = Color.white;

            if (_clip == Clip.Walk)
            {
                float stride = Mathf.Sin(_poseTime * 10.5f);
                float strideAbs = Mathf.Abs(stride);
                localPosition.x += stride * 0.018f;
                localPosition.y += strideAbs * 0.052f;
                localScale.x *= 1f - strideAbs * 0.025f;
                localScale.y *= 1f + strideAbs * 0.032f;
                rotationZ = stride * 1.8f;
            }
            else if (_clip == Clip.Attack)
            {
                float progress = Mathf.Clamp01(
                    _frameTimer / GetFoundryOneShotDuration(Clip.Attack));
                float pulse = Mathf.Sin(progress * Mathf.PI);
                localPosition.y += pulse * 0.055f;
                localScale.x *= 1f + pulse * 0.10f;
                localScale.y *= 1f - pulse * 0.045f;
                rotationZ = -pulse * 4.5f;
            }
            else if (_clip == Clip.Hit)
            {
                float progress = Mathf.Clamp01(
                    _frameTimer / GetFoundryOneShotDuration(Clip.Hit));
                float pulse = Mathf.Sin(progress * Mathf.PI);
                localPosition.x -= pulse * 0.07f;
                rotationZ = pulse * 6f;
                tint = Color.Lerp(
                    Color.white,
                    new Color(1f, 0.34f, 0.28f, 1f),
                    pulse);
            }
            else if (_clip == Clip.Death)
            {
                rotationZ = 86f;
                localPosition.y -= 0.15f;
                localScale.y *= 0.92f;
                tint = new Color(0.72f, 0.72f, 0.72f, 0.90f);
            }
            else
            {
                float breath = Mathf.Sin(_poseTime * 2.6f) * 0.008f;
                localScale.x *= 1f - breath;
                localScale.y *= 1f + breath;
            }

            _spriteTransform.localPosition = localPosition;
            _spriteTransform.localScale = localScale;
            _spriteTransform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            _renderer.color = tint;
        }

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            transform.rotation = camera.transform.rotation;
        }

        private void OnDestroy()
        {
            DestroyFoundrySprites();
        }

        private void DestroyFoundrySprites()
        {
            foreach (KeyValuePair<RuntimeDirection8, Sprite> pair in _foundrySprites)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
            }

            _foundrySprites.Clear();
        }

        private void SetClip(Clip clip, bool oneShot)
        {
            bool changed = _clip != clip || _oneShot != oneShot;
            _clip = clip;
            _oneShot = oneShot;

            if (changed)
            {
                _frameIndex = 0;
                _frameTimer = 0f;
            }

            ApplyFrame();
            if (_usesSpriteFoundry)
                ApplyFoundryPose();
        }

        private void ApplyFrame()
        {
            if (_renderer == null)
                return;

            if (_usesSpriteFoundry)
            {
                if (_foundrySprites.TryGetValue(_direction, out Sprite sprite))
                    _renderer.sprite = sprite;
                _renderer.flipX = false;
                return;
            }

            Sprite[] frames = GetFrames(_clip, _direction);
            if (frames == null || frames.Length == 0)
                return;

            _frameIndex = Mathf.Clamp(_frameIndex, 0, frames.Length - 1);
            _renderer.sprite = frames[_frameIndex];
            _renderer.flipX = false;
        }

        private Sprite[] GetFrames(
            Clip clip,
            RuntimeDirection8 direction)
        {
            if (!_clips.TryGetValue(
                    clip,
                    out Dictionary<RuntimeDirection8, Sprite[]> directionalFrames))
            {
                return null;
            }

            return directionalFrames.TryGetValue(direction, out Sprite[] frames)
                ? frames
                : null;
        }

        private static string ToFoundryToken(RuntimeDirection8 direction)
        {
            switch (direction)
            {
                case RuntimeDirection8.SouthEast:
                    return "front_right";
                case RuntimeDirection8.East:
                    return "right";
                case RuntimeDirection8.NorthEast:
                    return "back_right";
                case RuntimeDirection8.North:
                    return "back";
                case RuntimeDirection8.NorthWest:
                    return "back_left";
                case RuntimeDirection8.West:
                    return "left";
                case RuntimeDirection8.SouthWest:
                    return "front_left";
                default:
                    return "front";
            }
        }

        private static string GetStateToken(Clip clip)
        {
            switch (clip)
            {
                case Clip.Walk:
                    return "walk";
                case Clip.Attack:
                    return "attack";
                case Clip.Hit:
                    return "hit";
                case Clip.Death:
                    return "death";
                default:
                    return "idle";
            }
        }

        private static float GetFoundryOneShotDuration(Clip clip)
        {
            switch (clip)
            {
                case Clip.Hit:
                    return 0.16f;
                case Clip.Attack:
                    return 0.18f;
                default:
                    return 0.18f;
            }
        }

        private static float GetFrameDuration(Clip clip)
        {
            switch (clip)
            {
                case Clip.Walk:
                    return 0.100f;
                case Clip.Attack:
                    return 0.085f;
                case Clip.Hit:
                    return 0.110f;
                case Clip.Death:
                    return 0.135f;
                default:
                    return 0.160f;
            }
        }
    }
}
