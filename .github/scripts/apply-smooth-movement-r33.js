const fs = require('fs');

function load(path) {
  return fs.readFileSync(path, 'utf8');
}

function save(path, text) {
  fs.writeFileSync(path, text, 'utf8');
}

function replaceOnce(text, oldText, newText, label) {
  const first = text.indexOf(oldText);
  if (first < 0) throw new Error(`[R33] missing patch target: ${label}`);
  if (text.indexOf(oldText, first + oldText.length) >= 0)
    throw new Error(`[R33] patch target is not unique: ${label}`);
  return text.slice(0, first) + newText + text.slice(first + oldText.length);
}

// 1) Touch joystick: dead-zone remap + short acceleration/deceleration smoothing.
{
  const path = 'Assets/_Project/Presentation/SurvivorHudController.cs';
  let text = load(path);
  text = replaceOnce(
    text,
    '        private const string BuildLabel = "R32";\n',
    '        private const string BuildLabel = "R33";\n'
      + '        private const float JoystickDeadZone = 0.14f;\n'
      + '        private const float JoystickAcceleration = 12f;\n'
      + '        private const float JoystickDeceleration = 18f;\n',
    'SurvivorHudController constants');

  text = replaceOnce(
    text,
`            Vector2 input = ReadTouchInput();
            if (_activeFingerId < 0 && Input.touchCount == 0)
                input = ReadFallbackInput();

            _joystickInput = input;
            _arena.SetMoveInput(input);`,
`            bool hadTouch = _activeFingerId >= 0 || Input.touchCount > 0;
            Vector2 input = ReadTouchInput();

            if (!hadTouch && _activeFingerId < 0 && Input.touchCount == 0)
            {
                _joystickInput = ReadFallbackInput();
            }
            else
            {
                Vector2 targetInput = ApplyJoystickDeadZone(input);
                float response =
                    targetInput.sqrMagnitude > _joystickInput.sqrMagnitude
                        ? JoystickAcceleration
                        : JoystickDeceleration;

                _joystickInput = Vector2.MoveTowards(
                    _joystickInput,
                    targetInput,
                    response * Time.unscaledDeltaTime);

                if (_joystickInput.sqrMagnitude < 0.0004f)
                    _joystickInput = Vector2.zero;
            }

            _arena.SetMoveInput(_joystickInput);`,
    'SurvivorHudController Update input');

  text = replaceOnce(
    text,
`        private Vector2 ReadTouchInput()
        {`,
`        private static Vector2 ApplyJoystickDeadZone(Vector2 input)
        {
            float magnitude = Mathf.Clamp01(input.magnitude);
            if (magnitude <= JoystickDeadZone)
                return Vector2.zero;

            float remapped =
                (magnitude - JoystickDeadZone) / (1f - JoystickDeadZone);
            return input.normalized * remapped;
        }

        private Vector2 ReadTouchInput()
        {`,
    'SurvivorHudController dead zone helper');
  save(path, text);
}

// 2) 8-direction sprite selection: keep the current octant until the stick moves
// far enough across a boundary. This prevents rapid sprite flipping from finger noise.
{
  const path = 'Assets/_Project/Gameplay/Survivor/RuntimeDirection8.cs';
  let text = load(path);
  text = replaceOnce(
    text,
    '        private const float DeadZoneSqr = 0.0001f;\n',
    '        private const float DeadZoneSqr = 0.0001f;\n'
      + '        private const float SectorHalfAngle = 22.5f;\n'
      + '        private const float DirectionHysteresis = 6f;\n',
    'RuntimeDirection8 constants');

  text = replaceOnce(
    text,
`            float angle = Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            int octant = Mathf.RoundToInt(angle / 45f) & 7;`,
`            float angle = Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            float fallbackAngle = ToAngle(fallback);
            if (Mathf.Abs(Mathf.DeltaAngle(fallbackAngle, angle))
                <= SectorHalfAngle + DirectionHysteresis)
            {
                return fallback;
            }

            int octant = Mathf.RoundToInt(angle / 45f) & 7;`,
    'RuntimeDirection8 hysteresis');

  text = replaceOnce(
    text,
`        public static string ToResourceToken(RuntimeDirection8 direction)
        {`,
`        private static float ToAngle(RuntimeDirection8 direction)
        {
            switch (direction)
            {
                case RuntimeDirection8.SouthEast:
                    return 315f;
                case RuntimeDirection8.East:
                    return 0f;
                case RuntimeDirection8.NorthEast:
                    return 45f;
                case RuntimeDirection8.North:
                    return 90f;
                case RuntimeDirection8.NorthWest:
                    return 135f;
                case RuntimeDirection8.West:
                    return 180f;
                case RuntimeDirection8.SouthWest:
                    return 225f;
                default:
                    return 270f;
            }
        }

        public static string ToResourceToken(RuntimeDirection8 direction)
        {`,
    'RuntimeDirection8 angle helper');
  save(path, text);
}

// 3) Sprite Foundry pose: stop reassigning the same sprite every input frame and
// reduce exaggerated lateral sway / bob / rotation while retaining a subtle walk cue.
{
  const path = 'Assets/_Project/Gameplay/Survivor/RuntimeMageSpriteAnimator.cs';
  let text = load(path);
  text = replaceOnce(
    text,
`        public void SetMovement(Vector2 input)
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
        }`,
`        public void SetMovement(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
            bool moving = _moveInput.sqrMagnitude > MoveDeadZoneSqr;

            if (moving)
            {
                RuntimeDirection8 nextDirection = RuntimeDirection8Utility.FromVector(
                    _moveInput,
                    _direction);
                if (nextDirection != _direction)
                {
                    _direction = nextDirection;
                    ApplyFrame();
                }
            }

            if (_dead || _oneShot)
                return;

            Clip targetClip = moving ? Clip.Walk : Clip.Idle;
            if (_clip != targetClip)
                SetClip(targetClip, false);
        }`,
    'RuntimeMageSpriteAnimator SetMovement');

  text = replaceOnce(
    text,
`            if (_clip == Clip.Walk)
            {
                float stride = Mathf.Sin(_poseTime * 10.5f);
                float strideAbs = Mathf.Abs(stride);
                localPosition.x += stride * 0.018f;
                localPosition.y += strideAbs * 0.052f;
                localScale.x *= 1f - strideAbs * 0.025f;
                localScale.y *= 1f + strideAbs * 0.032f;
                rotationZ = stride * 1.8f;
            }`,
`            if (_clip == Clip.Walk)
            {
                float stride = Mathf.Sin(_poseTime * 8.5f);
                float strideAbs = Mathf.Abs(stride);
                localPosition.y += strideAbs * 0.018f;
                localScale.x *= 1f - strideAbs * 0.008f;
                localScale.y *= 1f + strideAbs * 0.010f;
                rotationZ = stride * 0.35f;
            }`,
    'RuntimeMageSpriteAnimator smooth walk pose');
  save(path, text);
}

// Visible build marker for phone-side confirmation.
for (const path of [
  'Assets/_Project/Presentation/UguiSurvivorHudController.cs',
  'Assets/_Project/Presentation/KaiCheRuntimeFlow.cs'
]) {
  let text = load(path);
  text = replaceOnce(
    text,
    'private const string BuildLabel = "R32";',
    'private const string BuildLabel = "R33";',
    `${path} build label`);
  save(path, text);
}

console.log('[PASS] R33 smooth movement patches applied.');
