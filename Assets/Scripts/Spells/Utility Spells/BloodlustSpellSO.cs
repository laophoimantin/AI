using UnityEngine;
using Wizardo;

namespace Spells
{
    // Health for mana/damage
    [CreateAssetMenu(menuName = "Spells/Bloodlust")]
    public class BloodlustSpellSO : BaseSpellSO
    {
        [SerializeField] private float _sacrificedHealth = 20f;
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            if (user.CurrentHealth <= _power) return 0;
            if (user.CurrentMana >= user.MaxMana - 5) return 0;

            float gainScore = _power * 1.5f;

            float painScore = _sacrificedHealth;

            if (user.Personality != null)
            {
                painScore = Mathf.Lerp(_sacrificedHealth, 0f, user.Personality.RiskTaking);
            }

            if (user.CurrentMana < 10)
            {
                gainScore += 50f;
            }

            _spellScore = gainScore - painScore;
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Sacrifice health for mana
            user.TakeDamage(user, _sacrificedHealth);
            user.RegenMana(_power);
            //Debug.Log($"{user.Name} sacrificed {_sacrificedHealth} hp for {_power} mana");
        }
    }
}