using UnityEngine;
using Wizardo;

namespace Spells
{
    [CreateAssetMenu(menuName = "AI/Spells/Shield")]
    public class ShieldSpellSO : BaseSpellSO
    {
        [Range (0, 1)]
        [SerializeField] private float _reductionPercent  = 0.5f;
        [SerializeField] private float _shieldValue = 8f;
        [SerializeField] private int _shieldDuration = 3;


        // public override float Evaluate(Agent self, Agent enemy)
        // {
        //     float underThreat = enemy.Health is > 0 and >= 10 ? 0.7f : 0.3f;
        //     float baseValue = underThreat * _shieldValue - _manaCost * 0.2f;
        //     return baseValue;
        // } 
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            if (!target.IsAlive)
                return 0;
            if (user.CurrentMana < _manaCost) 
                return 0;
            
            
            if (user.ShieldValue > 0)
                return 0;
            
            float baseValue = _shieldValue;
            
            if (user.CurrentHealth < 40)
            {
                // The AI desperately needs this shield or it will die
                baseValue *= 2.5f; 
            }
            else if (user.CurrentHealth < 70)
            {
                // If the health is low, a shield would be a good idea
                baseValue *= 1.5f;
            }
    
            // The enemy has a lot of mana to use a big spell
            if (target.CurrentMana > 40)
            {
                baseValue *= 1.2f;
            }

            // Mana cost penalty
            baseValue -= _manaCost * 0.2f;
            return Mathf.Max(0, baseValue);
        }
        
        

        protected override void SpellEffect(Agent user, Agent target)
        {
            user.AddShield(_reductionPercent, _shieldValue, _shieldDuration);
            Debug.Log($"{user.Name} raises a shield");
        }
    }
}