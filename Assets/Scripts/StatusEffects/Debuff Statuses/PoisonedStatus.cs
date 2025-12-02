using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class PoisonedStatus : StatusEffect
    {
        private float _chipDamage = 3f;
        public PoisonedStatus(int duration, float power, Sprite icon) : base(duration, power, icon)
        {
        }

        public override void OnApply(Agent target)
        {
            target.TakeDamage(_chipDamage, true);
        }

        public override void OnTurnEnd(Agent target)
        {
            target.TakeDamage(Power, true);
            Debug.Log($"{target.name} is poisoned! Dealt {Power}");
        }

    }
}