using UnityEngine;
using Wizardo;

namespace Spells
{
    public struct StrategyAlignment
    {
        [Range(0f, 1f)] public float Aggressive;
        [Range(0f, 1f)] public float Defensive;
        [Range(0f, 1f)] public float Balanced;

        public static StrategyAlignment PureOffense => new() { Aggressive = 1f, Balanced = 0.2f };
        public static StrategyAlignment PureDefense => new() { Defensive = 1f, Balanced = 0.2f };
        public static StrategyAlignment Hybrid => new() { Aggressive = 0.5f, Defensive = 0.5f, Balanced = 0.5f };
        public static StrategyAlignment BalancedOnly => new() { Balanced = 1f };
    }

    public struct StrategyContext
    {
        public float Aggressive;
        public float Defensive;
        public float Balanced;
    }

    public enum SpellType { Offense, Defense, Utility}
    
    public abstract class BaseSpellSO : ScriptableObject
    {
        // DATA CONFIGURATION ======
        [Header("Identity")]
        [SerializeField] protected string _name;
        [TextArea] [SerializeField] protected string _description; 
        [SerializeField] protected Sprite _icon;
        
        [Header("Economy")]
        [SerializeField] protected float _manaCost;
        [SerializeField] protected int _cooldownTurns;
        
        [Header("The Payload")]
        // The generic value of the spell.
        [SerializeField] protected float _power; // Damage, heal amount.... 
        [SerializeField, Range(0f, 1f)] protected float _successRate = 1.0f; // Accuracy (0.0 to 1.0)


        
        [Header("Spell Type")]
        [SerializeField] protected SpellType[] _types;

        // Store the spell's score during evaluation
        protected float _spellScore; 
        
        
        // Public Getters ======
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float ManaCost => _manaCost;
        public int CooldownTurns => _cooldownTurns;
        public SpellType[] Types => _types;
        public float Power => _power;
        public float SuccessRate => _successRate;


        [Header("AI Strategy Alignment")]
        [SerializeField] protected StrategyAlignment _alignment = StrategyAlignment.BalancedOnly;
        public StrategyAlignment Alignment => _alignment;



        
        // CORE LOGIC =======
        
        /// <summary>
        /// The public entry point for the AI to judge this spell.
        /// Acts as a "Gatekeeper" to filter out invalid moves (no mana, target dead...)
        /// before actually running the evaluation logic.
        /// </summary>
        public float Evaluate(Agent user, Agent target)
        {
            if (!target.IsAlive) return 0;
            if (user.CurrentMana < ManaCost) return 0;

            return Mathf.Max(0, EvaluateInternal(user, target));
        }

        /// <summary>
        /// Calculates a "Utility Score" for this spell based on the current battle state.
        /// High Score = AI is more likely to cast this.
        /// </summary>
        protected abstract float EvaluateInternal(Agent user, Agent target);
        /// <summary>
        /// The unique logic for the spell (deal damage, add status...).
        /// This is only called if the cast succeeds.
        /// </summary>
        protected abstract void SpellEffect(Agent user, Agent target);


        /// <summary>
        /// Attempts to cast the spell.
        /// Handles the Dice Roll (success/fail).
        /// </summary>
        public bool ApplyEffect(Agent user, Agent target)
        {
            // Note: Mana consumption is handled by the SpellInstance, not here.
            return TryCast(user, target);
        }

        private bool TryCast(Agent user, Agent target)
        {
            // Roll the dice
            if (CheckHitSuccess())
            {
                //Success: Execute
                SpellEffect(user,target);
                Debug.Log($"{user.Name} successful to cast with {_name}!");
                return true;
            }
            else
            {
                // Fail: Do nothing
                Debug.Log($"{user.Name} failed to cast with {_name}!");
                return false;
            }
        }
        
        /// Rolls a random float (0.0 to 1.0) against the Success Rate.
        private bool CheckHitSuccess()
        {
            float roll = Random.Range(0f, 1f);
            return roll <= _successRate;
        }
        
        /// <summary>
        /// Calculates how the Agent "feels" about the accuracy of this spell.
        /// Used in the evaluation logic to adjust the spell's score.'
        /// A "Risk Taker" ignores low accuracy. A "Cautious" agent respects it.
        /// Distinguish between gamblers and normal people
        /// </summary>
        protected float GetPerceivedAccuracy(Agent user)
        {
            // If the spell has a high success rate, just ignore the chance of failure
            if (_successRate >= 0.85f || user.Personality == null)
                return 1.0f;
            
            // Interpolate based on RiskTaking trait.
            // Risk 0 (normal person) -> sees the real success rate (ex: 0.4).
            // Risk 1 (gambler) -> always sees 1.0 (delusional confidence).
            return Mathf.Lerp(_successRate, 1.0f, user.Personality.RiskTaking);
        }
    }
}