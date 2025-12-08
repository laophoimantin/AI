using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    [System.Serializable]
    public abstract class StatusEffect
    {
        public Agent User { get; protected set; }
        
        public int Duration { get; protected set; }
        public float Power { get; protected set; }

        public Sprite Icon { get; protected set; }


        protected StatusEffect(Agent user, int duration, float power, Sprite icon)
        {
            User = user;
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
        public virtual void OnApply(){} // Called when the effect is applied first time to the target.
        public virtual void ApplyEffect(){} // Called when the effect is called in a specific case.
        public virtual void OnTurnStart(){} // Called every turn before the effect is applied.
        public virtual void OnTurnEnd(){} // Called every turn after the effect is applied.
        public virtual void OnExpire(){} // Called when the effect expires.
    }

}