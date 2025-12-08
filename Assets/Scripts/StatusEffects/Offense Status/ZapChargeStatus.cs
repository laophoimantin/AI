using UnityEngine;
using Wizardo;

namespace StatusEffects
{
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

        public override void OnTurnStart()
        {
            _hasUsedThisTurn = false;
        }
        
        public void AddStack(int amount)
        {
            _hasUsedThisTurn = true;
            CurrentStacks = Mathf.Min(CurrentStacks + amount, _maxStacks);
            Refresh(Duration, 0); 
        }

        public override void OnTurnEnd()
        {
            if (!_hasUsedThisTurn && CurrentStacks > 0)
            {
                CurrentStacks = Mathf.Max(CurrentStacks - 1, 0);
            }
        }
    }
}