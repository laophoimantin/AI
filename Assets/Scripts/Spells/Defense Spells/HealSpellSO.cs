using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Heal")]
    public class HealSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            if (!target.IsAlive)
                return 0;
            if (user.CurrentMana < _manaCost) 
                return 0;

            _spellScore = 0f;
            
            float missingHealth = user.MaxHealth - user.CurrentHealth;
            if (missingHealth < 5) return 0;
            
            float actualHealAmount = Mathf.Min(missingHealth, _power);
            
            _spellScore  = actualHealAmount;
            
            float healthPct = user.CurrentHealth / user.MaxHealth;
            if (healthPct < 0.3f) // Critical (< 30%)
            {
                _spellScore *= 3.0f; 
            }
            else if (healthPct < 0.6f) // Wounded (< 60%)
            {
                _spellScore *= 1.5f;
            }
            
            // Mana cost penalty
            _spellScore -= _manaCost * 0.5f;
            
            return Mathf.Max(0, _spellScore);
        }
        
        protected override void SpellEffect(Agent user, Agent target)
        {
            user.Heal(_power);
            Debug.Log($"{user.name} heals (+{_power} HP)");
        }
    }
}