using UnityEngine;
using Wizardo;

namespace Spellbook
{
    [CreateAssetMenu(menuName = "AI/Spell")]
    public abstract class SpellSO : ScriptableObject
    {
        [SerializeField] private Sprite _icon;
        [Space(1)]
        [Header("Spell Properties")]
        [SerializeField] protected string _spellName;
        [SerializeField] protected float _manaCost;
        [SerializeField] protected int _cooldownTurns;

        public Sprite Icon => _icon;
        public string SpellName => _spellName;
        public float ManaCost => _manaCost;
        public int CooldownTurns => _cooldownTurns;

        public abstract float Evaluate(Agent self, Agent target);
        public abstract void Cast(Agent self, Agent enemy);
    }
}