using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class ExplosiveShieldStatus : BaseShieldStatus
    {
        private Agent _victim;
        public ExplosiveShieldStatus(Agent user,Agent victim, int duration, float power, Sprite icon, float reductionPercent, float durability) : base(user, duration, power, icon, reductionPercent, durability)
        {
            _victim = victim;
        }

        public override void OnExpire()
        {
            _victim.TakeDamage(User, Power, true);
        }
        
    }
}