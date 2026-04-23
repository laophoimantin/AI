using Wizardo;

public static class OffensiveSpellFuzzy
{
    //Opening Move (In the start, if the enemy has high hp
    public static float OpeningMove(float enemyHealthPercent, float min = 0.6f, float max = 0.9f)
    {
        return FuzzyMath.GradeUp(enemyHealthPercent, min, max);
    }

    // High risk high reward
    public static float HighRiskHighReward(float accuracy, float potentialDamage, float threshold = 100f)
    {
        return FuzzyMath.AND(
            FuzzyMath.GradeDown(accuracy, 0.3f, 0.7f),  // High Risk
            FuzzyMath.GradeUp(potentialDamage, threshold * 0.5f, threshold * 1.5f)  // High Reward
        );
    }

    // --- Finisher (reliable kill) ---
    public static float ReliableFinisher(float killPotential, float accuracy)
    {
        return FuzzyMath.AND(killPotential, accuracy);
    }
    // --- Overkill Penalty (wasted damage) ---
    public static float OverkillWaste(float damage, float targetHealth)
    {
        return targetHealth <= 0 ? 0f : FuzzyMath.GradeUp(damage / targetHealth, 1.0f, 2.0f);
    }
    // > 2x health = high waste (1.0), normal = 0 waste (0.0)
}