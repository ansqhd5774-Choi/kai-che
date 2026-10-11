using System;
using UnityHFSM;

namespace KaiChe.Gameplay.Survivor
{
    /// <summary>
    /// UnityHFSM-backed boss state foundation. It deliberately stays independent from
    /// SurvivorArenaController so boss behavior can migrate incrementally.
    /// </summary>
    internal sealed class RuntimeBossStateMachine
    {
        public const string Idle = "Idle";
        public const string Chase = "Chase";
        public const string Telegraph = "Telegraph";
        public const string Attack = "Attack";
        public const string Cooldown = "Cooldown";
        public const string Phase2 = "Phase2";
        public const string Dead = "Dead";

        private readonly StateMachine<string, string, string> _fsm;

        public RuntimeBossStateMachine(
            Action chaseLogic = null,
            Action telegraphEnter = null,
            Action attackEnter = null,
            Action phase2Enter = null,
            Action deadEnter = null)
        {
            _fsm = new StateMachine<string, string, string>();

            _fsm.AddState(Idle);
            _fsm.AddState(Chase, onLogic: _ => chaseLogic?.Invoke());
            _fsm.AddState(Telegraph, onEnter: _ => telegraphEnter?.Invoke());
            _fsm.AddState(Attack, onEnter: _ => attackEnter?.Invoke());
            _fsm.AddState(Cooldown);
            _fsm.AddState(Phase2, onEnter: _ => phase2Enter?.Invoke(), onLogic: _ => chaseLogic?.Invoke());
            _fsm.AddState(Dead, onEnter: _ => deadEnter?.Invoke());

            _fsm.SetStartState(Idle);

            _fsm.AddTriggerTransition("Engage", Idle, Chase);
            _fsm.AddTriggerTransition("Telegraph", Chase, Telegraph);
            _fsm.AddTriggerTransition("Attack", Telegraph, Attack);
            _fsm.AddTriggerTransition("Cooldown", Attack, Cooldown);
            _fsm.AddTriggerTransition("Resume", Cooldown, Chase);
            _fsm.AddTriggerTransition("Phase2", Chase, Phase2);
            _fsm.AddTriggerTransition("Telegraph", Phase2, Telegraph);
            _fsm.AddTriggerTransitionFromAny("Die", Dead, forceInstantly: true);

            _fsm.Init();
        }

        public string ActiveState => _fsm.ActiveStateName;

        public void Tick()
        {
            _fsm.OnLogic();
        }

        public void Trigger(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            _fsm.Trigger(eventName);
        }
    }
}
