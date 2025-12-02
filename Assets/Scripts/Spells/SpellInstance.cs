
using UnityEngine;
using Wizardo;

namespace Spells
{
    public class SpellInstance : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameUI.SpellIcon _icon; 
        
        public BaseSpellSO BaseSpell { get; private set; }
        private int _currentCooldown = 0;
        
        public string GetSpellName => BaseSpell.Name;

        public void Init(BaseSpellSO baseSpell)
        {
            BaseSpell = baseSpell;
            _currentCooldown = 0;       
            
            gameObject.name = $"Instance_{BaseSpell.Name}";
            
            if (_icon != null)
            {
                _icon.UpdateIcon(BaseSpell.Icon);
                _icon.UpdateCooldown(BaseSpell.CooldownTurns, _currentCooldown);
            }
        }

        public void ReduceCooldown()
        {
            if (_currentCooldown > 0)
                _currentCooldown--;
            
            if (_icon != null)
                _icon.UpdateCooldown(BaseSpell.CooldownTurns, _currentCooldown);
        }

        public bool IsReady(Agent user)
        {
            if (_currentCooldown > 0) return false;
        
            if (user.CurrentMana < BaseSpell.ManaCost) return false;
        
            return true;
        }

        public void ExecuteSpell(Agent user, Agent enemy)
        {
            if (!IsReady(user))
                return;
            
            BaseSpell.ApplyEffect(user, enemy);
            _currentCooldown = BaseSpell.CooldownTurns + 1;
            
            if (_icon != null)
                _icon.UpdateCooldown(BaseSpell.CooldownTurns, _currentCooldown);
            
        }
    }
}