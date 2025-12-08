using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    ///  Offense spell. Decent damage, low mana cost, decent accuracy, ignores shields.
    /// The user deals with damage based on accuracy and damage perception.
    /// </summary>
    /// <remarks>
    /// Offense spell with decent damage, low mana cost, and decent accuracy and can strike through shields.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Lightning Strike")]
    public class LightningStrikeSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAcc = GetPerceivedAccuracy(user);
            // 1. Base Effectiveness Calculation =================================================
            // Calculate raw damage (Post Armor)
            float damageOnHit = target.EstimateIncomingDamage(_power, true);
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
            target.TakeDamage(user, _power, true);
            //Debug.Log($"LIGHTNING STRIKE HITS! Dealt {_power}");
        }
    }
}