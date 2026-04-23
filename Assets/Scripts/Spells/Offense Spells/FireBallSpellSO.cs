using DG.Tweening;
using System.Data;
using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// A standard offensive spell.
    /// Basic offense spell with decent damage, mana cost, and accuracy.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Fireball")]
    public class FireBallSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // New fuzzy logic 

            // Input variables
            float damageOnHit = target.EstimateIncomingDamage(_power); 
            float healthPercent = target.HealthPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Base Effectiveness
            float enemyKillable = SpellFuzzyEvaluator.EnemyKillable(damageOnHit, target.CurrentHealth);
            float enemyLowHealth = SpellFuzzyEvaluator.EnemyLowHealth(healthPercent);
            float highAccuracy = SpellFuzzyEvaluator.HighAccuracy(accuracy); //Is this spell highly accurate ?

            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);
            float excessMana = SpellFuzzyEvaluator.ExcessMana(user.CurrentMana, user.MaxMana);

            // RULE EVALUATION (Fuzzy AND/OR) ==========
            // Rule 1: "if can kill + high accuracy" -> Super High
            float ruleFinisher = OffensiveSpellFuzzy.ReliableFinisher(enemyKillable, highAccuracy);

            // Rule 2: "if enemy is lowhp + high accuracy" -> High  
            float ruleCleanup = FuzzyMath.AND(enemyLowHealth, highAccuracy);

            // Rule 3: "if too much current mana + average accuracy" -> Medium 
            float decentAccuracy = FuzzyMath.GradeUp(accuracy, 0.5f, 0.8f);
            float ruleDumpMana = FuzzyMath.AND(excessMana, decentAccuracy) * 0.8f;

            // Rule 4: Base
            float ruleBase = 0.7f; // High Trust

            //SUGENO ==========

            float valFinisher = 2.5f;   // High Finisher bonus (Fireball is a good finisher)
            float valCleanup = 1.6f;    // Cleanup bonus x1.6
            float valDumpMana = 1.2f;   // Dump mana small bonus x1.2
            float valBase = 1.0f;       //Base damage

            float numerator = (ruleFinisher * valFinisher) +
                              (ruleCleanup * valCleanup) +
                              (ruleDumpMana * valDumpMana) +
                              (ruleBase * valBase);

            float denominator = ruleFinisher + ruleCleanup + ruleDumpMana + ruleBase;
            float sugenoMultiplier = numerator / denominator;


            // Score = BaseDamage × SugenoMultiplier × ManaPenalty × AccuracyPenalty

            // FINAL
            float score = damageOnHit * sugenoMultiplier;

            // Penalty: Mana 
            float manaPenaltyMultiplier = Mathf.Lerp(0.5f, 1.0f, cheapMana); // Small penalty (0.5f)
            score *= manaPenaltyMultiplier;

            // Penalty: accuracy (miss = 0 damage)
            score *= accuracy;

            return Mathf.Max(0, score);



            // Old 
            //// Calculate Probability
            //// Check if the user would actually risk using this spell if the spell has low accuracy
            //float perceivedAccuracy = GetPerceivedAccuracy(user);

            //// 1. Base Effectiveness
            //// Calculate raw damage (Post Armor)
            //float damageOnHit = target.EstimateIncomingDamage(_power);
            //// Total score that based on risk-taking and damage perception.
            //_spellScore = damageOnHit; 

            //// 2. Kill Confirmation 
            //// If the target can be killed using this spell, make it worth a lot
            //if (damageOnHit >= target.CurrentHealth)
            //{
            //    _spellScore += 300f; // Big Magic number
            //}
            //// If the enemy is low health (< 40%), bonus points
            //else if (target.HealthPercent < 0.4f)
            //{
            //    _spellScore *= 1.5f;
            //}

            //// 3. Costs
            //// Accuracy penalty
            //_spellScore *= perceivedAccuracy;
            //// Mana cost penalty
            //_spellScore -= ManaCost * 0.4f;

            //// Return the score
            //return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target
            target.TakeDamage(user, _power);
            //Debug.Log($"FIREBALL HITS! Dealt {_power}");
        }
    }
}