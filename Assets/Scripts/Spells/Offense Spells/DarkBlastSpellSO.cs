using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// High-Risk, High-Reward offensive spell.
    /// Deals massive damage but low accuracy and high mana cost.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/DarkBlast")]
    public class DarkBlastSpellSO : BaseSpellSO
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
                _spellScore *= 1.1f; // Lower than fireball because this spell is not a reliable finisher
            }
            // If the enemy has high health (< 70%), great opening move
            else if (target.HealthPercent > 0.7f)
            {
                _spellScore *= 1.3f;
            }

            // 3. Costs 
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;
            // Mana cost penalty
            _spellScore -= ManaCost * 0.8f; // This spell takes a lot of mana

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