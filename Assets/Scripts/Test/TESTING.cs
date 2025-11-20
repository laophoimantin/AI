using UnityEngine;
using System.Collections.Generic;
using System.Linq;







using UnityEngine;


























// ---------------------------------------------------------
// 1. THE DATA STRUCTURES
// ---------------------------------------------------------


[System.Serializable]
public class Spell
{
    public string spellName;
    public int damage;
    public int healAmount;
    public int manaCost;
    [Range(0f, 1f)] public float accuracy = 1.0f; // 1.0 = 100% hit chance
    public bool isUltimate; // Just for "Cool factor" bias
}

// Represents the current state of a wizard for the AI to analyze
[System.Serializable]
public class WizardState
{
    public int currentHealth;
    public int maxHealth;
    public int currentMana;
}

// ---------------------------------------------------------
// 2. THE PERSONALITY PROFILE (ScriptableObject)
// ---------------------------------------------------------
// Right-click in Project view -> Create -> WizardAI -> Personality Profile
// Create different profiles like "Aggressive Fire Mage" or "Cautious Healer"

[CreateAssetMenu(fileName = "NewPersonality", menuName = "WizardAI/Personality Profile")]
public class AIPersonalitySO : ScriptableObject
{
    
    [Header("Base Biases (Multipliers)")]
    [Tooltip("How much they value dealing damage.")]
    public float aggressionBias = 1.0f; 

    [Tooltip("How much they value keeping their own HP high.")]
    public float selfPreservationBias = 1.0f;

    [Tooltip("How much they hate spending mana.")]
    public float manaHoardingBias = 1.0f;

    [Header("Human Imperfections")]
    [Tooltip("If high, they will ignore low accuracy risks.")]
    public float riskTaking = 0.0f; 

    [Tooltip("If high, they prefer flashy 'Ultimate' spells regardless of efficiency.")]
    public float showOffFactor = 0.0f;

    [Tooltip("The 'Brain Fog'. Higher values mean more random decisions.")]
    [Range(0.1f, 5f)] public float randomness = 0.5f;
}



// ---------------------------------------------------------
// 3. THE BRAIN (MonoBehaviour)
// ---------------------------------------------------------

public class WizardBrain : MonoBehaviour
{
    [Header("Setup")]
    public AIPersonalitySO personality;
    public List<Spell> spellbook;

    // Call this function when it's the AI's turn
    public void TakeTurn(WizardState myState, WizardState enemyState)
    {
        Debug.Log($"--- AI Turn: {personality.name} is thinking... ---");

        // 1. Filter: Get spells we can actually cast (enough mana)
        List<Spell> validSpells = spellbook.Where(s => s.manaCost <= myState.currentMana).ToList();

        if (validSpells.Count == 0)
        {
            Debug.Log("AI has no mana! Skipping turn or basic attack.");
            return;
        }

        // 2. Score: Calculate the "Utility" of every spell
        Dictionary<Spell, float> spellScores = new Dictionary<Spell, float>();

        foreach (Spell spell in validSpells)
        {
            float score = CalculateUtility(spell, myState, enemyState);
            spellScores.Add(spell, score);
            Debug.Log($"AI Considered: {spell.spellName} | Score: {score:F1}");
        }

        // 3. Select: Use Weighted Random Selection (The "Human" element)
        Spell chosenSpell = WeightedRandomChoice(spellScores);

        // 4. Act
        Debug.Log($"<color=green>AI DECIDED:</color> Casting <b>{chosenSpell.spellName}</b>!");
        // ExecuteSpell(chosenSpell); // Hook this up to your battle system
    }

    // The logic engine: "How good is this spell right now?"
    private float CalculateUtility(Spell spell, WizardState me, WizardState enemy)
    {
        float score = 0;

        // --- FACTOR 1: OFFENSE (Modified by Aggression) ---
        // We assume 1 damage = 1 point of utility, scaled by personality
        float predictedDamage = spell.damage * spell.accuracy; // Factor in miss chance?
        
        // A Risk Taker ignores the accuracy penalty. 
        // If RiskTaking is 1.0, they treat 50% acc as 100% acc.
        float perceivedAccuracy = Mathf.Lerp(spell.accuracy, 1.0f, personality.riskTaking);
        float damageUtility = (spell.damage * perceivedAccuracy) * personality.aggressionBias;

        // Bonus: If this kills the enemy, MASSIVE bonus (Killer instinct)
        if (spell.damage >= enemy.currentHealth) damageUtility += 500;

        score += damageUtility;


        // --- FACTOR 2: DEFENSE (Modified by Self Preservation) ---
        float healthPct = (float)me.currentHealth / me.maxHealth;
        float healUtility = spell.healAmount * personality.selfPreservationBias;

        // Dynamic Urgency: Healing is worth WAY more if we are dying
        if (healthPct < 0.3f) healUtility *= 3.0f; // Panic multiplier
        
        score += healUtility;


        // --- FACTOR 3: EFFICIENCY (Modified by Mana Hoarding) ---
        // Costs are negative utility
        float costUtility = spell.manaCost * personality.manaHoardingBias;
        score -= costUtility;


        // --- FACTOR 4: THE "COOL" FACTOR ---
        if (spell.isUltimate) score += (20 * personality.showOffFactor);


        // --- FINAL: NOISE ---
        // Add pure randomness so they don't calculate the exact same number every time
        // This simulates "mood" or minor miscalculations
        score += UnityEngine.Random.Range(-5.0f, 5.0f);

        // Ensure we don't have negative scores for the probability wheel
        return Mathf.Max(0.1f, score);
    }

    // This is the secret sauce to making AI unpredictable.
    // Instead of `scores.Max()`, we put all scores in a "raffle".
    // Better spells have more tickets, but bad spells still have a chance.
    private Spell WeightedRandomChoice(Dictionary<Spell, float> scores)
    {
        // Softmax-ish approach: Raise scores to a power to accentuate differences
        // Low "randomness" (high power) makes the best choice more likely.
        // High "randomness" (low power) makes choices more equal.
        float exponent = 1.0f / personality.randomness; 

        float totalWeight = 0;
        Dictionary<Spell, float> weightedScores = new Dictionary<Spell, float>();

        foreach(var kvp in scores)
        {
            float weight = Mathf.Pow(kvp.Value, exponent);
            weightedScores[kvp.Key] = weight;
            totalWeight += weight;
        }

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float cursor = 0;

        foreach (var kvp in weightedScores)
        {
            cursor += kvp.Value;
            if (cursor >= randomValue)
            {
                return kvp.Key;
            }
        }

        // Fallback (should rarely happen)
        return scores.Keys.First();
    }
}