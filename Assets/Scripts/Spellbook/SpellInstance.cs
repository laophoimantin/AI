using UnityEngine;
using Wizardo;

namespace Spellbook
{
    public class SpellInstance
    {
        #region Private Fields

        private readonly SpellSO _spell;
        private int _currentCooldown;

        #endregion

        #region Public Fields

        public SpellSO Spell => _spell;
        public string GetSpellName => Spell.SpellName;

        #endregion

        public SpellInstance(SpellSO spell)
        {
            _spell = spell;
            _currentCooldown = 0;
        }

        public void TickCooldown()
        {
            if (_currentCooldown > 0)
                _currentCooldown--;
        }

        public bool CanCast(Agent self)
        {
            return _currentCooldown == 0 && self.CurrentMana >= Spell.ManaCost;
        }

        public void Cast(Agent self, Agent enemy)
        {
            if (!CanCast(self)) return;
            Spell.Cast(self, enemy);
            _currentCooldown = Spell.CooldownTurns;
        }


        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        void BuddaBlessing()
        {
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //                   _ooOoo_
            //                  o8888888o
            //                  88" . "88
            //                  (| -_- |)
            //                  O\  =  /O
            //               ____/`---'\____
            //             .'  \\|     |//  `.
            //            /  \\|||  :  |||//  \
            //           /  _||||| -:- |||||-  \
            //           |   | \\\  -  /// |   |
            //           | \_|  ''\---/''  |   |
            //           \  .-\__  `-`  ___/-. /
            //         ___`. .'  /--.--\  `. . __
            //      ."" '<  `.___\_<|>_/___.'  >'"".
            //     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
            //     \  \ `-.   \_ __\ /__ _/   .-` /  /
            //======`-.____`-.___\_____/___.-`____.-'======
            //                   `=---='
            //
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            //          佛祖保佑           永无BUG
            //         God Bless        Never Crash
            //        Phật phù hộ, không bao giờ BUG
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }
}