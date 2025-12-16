using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    /// <summary>
    /// A "combo meter" status.
    /// Stores charges for the Zap spell. Decays by 1 if not refreshed this turn.
    /// </summary>
    public class ZapChargeStatus : StatusEffect
    {
        public int CurrentStacks { get; private set; } = 1;
        private int _maxStacks;
        
        private bool _hasUsedThisTurn;

        public ZapChargeStatus(Agent user, int duration, float power, Sprite icon, int maxStacks) 
            : base(user, duration, power, icon)
        {
            _maxStacks = maxStacks;
        }

        // Reset the charge status on turn start
        public override void OnTurnStart()
        {
            _hasUsedThisTurn = false;
        }
        
        // Add a charge to the status if used this turn
        public void AddStack(int amount)
        {
            _hasUsedThisTurn = true;
            CurrentStacks = Mathf.Min(CurrentStacks + amount, _maxStacks);
            Refresh(Duration, 0); 
        }

        // Decrease the charge by 1 if not used this turn
        public override void OnTurnEnd()
        {
            if (!_hasUsedThisTurn && CurrentStacks > 0)
            {
                CurrentStacks = Mathf.Max(CurrentStacks - 1, 0);
            }
        }
    }
}