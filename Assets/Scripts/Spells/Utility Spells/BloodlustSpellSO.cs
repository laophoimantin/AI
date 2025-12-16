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
            // 1. Base Effectiveness
            // If the user doesn't have enough health to sacrifice, or if the user has almost full mana, don't cast
            if (user.CurrentHealth <= _sacrificedHealth + 15f) // (+20f to prevent user from being paralyzed after casting)
            {
                return 0;
            }

            // If the user has almost full mana, don't cast
            if (user.ManaPercent >= 0.9f)
            {
                return 0;
            }

            // 2. Value Calculation 
            // 1 Mana is worth ~1.5 to 2.0 Score Points because it enables damage.
            // 1 HP is worth 1 Score Point.
            float gainScore = _power * 1.8f;
            float painScore = _sacrificedHealth;

            // If the user's health is low, the pain of sacrificing health is higher
            if (user.HealthPercent < 0.5f)
            {
                painScore *= 1.7f;
            }

            if (user.Personality != null)
            {
                // A Gambler (Risk 1.0) thinks the pain is 0!
                painScore = Mathf.Lerp(_sacrificedHealth, 0f, user.Personality.RiskTaking);
            }

            // If the user's mana is low, bonus points
            if (user.ManaPercent < 0.4f)
            {
                gainScore *= 1.5f;
            }


            _spellScore = gainScore - painScore;

            // 3. Costs 
            // Accuracy penalty
            _spellScore -= ManaCost * 0.4f;


            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Sacrifice health for mana
            user.TakeDamage(user, _sacrificedHealth, true);
            user.RegenMana(_power);
            //Debug.Log($"{user.Name} sacrificed {_sacrificedHealth} hp for {_power} mana");
        }
    }
}