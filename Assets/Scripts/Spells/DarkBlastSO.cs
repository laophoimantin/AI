using UnityEngine;
using Wizardo;


namespace Spells
{
    // Big Button
    [CreateAssetMenu(menuName = "AI/Spells/DarkBlast")]
    public class DarkBlastSO : SpellSO
    {
        
        [SerializeField] private float _damage = 30f;
        
        
        public override float Evaluate(Agent self, Agent enemy)
        {
            if (enemy.IsAlive)
            {
                return 0;
            }

            float damageAfterShield = _damage - enemy.ShieldValue;
            float healthDamage = 0;
            float shieldDamage = 0;

            if (damageAfterShield > 0)
            {
                // Spell breaks shield and hits health
                healthDamage = damageAfterShield;
                shieldDamage = enemy.ShieldValue;
            }
            else
            {
                // Spell only hits shield (or does nothing)
                healthDamage = 0;
                shieldDamage = _damage;
            }
    

            // If this spell can kiel
            if (enemy.CurrentHealth <= healthDamage)
            {
                return 800 + _damage; 
            }
            
            // Value health damage and shield damage less
            float baseValue = (healthDamage * 1.0f) + (shieldDamage * 0.5f);

            //If the enemy is low, damage is more valuable
            if (enemy.CurrentHealth < 30)
            {
                baseValue *= 1.5f;
            }
    
            // Mana cost penalty
            baseValue -= _manaCost * 0.4f;

            return Mathf.Max(0, baseValue);
        }

        public override void ApplyEffect(Agent user, Agent enemy)
        {
            if (user.CurrentMana < _manaCost) return;
            user.ModifyMana(-_manaCost);
            enemy.ApplyDamage(_damage);
            Debug.Log($"{user.Name} uses {_name} (-{_damage} HP)");
        }
    }
}