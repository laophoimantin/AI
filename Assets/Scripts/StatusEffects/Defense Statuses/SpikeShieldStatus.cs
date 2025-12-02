using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class SpikeShieldStatus : BaseShieldStatus
    {
        public SpikeShieldStatus(int duration, float power, Sprite icon, float reductionPercent, float durability) : base(duration, power, icon, reductionPercent, durability)
        {
        }

        public override float AbsorbDamage(float incomingDamage, Agent target)
        {
            target.TakeDamage(Power, true);
            return base.AbsorbDamage(incomingDamage, target);
        }
    }
}