using UnityEngine;
using Wizardo;
using StatusEffects;

namespace Spells
{
    /// <summary>
    /// Zap Spell, small chip damage with low mana and 0 cooldowns, but charges up over time to deal with more damage.
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Zap")]
    public class ZapSpellSO : BaseSpellSO
    {
        [Header("Zap Config")] [SerializeField]
        private float _damagePerCharge = 3f;
        [SerializeField] private int _maxCharges = 5;
        [SerializeField] private int _chargeGainPerCast = 1;

        protected override float EvaluateInternal(Agent user, Agent target)
        {
            int currentStacks = 0;

            var chargeStatus = user.GetStatus<ZapChargeStatus>();
            if (chargeStatus != null)
            {
                currentStacks = chargeStatus.CurrentStacks;
            }

            // 2. Tính Damage ước tính
            float estimatedDamage = _power + (_damagePerCharge * currentStacks);

            if (estimatedDamage >= target.CurrentHealth)
                return estimatedDamage + 9999;

            if (currentStacks >= _maxCharges - 1)
            {
                estimatedDamage *= 2.0f;
            }

            if (user.CurrentMana < 35)
            {
                estimatedDamage *= 1.5f;
            }

            return estimatedDamage;
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            int currentStacks = 0;

            var chargeStatus = user.GetStatus<ZapChargeStatus>();

            if (chargeStatus != null)
            {
                currentStacks = chargeStatus.CurrentStacks;
                chargeStatus.AddStack(_chargeGainPerCast);
            }
            else
            {
                var newBuff = new ZapChargeStatus(_duration, _power, _icon, _maxCharges);
                user.AddStatus(newBuff);
            }

            float totalDamage = _power + (_damagePerCharge * currentStacks);

            target.TakeDamage(totalDamage, true);
            Debug.Log($"ZAP! Charges: {currentStacks} -> Damage: {totalDamage}");
        }
    }
}