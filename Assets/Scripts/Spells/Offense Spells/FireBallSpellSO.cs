using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// A standard offensive spell.
    /// Basic offense spell with decent damage, mana cost, and accuracy.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Fireball")]
    public class FireBallSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAccuracy = GetPerceivedAccuracy(user);
            
            // 1. Base Effectiveness
            // Calculate raw damage (Post Armor)
            float damageOnHit = target.EstimateIncomingDamage(_power);
            // Total score that based on risk-taking and damage perception.
            _spellScore = damageOnHit; 

            // 2. Kill Confirmation 
            // If the target can be killed using this spell, make it worth a lot
            if (damageOnHit >= target.CurrentHealth)
            {
                _spellScore += 300f; // Big Magic number
            }
            // If the enemy is low health (< 40%), bonus points
            else if (target.HealthPercent < 0.4f)
            {
                _spellScore *= 1.5f;
            }

            // 3. Costs
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;
            // Mana cost penalty
            _spellScore -= ManaCost * 0.4f;

            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target
            target.TakeDamage(user, _power);
            //Debug.Log($"FIREBALL HITS! Dealt {_power}");
        }
    }
}