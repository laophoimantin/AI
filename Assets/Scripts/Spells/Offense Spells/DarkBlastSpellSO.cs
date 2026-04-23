using DG.Tweening;
using System.Data;
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
            // Input variables
            float damageOnHit = target.EstimateIncomingDamage(_power);
            float healthPercent = target.HealthPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Base Effectiveness
            float enemyKillable = SpellFuzzyEvaluator.EnemyKillable(damageOnHit, target.CurrentHealth);
            // Main strength: High Opening Damage
            float openingMove = OffensiveSpellFuzzy.OpeningMove(healthPercent);
            float highAccuracy = SpellFuzzyEvaluator.HighAccuracy(accuracy);
            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);
            // Risk-Reward: low accuracy but high damage = (gambler)
            float riskReward = OffensiveSpellFuzzy.HighRiskHighReward(accuracy, damageOnHit, target.MaxHealth);

            //RULES ==========
            // Rule 1: Opening 
            float ruleOpening = openingMove;

            // Rule 2: Finisher (small than Fireball cus not reliable)
            float ruleFinisher = OffensiveSpellFuzzy.ReliableFinisher(enemyKillable, highAccuracy) * 0.4f;

            // Rule 3: Risk-Reward (liều ăn nhiều)
            float ruleGamble = riskReward * 0.7f;

            // Rule 4: Base
            float ruleBase = 0.3f; // Low Trust

            //SUGENO ==========

            float valOpening = 2.5f;   // Opening HIGH  x 2.5 value
            float valFinisher = 1.3f;  // Finisher LOW  -> only x1.3 
            float valGamble = 1.5f;    //  Gamble bonus
            float valBase = 1.0f;      //Base

            float numerator = (ruleOpening * valOpening) +
                          (ruleFinisher * valFinisher) +
                          (ruleGamble * valGamble) +
                          (ruleBase * valBase);

            float denominator = ruleOpening + ruleFinisher + ruleGamble + ruleBase;
            float sugenoMultiplier = numerator / denominator;


            //FINAL ==========
            float score = damageOnHit * sugenoMultiplier;

            // Penalty: Mana 
            float manaPenaltyMultiplier = Mathf.Lerp(0.2f, 1.0f, cheapMana);// High penalty (0.2f)
            score *= manaPenaltyMultiplier;

            // Penalty: accuracy 
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
            //    _spellScore *= 1.1f; // Lower than fireball because this spell is not a reliable finisher
            //}
            //// If the enemy has high health (< 70%), great opening move
            //else if (target.HealthPercent > 0.7f)
            //{
            //    _spellScore *= 1.3f;
            //}

            //// 3. Costs 
            //// Accuracy penalty
            //_spellScore *= perceivedAccuracy;
            //// Mana cost penalty
            //_spellScore -= ManaCost * 0.8f; // This spell takes a lot of mana

            //// Return the score 
            //return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Damage the target
            target.TakeDamage(user, _power);
            //Debug.Log($"DARK BLAST HITS! Dealt {_power}");
        }
    }
}