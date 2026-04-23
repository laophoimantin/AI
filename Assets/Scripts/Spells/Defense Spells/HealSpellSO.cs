using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Defense spell.
    /// The user heals itself for a fixed amount of HP.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Heal")]
    public class HealSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // New fuzzy logic 

            // Input variables
            float missingHealth = user.MaxHealth - user.CurrentHealth;
            float actualHeal = Mathf.Min(missingHealth, _power);
            float efficiency = actualHeal / _power; // from 0.0 (Overheal) to 1.0
            float healthPercent = user.HealthPercent;

            // Gatekeeper
            // The current hp is high (>70%) and low efficiency
            if (healthPercent > 0.7f && efficiency < 0.4f) return 0f;
            if (actualHeal <= 0) return 0f;

            // Fuzzy sets
            // Low hp (<20 danger, > 50% safe)
            float criticalHealth = FuzzyMath.GradeDown(healthPercent, 0.2f, 0.5f);

            // Mid hp ( 40% to 70%)
            float injured = FuzzyMath.GradeDown(healthPercent, 0.4f, 0.7f);

            // Has shield, the Ai will feel "less danger"
            float thickShield = user.HasShield ? FuzzyMath.GradeUp(user.DurabilityPercent, 0.2f, 0.5f) : 0f;
            float exposed = 1f - thickShield;
            float cheapMana = SpellFuzzyEvaluator.CheapManaRatio(ManaCost, user.MaxMana);

            // RULE EVALUATION
            // Rule 1: Almost die -> number 1
            float ruleEmergency = criticalHealth;

            // Rule 2: Decent hp + no protection (shield)
            float ruleMaintenance = FuzzyMath.AND(injured, exposed);

            // Rule 3: Decent hp + has protection
            float ruleSafeHeal = FuzzyMath.AND(injured, thickShield);

            float ruleBase = 0.3f; // base value

            //SUGENO ==========
            float valEmergency = 4.5f;   
            float valMaintenance = 1.7f;
            float valSafeHeal = 1.2f;

            float valBase = 1.0f;

            float numerator = (ruleEmergency * valEmergency) +
                              (ruleMaintenance * valMaintenance) +
                              (ruleSafeHeal * valSafeHeal) +
                              (ruleBase * valBase);

            float denominator = ruleEmergency + ruleMaintenance + ruleSafeHeal + ruleBase;
            float sugenoMultiplier = numerator / denominator;

            // FINAL
            float score = actualHeal * sugenoMultiplier;

            // Penalty: Mana base on desperation
            // Normal state: full penaty
            // Almost die: prioritize healing
            float basePenaltyFloor = Mathf.Lerp(0.5f, 1.0f, criticalHealth);
            float manaPenaltyMultiplier = Mathf.Lerp(basePenaltyFloor, 1.0f, cheapMana);

            score *= manaPenaltyMultiplier;

            // no accuracy penalty

            return score;
        }

        // Old crisp logic


        //protected override float EvaluateInternal(Agent user, Agent target)
        //{
        //    // 1. Base Effectiveness Calculation =================================================
        //    // If the user is already at full health (> 90%), the spell is not effective
        //    float missingHealth = user.MaxHealth - user.CurrentHealth;
        //    float actualHeal = Mathf.Min(missingHealth, _power);

        //    _spellScore = actualHeal;

        //    //3. Efficiency Check
        //    float efficiency = actualHeal / _power; 

        //    // The user is healthy enough,
        //    if (user.HealthPercent > 0.7f) 
        //    {
        //        // If the user is wasting more than 40% of the spell, don't use it.
        //        if (efficiency < 0.4f) return 0; 
        //    }

        //    // 2. Survival Priorities
        //    // Critical: If the user is near death (< 30%), bonus points
        //    if (user.HealthPercent < 0.3f) 
        //    {
        //        //The user desperately needs this hp or it will die
        //        _spellScore *= 5.0f; // Raw Hp is better than a shield
        //    }
        //    // Warning: If health is low (< 60%), bonus points
        //    else if (user.HealthPercent < 0.5f)
        //    {
        //        _spellScore *= 1.7f;
        //    }

        //    // 3. Shield Context 
        //    // If the user is on low Hp but has a Shield, lower the score
        //    if (user.HasShield && user.DurabilityPercent > 0.3f)
        //    {
        //        _spellScore *= 0.8f;
        //    }

        //    // 4. Costs 
        //    // Mana cost penalty
        //    _spellScore -= _manaCost * 0.5f;

        //    // Return the score
        //    return Mathf.Max(0, _spellScore);
        //}

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Heal the user
            user.Heal(_power);
            //Debug.Log($"{user.name} heals (+{_power} HP)");
        }
    }
}