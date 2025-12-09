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
            // Calculate Probability
            // Check if the user would actually risk using this spell if the spell has low accuracy
            // It would be funny to see the agent to stab himself for a spell that might not work
            float perceivedAcc = GetPerceivedAccuracy(user);

            // 1. Base Effectiveness
            // If the user doesn't have enough health to sacrifice, or if the user has almost full mana, don't cast
            if (user.CurrentHealth <= _sacrificedHealth)
            {
                return 0;
            }
            // If the user has almost full mana, don't cast
            if (user.ManaPercent >= 0.9f)
            {
                return 0;
            }

            // 2. Value Calculation ====================================================
            // 1 Mana is worth ~1.5 to 2.0 Score Points because it enables damage.
            // 1 HP is worth 1 Score Point.
            float gainScore = _power * 1.8f;
            float painScore = _sacrificedHealth;

            // If the user's mana is low, bonus points
            if (user.ManaPercent < 0.3f)
            {
                gainScore *= 1.5f;
            }

            if (user.Personality != null)
            {
                // A Gambler (Risk 1.0) thinks the pain is 0!
                painScore = Mathf.Lerp(_sacrificedHealth, 0f, user.Personality.RiskTaking);
            }


            
            _spellScore = gainScore - painScore;

            _spellScore *= perceivedAcc;
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