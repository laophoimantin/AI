using UnityEngine;
using Wizardo;
using StatusEffects;


namespace Spells
{
    /// <summary>
    /// The "Filler" Spell. Low cost, low cooldown, stacking damage.
    /// Good for finishing off enemies, countering shields, or saving mana?
    /// Maybe the AI prioritizes this when Mana is low
    /// 
    /// NO LONGER IN USE
    /// </summary>
    [CreateAssetMenu(menuName = "Spells/Zap")]
    public class ZapSpellSO : BaseSpellSO
    {
        [Header("Utility Config")]
        // Duration in turns for utility spells.
        [SerializeField] protected int _utilityDuration; 
        
        [Header("Zap Config")] 
        [SerializeField] private float _damagePerCharge = 3f;
        [SerializeField] private int _maxCharges = 5;
        [SerializeField] private int _chargeGainPerCast = 1;

        protected override float EvaluateInternal(Agent user, Agent target)
        {
            
            // 1. Base Effectiveness
            // Get current stacks
            int currentStacks = 0;
            var chargeStatus = user.GetStatus<ZapChargeStatus>();
            if (chargeStatus != null)
            {
                currentStacks = chargeStatus.CurrentStacks;
            }
            
            // Calculate potential damage
            float estimatedDamage = _power + (_damagePerCharge * currentStacks); // Damage at next charge
            _spellScore = estimatedDamage;

                
            //2. "Shield Hunter"
            if (target.HasShield) 
            {
                // If the shield has lots of HP, ignoring it makes this spell worth more
                if (target.DurabilityPercent > 0.5f) 
                {
                    _spellScore *= 2f; // This spell is worth
                }
                else
                {
                    _spellScore *= 1.2f; // Small shield, small bonus
                }
            }
            
            // 3. Economy 
            // If the spell is almost at max stacks, make it even more powerful
            if (currentStacks < _maxCharges - 2) // magic number
            {
                _spellScore += 10f; // A flat "potential" bonus
            }
            else
            {
                // The spell is at max stacks, so it's worth a lot more
                _spellScore *= 1.7f; 
            }
        
            
            // If the user has little mana to use other offensive spells
            // If the user has low mana, this spell is worth a lot
            if (user.CurrentMana < 20)
            {
                _spellScore *= 2.5f; // This is time for this spell to shine
            }
            // If the user has High Mana, just use Fireball instead.
            else if (user.ManaPercent > 0.8f)
            {
                _spellScore *= 0.6f; 
            }

            // 4. Kill Confirmation
            // If the target can be killed using this spell, make it super worth a lot
            if (estimatedDamage >= target.CurrentHealth)
            {
                _spellScore += 700f; // Big Magic number, higher than everything else
            }
            
            
            // Return the score
            return Mathf.Max(0, _spellScore);
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            int currentStacks = 0;

            // Check if the user has used this spell before
            var chargeStatus = user.GetStatus<ZapChargeStatus>();
            if (chargeStatus != null)
            {
                // If so, add a charge
                currentStacks = chargeStatus.CurrentStacks;
                chargeStatus.AddStack(_chargeGainPerCast);
            }
            else
            {
                // If not, add a new charge
                var newBuff = new ZapChargeStatus(user,  _utilityDuration, _power, _icon, _maxCharges);
                user.AddStatus(newBuff);
            }

            // Calculate the total damage
            float totalDamage = _power + (_damagePerCharge * currentStacks);

            target.TakeDamage(user, totalDamage, true);
            //Debug.Log($"ZAP! Charges: {currentStacks} -> Damage: {totalDamage}");
        }
    }
}