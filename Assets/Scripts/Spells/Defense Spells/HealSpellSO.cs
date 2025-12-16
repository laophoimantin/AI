using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Defense spell.
    /// The user heals itself for a fixed amount of HP.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Heal")]
    public class HealSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // 1. Base Effectiveness Calculation =================================================
            // If the user is already at full health (> 90%), the spell is not effective
            float missingHealth = user.MaxHealth - user.CurrentHealth;
            float actualHeal = Mathf.Min(missingHealth, _power);
            
            _spellScore = actualHeal;
            
            //3. Efficiency Check
            float efficiency = actualHeal / _power; 

            // The user is healthy enough,
            if (user.HealthPercent > 0.7f) 
            {
                // If the user is wasting more than 40% of the spell, don't use it.
                if (efficiency < 0.4f) return 0; 
            }
            
            
            
            // 2. Survival Priorities
            // Critical: If the user is near death (< 30%), bonus points
            if (user.HealthPercent < 0.3f) 
            {
                //The user desperately needs this hp or it will die
                _spellScore *= 5.0f; // Raw Hp is better than a shield
            }
            // Warning: If health is low (< 60%), bonus points
            else if (user.HealthPercent < 0.5f)
            {
                _spellScore *= 1.7f;
            }
            
            // 3. Shield Context 
            // If the user is on low Hp but has a Shield, lower the score
            if (user.HasShield && user.DurabilityPercent > 0.3f)
            {
                _spellScore *= 0.8f;
            }
            
            // 4. Costs 
            // Mana cost penalty
            _spellScore -= _manaCost * 0.5f;
            
            // Return the score
            return Mathf.Max(0, _spellScore);
        }
        
        protected override void SpellEffect(Agent user, Agent target)
        {
            // Heal the user
            user.Heal(_power);
            //Debug.Log($"{user.name} heals (+{_power} HP)");
        }
    }
}