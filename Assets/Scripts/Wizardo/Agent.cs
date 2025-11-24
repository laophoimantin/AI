using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Spells;
using UnityEngine;
using UnityEngine.Serialization;

namespace Wizardo
{
    public class Agent : MonoBehaviour
    {
        // --- EVENTS ---
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<string> OnDeath;


        [Header("AI Personality")] [SerializeField]
        private PersonalitySO _personality;
        public PersonalitySO Personality => _personality;

        [Header("Wizard Stats")] [SerializeField]
        private string _wizardName;

        [SerializeField] private float _maxHealth;
        [SerializeField] private float _maxMana = 30f;
        [SerializeField] private float _manaRegenRate = 2f;

        // --- STATE ---
        private float _currentHealth;
        private float _currentMana;
        private float _reductionPercent;
        private float _shieldValue;
        private int _shieldDuration;


        // Getters
        public string Name => _wizardName;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float CurrentMana => _currentMana;
        public bool IsAlive => _currentHealth > 0;
        public float ReductionPercent => _reductionPercent;
        public int ShieldDuration => _shieldDuration;
        public float ShieldValue => _shieldValue;


        [Header("Spells")] [SerializeField] private SpellInstance _spellInstancePrefab;
        [SerializeField] private Transform _spellContainer;
        private readonly List<SpellInstance> _spellBook = new();
        private SpellInstance _currentSpell;


        [Header("Status Effects")] [SerializeField]
        private List<StatusEffect> _statuses = new();
        
        public event Action<StatusEffect> OnStatusApply;
        public event Action OnStatusExpire;



        void Start()
        {
            _currentHealth = _maxHealth;
            _currentMana = _maxMana;

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnManaChanged?.Invoke(_currentMana, _maxMana);
        }

        public void Initialize(List<BaseSpellSO> spellTemplates)
        {
            foreach (Transform child in _spellContainer)
                Destroy(child.gameObject);

            _spellBook.Clear();
            foreach (var spell in spellTemplates)
            {
                SpellInstance instance = Instantiate(_spellInstancePrefab, _spellContainer);
                instance.Init(spell);
                _spellBook.Add(instance);
            }
        }

        // Main Logic ==================================================================================================

        public void TakeTurn(Agent enemy)
        {
            if (!IsAlive) return;

            RegenMana(_manaRegenRate);
            DecayShield();
            ReduceSpellCooldowns();

            Dictionary<SpellInstance, float> spellChances = new Dictionary<SpellInstance, float>();
            float totalWeight = 0f;
            foreach (var spellInstance in _spellBook)
            {
                if (!spellInstance.IsReady(this)) continue;

                float rawScore = spellInstance.BaseSpell.Evaluate(this, enemy);

                if (rawScore <= 0) continue;

                float personalityMod = 1.0f;
                if (_personality != null)
                {
                    switch (spellInstance.BaseSpell.Type)
                    {
                        case SpellType.Offense:
                            personalityMod = _personality.Aggression;
                            break;
                        case SpellType.Defense:
                            personalityMod = _personality.Caution;
                            break;
                        case SpellType.Utility:
                            personalityMod = _personality.Utility;
                            break;
                    }
                }

                float finalScore = rawScore * personalityMod;

                if (_personality != null)
                {
                    float noise =
                        UnityEngine.Random.Range(1.0f - _personality.Randomness, 1.0f + _personality.Randomness);
                    finalScore *= noise;
                }

                spellChances.Add(spellInstance, finalScore);
                totalWeight += finalScore;
            }

            // Decision Phase (Weighted Random)
            SpellInstance selectedSpell = null;

            if (spellChances.Count > 0)
            {
                float randomPick = UnityEngine.Random.Range(0, totalWeight);
                float currentSum = 0;

                foreach (var pair in spellChances)
                {
                    currentSum += pair.Value;
                    if (currentSum >= randomPick)
                    {
                        selectedSpell = pair.Key;
                        break;
                    }
                }

                // Fallback: If math failed due to float errors, pick the highest score
                if (selectedSpell == null)
                {
                    selectedSpell = spellChances.OrderByDescending(x => x.Value).First().Key;
                }
            }

            // Action Phase
            _currentSpell = selectedSpell;

            if (_currentSpell != null)
            {
                // Note: Ensure ExecuteSpell calls SpellSO.Cast internally
                _currentSpell.ExecuteSpell(this, enemy);
                BattleManager.Instance.DisplayCombatMessage($"{Name} casts {_currentSpell.BaseSpell.Name}");
            }
            else
            {
                Debug.Log($"{_wizardName} has no valid spell to cast.");
                BattleManager.Instance.DisplayCombatMessage($"{Name} skips turn (No valid spells).");
            }

            ProcessStatuses();
        }


        
        
        
        
        
        
        // Method ======================================================================================================
        private void ReduceSpellCooldowns()
        {
            foreach (var spell in _spellBook)
                spell.ReduceCooldown();
        }

        private void DecayShield()
        {
            if (_shieldDuration > 0)
            {
                _shieldDuration--;
                if (_shieldDuration <= 0)
                {
                    _shieldValue = 0f;
                    _reductionPercent = 0f;
                    BattleManager.Instance.DisplayCombatMessage($"{Name}'s shield faded.");
                }
            }
        }

