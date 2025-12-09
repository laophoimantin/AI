using StatusEffects;
using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Both utility and defense spell.
    /// The user gains a shield that reduces incoming damage by a percentage.
    /// After a short duration, if the shield still exits, it explodes and deals fixed damage to attackers.
    /// </summary>
    /// <remarks>
    /// Design Note: This shield is weaker than a normal shield (lower durability),
    /// but it can deal good damage if the attacker is on low mana to cast offensive spells.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Explosive Shield")]
    public class ExplosiveShieldSpellSO : BaseSpellSO
    {
        // Reduces lower damage, but have more durability and duration.
        [Header("Explosive Shield Config")]
        [Tooltip("The percentage of damage blocked (0.0 to 1.0)")]
        [SerializeField, Range(0, 0.9f)] private float _reductionPercent = 0.1f; // Lower reduction than other shields

        [Tooltip("The base durability (Health) of the shield.")]
        [SerializeField] private float _shieldDurability = 30f; // Higher Hp than other shields

        [Tooltip("How many turns the shield lasts.")]
        [SerializeField] private int _shieldDuration = 4;

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
            float perceivedAcc = GetPerceivedAccuracy(user);
            
            // 1. Base Effectiveness Calculation =================================================
            // The higher the reduction percent, the more effective the shield is
            // Formula: Durability / (1 - reductionPercent)
            float effectiveShieldHP = _shieldDurability / (1.0f - _reductionPercent);


            // Score = Defense + Offense (Reflection).
            // Reduce Reflected Damage value (0.6) because this spell is not a main damage source than other offensive spells
            _spellScore = effectiveShieldHP + (_power * 0.6f);


            // 2. Situational Modifiers =================================================
            // If the user already has a shield
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
            
            // 3. Survival Priorities =================================================
            // Critical: If the user is near death (< 30%), bonus points
            if (user.HealthPercent < 0.3f)
            {
                _spellScore *= 1.5f; // This spell is not mainly for survival
            }
            // Warning: If health is low (< 70%), bonus points
            else if (user.HealthPercent < 0.7f)
            {
                // If the health is low, a shield would be a good idea
                _spellScore *= 1.1f; // This spell is not mainly for survival
            }

            // 4. Counters =================================================
            // Counter-Play: If the enemy has not enough Mana (<30%) to cast multiple nukes and destroy the shield, gives it a change to explode, bonus points
            if (target.ManaPercent < 0.3f)
            {
                _spellScore *= 1.5f;
            }
            else if (target.ManaPercent > 0.7f)
            {
                // Enemy is full of mana and can break the shield instantly, 0 point to use
                _spellScore *= 0.5f; 
            }

            // 5. Costs =================================================
            // Mana cost penalty
            _spellScore -= _manaCost * 0.4f;
            
            _spellScore *= perceivedAcc;

            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the explosive shield status
            user.AddStatus(new ExplosiveShieldStatus(user, target, _shieldDuration, _power, _icon, _reductionPercent, _shieldDurability));
            //Debug.Log($"{user.Name} raises a time bomb shield!");
        }
    }
}