using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Spells;
using UnityEngine;
using StatusEffects;
using System.Text;

namespace Wizardo
{
    [System.Serializable]
    public class SpellDecisionDebug
    {
        public string SpellName;
        public float RawScore;
        public float PersonalityMod;
        public float Noise;
        public float FinalScore;
        public float WinChance;
        public bool IsWinner;
    }


    public class Agent : MonoBehaviour
    {
        // --- EVENTS ---
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<string> OnDeath;

        [Tooltip("TOGGLE THIS to see the logs")]
        [SerializeField] private bool _debugAI = true;

        [Header("AI Personality")]
        [SerializeField]
        private PersonalitySO _personality;

        public PersonalitySO Personality => _personality;

        [Header("Wizard Stats")]
        [SerializeField] private string _wizardName;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _maxMana = 30f;
        [SerializeField] private float _manaRegenRate = 2f;

        // --- STATE ---
        private float _currentHealth;
        private float _currentMana;


        // Getters
        public string Name => _wizardName;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _maxHealth;
        public float CurrentMana => _currentMana;
        public bool IsAlive => _currentHealth > 0;


        [Header("Spells")]
        [SerializeField] private SpellInstance _spellInstancePrefab;
        [SerializeField] private Transform _spellContainer;
        private readonly List<SpellInstance> _spellBook = new();
        private SpellInstance _currentSpell;


        [Header("Status Effects")]
        private readonly List<StatusEffect> _statuses = new();

        private BaseShieldStatus _currentBaseShield;
        public bool HasShield => _currentBaseShield is { Durability: > 0 };


        public event Action<StatusEffect> OnStatusApply;
        public event Action<StatusEffect> OnStatusRemove;


        // Debug
        [HideInInspector] public List<SpellDecisionDebug> LastTurnData = new List<SpellDecisionDebug>();
        [HideInInspector] public float LastTotalWeight;
        [HideInInspector] public float LastWinningTicket;

        void OnDisable()
        {
            BattleManager.Instance.OnTurnChanged -= HandleTurnChanged;
        }

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

            BattleManager.Instance.OnTurnChanged += HandleTurnChanged;
        }


        // =============================================================================================================
        // Main Logic ==================================================================================================
        // =============================================================================================================
        public void TakeTurn(Agent enemy)
        {
            if (!IsAlive) return;

            ProcessEffects(true);

            StringBuilder debugLog = null;
            if (_debugAI)
            {
                debugLog = new StringBuilder();
                debugLog.AppendLine($"--- Agent: {_wizardName} ---");
                debugLog.AppendLine($"HP: {_currentHealth}/{_maxHealth} | Mana: {_currentMana}");
                debugLog.AppendLine($"Personality: {(_personality != null ? _personality.name : "None")}");
                debugLog.AppendLine("----------------------------------------------------------------");
                debugLog.AppendLine($"{"SPELL",-40} | {"RAW",-5} | {"PERS",-5} | {"NOISE",-5} | {"FINAL",-5}");
            }

            LastTurnData.Clear();

            // Phase 1: Evaluation
            var spellScores = EvaluateAvailableSpells(enemy, debugLog);

            // Phase 2: Decision
            SpellInstance chosenSpell = SelectSpellWeighted(spellScores, debugLog);

            if (_debugAI && debugLog != null)
            {
                Debug.Log(debugLog.ToString());
            }

            // Phase 3: Action
            TryCastSelectedSpellOn(chosenSpell, enemy);
            ProcessEffects(false);
        }

        private Dictionary<SpellInstance, float> EvaluateAvailableSpells(Agent enemy, StringBuilder log)
        {
            var scoreDict = new Dictionary<SpellInstance, float>();

            foreach (var spellInstance in _spellBook)
            {
                if (!spellInstance.IsReady(this)) continue;

                float rawScore = spellInstance.BaseSpell.Evaluate(this, enemy);
                if (rawScore <= 0) continue;

                float finalScore = rawScore;
                float personalityMod = 1.0f;
                float noise = 1.0f;

                if (_personality != null)
                {
                    personalityMod = _personality.GetModifierForType(spellInstance.BaseSpell.Type);
                    finalScore *= personalityMod;

                    noise = UnityEngine.Random.Range(1.0f - _personality.Randomness, 1.0f + _personality.Randomness);
                    finalScore *= noise;
                }

                scoreDict.Add(spellInstance, finalScore);
                if (log != null)
                {
                    log.AppendLine($"{spellInstance.BaseSpell.Name,-40} | {rawScore,-5:F0} | {personalityMod,-5:F1} | {noise,-5:F2} | {finalScore,-5:F0}");
                }


                LastTurnData.Add(new SpellDecisionDebug
                {
                    SpellName = spellInstance.BaseSpell.Name,
                    RawScore = rawScore,
                    PersonalityMod = personalityMod,
                    Noise = noise,
                    FinalScore = finalScore,
                    IsWinner = false
                });
            }


            return scoreDict;
        }

