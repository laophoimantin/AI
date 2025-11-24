using System;
using UnityEngine;

namespace Wizardo
{
    [System.Serializable]
    public abstract class StatusEffect
    {
        public int Duration { get; protected set; }
        public float Power { get; protected set; }

        public Sprite Icon { get; protected set; }

        
     
        public bool IsExpired => Duration <= 0;
        protected StatusEffect(int duration, float power, Sprite icon)
        {
            Duration = duration;
            Power = power;
            Icon = icon;
        }

        public abstract void Apply(Agent target);
        
        public void Refresh(int duration, float power)
        {
            Duration = duration;
            Power = power;
        }

        public void ReduceDuration()
        {
            Duration--;

        }

    }
}