        public void AddShield(float reductionPercent, float shieldAmount, int duration)
        {
            _reductionPercent = Mathf.Clamp01(reductionPercent);
            _shieldValue = shieldAmount;
            _shieldDuration = duration;
        }

        // Health ======
        public void TakeDamage(float incomingDamage, bool isShieldIgnored = false)
        {
            if (!IsAlive) return;

            float finalDamage = incomingDamage;

            if (!isShieldIgnored)
            {
                if (_shieldValue > 0)
                {
                    finalDamage *= (1.0f - _reductionPercent);
                }

                if (_shieldValue > 0)
                {
                    if (_shieldValue >= finalDamage)
                    {
                        _shieldValue -= finalDamage;
                        finalDamage = 0;
                    }
                    else
                    {
                        finalDamage -= _shieldValue;
                        _shieldValue = 0;
                    }
                }
            }

            if (finalDamage > 0)
            {
                ModifyHealth(-finalDamage);
            }
        }

        public void Heal(float amount)
        {
            ModifyHealth(amount);
        }

        
        
        
        private void ModifyHealth(float amount)
        {
            _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke(_wizardName);
            }
        }
        
        
        // Mana ======
        public void ReduceMana(float amount)
        {
            ModifyMana(-amount);
        }

        public void RegenMana(float amount)
        {
            ModifyMana(amount);
        }

        private void ModifyMana(float amount)
        {
            _currentMana = Mathf.Clamp(_currentMana + amount, 0, _maxMana);
            OnManaChanged?.Invoke(_currentMana, _maxMana);
        }

        // Status ======================================================================================================
        public bool HasStatus<T>() where T : StatusEffect
        {
            return _statuses.Exists(s => s is T);
        }

        public void AddStatus(StatusEffect status)
        {
            var existing = _statuses.Find(s => s.GetType() == status.GetType());
            if (existing != null)
            {
                existing.Refresh(status.Duration, status.Power);
                OnStatusApply?.Invoke(status);
            }
            else
            {
                _statuses.Add(status);
                OnStatusApply?.Invoke(status);
            }
        }
        
        public void ProcessStatuses()
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                var status = _statuses[i];
                status.Apply(this);
                status.ReduceDuration();
                if (status.IsExpired)
                {
                    _statuses.RemoveAt(i);
                    OnStatusExpire?.Invoke();
                }
            }
        }
        
        // public bool HasStatus(StatusType type)
        // {
        //     return _statuses.Exists(status => status.Type == type);
        // }

        // public void AddStatus(StatusType type, int duration, float power)
        // {
        //     var existing = _statuses.Find(status => status.Type == type);
        //
        //     if (existing != null)
        //     {
        //         existing.Duration = duration;
        //         existing.Power = power;
        //     }
        //     else
        //     {
        //         _statuses.Add(new StatusEffect { Type = type, Duration = duration, Power = power });
        //         
        //     }
        // }


        // public void TakeTurn(Agent enemy)
        // {
        //     if (!IsAlive) return;
        //     
        //     ModifyMana(_manaRegenRate);
        //     DecayShield();
        //     ReduceCooldown();
        //     
        //     SpellInstance bestSpell = null;
        //     float bestValue = float.MinValue;
        //
        //     foreach (var spell in _spellBook)
        //     {
        //         if (!spell.IsReady(this)) continue;
        //
        //         float value = spell.Spell.Evaluate(this, enemy);
        //         if (value > bestValue)
        //         {
        //             bestValue = value;
        //             bestSpell = spell;
        //         }
        //     }
        //
        //     _currentSpell = bestSpell;
        //     
        //     
        //     if (_currentSpell != null)
        //     {
        //         _currentSpell.ExecuteSpell(this, enemy);
        //         BattleManager.Instance.DisplayCombatMessage($"{Name} casts {_currentSpell.Spell.Name}");
        //     }
        //     else
        //     {
        //         Debug.Log($"{_wizardName} has no valid spell to cast.");
        //         BattleManager.Instance.DisplayCombatMessage($"{Name} skips turn (No valid spells).");
        //     }
        // }
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
    
    /*
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                      _ooOoo_
                     o8888888o
                     88" . "88
                     (| -_- |)
                     O\  =  /O
                  ____/`---'\____
                .'  \\|     |//  `.
               /  \\|||  :  |||//  \
              /  _||||| -:- |||||-  \
              |   | \\\  -  /// |   |
              | \_|  ''\---/''  |   |
              \  .-\__  `-`  ___/-. /
            ___`. .'  /--.--\  `. . __
         ."" '<  `.___\_<|>_/___.'  >'"".
        | | :  `- \`.;`\ _ /`;.`/ - ` : | |
        \  \ `-.   \_ __\ /__ _/   .-` /  /
    ======`-.____`-.___\_____/___.-`____.-'======
                      `=---='
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
              佛祖保佑           永无BUG
             God Bless        Never Crash
           Phật phù hộ, không bao giờ BUG
    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
*/

}