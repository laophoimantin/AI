using UnityEngine;
using Wizardo;
using GameUI;

namespace Spells
{
    /// <summary>
    /// An instance of a spell.
    /// Stores the spell's data and manages cooldowns and execution.
    /// </summary>
    public class SpellInstance : MonoBehaviour
    {
        [Header("UI Reference")]
        [SerializeField] private SpellIcon _icon; 
        
        public BaseSpellSO BaseSpell { get; private set; }
        
        private int _currentCooldown = 0;
        
        public string SpellName => BaseSpell != null ? BaseSpell.Name : "No Spell!";

        // Initialization
        public void Init(BaseSpellSO baseSpell)
        {
            BaseSpell = baseSpell;
            _currentCooldown = 0;       
            
            gameObject.name = $"Instance_{BaseSpell.Name}";
            
            UpdateUI();
        }
     
        // Cooldown
        public void ReduceCooldown()
        {
            if (_currentCooldown > 0)
                _currentCooldown--;

            UpdateUI();
        }

        // Checks if the spell is ready to be executed.
        public bool IsReady(Agent user)
        {
            // Check Cooldown
            if (_currentCooldown > 0) return false;
            // Check Resource
            if (user.CurrentMana < BaseSpell.ManaCost) return false;
        
            return true;
        }

        // Executes the spell.
        public bool ExecuteSpell(Agent user, Agent enemy)
        {
            // Double-check 
            if (!IsReady(user)) 
                return false;

            // Consume Mana
            user.ReduceMana(BaseSpell.ManaCost);
            // Apply Effect
            bool isSuccess = BaseSpell.ApplyEffect(user, enemy);
            // Reset Cooldown
            _currentCooldown = BaseSpell.CooldownTurns;

            UpdateUI();

            return isSuccess;
        }
        
        
        // UI
        private void UpdateUI()
        {
            if (_icon != null && BaseSpell != null)
            {
                _icon.UpdateIcon(BaseSpell.Icon);
                _icon.UpdateCooldown(BaseSpell.CooldownTurns, _currentCooldown);
            }
        }
    }
}