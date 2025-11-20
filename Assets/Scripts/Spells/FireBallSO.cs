using UnityEngine;
using Wizardo;

namespace Spells
{
    // Reliable
    [CreateAssetMenu(menuName = "AI/Spells/Fireball")]
    public class FireBallSO : SpellSO
    {
        public override float Evaluate(Agent user, Agent enemy)
        {
            if (!enemy.IsAlive) 
                return 0;

            float reduction = enemy.ReductionPercent;
            float damageDealt = _power * (1.0f - reduction);
            
            // If this spell can kiel
            if (enemy.CurrentHealth <= damageDealt) 
                return 1000f;
            
            float baseValue = (damageDealt * 1.0f) - (_manaCost * 0.4f);
            //If the enemy is low, damage is more valuable
            if (enemy.CurrentHealth < 30) 
                baseValue *= 1.5f;
            
            return Mathf.Max(0, baseValue);
        }
        

        public override void ApplyEffect(Agent user, Agent enemy)
        {
            if (user.CurrentMana < ManaCost) return;
            user.ModifyMana(-_manaCost);
            
            if (Random.value <= _accuracy)
            {
                // HIT!
                enemy.ApplyDamage(_power);
                Debug.Log($"{user.Name} hits with {_name}!");
            }
            else
            {
                // MISS!
                Debug.Log($"{user.Name} MISSED {_name}!");
            }
        }
    }
}