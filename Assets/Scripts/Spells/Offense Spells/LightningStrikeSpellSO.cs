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
            // Input variables
            float damageOnHit = target.EstimateIncomingDamage(_power, true);
            float healthPercent = target.HealthPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Base Effectiveness
            float enemyKillable = SpellFuzzyEvaluator.EnemyKillable(damageOnHit, target.CurrentHealth);
            float enemyLowHealth = SpellFuzzyEvaluator.EnemyLowHealth(healthPercent);
            float highAccuracy = SpellFuzzyEvaluator.HighAccuracy(accuracy);
            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);

            // Armor pierce
            float thickShield = target.HasShield ? FuzzyMath.GradeUp(target.DurabilityPercent, 0.3f, 0.8f) : 0f;
            float thinShield = target.HasShield ? FuzzyMath.GradeDown(target.DurabilityPercent, 0.1f, 0.5f) : 0f;

            // --- RULES ---
            // Rule 1 & 2: Enemy has shield
            float ruleAntiThickShield = thickShield;
            float ruleAntiThinShield = thinShield;

            // Rule 3: Finisher (worse than fire ball but better than dark bvlash
            float ruleFinisher = OffensiveSpellFuzzy.ReliableFinisher(enemyKillable, highAccuracy) * 0.7f;

            // Rule 4: Cleanup (deal dmg to low hp)
            float ruleCleanup = FuzzyMath.AND(enemyLowHealth, highAccuracy);

            // Rule Base
            float ruleBase = 0.4f; // Decent Trust

            //SUGENO ==========
            float valAntiThickShield = 2.0f; // Enemy has thick shield x2 val
            float valAntiThinShield = 1.3f;  //  Enemy has thin shield xlower val
            float valFinisher = 1.8f;        // Lower than Fireball
            float valCleanup = 1.4f;        // Lower than Fireball
            float valBase = 1.0f;            //Base damage

            float numerator = (ruleAntiThickShield * valAntiThickShield) +
                              (ruleAntiThinShield * valAntiThinShield) +
                              (ruleFinisher * valFinisher) +
                              (ruleCleanup * valCleanup) +
                              (ruleBase * valBase);

            float denominator = ruleAntiThickShield + ruleAntiThinShield + ruleFinisher + ruleCleanup + ruleBase;
            float sugenoMultiplier = numerator / denominator;

            // FINAL
            float score = damageOnHit * sugenoMultiplier;

            // Penalty: Mana 
            float manaPenaltyMultiplier = Mathf.Lerp(0.5f, 1.0f, cheapMana);
            score *= manaPenaltyMultiplier;

            // Penalty: accuracy
            score *= accuracy;

            return score;
        }
        //protected override float EvaluateInternal(Agent user, Agent target)
        //{
        //    // Calculate Probability
        //    // Check if the user would actually risk using this spell if the spell has low accuracy
        //    float perceivedAccuracy = GetPerceivedAccuracy(user);

        //    // 1. Base Effectiveness
        //    // Calculate raw damage (Ignore Shield)
        //    float damageOnHit = target.EstimateIncomingDamage(_power, true);
        //    // Total score that based on risk-taking and damage perception.
        //    _spellScore = damageOnHit; 

        //    //2. "Shield Hunter"
        //    if (target.HasShield) 
        //    {
        //        // If the shield has lots of HP, ignoring it makes this spell worth more
        //        if (target.DurabilityPercent > 0.5f) 
        //        {
        //            _spellScore *= 1.5f; // This spell is worth
        //        }
        //        else
        //        {
        //            _spellScore *= 1.2f; // Small shield, small bonus
        //        }
        //    }
            
        //    // 3. Kill Confirmation
        //    // If the target can be killed using this spell, make it worth a lot
        //    if (damageOnHit >= target.CurrentHealth)
        //    {
        //        _spellScore += 200; // Lower than fireball because this spell is not a reliable finisher
        //    }
        //    // If the enemy is low health (< 30%), bonus points
        //    else if (target.HealthPercent < 0.3f)
        //    {
        //        _spellScore *= 1.3f;
        //    }

        //    // 4. Costs
        //    // Mana cost penalty
        //    _spellScore -= ManaCost * 0.4f;
        //    // Accuracy penalty
        //    _spellScore *= perceivedAccuracy;

        //    // Return the score
        //    return Mathf.Max(0, _spellScore);
        //}

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target (Ignore Shield)
            target.TakeDamage(user, _power, true);
            //Debug.Log($"LIGHTNING STRIKE HITS! Dealt {_power}");
        }
    }
}