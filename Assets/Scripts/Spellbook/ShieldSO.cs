using UnityEngine;
using Wizardo;

namespace Spellbook
{
    [CreateAssetMenu(menuName = "AI/Spells/Shield")]
    public class ShieldSO : SpellSO
    {
        [SerializeField] private float _shieldValue = 8f;
        [SerializeField] private int _shieldDuration = 3;


        // public override float Evaluate(Agent self, Agent enemy)
        // {
        //     float underThreat = enemy.Health is > 0 and >= 10 ? 0.7f : 0.3f;
        //     float baseValue = underThreat * _shieldValue - _manaCost * 0.2f;
        //     return baseValue;
        // } 
        public override float Evaluate(Agent self, Agent enemy)
        {
            if (self.ShieldValue > 0)
            {
                return 0;
            }
            
            float baseValue = _shieldValue;
            
            if (self.Health < 40)
            {
                // The AI desperately needs this shield or it will die
                baseValue *= 2.5f; 
            }
            else if (self.Health < 70)
            {
                // If the health is low, a shield would be a good idea
                baseValue *= 1.5f;
            }
    
            // The enemy has a lot of mana to use a big spell
            if (enemy.CurrentMana > 40)
            {
                baseValue *= 1.2f;
            }

            // Mana cost penalty
            baseValue -= _manaCost * 0.2f;

            return Mathf.Max(0, baseValue);
        }
        
        

        public override void Cast(Agent self, Agent enemy)
        {
            if (self.CurrentMana < _manaCost) return;
            self.CurrentMana -= _manaCost;
            self.AddShield(_shieldValue, _shieldDuration);
            Debug.Log($"{self.Name} raises a shield");
        }
    }
}