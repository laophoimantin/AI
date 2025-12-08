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
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAcc = GetPerceivedAccuracy(user);
            
            
            // 1. Base Effectiveness Calculation =================================================
            // If the user is already at full health (> 90%), the spell is not effective
            float missingHealth = user.MaxHealth - user.CurrentHealth;
            float actualHeal = Mathf.Min(missingHealth, _power);
            _spellScore = actualHeal;
            
            // If the spell heals less than 80% of its power, it's not effective
            if (actualHeal/_power < 0.8f) return 0;
            
            
            
            // 2. Survival Priorities =================================================
            // Critical: If the user is near death (< 30%), bonus points
            if (user.HealthPercent < 0.3f) 
            {
                //The user desperately needs this hp or it will die
                _spellScore *= 3.0f; // Raw Hp is better than a shield
            }
            // Warning: If health is low (< 60%), bonus points
            else if (user.HealthPercent < 0.6f)
            {
                _spellScore *= 1.7f;
            }
            
            // 3. SHIELD CONTEXT =================================================
            // If user have low Hp but a 50 Hp Shield, maybe it can save it?
            if (user.HasShield && user.DurabilityPercent > 0.3f)
            {
                _spellScore *= 0.8f; // Yser is safe for this turn, maybe save mana?
            }
            
            // 4. Costs  =================================================
            // Mana cost penalty
            _spellScore -= _manaCost * 0.5f;
            
            _spellScore *= perceivedAcc;
            
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