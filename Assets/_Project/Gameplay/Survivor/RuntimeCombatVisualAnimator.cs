using UnityEngine;

namespace KaiChe.Gameplay.Survivor
{
    public sealed class RuntimeCombatVisualAnimator : MonoBehaviour
    {
        private Animator _animator;
        private int _speedHash;
        private int _attackHash;
        private int _hitHash;
        private int _deadHash;
        private bool _hasSpeed;
        private bool _hasAttack;
        private bool _hasHit;
        private bool _hasDead;

        public bool IsBound => _animator != null;
        public bool SupportsAttack => _animator != null && _hasAttack;

        public void Bind(GameObject model)
        {
            _animator = model != null
                ? model.GetComponentInChildren<Animator>(true)
                : null;

            _hasSpeed = false;
            _hasAttack = false;
            _hasHit = false;
            _hasDead = false;

            if (_animator == null)
                return;

            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                string name = parameter.name;

                if ((name == "Speed" || name == "MoveSpeed")
                    && parameter.type == AnimatorControllerParameterType.Float)
                {
                    _speedHash = parameter.nameHash;
                    _hasSpeed = true;
                }
                else if (name == "Attack"
                    && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _attackHash = parameter.nameHash;
                    _hasAttack = true;
                }
                else if (name == "Hit"
                    && parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    _hitHash = parameter.nameHash;
                    _hasHit = true;
                }
                else if (name == "Dead"
                    && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _deadHash = parameter.nameHash;
                    _hasDead = true;
                }
            }

            Debug.Log("KAI_CHE_R16_ANIMATOR_BOUND");
        }

        public void SetMove(float normalizedSpeed)
        {
            if (_animator != null && _hasSpeed)
                _animator.SetFloat(_speedHash, Mathf.Clamp01(normalizedSpeed));
        }

        public void TriggerAttack()
        {
            if (_animator != null && _hasAttack)
                _animator.SetTrigger(_attackHash);
        }

        public void TriggerHit()
        {
            if (_animator != null && _hasHit)
                _animator.SetTrigger(_hitHash);
        }

        public void SetDead(bool dead)
        {
            if (_animator != null && _hasDead)
                _animator.SetBool(_deadHash, dead);
        }
    }
}
