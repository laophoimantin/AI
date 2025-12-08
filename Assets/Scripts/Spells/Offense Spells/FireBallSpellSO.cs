using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Offense spell. Basic stats. Reliable. 
    /// The user fires a fireball at a target, dealing damage based on accuracy and damage perception.
    /// The user deals with damage based on accuracy and damage perception.
    /// </summary>
    /// <remarks>
    /// Basic offense spell with decent damage, mana cost, and accuracy.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Fireball")]
    public class FireBallSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAcc = GetPerceivedAccuracy(user);
            // 1. Base Effectiveness Calculation =================================================
            // Calculate raw damage (Post Armor)
            float damageOnHit = target.EstimateIncomingDamage(_power);
            // Total score that based on risk-taking and damage perception.
            _spellScore = damageOnHit * perceivedAcc; // Expected Value = Damage * Probability

            // 2. Kill Confirmation
            // If the target can be killed using this spell, make it worth a lot
            if (damageOnHit >= target.CurrentHealth)
                _spellScore += 1000f * perceivedAcc; // Scale the bonus by accuracy

            // If the enemy is low health (< 30%), bonus points
            if (target.HealthPercent < 0.3f)
                _spellScore *= 1.5f;

            // 3. Costs  
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