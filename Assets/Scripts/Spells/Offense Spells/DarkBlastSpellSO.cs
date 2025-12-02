using UnityEngine;
using Wizardo;

namespace Spells
{
    // Big Button
    [CreateAssetMenu(menuName = "AI/Spells/DarkBlast")]
    public class DarkBlastSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            float damageOnHit = target.EstimateIncomingDamage(_power);
            float perceivedAcc = GetPerceivedAccuracy(user);
            float totalPotentialDamage = damageOnHit * perceivedAcc;
            
            _spellScore = totalPotentialDamage;
            
            if (damageOnHit >= target.CurrentHealth)
                _spellScore += 25f;
                
            // Enemy is low health, bonus points
            if (target.CurrentHealth < target.MaxHealth * 0.3f)
                _spellScore *= 1.3f;
            
            _spellScore -= ManaCost * 0.4f;

            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            target.TakeDamage(_power);
            Debug.Log($"DARK BLAST HITS! Dealt {_power}");
        }
    }
}