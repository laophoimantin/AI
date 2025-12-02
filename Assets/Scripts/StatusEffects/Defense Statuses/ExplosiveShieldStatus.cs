using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class ExplosiveShieldStatus : BaseShieldStatus
    {
        public ExplosiveShieldStatus(int duration, float power, Sprite icon, float reductionPercent, float durability) : base(duration, power, icon, reductionPercent, durability)
        {
        }

        public override void OnExpire(Agent target)
        {
            target.TakeDamage(Power, true);
        }
        
    }
}