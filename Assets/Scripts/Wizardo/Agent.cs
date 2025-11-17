using System.Collections.Generic;
using Spellbook;
using UnityEngine;

namespace Wizardo
{
    public class Agent : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private string _wizardName;
        [SerializeField] private float _health;
        [SerializeField] private float _maxMana = 30f;
        [SerializeField] private float _manaRegenRate = 2f;
        private float _currentMana;
        
        private float _reductionPercent = 0f;
        private float _shieldValue = 0f;
        private int _shieldDuration = 0;
        
        //[SerializeField] private List<SpellSO> _spells = new();
        private List<SpellInstance> _spells = new();
        private SpellSO _currentSpellSo;
        private SpellInstance _currentSpell;
        
        private float _minimumActionThreshold = 10f;

        #endregion

        #region Public Fields
        public string Name => _wizardName;
        public float Health
        {
            get => _health;
            set => _health = value;
        }

        public float CurrentMana
        {
            get => _currentMana;
            set => _currentMana = value;
        }
        
        public bool IsAlive => _health > 0;
        
        public float ReductionPercent => _reductionPercent;
        public int ShieldDuration => _shieldDuration;
        public float ShieldValue => _shieldValue;

        #endregion

        void Start()
        {
            _currentMana = _maxMana;
        }
        
        public void Initialize(List<SpellSO> spellTemplates)
        {
            _spells.Clear();
            foreach (var spell in spellTemplates)
                _spells.Add(new SpellInstance(spell));
        }
        
        private void TickCooldowns()
        {
            foreach (var spell in _spells)
                spell.TickCooldown();
        }
        
        private void DecayShield()
        {
            if (_shieldDuration > 0)
                _shieldDuration--;
            if (_shieldDuration == 0)
                _shieldValue = 0f;
        }
        
  
        public void TakeTurn(Agent self, Agent enemy)
        {
            SpellInstance best = null;
            float bestValue = float.MinValue;

            foreach (var spell in self._spells)
            {
                if (!spell.CanCast(self)) continue;

                float value = spell.Spell.Evaluate(self, enemy);
                if (value > bestValue)
                {
                    bestValue = value;
                    best = spell;
                }
            }
            
            _currentSpell = best;
            if (_currentSpell != null)
            {
                _currentSpell.Cast(self, enemy);
            }
            else
            {
                Debug.Log($"{self.Name} has no valid spell to cast.");
            }

            _currentMana = Mathf.Min(_maxMana, _currentMana + _manaRegenRate);
            self.TickCooldowns();
            self.DecayShield();
        }
        
        public void AddShield(float percent, float value, int duration)
        {
            _reductionPercent = Mathf.Clamp01(percent);
            _shieldValue = value;
            _shieldDuration = duration;
        }
        
        public void ApplyDamage(float damage)
        {
            float actualDamage = 0f;
            if (_shieldValue > 0)
            {
                actualDamage = damage * (1.0f - _reductionPercent);
                _shieldValue -= actualDamage;
            }
            _health -= actualDamage;
        }
        
        
        
        
        // public void TakeTurn(Agent enemy)
        // {
        //     SpellSO bestSpellSo = null;
        //     float bestUtility = float.NegativeInfinity;
        //
        //     foreach (var spell in _spells)
        //     {
        //         if (_currentMana < spell.ManaCost) continue;
        //         
        //         float utility = spell.Evaluate(this, enemy);
        //         if (utility > bestUtility)
        //         {
        //             bestUtility = utility;
        //             bestSpellSo = spell;
        //         }
        //     }
        //     
        //     if (bestSpellSo == null)
        //     {
        //         Debug.LogWarning($"{name} cannot cast any spell (mana: {_currentMana})");
        //         _currentMana = Mathf.Min(_maxMana, _currentMana + _manaRegenRate);
        //         return;
        //     }
        //
        //     _currentSpellSo = bestSpellSo;
        //     _currentSpellSo?.Cast(this, enemy);
        //     
        //     _currentMana = Mathf.Min(_maxMana, _currentMana + _manaRegenRate);
        // }

        


        // private void DrawLabel(Vector2 screenPoint, string text, float yOffset)
        // {
        //     var style = GUI.skin.label;
        //
        //     var content = new GUIContent(text);
        //     var size = style.CalcSize(content);
        //
        //     float x = screenPoint.x - (size.x / 2f);
        //
        //     float y = Screen.height - screenPoint.y - size.y - yOffset;
        //
        //     GUI.Label(new Rect(x, y, size.x, size.y), text);
        // }
        //
        // public void OnGUI()
        // {
        //     float LINESPACING = 20f;
        //     
        //     if (!Camera.main) return;
        //     var worldPoint = transform.position + Vector3.up * 5f;
        //     Vector2 p = Camera.main.WorldToScreenPoint(worldPoint);
        //
        //     float lineIndex = 0;
        //
        //     DrawLabel(p, _wizardName, lineIndex * LINESPACING);
        //     lineIndex++;
        //
        //     DrawLabel(p, $"Health: {_health}", lineIndex * LINESPACING);
        //     lineIndex++;
        //
        //     DrawLabel(p, $"Mana: {_currentMana}", lineIndex * LINESPACING);
        //     lineIndex++;
        //
        //     string currentSpellLabel = _currentSpell != null
        //         ? $"Current Spell: {_currentSpell.GetSpellName}"
        //         : $"No Spell";
        //
        //     DrawLabel(p, currentSpellLabel, lineIndex * LINESPACING);
        // }

       
    }
}