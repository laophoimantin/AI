using UnityEngine;
using Wizardo;

namespace Spellbook
{
    [CreateAssetMenu(menuName = "AI/Spell")]
    public abstract class SpellSO : ScriptableObject
    {
        #region Private Fields

        [SerializeField] protected string _spellName;
        [SerializeField] protected float _manaCost;
        [SerializeField] protected int _cooldownTurns;

        #endregion

        #region Public Fields

        public string SpellName => _spellName;
        public float ManaCost => _manaCost;
        public int CooldownTurns => _cooldownTurns;

        #endregion

        public abstract float Evaluate(Agent self, Agent target);
        public abstract void Cast(Agent self, Agent enemy);
    }
}