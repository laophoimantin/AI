using UnityEngine;
using Wizardo;

namespace Spells
{
    // Reliable
    [CreateAssetMenu(menuName = "AI/Spells/Fireball")]
    public class FireBallSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            float damageOnHit = target.EstimateIncomingDamage(_power);
            
            float perceivedAcc = GetPerceivedAccuracy(user);
            float estimatedDamage = damageOnHit * perceivedAcc;
            
            if (damageOnHit >= target.CurrentHealth)
                estimatedDamage += 1000f;
                
            // Enemy is low health, bonus points
            if (target.CurrentHealth < target.MaxHealth * 0.3f)
                estimatedDamage *= 1.3f;
            
            estimatedDamage -= ManaCost * 0.4f;

            return Mathf.Max(0, estimatedDamage);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            target.TakeDamage(_power);
            Debug.Log($"FIREBALL HITS! Dealt {_power}");
        }
    }
}