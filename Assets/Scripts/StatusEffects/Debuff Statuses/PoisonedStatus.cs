using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    /// <summary>
    /// Poisoned Status. Deals damage to the target every turn.
    /// </summary>
    public class PoisonedStatus : StatusEffect
    {
        private Agent _victim;
        public PoisonedStatus(Agent user, Agent victim, int duration, float power, Sprite icon) : base(user,  duration, power, icon)
        {
            _victim = victim;
        }

        // Deal chip damage on applying the status
        public override void OnApply()
        {
            _victim.TakeDamage(User, Power * 0.3f, true);
        }

        // Deal damage on the end of turn
        public override void OnTurnEnd()
        {
            _victim.TakeDamage(User, Power, true);
            Debug.Log($"{_victim.name} is poisoned! Dealt {Power}");
        }
    }
}