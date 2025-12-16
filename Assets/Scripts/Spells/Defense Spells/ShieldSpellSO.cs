using StatusEffects;
using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// A standard defense spell. Provides a shield that reduces incoming damage by a percentage.
    /// </summary>
    /// <remarks>
    /// Design Note: Basic defense. Good stats. Nothing else.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Standard Shield")]
    public class ShieldSpellSO : BaseSpellSO
    {
        [Header("Shield Config")]
        [Tooltip("The percentage of damage blocked (0.0 to 1.0)")]
        [SerializeField, Range (0, 0.9f)] private float _reductionPercent  = 0.25f;
        
        [Tooltip("The base durability (Health) of the shield.")]
        [SerializeField] private float _shieldDurability = 20f;
        
        [Tooltip("How many turns the shield lasts.")]
        [SerializeField] private int _shieldDuration = 3;
        
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
            _spellScore = _shieldDurability / (1.0f - _reductionPercent);
            
            // 2. Situational Modifiers
            if (user.HasShield)
            {
                // Only replace an existing shield if it's about to break (< 30% durability)
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
            // Critical: If the user is near death (< 30%), need a shield to survive
            if (user.HealthPercent < 0.3f)
            {
                // The user desperately needs this shield or it will die
                _spellScore *= 2.5f; // Cheap shield so it has more values.
            }
            // Warning: If health is low (< 70%), a shield would be a good idea
            else if (user.HealthPercent < 0.7f)
            {
                _spellScore *= 1.5f;
            }
            
            // 4. Counters
            // Counter-Play: If the enemy has Mana (>40%) to deal high damage, a shield would be a good idea
            if (target.ManaPercent > 0.4f)
            {
                _spellScore *= 1.2f;
            }

            // 5. Cost 
            // Accuracy penalty
            _spellScore *= perceivedAccuracy;
            // Mana cost penalty
            _spellScore -= _manaCost * 0.5f;
            
            // Return the score
            return Mathf.Max(0, _spellScore);
        }
        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the standard shield status
            user.AddStatus(new StandardShieldStatus(user, _shieldDuration, _power, _icon, _reductionPercent, _shieldDurability));
            //Debug.Log($"{user.Name} raises a shield");
        }
    }
}