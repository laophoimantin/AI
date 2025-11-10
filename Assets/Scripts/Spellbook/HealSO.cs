using UnityEngine;
using Wizardo;

namespace Spellbook
{
    [CreateAssetMenu(menuName = "AI/Spells/Heal")]
    public class HealSO : SpellSO
    {
        [SerializeField] private float _healAmount = 20f;

        public override float Evaluate(Agent self, Agent enemy)
        {
            float missingHealth = 100 - self.Health;
            if (missingHealth < 5)  // If the AI has enough health, don't cast
            {
                return 0;
            }
            
            float actualHealAmount = Mathf.Min(missingHealth, _healAmount);
            
            float baseValue = actualHealAmount * 1f;
            
            if (self.Health < 30) // If the AI is low on health
            {
                baseValue *= 3.0f; 
            }
            else if (self.Health < 60) // If the AI is medium on health
            {
                baseValue *= 1.5f;
            }
            
            // Mana cost penalty
            baseValue -= _manaCost * 0.2f;
            
            return Mathf.Max(0, baseValue);
        }
        
        public override void Cast(Agent self, Agent enemy)
        {
            if (self.CurrentMana < _manaCost) return;
            self.CurrentMana -= _manaCost;
            self.Health = Mathf.Min(100, self.Health + _healAmount);
            Debug.Log($"{self.name} heals (+{_healAmount} HP)");
        }
        
    }
}