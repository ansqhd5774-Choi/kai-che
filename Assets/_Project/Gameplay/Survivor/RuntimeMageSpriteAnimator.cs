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

        private const string ResourceRoot = "Characters/PicoMage";
        private const float MoveDeadZoneSqr = 0.01f;

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

        private SpriteRenderer _renderer;
        private Clip _clip = Clip.Idle;
        private RuntimeDirection8 _direction = RuntimeDirection8.South;
        private Vector2 _moveInput;
        private int _frameIndex;
        private float _frameTimer;
        private bool _oneShot;
        private bool _dead;

        public RuntimeDirection8 Direction => _direction;

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
            _clips.Clear();

            foreach (Clip clip in Enum.GetValues(typeof(Clip)))
            {
                string stateToken = GetStateToken(clip);
                var directionalFrames =
                    new Dictionary<RuntimeDirection8, Sprite[]>();

                for (int i = 0; i < Directions.Length; i++)
                {
                    RuntimeDirection8 direction = Directions[i];
                    string path = ResourceRoot
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

        private void Update()
        {
            if (_renderer == null)
                return;

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

        private void LateUpdate()
        {
            Camera camera = Camera.main;
            if (camera == null)
                return;

            // Keep the sprite plane parallel to the active camera plane.
            // This works for the previous angled camera and for the new
            // orthographic top-down shooter camera without mirroring frames.
            transform.rotation = camera.transform.rotation;
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
        }

        private void ApplyFrame()
        {
            Sprite[] frames = GetFrames(_clip, _direction);
            if (_renderer == null || frames == null || frames.Length == 0)
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
