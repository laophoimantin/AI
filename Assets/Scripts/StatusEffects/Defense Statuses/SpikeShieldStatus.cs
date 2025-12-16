using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    /// <summary>
    /// Absorbs incoming damage and reflects flat damage back
    /// </summary>
    public class SpikeShieldStatus : BaseShieldStatus
    {
        public SpikeShieldStatus(Agent user, int duration, float power, Sprite icon, float reductionPercent, float durability) : base(user,  duration, power, icon, reductionPercent, durability)
        {
        }

        public override float AbsorbDamage(Agent attacker, float incomingDamage)
        {
            attacker.TakeDamage(User, Power, true);
            return base.AbsorbDamage(attacker, incomingDamage);
        }
    }
}