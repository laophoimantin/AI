using UnityEngine;
using Wizardo;

namespace Spellbook
{
    [CreateAssetMenu(menuName = "AI/Spells/Fireball")]
    public class FireBallSO : SpellSO
    {
        [SerializeField] private float _damage = 15f;

        // public override float Evaluate(Agent self, Agent enemy)
        // {
        //     float baseValue = _damage * 1.0f - _manaCost * 0.4f;
        //     return baseValue;
        // }
        public override float Evaluate(Agent self, Agent enemy)
        {
            if (enemy.IsAlive)
            {
                return 0;
            }

            float reduction = enemy.ReductionPercent;
            float damageDealt = _damage * (1.0f - reduction);

            // If this spell can kiel
            if (enemy.CurrentHealth <= damageDealt)
            {
                return 1000;
            }
    
            float baseValue = (damageDealt * 1.0f) - (_manaCost * 0.4f);
            
            //If the enemy is low, damage is more valuable
            if (enemy.CurrentHealth < 30)
            {
                baseValue *= 1.5f;
            }
    
            // Mana cost penalty
            baseValue -= _manaCost * 0.4f;

            return Mathf.Max(0, baseValue);
        }
        

        public override void Cast(Agent self, Agent enemy)
        {
            if (self.CurrentMana < _manaCost) return;
            self.UpdateMana(-_manaCost);
            enemy.ApplyDamage(_damage);
            Debug.Log($"{self.Name} uses {_spellName} (-{_damage} HP)");
        }
    }
}