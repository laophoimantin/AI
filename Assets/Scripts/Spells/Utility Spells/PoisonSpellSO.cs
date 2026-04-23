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
            // New fuzzy logic 

            // Gatekeeper
            // Target is already poisoned -> no need to reapply and waste mana
            if (target.HasStatus<PoisonedStatus>()) return 0f;

            // Target is very low HP (< 30%) -> use burst damage (Fireball) instead
            if (target.HealthPercent < 0.3f) return 0f;

            // Input variables
            // Total potential damage if the poison runs its full duration
            float totalPotentialDamage = _power * _utilityDuration;
            float baseValue = totalPotentialDamage;

            float targetHealthPercent = target.HealthPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Base Effectiveness
            // Higher target hp -> poison becomes more effective over time
            float beefyTarget = FuzzyMath.GradeUp(targetHealthPercent, 0.5f, 0.8f);

            // Lower target hp (30%–60%)
            // Poison may not fully deal damage before the target dies -> inefficient
            float weakTarget = FuzzyMath.GradeDown(targetHealthPercent, 0.3f, 0.6f);

            // Evaluate target shield: poison ignores shields and damages health directly
            // The stronger the shield, the more valuable poison becomes
            float turtleShield = target.HasShield ? FuzzyMath.GradeUp(target.DurabilityPercent, 0.3f, 0.8f) : 0f;

            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);

            // RULE EVALUATION
            // Rule 1: Sustained damage (target is very healthy)
            float ruleLongSuffering = beefyTarget;

            // Rule 2: Anti-turtle (target relies on shields)
            float ruleAntiTurtle = turtleShield;

            // Rule 3: Inefficiency (target is low HP → burst damage is better)
            float ruleWasteOfTime = weakTarget;

            float ruleBase = 0.5f;

            //SUGENO ==========
            float valLongSuffering = 1.6f; // Applying poison to a high hp target is very effective
            float valAntiTurtle = 1.4f;    // Bypassing shields is valuable

            // If the target is low hp, heavily reduce the value (0.3)
            // This pushes the AI to prefer burst damage instead of poison
            float valWasteOfTime = 0.3f;

            // Base value adjusted to reflect that damage-over-time is less efficient than instant damage
            float valBase = 0.6f;

            float numerator = (ruleLongSuffering * valLongSuffering) +
                              (ruleAntiTurtle * valAntiTurtle) +
                              (ruleWasteOfTime * valWasteOfTime) +
                              (ruleBase * valBase);

            float denominator = ruleLongSuffering + ruleAntiTurtle + ruleWasteOfTime + ruleBase;
            float sugenoMultiplier = numerator / denominator;

            // FINAL
            float score = baseValue * sugenoMultiplier;

            // Penalty: mana
            float manaPenaltyMultiplier = Mathf.Lerp(0.5f, 1.0f, cheapMana);
            score *= manaPenaltyMultiplier;

            // Penalty: acc
            score *= accuracy;

            return score;
        }


        // Old crisp logic


        //protected override float EvaluateInternal(Agent user, Agent target)
        //{
        //    // Calculate Probability
        //    // Check if the user would actually risk using this spell if the spell has low accuracy
        //    float perceivedAccuracy = GetPerceivedAccuracy(user);

        //    // 1. Base Effectiveness
        //    // If the target is already poisoned, the spell is useless.
        //    if (target.HasStatus<PoisonedStatus>())
        //    {
        //        return 0f;
        //    }
        //    // If the target is low health (< 30%), the user can use other offensive spells instead of this one
        //    if (target.HealthPercent < 0.3f) 
        //    {
        //        return 0f; 
        //    }

        //    // Calculate Total Damage over the full duration.
        //    float totalPotentialDamage = _power * _utilityDuration;

        //    // Because this is not instant damage, add a small penalty (0.6f)
        //    // (50 damage over 5 turns != 50 instant damage)
        //    _spellScore = totalPotentialDamage * 0.7f;

        //    // 3. Strategic Bonuses
        //    // If the target is healthy, they will likely live long enough to take all the damage
        //    if (target.HealthPercent > 0.7f)
        //    {
        //        _spellScore *= 1.5f;
        //    }

        //    // This spell can ignore shields, bonus points
        //    if (target.HasShield)
        //    {
        //        _spellScore *= 1.3f; 
        //    }

        //    // 4. Costs
        //    // Accuracy penalty
        //    _spellScore *= perceivedAccuracy;
        //    // Mana cost penalty
        //    _spellScore -= ManaCost * 0.4f; 


        //    // Return the score
        //    return Mathf.Max(0, _spellScore);
        //}

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the poison status effect to the target
            target.AddStatus(new PoisonedStatus(user, target, _utilityDuration, _power, _icon));
            //Debug.Log($"POISON HITS! Dealt {_power} per {_utilityDuration}");
        }
    }
}