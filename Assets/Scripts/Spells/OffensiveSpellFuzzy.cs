using Wizardo;

// Todo: Use Anim Curve instead of hardcoding!!!

public static class OffensiveSpellFuzzy
{
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