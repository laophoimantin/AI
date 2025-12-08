using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    // Acting more like a Damage Damper than a shield
    public abstract class BaseShieldStatus : StatusEffect
    {
        public float ReductionPercent { get; protected set; }
        public float Durability { get; protected set; }
        public float CurrentShieldDurability { get; protected set; }
        public float DurabilityPercent => CurrentShieldDurability / Durability;

        protected BaseShieldStatus(Agent user, int duration, float power, Sprite icon, float reductionPercent, float durability) :
            base(user, duration, power, icon)
        {
            ReductionPercent = reductionPercent;
            Durability = durability;
            CurrentShieldDurability = Durability;
        }

        public override void Refresh(int duration, float power)
        {
            base.Refresh(duration, power);
            CurrentShieldDurability = Durability;
        }

        public virtual float ModifyDamage(float damage)
        {
            return damage * (1f - ReductionPercent);
        }

        public virtual float AbsorbDamage(Agent attacker, float incomingDamage)
        {
            float potentialBlock = incomingDamage * ReductionPercent;
            float actualBlock = Mathf.Min(potentialBlock, CurrentShieldDurability);
            CurrentShieldDurability -= actualBlock;
            if (CurrentShieldDurability <= 0)
            {
                CurrentShieldDurability = 0;
                Duration = 0;
            }

            return incomingDamage - actualBlock;
        }
    }
}