        private SpellInstance SelectSpellWeighted(Dictionary<SpellInstance, float> spellScores, StringBuilder log)
        {
            if (spellScores.Count == 0) return null;

            float totalScore = spellScores.Sum(x => x.Value);
            float randomValue = UnityEngine.Random.Range(0, totalScore);

            if (log != null)
            {
                log.AppendLine("----------------------------------------------------------------");
                log.AppendLine($"Total Weight: {totalScore:F1} | Winning Ticket: {randomValue:F1}");
                log.AppendLine("--- PROBABILITY BREAKDOWN ---");
            }

            // SAVE DATA FOR EDITOR
            LastTotalWeight = totalScore; // <--- Capture
            LastWinningTicket = randomValue; // <--- Capture

            SpellInstance selected = null;
            float currentSum = 0;
            bool found = false;

            // DEBUG ===============================
            foreach (var pair in spellScores)
            {
                if (log != null)
                {
                    float percent = (pair.Value / totalScore) * 100f;
                    // Check if this is the winner to mark it in the log
                    bool isThisTheWinner = !found && (currentSum + pair.Value >= randomValue);
                    string winnerMarker = isThisTheWinner ? "<< WON" : "";
                    log.AppendLine($"{pair.Key.BaseSpell.Name,-40}: {percent:F1}% {winnerMarker}");
                }

                if (!found)
                {
                    currentSum += pair.Value;
                    if (currentSum >= randomValue)
                    {
                        selected = pair.Key;
                        found = true;
                    }
                }
            }

            foreach (var pair in spellScores)
            {
                var debugEntry = LastTurnData.FirstOrDefault(x => x.SpellName == pair.Key.BaseSpell.Name);

                if (debugEntry != null)
                {
                    // Calculate percentage for the UI
                    debugEntry.WinChance = (pair.Value / totalScore);
                }

                if (!found)
                {
                    currentSum += pair.Value;
                    if (currentSum >= randomValue)
                    {
                        selected = pair.Key;
                        found = true;

                        // MARK THE WINNER
                        if (debugEntry != null) debugEntry.IsWinner = true;
                    }
                }
            }
            //Debugggggggggggggggggggggggggggggggggggggg


            // Fallback: Return the highest score (handles potential float precision edge cases)
            if (selected == null)
            {
                selected = spellScores.OrderByDescending(x => x.Value).First().Key;
                if (log != null) log.AppendLine("!! FALLBACK TRIGGERED !!");
            }

            return selected;
        }

        private void TryCastSelectedSpellOn(SpellInstance spell, Agent target)
        {
            _currentSpell = spell;
            if (spell != null)
            {
                spell.ExecuteSpell(this, target);
                BattleManager.Instance.DisplayCombatMessage($"{Name} casts {_currentSpell.BaseSpell.Name}");
            }
            else
            {
                Debug.Log($"{_wizardName} has no valid spell to cast.");
                BattleManager.Instance.DisplayCombatMessage($"{Name} skips turn (No valid spells).");
            }
        }


        // Methods ======================================================================================================
        private void HandleTurnChanged()
        {
            RegenMana(_manaRegenRate);
            ReduceSpellCooldowns();
        }

        private void ReduceSpellCooldowns()
        {
            foreach (var spell in _spellBook)
                spell.ReduceCooldown();
        }

        #region Health and Mana

        // Health ======
        public float EstimateIncomingDamage(float damage)
        {
            if (!HasShield) return damage;
            return _currentBaseShield.ModifyDamage(damage);
        }

        public void TakeDamage(float incomingDamage, bool isShieldIgnored = false)
        {
            if (!IsAlive) return;

            float finalDamage = incomingDamage;

            if (!isShieldIgnored && HasShield)
            {
                finalDamage = _currentBaseShield.AbsorbDamage(finalDamage, this);
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

        #endregion

        // Status ======================================================================================================
        public bool HasStatus<T>() where T : StatusEffect
        {
            return GetStatus<T>() != null;
        }

        public T GetStatus<T>() where T : StatusEffect
        {
            foreach (var status in _statuses)
                if (status is T foundStatus)
                    return foundStatus;
            return null;
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
                status.OnApply(this);
                OnStatusApply?.Invoke(status);
            }

            if (status is BaseShieldStatus shieldStatus)
            {
                _currentBaseShield = shieldStatus;
            }
        }


        private void ProcessEffects(bool isStartOfTurn)
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                var status = _statuses[i];

                if (isStartOfTurn)
                {
                    status.OnTurnStart(this);
                }
                else
                {
                    status.OnTurnEnd(this);
                    status.ReduceDuration();
                }

                if (status.IsExpired)
                {
                    RemoveStatus(status, i);
                }
            }
        }

        private void RemoveStatus(StatusEffect status, int index)
        {
            status.OnExpire(this);
            _statuses.RemoveAt(index);

            if (status is BaseShieldStatus)
            {
                _currentBaseShield = null;
            }

            OnStatusRemove?.Invoke(status);
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