using UnityEngine;
using Wizardo;
using StatusEffects;

namespace Spells
{
    [CreateAssetMenu(menuName = "Spells/Poison")]
    public class PoisonSpellSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            if (target.HasStatus<PoisonedStatus>())
            {
                _spellScore = 5f;
                return Mathf.Max(0, _spellScore);
            }
            // TTL "Time To Live"
            float estimatedTtl = target.CurrentHealth / 15f /*avg damage*/;
            // If they are going to die fast, the Poison score is low.
            if (estimatedTtl < 2) return 2;

            float totalPotentialDamage = _power * _utilityDuration;
            _spellScore = totalPotentialDamage;

            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            // Add the poison status effect to the target
            target.AddStatus(new PoisonedStatus(user, target, _utilityDuration, _power, _icon));
            //Debug.Log($"POISON HITS! Dealt {_power} per {_utilityDuration}");
        }
    }

}