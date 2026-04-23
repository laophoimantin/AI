using StatusEffects;
using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// A "medium-risk" Shield. It has high durability but very low damage reduction.
    /// If the shield survives the full duration, it explodes dealing damage to the target.
    /// </summary>
    /// <remarks>
    /// Design Note: Medium Risk, Medium Reward.
    /// Great against low mana enemies who can't break the shield
    /// Terrible for survival.
    /// </remarks>
    [CreateAssetMenu(menuName = "Spells/Explosive Shield")]
    public class ExplosiveShieldSpellSO : BaseSpellSO
    {
        [Header("Explosive Shield Config")]
        [Tooltip("The percentage of damage blocked (0.0 to 1.0)")]
        [SerializeField, Range(0, 0.9f)] private float _reductionPercent = 0.1f; // Lower reduction than other shields

        [Tooltip("The base durability (Health) of the shield.")]
        [SerializeField] private float _shieldDurability = 30f; // Higher Hp than other shields

        [Tooltip("How many turns the shield lasts.")]
        [SerializeField] private int _shieldDuration = 4;

        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // New fuzzy logic 

            // Safety checks
            if (_reductionPercent >= 1.0f) _reductionPercent = 0.99f;
            if (_reductionPercent < 0f) return 0f;

            // Gatekeeper
            // Already has shield + the shield is still durable
            if (user.HasShield && user.DurabilityPercent > 0.4f) return 0f;

            // Input variables
            float effectiveShieldHp = _shieldDurability / (1.0f - _reductionPercent);
            float baseValue = effectiveShieldHp + (_power * 1.2f);
            float healthPercent = user.HealthPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Fuzzy sets
            float criticalHealth = FuzzyMath.GradeDown(healthPercent, 0.2f, 0.5f);
            float safeHealth = 1.0f - criticalHealth;
            float enemyLowMana = FuzzyMath.GradeDown(target.ManaPercent, 0.3f, 0.6f);
            float enemyHighMana = FuzzyMath.GradeUp(target.ManaPercent, 0.4f, 0.7f);
            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);

            // RULE EVALUATION
            float rulePerfectSetup = FuzzyMath.AND(safeHealth, enemyLowMana);

            // Rule 2
            float ruleEasilyBroken = enemyHighMana;

            // Rule 3: This shield is not mainly for survive
            float ruleBadSurvival = criticalHealth;

            float ruleBase = 0.4f;

            // DEFUZZIFICATION (Sugeno)
            float valPerfectSetup = 2.5f; 
            float valEasilyBroken = 0.5f;   
            float valBadSurvival = 0.1f;  
            float valBase = 1.0f;

            float numerator = (rulePerfectSetup * valPerfectSetup) +
                              (ruleEasilyBroken * valEasilyBroken) +
                              (ruleBadSurvival * valBadSurvival) +
                              (ruleBase * valBase);

            float denominator = rulePerfectSetup + ruleEasilyBroken + ruleBadSurvival + ruleBase;
            float sugenoMultiplier = numerator / denominator;

            // FINAL
            float score = baseValue * sugenoMultiplier;

            // Penalty:
            float overlapPenaltyMultiplier = user.HasShield ? 0.6f : 1.0f;
            score *= overlapPenaltyMultiplier;

            // Penalty: Mana
            float manaPenaltyMultiplier = Mathf.Lerp(0.5f, 1.0f, cheapMana);
            score *= manaPenaltyMultiplier;

            score *= accuracy;

            return score;
        }



        // Old crisp logic


        //protected override float EvaluateInternal(Agent user, Agent target)
        //{
        //    // Safety checks
        //    if (_reductionPercent <= 0)
        //    {
        //        Debug.Log("Reduction percent cannot be less than or equal to zero");
        //        return 0;
        //    }
        //    // Calculate Probability
        //    // Check if the user would actually risk using this spell if the spell has low accuracy
        //    float perceivedAccuracy = GetPerceivedAccuracy(user);

        //    // 1. Base Effectiveness
        //    // The higher the reduction percent, the more effective the shield is
        //    // Formula: Durability / (1 - reductionPercent)
        //    float effectiveShieldHp = _shieldDurability / (1.0f - _reductionPercent); // The Hp might be high but blocking efficiency is low


        //    // Score = Defense + Offense (Explosion).
        //    // Increase Reflected Damage value (1.2) because that's the main point of this spell.
        //    _spellScore = effectiveShieldHp + (_power * 1.2f);


        //    // 2. Situational Modifiers
        //    if (user.HasShield)
        //    {
        //        // Only replace an existing shield if it's about to break (< 30% durability)
        //        // If the current shield is about to break, this spell is worth a bit
        //        if (user.DurabilityPercent < 0.3f)
        //        {
        //            _spellScore *= 0.6f; // Slight penalty for overlap
        //        }
        //        // If the shield is still strong, this spell is not worth it
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    // 3. Survival Priorities 
        //    // Critical: If the user is near death (< 30%), ignores this shield
        //    if (user.HealthPercent < 0.3f)
        //    {
        //        _spellScore *= 0.2f; // This spell is not mainly for survival
        //    }
        //    // Warning: If health is low (< 70%), a shield would be a good idea
        //    else if (user.HealthPercent < 0.7f)
        //    {
        //        _spellScore *= 1.2f; // This spell is not mainly for survival
        //    }

        //    // 4. Counters
        //    // Counter-Play: if the enemy is low on mana (<30%), they're unlikely to use enough big spells to break this shield
        //    // This spell has a chance to explode 
        //    if (target.ManaPercent < 0.3f)
        //    {
        //        _spellScore *= 1.5f; // Bonus 
        //    }
        //    // Enemies have full mana and can break the shield instantly, so not worth it
        //    else if (target.ManaPercent > 0.6f)
        //    {
        //        _spellScore *= 0.5f; 
        //    }

        //    // 5. Costs 
        //    // Accuracy penalty
        //    _spellScore *= perceivedAccuracy;
        //    // Mana cost penalty
        //    _spellScore -= _manaCost * 0.4f;

        //    // Return the score
        //    return Mathf.Max(0, _spellScore);
        //}

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the explosive shield status
            user.AddStatus(new ExplosiveShieldStatus(user, target, _shieldDuration, _power, _icon, _reductionPercent, _shieldDurability));
            //Debug.Log($"{user.Name} raises a time bomb shield!");
        }
    }
}