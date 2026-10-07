using System.Collections.Generic;
using UnityEngine;

namespace KaiChe.Prototypes.BumperSeed
{
    public sealed class BumperSeedPrototype : MonoBehaviour
    {
        private const float ArenaHalfWidth = 3.15f;
        private const float ArenaHalfHeight = 7.0f;
        private const float WallThickness = 0.35f;
        private const float BallRadius = 0.28f;
        private const float BumperRadius = 0.52f;
        private const float BallSpeed = 7.0f;
        private const float TapPreBufferSeconds = 0.10f;
        private const float TapAfterCollisionSeconds = 0.18f;
        private const int MaxBumpers = 3;

        private readonly Queue<GameObject> _bumpers = new Queue<GameObject>();

        private readonly Vector2[] _targetPattern =
        {
            new Vector2(2.45f, 4.6f),
            new Vector2(-2.55f, 3.0f),
            new Vector2(1.85f, 1.1f),
            new Vector2(-2.35f, -1.8f),
            new Vector2(2.45f, -4.2f),
            new Vector2(0.0f, 5.35f),
            new Vector2(-2.7f, 0.2f),
            new Vector2(2.65f, -0.6f),
            new Vector2(-1.4f, -4.8f),
            new Vector2(1.2f, 3.9f)
        };

        private Rigidbody2D _ball;
        private GameObject _target;
        private PhysicsMaterial2D _bounceMaterial;
        private Camera _camera;
        private BumperSeedR20Feedback _feedback;

        private Material _wallMaterial;
        private Material _ballMaterial;
        private Material _bumperMaterial;
        private Material _oldestBumperMaterial;
        private Material _targetMaterial;

        private Vector2 _pendingImpactPoint;
        private float _pendingImpactExpiresAt = -1f;
        private float _lastTapAt = -100f;
        private bool _pendingImpactConsumed = true;
        private int _score;
        private int _combo;
        private int _targetIndex;
        private int _createdBumpers;
        private string _status = "TAP ON IMPACT";
        private float _statusExpiresAt;

        private GUIStyle _titleStyle;
        private GUIStyle _hudStyle;
        private GUIStyle _smallStyle;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Physics2D.gravity = Vector2.zero;

            CreateMaterials();
            CreateCamera();
            CreateFeedback();
            CreateArena();
            CreateBall();
            CreateTarget();

            Debug.Log("BUMPER_SEED_R19_CORE_ACTIVE");
            Debug.Log("BUMPER_SEED_R20_FEEDBACK_ACTIVE");
        }

