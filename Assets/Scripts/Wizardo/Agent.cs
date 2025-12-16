using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using GameUI;
using Spells;
using UnityEngine;
using StatusEffects;

namespace Wizardo
{
    // Debug Data
    // Used to capture data for the editor.
    [Serializable]
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

    /// <summary>
    /// The main brain of the AI.
    /// Handles Stats (Health/Mana), Status Effects, Spell Management, and AI Decision-Making.
    /// </summary>
    public class Agent : MonoBehaviour
    {
        // EVENTS =======
        // For UI 
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnManaChanged;
        public event Action<PersonalitySO> OnPersonalityChanged;
        public event Action<string> OnDeath;

        // DATA =======
        [Header("AI Personality")]
        [SerializeField] private PersonalitySO _personality;
        public PersonalitySO Personality => _personality;


        [Header("Wizard Stats")]
        [SerializeField] private string _wizardName;
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _maxMana = 30f;
        [SerializeField] private float _manaRegenRate = 2f;

        private float _currentHealth;
        private float _currentMana;


        // Getters
        public string Name => _wizardName;
        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float HealthPercent => _currentHealth / _maxHealth;
        public float MaxMana => _maxMana;
        public float CurrentMana => _currentMana;
        public float ManaPercent => _currentMana / _maxMana;
        public bool IsAlive => _currentHealth > 0;


        // SPELL ======
        [Header("Spells")]
        [SerializeField] private SpellInstance _spellInstancePrefab;
        [SerializeField] private Transform _spellContainer;
        private readonly List<SpellInstance> _spellBook = new();
        private SpellInstance _currentSpell;

        // STATUS =======
        [Header("Status Effects")]
        private readonly List<StatusEffect> _statuses = new();

        // Shield
        private BaseShieldStatus _currentBaseShield;
        public bool HasShield => _currentBaseShield is { Durability: > 0 };
        public float DurabilityPercent => _currentBaseShield?.DurabilityPercent == null ? 0 : _currentBaseShield.DurabilityPercent;


        public event Action<StatusEffect> OnStatusApply;
        public event Action<StatusEffect> OnStatusRemove;


        // DEBUGGING ======
        [HideInInspector] public List<SpellDecisionDebug> LastTurnData = new();
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
            OnPersonalityChanged?.Invoke(_personality);
        }

        public void Initialize(List<BaseSpellSO> spellTemplates, PersonalitySO personality = null)
        {
            // Clear old spells 
            foreach (Transform child in _spellContainer)
                Destroy(child.gameObject);
            _spellBook.Clear();

            // Instantiate new spells
            foreach (var spell in spellTemplates)
            {
                SpellInstance instance = Instantiate(_spellInstancePrefab, _spellContainer);
                instance.Init(spell);
                _spellBook.Add(instance);
            }

            // Apply personality
            if (personality != null)
                _personality = personality;

            BattleManager.Instance.OnTurnChanged += HandleTurnChanged;
        }


        // =============================================================================================================
        // Main Logic 
        // =============================================================================================================
        /// <summary>
        /// The main AI logic.
        /// Takes a turn, evaluates available spells, chooses a spell, and executes it.
        /// </summary>
        /// <param name="enemy"> Target </param>
        public void TakeTurn(Agent enemy)
        {
            if (!IsAlive) return;

            // Process effects if the AI has any (start of turn)
            ProcessEffects(true);

            // Clear old debug data
            LastTurnData.Clear();

            // Phase 1: Evaluation
            var spellScores = EvaluateAvailableSpells(enemy);

            // Phase 2: Decision
            SpellInstance chosenSpell = SelectSpellWeighted(spellScores);

            // Phase 3: Action
            TryCastSelectedSpellOn(chosenSpell, enemy);

            // Process effects if the AI has any (end of turn)
            ProcessEffects(false);
        }


