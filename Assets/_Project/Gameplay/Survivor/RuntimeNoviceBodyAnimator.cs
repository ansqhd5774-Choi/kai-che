using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class RuntimeNoviceBodyAnimator : MonoBehaviour
    {
        private Transform _modelRoot;
        private Transform _pelvis;
        private Transform _chest;
        private Transform _upperArmLeft;
        private Transform _upperArmRight;
        private Transform _forearmLeft;
        private Transform _forearmRight;
        private Transform _thighLeft;
        private Transform _thighRight;
        private Transform _shinLeft;
        private Transform _shinRight;

        private Quaternion _pelvisBaseRotation;
        private Quaternion _chestBaseRotation;
        private Quaternion _upperArmLeftBaseRotation;
        private Quaternion _upperArmRightBaseRotation;
        private Quaternion _forearmLeftBaseRotation;
        private Quaternion _forearmRightBaseRotation;
        private Quaternion _thighLeftBaseRotation;
        private Quaternion _thighRightBaseRotation;
        private Quaternion _shinLeftBaseRotation;
        private Quaternion _shinRightBaseRotation;
        private Vector3 _modelBasePosition;
        private float _walkCycle;
        private bool _facingLogged;

        public void Configure(Transform modelRoot)
        {
            _modelRoot = modelRoot;
            if (_modelRoot == null)
                return;

            _pelvis = _modelRoot.Find("PlayerPelvis3D");
            _chest = _modelRoot.Find("PlayerChest3D");
            _upperArmLeft = _modelRoot.Find("PlayerUpperArmLeft3D");
            _upperArmRight = _modelRoot.Find("PlayerUpperArmRight3D");
            _forearmLeft = _modelRoot.Find("PlayerForearmLeft3D");
            _forearmRight = _modelRoot.Find("PlayerForearmRight3D");
            _thighLeft = _modelRoot.Find("PlayerThighLeft3D");
            _thighRight = _modelRoot.Find("PlayerThighRight3D");
            _shinLeft = _modelRoot.Find("PlayerShinLeft3D");
            _shinRight = _modelRoot.Find("PlayerShinRight3D");

            _modelBasePosition = _modelRoot.localPosition;
            _pelvisBaseRotation = RotationOf(_pelvis);
            _chestBaseRotation = RotationOf(_chest);
            _upperArmLeftBaseRotation = RotationOf(_upperArmLeft);
            _upperArmRightBaseRotation = RotationOf(_upperArmRight);
            _forearmLeftBaseRotation = RotationOf(_forearmLeft);
            _forearmRightBaseRotation = RotationOf(_forearmRight);
            _thighLeftBaseRotation = RotationOf(_thighLeft);
            _thighRightBaseRotation = RotationOf(_thighRight);
            _shinLeftBaseRotation = RotationOf(_shinLeft);
            _shinRightBaseRotation = RotationOf(_shinRight);

            bool legsReady =
                _thighLeft != null
                && _thighRight != null
                && _shinLeft != null
                && _shinRight != null;

            Debug.Log("KAI_CHE_R30_PROCEDURAL_WALK_READY");
            if (legsReady)
                Debug.Log("KAI_CHE_R30_LEG_RIG_READY");
        }

        public void SetMovement(Vector2 input, float deltaTime)
        {
            float speed = Mathf.Clamp01(input.magnitude);
            bool moving = speed > 0.01f;

            if (moving)
            {
                Vector3 worldDirection =
                    new Vector3(input.x, 0f, input.y).normalized;
                Quaternion desired =
                    Quaternion.LookRotation(-worldDirection, Vector3.up);
                float turnBlend =
                    1f - Mathf.Exp(-12f * Mathf.Max(0f, deltaTime));
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, desired, turnBlend);

                if (!_facingLogged)
                {
                    _facingLogged = true;
                    Debug.Log("KAI_CHE_R30_FACING_ROTATION_ACTIVE");
                }

                _walkCycle +=
                    Mathf.Max(0f, deltaTime)
                    * Mathf.Lerp(5.2f, 8.0f, speed);
            }

            float phase = moving ? Mathf.Sin(_walkCycle) : 0f;
            float lift = moving ? Mathf.Abs(Mathf.Cos(_walkCycle)) : 0f;
            float stride = phase * 28f * speed;
            float armSwing = -stride * 0.68f;
            float kneeLeft = Mathf.Max(0f, -phase) * 20f * speed;
            float kneeRight = Mathf.Max(0f, phase) * 20f * speed;
            float settle = 1f - Mathf.Exp(-14f * Mathf.Max(0f, deltaTime));

            SetLocalRotation(
                _thighLeft,
                _thighLeftBaseRotation * Quaternion.Euler(stride, 0f, 0f),
                settle);
            SetLocalRotation(
                _thighRight,
                _thighRightBaseRotation * Quaternion.Euler(-stride, 0f, 0f),
                settle);
            SetLocalRotation(
                _shinLeft,
                _shinLeftBaseRotation * Quaternion.Euler(-kneeLeft, 0f, 0f),
                settle);
            SetLocalRotation(
                _shinRight,
                _shinRightBaseRotation * Quaternion.Euler(-kneeRight, 0f, 0f),
                settle);

            SetLocalRotation(
                _upperArmLeft,
                _upperArmLeftBaseRotation * Quaternion.Euler(armSwing, 0f, 0f),
                settle);
            SetLocalRotation(
                _upperArmRight,
                _upperArmRightBaseRotation * Quaternion.Euler(-armSwing, 0f, 0f),
                settle);
            SetLocalRotation(
                _forearmLeft,
                _forearmLeftBaseRotation * Quaternion.Euler(5f + kneeLeft * 0.45f, 0f, 0f),
                settle);
            SetLocalRotation(
                _forearmRight,
                _forearmRightBaseRotation * Quaternion.Euler(5f + kneeRight * 0.45f, 0f, 0f),
                settle);

            SetLocalRotation(
                _pelvis,
                _pelvisBaseRotation * Quaternion.Euler(0f, 0f, -phase * 2.2f * speed),
                settle);
            SetLocalRotation(
                _chest,
                _chestBaseRotation * Quaternion.Euler(0f, 0f, phase * 1.4f * speed),
                settle);

            if (_modelRoot != null)
            {
                Vector3 targetPosition =
                    _modelBasePosition
                    + Vector3.up * (lift * 0.035f * speed);
                _modelRoot.localPosition =
                    Vector3.Lerp(
                        _modelRoot.localPosition,
                        targetPosition,
                        settle);
            }
        }

        private static Quaternion RotationOf(Transform target)
        {
            return target != null
                ? target.localRotation
                : Quaternion.identity;
        }

        private static void SetLocalRotation(
            Transform target,
            Quaternion rotation,
            float blend)
        {
            if (target == null)
                return;

            target.localRotation =
                Quaternion.Slerp(
                    target.localRotation,
                    rotation,
                    blend);
        }
    }
}
