
using UnityEngine;
using Wizardo;

namespace Spells
{
    // Replaced with Bloodlust
    [CreateAssetMenu(menuName = "Spells/Meditate")]
    public class MeditateSO : BaseSpellSO
    {
        protected override float EvaluateInternal(Agent user, Agent target)
        {
            if (user.CurrentMana >= user.MaxMana) return 0;

            float missingMana = user.MaxMana - user.CurrentMana;
            _spellScore = missingMana * 2.0f; 

       
            if (user.CurrentMana < 10) 
            {
                _spellScore += 50; 
            }
            
            if (user.CurrentHealth < 20)
            {
                _spellScore = 0; 
            }

            return _spellScore;
        }

        protected override void SpellEffect(Agent user, Agent target)
        {
            user.Heal(_power);
            Debug.Log($"{user.Name} Meditates and recovers {_power} Mana!");
        }
    }
}