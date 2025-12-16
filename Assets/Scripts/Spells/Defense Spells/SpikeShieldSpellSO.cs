using StatusEffects;
using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// A Spiked Shield. High reduction, low durability.
    /// Reduces incoming damage significantly and reflects chip damage back to the attacker.
    /// </summary>
    /// <remarks>
    /// Design Note: Good at blocking and dealing damage but easy to break.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Spike Shield")]
    public class SpikeShieldSpellSO : BaseSpellSO
    {
        [Header("Spike Shield Config")]
        [Tooltip("The percentage of damage blocked (0.0 to 1.0)")]
        [SerializeField, Range(0, 0.9f)] private float _reductionPercent = 0.4f;
        
        [Tooltip("The base durability (Health) of the shield.")]
        [SerializeField] private float _shieldDurability = 15f; 
        
        [Tooltip("How many turns the shield lasts.")]
        [SerializeField] private int _shieldDuration = 2;

        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Safety checks
            if (_reductionPercent <= 0)
            {
                Debug.Log("Reduction percent cannot be less than or equal to zero");
                return 0;
            }
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            float perceivedAccuracy = GetPerceivedAccuracy(user);
            
            // 1. Base Effectiveness
            // The higher the reduction percent, the more effective the shield is
            // Formula: Durability / (1 - reductionPercent)
            float effectiveShieldHp = _shieldDurability / (1.0f - _reductionPercent);

            // Score = Defense + Offense (Reflection).
            // Reduce Reflected Damage value (0.4) because this spell is not a main damage source than other offensive spells
            _spellScore = effectiveShieldHp + (_power * 0.4f);


            // 2. Situational Modifiers
            if (user.HasShield)
            {
                // Only replace an existing shield if it's about to break (< 30% durability)
                // If the current shield is about to break, this spell is worth a bit
                if (user.DurabilityPercent < 0.3f)
                {
                    _spellScore *= 0.8f; // Slight penalty for overlap
                }
                // If the shield is still strong, this spell is not worth it
                else
                {
                    return 0;
                }
            }

            
            // 3. Survival Priorities
            // Critical: If the user is near death (< 30%), bonus points
            if (user.HealthPercent < 0.3f)
            {
                // The user desperately needs this shield or it will die
                _spellScore *= 2;
            }
            // Warning: If health is low (< 70%), bonus points
            else if (user.HealthPercent < 0.7f)
            {
                // If the health is low, a shield would be a good idea
                _spellScore *= 1.3f;
            }

            // 4. Counters
            // Counter-Play: If the enemy has Mana (>40%) to deal high damage, a shield would be a good idea
            if (target.ManaPercent > 0.4f)
            {
                _spellScore *= 1.2f;
            }

            // 5. Costs 
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;
            // Mana cost penalty
            _spellScore -= _manaCost * 0.6f; // This spell takes a lot of mana
            
            
            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the spike shield status
            user.AddStatus(new SpikeShieldStatus(user, _shieldDuration, _power, _icon, _reductionPercent, _shieldDurability));

            //Debug.Log($"{user.Name} raises a shield");
        }
    }
}