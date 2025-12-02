using StatusEffects.Shield;
using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Shield")]
    public class ShieldSpellSO : BaseSpellSO
    {
        [Range (0, 1)]
        [SerializeField] private float _reductionPercent  = 0.5f;
        [SerializeField] private float _shieldDurability = 8f;
        [SerializeField] private int _shieldDuration = 3;
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            _spellScore = _shieldDurability;
            
            if (user.HasShield)
            {
                _spellScore *= 0.3f;
            }
            
            if (user.CurrentHealth < 30)
            {
                // The AI desperately needs this shield or it will die
                _spellScore *= 2.5f; 
            }
            else if (user.CurrentHealth < 70)
            {
                // If the health is low, a shield would be a good idea
                _spellScore *= 1.2f;
            }
    
            // The enemy has a lot of mana to use a big spell
            if (target.CurrentMana > 40)
            {
                _spellScore *= 1.2f;
            }

            // Mana cost penalty
            _spellScore -= _manaCost * 0.2f;
            return Mathf.Max(0, _spellScore);
        }
        protected override void SpellEffect(Agent user, Agent target)
        {
            user.AddStatus(new NormalBaseShieldStatus(_shieldDuration, _reductionPercent, _icon, _reductionPercent, _shieldDurability));
            Debug.Log($"{user.Name} raises a shield");
        }
    }
}