        private void Update()
        {
            if (ReadTap())
            {
                _lastTapAt = Time.unscaledTime;
                if (!_pendingImpactConsumed &&
                    Time.unscaledTime <= _pendingImpactExpiresAt)
                {
                    SpawnBumperFromPendingImpact();
                }
                else
                {
                    SetStatus("WAIT FOR IMPACT", 0.35f);
                }
            }

            if (_statusExpiresAt > 0f &&
                Time.unscaledTime > _statusExpiresAt)
            {
                _status = "TAP ON IMPACT";
                _statusExpiresAt = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (_ball == null)
                return;

            Vector2 velocity = _ball.linearVelocity;
            if (velocity.sqrMagnitude < 0.25f)
            {
                _ball.linearVelocity = new Vector2(0.73f, 0.68f).normalized * BallSpeed;
                return;
            }

            _ball.linearVelocity = velocity.normalized * BallSpeed;
        }

        internal void RegisterWallCollision(Collision2D collision)
        {
            if (collision.contactCount <= 0)
                return;

            ContactPoint2D contact = collision.GetContact(0);
            _pendingImpactPoint = contact.point;
            _pendingImpactExpiresAt = Time.unscaledTime + TapAfterCollisionSeconds;
            _pendingImpactConsumed = false;
            _feedback?.PlayWallImpact(contact.point);

            if (Time.unscaledTime - _lastTapAt <= TapPreBufferSeconds)
                SpawnBumperFromPendingImpact();
        }

        internal void RegisterTargetHit(Collider2D targetCollider)
        {
            if (_target == null || targetCollider.gameObject != _target)
                return;

            _combo += 1;
            int reward = 100 + ((_combo - 1) * 25);
            _score += reward;
            _targetIndex = (_targetIndex + 1) % _targetPattern.Length;
            _target.transform.position = ToWorld(_targetPattern[_targetIndex]);
            SetStatus("TARGET +" + reward, 0.55f);
            _feedback?.PlayTargetSuccess(_target.transform);
            Debug.Log("BUMPER_SEED_R20_TARGET_HIT_SCORE_" + _score);
        }

        private void SpawnBumperFromPendingImpact()
        {
            if (_pendingImpactConsumed)
                return;

            _pendingImpactConsumed = true;

            Vector2 towardCenter = -_pendingImpactPoint;
            if (towardCenter.sqrMagnitude < 0.001f)
                towardCenter = Vector2.up;
            towardCenter.Normalize();

            Vector2 spawn = _pendingImpactPoint + towardCenter * (BumperRadius + 0.18f);
            spawn.x = Mathf.Clamp(
                spawn.x,
                -ArenaHalfWidth + BumperRadius + 0.25f,
                ArenaHalfWidth - BumperRadius - 0.25f);
            spawn.y = Mathf.Clamp(
                spawn.y,
                -ArenaHalfHeight + BumperRadius + 0.25f,
                ArenaHalfHeight - BumperRadius - 0.25f);

            GameObject bumper = CreateDisc(
                "Bumper_" + (++_createdBumpers),
                spawn,
                BumperRadius * 2f,
                _bumperMaterial);

            CircleCollider2D collider = bumper.AddComponent<CircleCollider2D>();
            collider.radius = BumperRadius;
            collider.sharedMaterial = _bounceMaterial;

            _bumpers.Enqueue(bumper);
            if (_bumpers.Count > MaxBumpers)
            {
                GameObject oldest = _bumpers.Dequeue();
                if (oldest != null)
                    Destroy(oldest);
            }

            RefreshOldestBumperVisual();
            SetStatus("BUMPER " + _bumpers.Count + "/3", 0.45f);
            _feedback?.PlayBumperCreated(bumper.transform);
            Debug.Log("BUMPER_SEED_R20_BUMPER_CREATED_COUNT_" + _createdBumpers);
        }

        private void RefreshOldestBumperVisual()
        {
            foreach (GameObject bumper in _bumpers)
            {
                if (bumper == null)
                    continue;

                Renderer renderer = bumper.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = _bumperMaterial;
            }

            if (_bumpers.Count == 0)
                return;

            GameObject oldest = _bumpers.Peek();
            if (oldest == null)
                return;

            Renderer oldestRenderer = oldest.GetComponent<Renderer>();
            if (oldestRenderer != null)
                oldestRenderer.sharedMaterial = _oldestBumperMaterial;
        }

        private void CreateCamera()
        {
            GameObject cameraObject = new GameObject("BumperSeedCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            _camera = camera;
            camera.orthographic = true;
            camera.orthographicSize = 8.1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.03f, 0.05f);
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;
        }

        private void CreateFeedback()
        {
            _feedback = gameObject.AddComponent<BumperSeedR20Feedback>();
            _feedback.Initialize(_camera);
        }

        private void CreateArena()
        {
            CreateWall(
                "Wall_Left",
                new Vector2(-ArenaHalfWidth - (WallThickness * 0.5f), 0f),
                new Vector2(WallThickness, ArenaHalfHeight * 2f + WallThickness * 2f));
            CreateWall(
                "Wall_Right",
                new Vector2(ArenaHalfWidth + (WallThickness * 0.5f), 0f),
                new Vector2(WallThickness, ArenaHalfHeight * 2f + WallThickness * 2f));
            CreateWall(
                "Wall_Top",
                new Vector2(0f, ArenaHalfHeight + (WallThickness * 0.5f)),
                new Vector2(ArenaHalfWidth * 2f, WallThickness));
            CreateWall(
                "Wall_Bottom",
                new Vector2(0f, -ArenaHalfHeight - (WallThickness * 0.5f)),
                new Vector2(ArenaHalfWidth * 2f, WallThickness));
        }

        private void CreateWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = ToWorld(position);

            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = _bounceMaterial;
            wall.AddComponent<BumperSeedWall>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(wall.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(size.x, size.y, 0.25f);

            Collider threeDimensionalCollider = visual.GetComponent<Collider>();
            if (threeDimensionalCollider != null)
                threeDimensionalCollider.enabled = false;

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = _wallMaterial;
        }

        private void CreateBall()
        {
            GameObject ball = CreateDisc(
                "Ball",
                Vector2.zero,
                BallRadius * 2f,
                _ballMaterial);

            CircleCollider2D collider = ball.AddComponent<CircleCollider2D>();
            collider.radius = BallRadius;
            collider.sharedMaterial = _bounceMaterial;

            _ball = ball.AddComponent<Rigidbody2D>();
            _ball.gravityScale = 0f;
            _ball.linearDamping = 0f;
            _ball.angularDamping = 0f;
            _ball.constraints = RigidbodyConstraints2D.FreezeRotation;
            _ball.interpolation = RigidbodyInterpolation2D.Interpolate;
            _ball.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _ball.linearVelocity = new Vector2(0.73f, 0.68f).normalized * BallSpeed;

            BumperSeedBallSensor sensor = ball.AddComponent<BumperSeedBallSensor>();
            sensor.Owner = this;
        }

        private void CreateTarget()
        {
            _target = CreateDisc(
                "Target",
                _targetPattern[0],
                0.76f,
                _targetMaterial);

            CircleCollider2D collider = _target.AddComponent<CircleCollider2D>();
            collider.radius = 0.38f;
            collider.isTrigger = true;
        }

        private GameObject CreateDisc(
            string name,
            Vector2 position,
            float diameter,
            Material material)
        {
            GameObject instance = new GameObject(name);
            instance.transform.position = ToWorld(position);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(instance.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(diameter, diameter, 0.28f);

            Collider threeDimensionalCollider = visual.GetComponent<Collider>();
            if (threeDimensionalCollider != null)
                threeDimensionalCollider.enabled = false;

            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            return instance;
        }

        private void CreateMaterials()
        {
            _bounceMaterial = new PhysicsMaterial2D("BumperSeedBounce")
            {
                bounciness = 1f,
                friction = 0f
            };

            _wallMaterial = CreateMaterial(new Color(0.18f, 0.22f, 0.30f));
            _ballMaterial = CreateMaterial(new Color(0.95f, 0.96f, 1f));
            _bumperMaterial = CreateMaterial(new Color(0.20f, 0.72f, 0.95f));
            _oldestBumperMaterial = CreateMaterial(new Color(1.0f, 0.72f, 0.18f));
            _targetMaterial = CreateMaterial(new Color(0.35f, 1.0f, 0.48f));
        }

        private static Material CreateMaterial(Color color)
        {
            Material template = Resources.Load<Material>("KaiCheRuntimeLit");
            Material material;

            if (template != null)
            {
                material = new Material(template);
            }
            else
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Unlit/Color");
                if (shader == null)
                    shader = Shader.Find("Standard");
                if (shader == null)
                    throw new System.InvalidOperationException("No compatible shader found.");

                material = new Material(shader);
            }

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            else
                material.color = color;

            return material;
        }

        private static bool ReadTap()
        {
            if (Input.touchCount > 0 &&
                Input.GetTouch(0).phase == TouchPhase.Began)
            {
                return true;
            }

            return Input.GetMouseButtonDown(0);
        }

        private void SetStatus(string value, float duration)
        {
            _status = value;
            _statusExpiresAt = Time.unscaledTime + duration;
        }

        private void OnGUI()
        {
            EnsureGuiStyles();

            float margin = Mathf.Max(18f, Screen.width * 0.035f);
            GUI.Label(
                new Rect(margin, margin, Screen.width - margin * 2f, 70f),
                "BUMPER SEED  •  R20",
                _titleStyle);

            GUI.Label(
                new Rect(margin, margin + 72f, Screen.width - margin * 2f, 60f),
                "SCORE " + _score + "    COMBO x" + Mathf.Max(1, _combo),
                _hudStyle);

            GUI.Label(
                new Rect(margin, Screen.height - 150f, Screen.width - margin * 2f, 54f),
                _status,
                _hudStyle);

            GUI.Label(
                new Rect(margin, Screen.height - 92f, Screen.width - margin * 2f, 54f),
                "Impact 때 탭  •  주황 범퍼 = 다음 삭제",
                _smallStyle);
        }

        private void EnsureGuiStyles()
        {
            if (_titleStyle != null)
                return;

            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width / 27f, 26f, 48f));

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize + 10,
                fontStyle = FontStyle.Bold
            };
            _titleStyle.normal.textColor = Color.white;

            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = baseSize,
                fontStyle = FontStyle.Bold
            };
            _hudStyle.normal.textColor = Color.white;

            _smallStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Mathf.Max(20, baseSize - 7),
                fontStyle = FontStyle.Normal
            };
            _smallStyle.normal.textColor = new Color(0.82f, 0.86f, 0.92f);
        }

        private static Vector3 ToWorld(Vector2 point)
        {
            return new Vector3(point.x, point.y, 0f);
        }
    }

    internal sealed class BumperSeedWall : MonoBehaviour
    {
    }

    internal sealed class BumperSeedBallSensor : MonoBehaviour
    {
        public BumperSeedPrototype Owner { get; set; }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (Owner == null)
                return;

            if (collision.gameObject.GetComponent<BumperSeedWall>() != null)
                Owner.RegisterWallCollision(collision);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Owner != null)
                Owner.RegisterTargetHit(other);
        }
    }
}
