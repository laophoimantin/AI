using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Offense spell. Big button spell. Low accuracy. High mana cost. High damage. High risk.
    /// The user deals with damage based on accuracy and damage perception.
    /// </summary>
    /// <remarks>
    /// Big offense spell, high risk high reward.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/DarkBlast")]
    public class DarkBlastSpellSO : BaseSpellSO
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
            
            // 2. Kill Confirmation =================================================
            // If the target can be killed using this spell, make it worth a lot
            if (damageOnHit >= target.CurrentHealth)
                _spellScore += 500 * perceivedAcc; // Scale the bonus by accuracy, lower than fireball because this spell is not a reliable finisher
   
            // If the enemy is low health (< 30%), bonus points
            if (target.HealthPercent < 0.3f)
                _spellScore *= 1.3f;
            
            // 3. Costs =================================================  
            // Mana cost penalty
            _spellScore -= ManaCost * 0.9f; // This spell takes a lot of mana

            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target
            target.TakeDamage(user, _power);
            //Debug.Log($"DARK BLAST HITS! Dealt {_power}");
        }
    }
}