using UnityEngine;
using Wizardo;

namespace Spells
{
    public enum SpellType { Offense, Defense, Utility, Buff, Debuff }
    public enum TargetType { Enemy, Self }
    
    [CreateAssetMenu(menuName = "AI/Spell")]
    public abstract class SpellSO : ScriptableObject
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
        [Range(0f, 1f)] [SerializeField] protected float _accuracy = 1.0f;
        [SerializeField] protected int _duration; 
        
        [Header("AI Tags")]
        [SerializeField] protected SpellType _type;

        // Public Getters
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public float ManaCost => _manaCost;
        public int CooldownTurns => _cooldownTurns;
        public SpellType Type => _type;
        public float Power => _power;
        public float Accuracy => _accuracy;
        public int Duration => _duration;


        public abstract float Evaluate(Agent user, Agent target);
        public abstract void ApplyEffect(Agent user, Agent enemy);
    }
}