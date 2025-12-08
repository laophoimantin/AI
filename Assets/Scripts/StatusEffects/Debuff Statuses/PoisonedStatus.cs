using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class PoisonedStatus : StatusEffect
    {
        public Agent Victim { get; private set; }
        private float _chipDamage = 3f;
        public PoisonedStatus(Agent user, Agent victim, int duration, float power, Sprite icon) : base(user,  duration, power, icon)
        {
            Victim = victim;
        }

        public override void OnApply()
        {
            Victim.TakeDamage(User, _chipDamage, true);
        }

        public override void OnTurnEnd()
        {
            Victim.TakeDamage(User, Power, true);
            Debug.Log($"{Victim.name} is poisoned! Dealt {Power}");
        }
    }
}