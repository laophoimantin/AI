using UnityEngine;
using Wizardo;

namespace Spells
{
    /// <summary>
    /// Utility spell. The user sacrifices a fixed amount of health to regain mana. Opposite of Heal.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Bloodlust")]
    public class BloodlustSpellSO : BaseSpellSO
    {
        [Header("Bloodlust Spell Config")]
        [SerializeField] private float _sacrificedHealth = 20f;

        protected override float EvaluateInternal(Agent user, Agent target)
        {
            // Gatekeeper
            if (user.CurrentHealth <= _sacrificedHealth + 15f) return 0f; // Prevent Suside 

            if (user.ManaPercent >= 0.8f) return 0f;

            // Input variables
            float baseValue = _power;
            float healthPercent = user.HealthPercent;
            float manaPercent = user.ManaPercent;
            float accuracy = GetPerceivedAccuracy(user);

            // Fuzzy sets
            float lowMana = FuzzyMath.GradeDown(manaPercent, 0.1f, 0.4f);
            float averageMana = FuzzyMath.GradeUp(manaPercent, 0.3f, 0.6f);
            float safeHealth = FuzzyMath.GradeUp(healthPercent, 0.4f, 0.7f);
            float criticalHealth = FuzzyMath.GradeDown(healthPercent, 0.2f, 0.5f);

            // --- PSYCHOLOGY ---
            // Core AI behavior: perceived danger depends on the agent's RiskTaking value.
            // Risk = 0 (very cautious) -> fearOfDeath mirrors criticalHealth (low HP triggers strong fear).
            // Risk = 1 (reckless) -> fearOfDeath = 0 (no fear, even at 1 HP).
            float riskTaking = user.Personality != null ? user.Personality.RiskTaking : 0.5f;
            float fearOfDeath = Mathf.Lerp(criticalHealth, 0f, riskTaking);

            // RULE EVALUATION
            // Rule 1:(Safe Investment)
            float ruleSafeInvestment = FuzzyMath.AND(lowMana, safeHealth);

            // Rule 2: Gamble (High Risk)
            // Low mana + critical health.
            // Uses (1f - fearOfDeath) as a factor: cautious agents evaluate this rule near 0,
            // while risk-taking agents strongly activate it.
            float ruleGamble = FuzzyMath.AND(lowMana, criticalHealth, 1f - fearOfDeath);

            // Rule 3: Top Off (Resource Management)
            // Health is high and mana is sufficient; convert a bit more mana when possible.
            float ruleTopOff = FuzzyMath.AND(averageMana, safeHealth);

            float ruleBase = 0.2f;

            // DEFUZZIFICATION (Sugeno)
            float valSafeInvestment = 2.8f; // High-value action -> x2.8
            float valGamble = 2.5f;       // High risk, high reward -> x2.5 (mainly for risk-takers)
            float valTopOff = 1.3f;         // Minor gain -> x1.3
            float valBase = 1.0f;

            float numerator = (ruleSafeInvestment * valSafeInvestment) +
                              (ruleGamble * valGamble) +
                              (ruleTopOff * valTopOff) +
                              (ruleBase * valBase);

            float denominator = ruleSafeInvestment + ruleGamble + ruleTopOff + ruleBase;
            float sugenoMultiplier = numerator / denominator;

            // FINAL
            float score = baseValue * sugenoMultiplier;

            // Pain Penalty (Health Loss):
            // If the AI fears death (high fearOfDeath), it penalizes this action
            // (reducing its value down to 30%).
            // If the AI is risk-seeking (fearOfDeath = 0), the penalty is x1.0 (no penalty).
            float painPenaltyMultiplier = Mathf.Lerp(1.0f, 0.3f, fearOfDeath);
            score *= painPenaltyMultiplier;

            score *= accuracy;

            return score;
        }

        //protected override float EvaluateInternal(Agent user, Agent target)
        //{
        //    // 1. Base Effectiveness
        //    // If the user doesn't have enough health to sacrifice, or if the user has almost full mana, don't cast
        //    if (user.CurrentHealth <= _sacrificedHealth + 15f) // (+20f to prevent user from being paralyzed after casting)
        //    {
        //        return 0;
        //    }

        //    // If the user has almost full mana, don't cast
        //    if (user.ManaPercent >= 0.9f)
        //    {
        //        return 0;
        //    }

        //    // 2. Value Calculation 
        //    // 1 Mana is worth ~1.5 to 2.0 Score Points because it enables damage.
        //    // 1 HP is worth 1 Score Point.
        //    float gainScore = _power * 1.8f;
        //    float painScore = _sacrificedHealth;

        //    // If the user's health is low, the pain of sacrificing health is higher
        //    if (user.HealthPercent < 0.5f)
        //    {
        //        painScore *= 1.7f;
        //    }

        //    if (user.Personality != null)
        //    {
        //        // A Gambler (Risk 1.0) thinks the pain is 0!
        //        painScore = Mathf.Lerp(_sacrificedHealth, 0f, user.Personality.RiskTaking);
        //    }

        //    // If the user's mana is low, bonus points
        //    if (user.ManaPercent < 0.4f)
        //    {
        //        gainScore *= 1.5f;
        //    }


        //    _spellScore = gainScore - painScore;

        //    // 3. Costs 
        //    // Accuracy penalty
        //    _spellScore -= ManaCost * 0.4f;


        //    // Return the score
        //    return Mathf.Max(0, _spellScore);
        //}

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Sacrifice health for mana
            user.TakeDamage(user, _sacrificedHealth, true);
            user.RegenMana(_power);
            //Debug.Log($"{user.Name} sacrificed {_sacrificedHealth} hp for {_power} mana");
        }
    }
}