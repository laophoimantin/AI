using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    public class StandardShieldStatus : BaseShieldStatus
    {
        public StandardShieldStatus(Agent user,  int duration, float power, Sprite icon, float reductionPercent, float durability) : base(user, duration, power, icon, reductionPercent, durability)
        {
            
        }
    }
}