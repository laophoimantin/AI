using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Poison")]
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
            float estimatedTTL = target.CurrentHealth / 15f /*avg damage*/;
            // If they are going to die fast, the Poison score is low.
            if (estimatedTTL < 2) return 2;

            float totalPotentialDamage = _power * _duration;
            _spellScore = totalPotentialDamage * 1.2f;

            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            target.TakeDamage(2f, true);
            target.AddStatus(new PoisonedStatus(_duration, _power, _icon));
            Debug.Log($"POISON HITS! Dealt {_power} per {_duration}");
        }
    }

    public class PoisonedStatus : StatusEffect
    {
        public PoisonedStatus(int duration, float power, Sprite icon) : base(duration, power, icon)
        {
        }

        public override void Apply(Agent target)
        {
            target.TakeDamage(Power, true);
            Debug.Log($"{target.name} is poisoned! Dealt {Power}");
        }
    }
}