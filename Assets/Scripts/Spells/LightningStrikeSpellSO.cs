using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Lightning Strike")]
    public class LightningStrikeSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Ignore reduction percent 
            float damageOnHit = _power * (1.0f - Mathf.Max(0, target.ReductionPercent - 0.3f));
            float perceivedAcc = GetPerceivedAccuracy(user);
            float totalPotentialDamage = damageOnHit * perceivedAcc;

            _spellScore = totalPotentialDamage;
            
            if (target.CurrentHealth <= damageOnHit)
            {
                if (user.CurrentHealth < 15) 
                {
                    _spellScore += 50; 
                }
            }
            
            if (target.CurrentHealth < target.MaxHealth * 0.3f)
            {
                _spellScore *= 1.3f;
            }

            _spellScore -= ManaCost * 0.4f;
            
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            target.TakeDamage(_power);
            Debug.Log($"LIGHTNING STRIKE HITS! Dealt {_power}");
        }
    }
}