        /// <summary>
        /// Scores every spell in the spellbook based on the current combat state.
        /// Applies Personality Modifiers and Random Noise to the scores (to make the AI more unpredictable and random).
        /// </summary>
        private Dictionary<SpellInstance, float> EvaluateAvailableSpells(Agent enemy)
        {
            // 1. Evaluation
            var scoreDict = new Dictionary<SpellInstance, float>();

            // Evaluate all spells
            foreach (var spellInstance in _spellBook)
            {
                // Validity Check
                if (!spellInstance.IsReady(this))
                {
                    //Debug.Log($"{Name} cannot cast {spellInstance.BaseSpell.Name} (not ready)");
                    continue;
                }

                // Calculate the raw score
                float rawScore = spellInstance.BaseSpell.Evaluate(this, enemy);
                if (rawScore <= 0) continue; // Spell is useless

                // Modify the score based on the AI's personality
                float finalScore = rawScore;
                float personalityMod = 1.0f;
                float noise = 1.0f;

                if (_personality != null)
                {
                    // Apply Type Bias (aggressive wizards boost offense spells)
                    // Formula: Score *= Modifier (Personality)
                    foreach (var type in spellInstance.BaseSpell.Types)
                    {
                        personalityMod = _personality.GetModifierForType(type);
                        finalScore *= personalityMod;
                    }

                    // Apply Chaos/Randomness 
                    // (Higher randomness = the AI’s choices become more unpredictable)
                    // Formula: Score *= Randomness
                    noise = UnityEngine.Random.Range(1.0f - _personality.Randomness, 1.0f + _personality.Randomness);
                    finalScore *= noise;
                }

                //Store result
                scoreDict.Add(spellInstance, finalScore);

                // Capture debug data
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

            // 2. Acting smart
            // If the agent is smart (randomness <= 0.5), remove everything that isn't in the Top 3.
            if (_personality != null && _personality.Randomness <= 0.5 && scoreDict.Count > 3)
            {
                // Sort descending, take the top 3 Keys, and convert to a HashSet for fast lookup
                var top3Spells = scoreDict.OrderByDescending(x => x.Value)
                    .Take(3)
                    .Select(x => x.Key)
                    .ToHashSet();

                // Find the losers (keys not in the top 3)
                var losers = scoreDict.Keys.Where(k => !top3Spells.Contains(k)).ToList();

                // Delete the losers
                foreach (var loser in losers)
                {
                    scoreDict.Remove(loser);
                }
            }

            return scoreDict;
        }

        /// <summary>
        /// Performs a Roulette Wheel Selection, also known as Fitness Proportionate Selection or Spinning Wheel selection (wikipedia)
        /// "Spells with higher scores occupy a larger section of the wheel, making them more likely to be picked."
        /// Make the AI more unpredictable and random rather than always choosing the top spell.
        /// </summary>
        private SpellInstance SelectSpellWeighted(Dictionary<SpellInstance, float> spellScoresDict)
        {
            // If there are no spells, return null
            if (spellScoresDict.Count == 0) return null;

            // Calculate the total score/weight of the wheel
            float totalScore = spellScoresDict.Sum(x => x.Value);

            // Roll the dice/Spin the wheel
            float randomValue = UnityEngine.Random.Range(0, totalScore);

            // Debugging
            LastTotalWeight = totalScore;
            LastWinningTicket = randomValue;


            SpellInstance selected = null;
            float currentSum = 0;
            bool found = false;

            // Find where the needle landed
            foreach (var pair in spellScoresDict)
            {
                // Update debug info
                var debugEntry = LastTurnData.FirstOrDefault(x => x.SpellName == pair.Key.SpellName);

                if (debugEntry != null)
                {
                    debugEntry.WinChance = totalScore > 0 ? (pair.Value / totalScore) : 0;
                }

                // CCheck selection
                if (!found)
                {
                    currentSum += pair.Value;
                    // If the accumulated sum passes our random ticket, this is the winner
                    if (currentSum >= randomValue)
                    {
                        selected = pair.Key;
                        found = true;

                        // MARK THE WINNER
                        if (debugEntry != null) debugEntry.IsWinner = true;
                    }
                }
            }


            // Fallback: If floating point errors caused a miss, pick the highest score
            if (selected == null)
            {
                selected = spellScoresDict.OrderByDescending(x => x.Value).First().Key;

                var fallbackEntry = LastTurnData.FirstOrDefault(x => x.SpellName == selected.BaseSpell.Name);
                if (fallbackEntry != null) fallbackEntry.IsWinner = true;
            }

            return selected;
        }

        /// <summary>
        /// Executes the chosen spell.
        /// </summary>
        private void TryCastSelectedSpellOn(SpellInstance spell, Agent target)
        {
            _currentSpell = spell;
            if (spell != null)
            {
                spell.ExecuteSpell(this, target);
                BattleManager.Instance.DisplayCombatMessage($"{Name} casts {_currentSpell.BaseSpell.Name}");
            }
            else // No valid spell found
            {
                Debug.Log($"{_wizardName} has no valid spell to cast.");
                BattleManager.Instance.DisplayCombatMessage($"{Name} skips turn (No valid spells).");
            }
        }


        // Methods ======================================================================================================
        //// Passive regeneration and cooldown management each turn
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
        // Predicts damage for AI planning.
        // Estimate how much incoming damage will be absorbed if the agent currently has a shield.
        public float EstimateIncomingDamage(float damage, bool isShieldIgnored = false)
        {
            if (!HasShield) return damage;
            if (isShieldIgnored) return damage;
            return _currentBaseShield.ModifyDamage(damage);
        }

        // Take damage.
        // If the AI has a shield, absorb the incoming damage.
        public void TakeDamage(Agent attacker, float incomingDamage, bool isShieldIgnored = false)
        {
            if (!IsAlive) return;

            float finalDamage = incomingDamage;

            if (!isShieldIgnored && HasShield)
            {
                finalDamage = _currentBaseShield.AbsorbDamage(attacker, finalDamage);
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
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth); // Update health bar

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
        /// <summary>
        /// Finds an active status effect of type T.
        /// </summary>
        public T GetStatus<T>() where T : StatusEffect
        {
            foreach (var status in _statuses)
                if (status is T foundStatus)
                    return foundStatus;
            return null;
        }

        public bool HasStatus<T>() where T : StatusEffect
        {
            return GetStatus<T>() != null;
        }

        /// <summary>
        /// Applies a new status effect or refreshes an existing one.
        /// </summary>
        public void AddStatus(StatusEffect status)
        {
            // Check for existing instance of this status type
            var existing = _statuses.Find(s => s.GetType() == status.GetType());
            if (existing != null)
            {
                // Refresh logic (extend duration, update power, add stacks...)
                existing.Refresh(status.Duration, status.Power);
                OnStatusApply?.Invoke(status);
            }
            else
            {
                // Add a new status
                _statuses.Add(status);
                status.OnApply();
                OnStatusApply?.Invoke(status);
            }

            // Handle/Store shield
            if (status is BaseShieldStatus shieldStatus)
            {
                // If the agent already had a shield, remove the old one
                if (_currentBaseShield != null) RemoveStatus(_currentBaseShield, _statuses.IndexOf(_currentBaseShield));
                _currentBaseShield = shieldStatus;
            }
        }


        /// <summary>
        /// Run the basic method of status effects.
        /// </summary>
        private void ProcessEffects(bool isStartOfTurn)
        {
            for (int i = _statuses.Count - 1; i >= 0; i--)
            {
                var status = _statuses[i];

                if (isStartOfTurn)
                {
                    status.OnTurnStart();
                }
                else
                {
                    status.OnTurnEnd();
                    status.ReduceDuration();
                }

                if (status.IsExpired)
                {
                    RemoveStatus(status, i);
                }
            }
        }

        /// <summary>
        /// Removes a status effect, triggers its expiration logic, and updates the UI.
        /// </summary>
        private void RemoveStatus(StatusEffect status, int index)
        {
            status.OnExpire();
            _statuses.RemoveAt(index);

            // Handle shield removal
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
//         ? $ "Current Spell: {_currentSpell.GetSpellName}"
//         : $ "No Spell";
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