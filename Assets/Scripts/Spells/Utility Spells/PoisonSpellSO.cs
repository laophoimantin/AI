using UnityEngine;
using Wizardo;
using StatusEffects;

namespace Spells
{
    /// <summary>
    /// Damage Over Time (DoT) Spell.
    /// Poisons the target for a short duration.
    /// Notes: Consider changing the damage to based on the target's health percentage to turn this spell into a Tank Killer
    ///       (The more Hp the target has, the more damage it deals)
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Poison")]
    public class PoisonSpellSO : BaseSpellSO
    {
        [Header("Utility Config")]
        // Duration in turns for utility spells.
        [SerializeField] protected int _utilityDuration; 
        
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAccuracy = GetPerceivedAccuracy(user);
            
            // 1. Base Effectiveness
            // If the target is already poisoned, the spell is useless.
            if (target.HasStatus<PoisonedStatus>())
            {
                return 0f;
            }
            // If the target is low health (< 30%), the user can use other offensive spells instead of this one
            if (target.HealthPercent < 0.3f) 
            {
                return 0f; 
            }
            
            // Calculate Total Damage over the full duration.
            float totalPotentialDamage = _power * _utilityDuration;
            
            // Because this is not instant damage, add a small penalty (0.6f)
            // (50 damage over 5 turns != 50 instant damage)
            _spellScore = totalPotentialDamage * 0.7f;
            
            // 3. Strategic Bonuses
            // If the target is healthy, they will likely live long enough to take all the damage
            if (target.HealthPercent > 0.7f)
            {
                _spellScore *= 1.5f;
            }
            
            // This spell can ignore shields, bonus points
            if (target.HasShield)
            {
                _spellScore *= 1.3f; 
            }
            
            // 4. Costs
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;
            // Mana cost penalty
            _spellScore -= ManaCost * 0.4f; 
            

            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the poison status effect to the target
            target.AddStatus(new PoisonedStatus(user, target, _utilityDuration, _power, _icon));
            //Debug.Log($"POISON HITS! Dealt {_power} per {_utilityDuration}");
        }
    }
}