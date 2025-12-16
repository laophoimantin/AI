using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    /// <summary>
    /// Base class for shields.
    /// Acting more like a Damage Damper than a shield
    /// </summary>
    public abstract class BaseShieldStatus : StatusEffect
    {
        // The percentage of damage blocked (0.0 to 1.0)
        public float ReductionPercent { get; protected set; } 
        
        //The maximum HP of the shield.
        public float Durability { get; protected set; } 
        
        // The current HP of the shield.
        public float CurrentShieldDurability { get; protected set; } 
        
        // 0.0 (broken) to 1.0 (full).
        public float DurabilityPercent => CurrentShieldDurability / Durability; 

        // Constructor
        protected BaseShieldStatus(Agent user, int duration, float power, Sprite icon, float reductionPercent, float durability) :
            base(user, duration, power, icon)
        {
            ReductionPercent = reductionPercent;
            Durability = durability;
            CurrentShieldDurability = Durability;
        }

        // Refresh the shield's durability
        public override void Refresh(int duration, float power)
        {
            base.Refresh(duration, power);
            CurrentShieldDurability = Durability;
        }

        // Returns the damage after reduction
        // Used when offensive spells calculate estimated damage when hitting a shield
        public virtual float ModifyDamage(float damage) 
        {
            return damage * (1f - ReductionPercent);
        }

        // Called when damage is actually taken
        public virtual float AbsorbDamage(Agent attacker, float incomingDamage)
        {
            // Calculate how much it wants to block
            float potentialBlock = incomingDamage * ReductionPercent; 
            // Calculate how much it can block
            float actualBlock = Mathf.Min(potentialBlock, CurrentShieldDurability); 
            
            // Reduce shield durability
            CurrentShieldDurability -= actualBlock; 
            
            if (CurrentShieldDurability <= 0)
            {
                // Shield is broken, remove it
                CurrentShieldDurability = 0;
                Duration = 0;
            }

            // Return the damage after reduction
            // (Total incoming damage - What it successfully stopped)
            return incomingDamage - actualBlock;
        }
    }
}