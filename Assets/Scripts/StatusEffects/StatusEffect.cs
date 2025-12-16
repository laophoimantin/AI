using UnityEngine;
using Wizardo;

namespace StatusEffects
{
    /// <summary>
    /// The runtime instance of a status effect (Poison, Shield, Stacks).
    /// </summary>
    [System.Serializable]
    public abstract class StatusEffect
    {
        public Agent User { get; protected set; } // The agent that cast this effect.
        public int Duration { get; protected set; } 
        public float Power { get; protected set; } 
        // Display
        public Sprite Icon { get; protected set; }

        // Constructor
        protected StatusEffect(Agent user, int duration, float power, Sprite icon)
        {
            User = user;
            Duration = duration;
            Power = power;
            Icon = icon;
        }


        // General Events
        /// <summary>
        /// Called immediately when the status is added to the Agent.
        /// Use this for instant effects (poison deals chip damage when added)
        /// </summary>
        public virtual void OnApply(){} // Called when the effect is applied first time to the target.
        
        /// <summary>
        /// Called at the start of the owner's turn.
        /// (Damage Over Time (poison))
        /// </summary>
        public virtual void OnTurnStart(){}
        
        /// <summary>
        /// Called at the end of the Owner's turn.
        /// (Expiring shields, reducing duration)
        /// </summary>
        public virtual void OnTurnEnd(){} 
        
        /// <summary>
        /// Called when duration hits 0 or the effect is cleansed.
        /// (Trigger explosions (explosive Shield))
        /// </summary>
        public virtual void OnExpire(){} 
        
        /// <summary>
        /// Called if the same status is applied again.
        /// Default behavior: Refresh duration to the new max.
        /// (Stacking logic (zap))
        /// </summary>
        public virtual void Refresh(int duration, float power)
        {
            Duration = duration;
            Power = power;
        }
        
        
        //public virtual void ApplyEffect(){} // Called when the effect is called in a specific case.
        
        // Helper Methods
        public void ReduceDuration() => Duration--; // Decrease duration by 1.
        public bool IsExpired => Duration <= 0; // Returns true if duration is reached.
    }
}