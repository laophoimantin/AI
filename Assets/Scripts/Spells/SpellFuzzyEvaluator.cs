using Wizardo.AI;

public static class SpellFuzzyEvaluator
{
    public static float EnemyLowHealth(float healthPercent) => FuzzyMath.GradeDown(healthPercent, 0.6f, 0.2f);

    public static float EnemyKillable(float damage, float targetHealth) => targetHealth <= 0 ? 0f : FuzzyMath.GradeUp(damage / targetHealth, 0.8f, 1.2f);

    public static float EnemyMediumHealth(float healthPercent) => FuzzyMath.Triangle(healthPercent, 0.3f, 0.5f, 0.7f);

    public static float HighAccuracy(float accuracyPercent) => FuzzyMath.GradeUp(accuracyPercent, 0.5f, 0.9f);


    public static float AffordableMana(float manaCost, float currentMana, float maxMana)
    {
        float costRatio = manaCost / maxMana;
        return FuzzyMath.GradeDown(costRatio, 0.2f, 0.6f); // cheap = 1, Expensive = 0
    }

    // Too much mana
    public static float ExcessMana(float currentMana, float maxMana) => FuzzyMath.GradeUp(currentMana / maxMana, 0.6f, 0.9f);
}