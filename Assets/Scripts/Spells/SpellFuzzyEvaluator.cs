using Wizardo;

// Evaluator for every spells
public static class SpellFuzzyEvaluator
{
    public static float EnemyLowHealth(float healthPercent)
    {
        return FuzzyMath.GradeDown(healthPercent, 0.6f, 0.2f); // 0.2 HP = 1.0 (die), 0.6 HP = 0.0 (normal)
    }

    public static float EnemyKillable(float damage, float targetHealth, float minRatio = 0.7f, float maxRatio = 1f)
    {
        return targetHealth <= 0 ? 0f : FuzzyMath.GradeUp(damage / targetHealth, minRatio, maxRatio);
        // Damage 80 vs Health 100 = 0.8 ratio → 0.0 (cant kill yet)
        // Damage 120 vs Health 100 = 1.2 ratio → 1.0 (overkill)
    }

    public static float EnemyMediumHealth(float healthPercent)
    {
        return FuzzyMath.Triangle(healthPercent, 0.3f, 0.5f, 0.7f);
    } // 0.5 HP = 1.0 (sweet spot)

    public static float HighAccuracy(float accuracyPercent)
    {
        return FuzzyMath.GradeUp(accuracyPercent, 0.5f, 0.9f);
        // 0.9 acc = 1.0, 0.5 acc = 0.0
    }

    public static float CheapManaRatio(float manaCost, float maxMana)
    {
        return FuzzyMath.GradeDown(manaCost / maxMana, 0.2f, 0.6f);
        // Cost 20/100 = 0.2 → 1.0 (cheap), Cost 60/100 = 0.6 → 0.0 (expen)
    }

    public static float ExcessMana(float currentMana, float maxMana)
    {
        return FuzzyMath.GradeUp(currentMana / maxMana, 0.6f, 0.9f);
        // 90+ mana = 1.0 (no worry), 60- = 0.0 (normal)
    }
}