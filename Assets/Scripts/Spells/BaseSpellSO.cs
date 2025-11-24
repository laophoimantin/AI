using UnityEngine;
using Wizardo;

namespace Spells
{
    public enum SpellType { Offense, Defense, Utility, Buff, Debuff }
    public enum TargetType { Enemy, Self }
    
    [CreateAssetMenu(menuName = "AI/Spell")]
    public abstract class BaseSpellSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] protected string _name;
        [TextArea] [SerializeField] protected string _description; 
        [SerializeField] protected Sprite _icon;
        
        [Header("Economy")]
        [SerializeField] protected float _manaCost;
        [SerializeField] protected int _cooldownTurns;
        
        [Header("The Payload")]
        [SerializeField] protected float _power; 
        [Range(0f, 1f)] [SerializeField] protected float _successRate = 1.0f;
        [SerializeField] protected int _duration; 
        
        [Header("AI Tags")]
        [SerializeField] protected SpellType _type;

        protected float _spellScore;
        
        // Public Getters
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float ManaCost => _manaCost;
        public int CooldownTurns => _cooldownTurns;
        public SpellType Type => _type;
        public float Power => _power;
        public float SuccessRate => _successRate;
        public int Duration => _duration;

        protected abstract float EvaluateInternal(Agent user, Agent target);
        protected abstract void SpellEffect(Agent user, Agent target);

        
        public float Evaluate(Agent user, Agent target)
        {
            if (!target.IsAlive) return 0;
            if (user.CurrentMana < ManaCost) return 0;

            return Mathf.Max(0, EvaluateInternal(user, target));
        }

        public void ApplyEffect(Agent user, Agent target)
        {
            if (user.CurrentMana < ManaCost) return;
            user.ReduceMana(_manaCost);

            TryCast(user, target);
        }
        
        
        
        private void TryCast(Agent user, Agent target)
        {
            if (CheckHitSuccess())
            {
                SpellEffect(user,target);
                Debug.Log($"{user.Name} successful to cast with {_name}!");
            }
            else
            {
                Debug.Log($"{user.Name} failed to cast with {_name}!");
            }
        }
        private bool CheckHitSuccess()
        {
            float roll = Random.Range(0f, 1f);
            return roll <= _successRate;
        }
        protected float GetPerceivedAccuracy(Agent user)
        {
            if (_successRate >= 1.0f || user.Personality == null)
                return 1.0f;
            return Mathf.Lerp(_successRate, 1.0f, user.Personality.RiskTaking);
        }

        
    }
}