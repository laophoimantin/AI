using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    [System.Serializable]
    public abstract class StatusEffect
    {
        public int Duration { get; protected set; }
        public float Power { get; protected set; }

        public Sprite Icon { get; protected set; }


        protected StatusEffect(int duration, float power, Sprite icon)
        {
            Duration = duration;
            Power = power;
            Icon = icon;
        }

        public void ReduceDuration() => Duration--;
        public bool IsExpired => Duration <= 0;

        public virtual void Refresh(int duration, float power)
        {
            Duration = duration;
            Power = power;
        }

        // General Events
        public virtual void OnApply(Agent target){} // Called when the effect is applied first time to the target.
        public virtual void ApplyEffect(Agent target){} // Called when the effect is called in a specific case.
        public virtual void OnTurnStart(Agent target){} // Called every turn before the effect is applied.
        public virtual void OnTurnEnd(Agent target){} // Called every turn after the effect is applied.
        public virtual void OnExpire(Agent target){} // Called when the effect expires.
    }

}