/*
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                      _ooOoo_
                     o8888888o
                     88" . "88
                     (| -_- |)
                     O\  =  /O
                  ____/`---'\____
                .'  \\|     |//  `.
               /  \\|||  :  |||//  \
              /  _||||| -:- |||||-  \
              |   | \\\  -  /// |   |
              | \_|  ''\---/''  |   |
              \  .-\__  `-`  ___/-. /
            ___`. .'  /--.--\  `. . __
         ."" '<  `.___\_<|>_/___.'  >'"".
        | | :  `- \`.;`\ _ /`;.`/ - ` : | |
        \  \ `-.   \_ __\ /__ _/   .-` /  /
    ======`-.____`-.___\_____/___.-`____.-'======
                      `=---='
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
              佛祖保佑           永无BUG
             God Bless        Never Crash
           Phật phù hộ, không bao giờ BUG
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
*/

using UnityEngine;
using Wizardo;

namespace Spells
{
    public class SpellInstance : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameUI.SpellIcon _icon; 
        
        public SpellSO Spell { get; private set; }
        private int _currentCooldown = 0;
        
        public string GetSpellName => Spell.Name;

        public void Init(SpellSO spell)
        {
            Spell = spell;
            _currentCooldown = 0;
            
            gameObject.name = $"Instance_{Spell.Name}";
            
            if (_icon != null)
            {
                _icon.UpdateIcon(Spell.Icon);
                _icon.UpdateCooldown(Spell.CooldownTurns, _currentCooldown);
            }
        }

        public void ReduceCooldown()
        {
            if (_currentCooldown > 0)
                _currentCooldown--;
            
            if (_icon != null)
                _icon.UpdateCooldown(Spell.CooldownTurns, _currentCooldown);
        }

        public bool IsReady(Agent user)
        {
            if (_currentCooldown > 0) return false;
        
            if (user.CurrentMana < Spell.ManaCost) return false;
        
            return true;
        }

        public void ExecuteSpell(Agent user, Agent enemy)
        {
            if (!IsReady(user))
                return;
            
            Spell.ApplyEffect(user, enemy);
            _currentCooldown = Spell.CooldownTurns + 1;
            
            if (_icon != null)
                _icon.UpdateCooldown(Spell.CooldownTurns, _currentCooldown);
            
        }
    }
}