using Core;
using GameUI;
using Spells;
using StatusEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
        private readonly Dictionary<SpellInstance, float> _scoreDict = new();



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

        public void Initialize(List<BaseSpellSO> spellTemplates, PersonalitySO personality = null)
        {
            // Clear old spells 
            foreach (Transform child in _spellContainer)
            {
                Destroy(child.gameObject);
            }
            _spellBook.Clear();

            // Instantiate new spells
            foreach (var spell in spellTemplates)
            {
                SpellInstance instance = Instantiate(_spellInstancePrefab, _spellContainer);
                instance.Init(spell);
                _spellBook.Add(instance);
            }

            _currentHealth = _maxHealth;
            _currentMana = _maxMana;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnManaChanged?.Invoke(_currentMana, _maxMana);

            // Apply personality
            if (personality != null)
            {
                _personality = personality;
                OnPersonalityChanged?.Invoke(_personality);
            }

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnTurnChanged -= HandleTurnChanged;
                BattleManager.Instance.OnTurnChanged += HandleTurnChanged;
            }
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

            StartCoroutine(TakeTurnCourontine(enemy));
        }

        private IEnumerator TakeTurnCourontine(Agent enemy)
        {
            // Process effects if the AI has any (start of turn)
            ProcessEffects(true);

            // Clear old debug data
            LastTurnData.Clear();

            // Phase 1: Evaluation
            var spellScores = EvaluateAvailableSpells(enemy);

            // Phase 2: Decision
            SpellInstance chosenSpell = SelectSpellWeighted(spellScores);

            yield return new WaitForSeconds(1f);
            // Phase 3: Action
            TryCastSelectedSpellOn(chosenSpell, enemy);

            // Process effects if the AI has any (end of turn)
            ProcessEffects(false);
        }


        private float PredictEnemyIntent(Agent enemy)
        {
            // Evaluate the enemy’s spell choice
            var enemyScores = enemy.EvaluateAvailableSpells(this, true);

            // No available actions =>  no threat
            if (enemyScores.Count == 0) return 0f;

            // Get the highest-priority spell
            var enemyBestSpell = enemyScores.OrderByDescending(x => x.Value).First().Key;

            // Check if the spell is offensive
            if (enemyBestSpell.BaseSpell.Types.Contains(SpellType.Offense))
            {
                // Estimate damage as a percentage of max health
                float predictedDamagePercent = enemyBestSpell.BaseSpell.Power / this.MaxHealth;

                // Map damage to threat level using fuzzy logic
                // ~ 10% damage => low threat, ~30% => high threat
                return FuzzyMath.GradeUp(predictedDamagePercent, 0.1f, 0.3f);
            }

            // Non-offensive actions (heal, shield, mana) => no immediate threat
            return 0f;
        }


        /// <summary>
        /// Scores every spell in the spellbook based on the current combat state.
        /// Applies Personality Modifiers and Random Noise to the scores (to make the AI more unpredictable and random).
        /// </summary>
        private Dictionary<SpellInstance, float> EvaluateAvailableSpells(Agent enemy, bool isSimulating = false)
        {
            // 1. Evaluation
            _scoreDict.Clear();


            // Dynamic Context --------------------------------------
            // 1.1. Survival instinct: lower health increases the desire to defend.
            float defenseBoost = FuzzyMath.GradeDown(HealthPercent, 0.2f, 0.6f);

            // 1.2. Finishing instinct: lower enemy health increases the desire to finish them.
            float killBoost = FuzzyMath.GradeDown(enemy.HealthPercent, 0.1f, 0.4f);

            // 1.3. Resource pressure: lower mana increases the need for utility (mana recovery).
            float manaPanic = FuzzyMath.GradeDown(ManaPercent, 0.1f, 0.3f);
            // --------------------------------------


            //Precognition ------------------------------
            float incomingThreat = 0f;
            if (!isSimulating)
            {
                incomingThreat = PredictEnemyIntent(enemy);
            }

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
                    float maxBias = 0f;
                    foreach (var type in spellInstance.BaseSpell.Types)
                    {
                        float bias = _personality.GetModifierForType(type);

                        switch (type)
                        {
                            case SpellType.Defense:
                                bias *= Mathf.Lerp(1f, 3f, defenseBoost); //Defense stronger when health is critical.
                                bias *= Mathf.Lerp(1f, 4f, incomingThreat); // If enemy is about to strike, block
                                break;
                            case SpellType.Offense:
                                bias *= Mathf.Lerp(1f, 2.5f, killBoost); // Offense increases significantly when the enemy is low.
                                break;
                            case SpellType.Utility:
                                bias *= Mathf.Lerp(1f, 2.2f, manaPanic); // Prioritize mana recovery when resources are low.
                                break;
                        }

                        if (bias > maxBias)
                            maxBias = bias;
                    }
                    personalityMod = maxBias > 0f ? maxBias : 1.0f;
                    finalScore *= personalityMod;

                    // Apply Chaos/Randomness 
                    // (Higher randomness = the AI’s choices become more unpredictable)
                    // Formula: Score *= Randomness
                    if (!isSimulating)
                    {
                        noise = UnityEngine.Random.Range(1.0f - _personality.Randomness, 1.0f + _personality.Randomness);
                        finalScore *= noise;
                    }
                }

                //Store result
                _scoreDict.Add(spellInstance, finalScore);

                // Capture debug data
                if (!isSimulating)
                {
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
            }

            // 2. Acting smart
            // If the agent is smart (randomness <= 0.2), remove everything that isn't in the Top 3.
            if (_personality != null && _personality.Randomness <= 0.2 && _scoreDict.Count > 3)
            {
                // Sort descending, take the top 3 Keys, and convert to a HashSet for fast lookup
                var top3Spells = _scoreDict.OrderByDescending(x => x.Value)
                    .Take(3)
                    .Select(x => x.Key)
                    .ToHashSet();

                // Find the losers (keys not in the top 3)
                var losers = _scoreDict.Keys.Where(k => !top3Spells.Contains(k)).ToList();

                // Delete the losers
                foreach (var loser in losers)
                {
                    _scoreDict.Remove(loser);
                }
            }

            return _scoreDict;
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
                string spellName = _currentSpell.BaseSpell.Name;

                bool success = spell.ExecuteSpell(this, target);
                if (success)
                {
                    BattleManager.Instance.DisplayCombatMessage($"<color=#00FF00>{Name} successfully casts {spellName}!</color>");
                }
                else
                {
                    BattleManager.Instance.DisplayCombatMessage($"<color=#FF0000>{Name} casts {spellName}... but MISSED!</color>");
                }
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