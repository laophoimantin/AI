using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    ///  Offense spell. Good damage, decent mana cost, decent accuracy, ignores shields.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Lightning Strike")]
    public class LightningStrikeSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAccuracy = GetPerceivedAccuracy(user);

            // 1. Base Effectiveness
            // Calculate raw damage (Ignore Shield)
            float damageOnHit = target.EstimateIncomingDamage(_power, true);
            // Total score that based on risk-taking and damage perception.
            _spellScore = damageOnHit; 

            //2. "Shield Hunter"
            if (target.HasShield) 
            {
                // If the shield has lots of HP, ignoring it makes this spell worth more
                if (target.DurabilityPercent > 0.5f) 
                {
                    _spellScore *= 1.5f; // This spell is worth
                }
                else
                {
                    _spellScore *= 1.2f; // Small shield, small bonus
                }
            }
            
            // 3. Kill Confirmation
            // If the target can be killed using this spell, make it worth a lot
            if (damageOnHit >= target.CurrentHealth)
            {
                _spellScore += 200; // Lower than fireball because this spell is not a reliable finisher
            }
            // If the enemy is low health (< 30%), bonus points
            else if (target.HealthPercent < 0.3f)
            {
                _spellScore *= 1.3f;
            }

            // 4. Costs
            // Mana cost penalty
            _spellScore -= ManaCost * 0.4f;
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;

            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target (Ignore Shield)
            target.TakeDamage(user, _power, true);
            //Debug.Log($"LIGHTNING STRIKE HITS! Dealt {_power}");
        }
    }
}