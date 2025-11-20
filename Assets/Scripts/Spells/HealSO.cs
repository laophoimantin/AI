using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Heal")]
    public class HealSO : SpellSO
    {

        public override float Evaluate(Agent user, Agent enemy)
        {
            float missingHealth = user.MaxHealth - user.CurrentHealth;
            if (missingHealth < 5) return 0;
            
            float actualHealAmount = Mathf.Min(missingHealth, _power);
            
            float baseValue = actualHealAmount;
            
            float healthPct = user.CurrentHealth / user.MaxHealth;
            if (healthPct < 0.3f) // Critical (< 30%)
            {
                baseValue *= 3.0f; 
            }
            else if (healthPct < 0.6f) // Wounded (< 60%)
            {
                baseValue *= 1.5f;
            }
            
            // Mana cost penalty
            baseValue -= _manaCost * 0.5f;
            
            return Mathf.Max(0, baseValue);
        }
        
        public override void ApplyEffect(Agent user, Agent enemy)
        {
            if (user.CurrentMana < _manaCost) return;
            user.ModifyMana(-_manaCost);
            user.ModifyHealth(_power);
            Debug.Log($"{user.name} heals (+{_power} HP)");
        }
        
    }
}