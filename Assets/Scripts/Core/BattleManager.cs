using System;
using System.Collections;
using System.Collections.Generic;
using Spells;
using TMPro;
using UnityEngine;
using Wizardo;
using Random = UnityEngine.Random;

namespace Core
{
    /// <summary>
    /// Manages the battle between two wizards.
    /// Simulates a turn-based battle.
    /// Hardcoded to be two-player.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; } // Singleton

        [Header("Battle Settings")]
        [SerializeField] private bool _applyNewPers = true;
        // Time in seconds between actions.
        [SerializeField] private float _turnDuration;

        [Header("Wizards")]
        [SerializeField] private Agent _redWizard;
        [SerializeField] private Agent _blueWizard;

        // The shared pool of spells available to both wizards.
        [SerializeField] private List<BaseSpellSO> _spellBook;

        // The pool of personality profiles available to both wizards.
        [SerializeField] private List<PersonalitySO> _personalityPool;

        // Getters
        public Agent RedWizard => _redWizard;
        public Agent BlueWizard => _blueWizard;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _roundDisplay;
        [SerializeField] private TextMeshProUGUI _turnArrowText;
        [SerializeField] private TextMeshProUGUI _spellCastText;

        private int _currentRound = 1;

        public event Action OnTurnChanged;

        void Awake()
        {
            // Singleton pattern, nothing else
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            if (_redWizard == null || _blueWizard == null)
            {
                Debug.LogError("Wizards are not assigned in BattleManager!");
                return;
            }

            if (_applyNewPers)
            {
                // Pick Random Personality
                int redRandomIndex = Random.Range(0, _personalityPool.Count);
                PersonalitySO redPersonality = _personalityPool[redRandomIndex];

                int blueRandomIndex = Random.Range(0, _personalityPool.Count);
                if (redRandomIndex == blueRandomIndex) // Prevent the same personality
                {
                    blueRandomIndex++;
                    if (blueRandomIndex >= _personalityPool.Count) blueRandomIndex = 0;
                }

                PersonalitySO bluePersonality = _personalityPool[blueRandomIndex];

                // Initialize the wizards
                _redWizard.Initialize(_spellBook, redPersonality);
                _blueWizard.Initialize(_spellBook, bluePersonality);
            }
            else
            {
                _redWizard.Initialize(_spellBook);
                _blueWizard.Initialize(_spellBook);
            }

            // Start the battle
            StartCoroutine(SimulateBattle());
        }


        /// <summary>
        /// Simulates a turn-based battle between the two wizards.
        /// </summary>
        private IEnumerator SimulateBattle()
        {
            // Coin Flip: 0 = Red, 1 = Blue
            bool redTurn = Random.Range(0, 2) == 0;

            DisplayCombatMessage("Battle Start!");
            UpdateRoundUI();

            // Delay the start of the battle
            yield return new WaitForSeconds(_turnDuration / 2);

            // Main Loop =======
            while (_redWizard.IsAlive && _blueWizard.IsAlive)
            {
                Agent currentAgent = redTurn ? _redWizard : _blueWizard;
                Agent targetAgent = redTurn ? _blueWizard : _redWizard;

                OnTurnChanged?.Invoke();
                Debug.LogWarning($"--- Turn Start: {currentAgent.Name} ---");

                // Display the turn indicator (arrow)
                if (_turnArrowText != null)
                {
                    _turnArrowText.text = redTurn ? "<=====" : "=====>";
                    _turnArrowText.color = redTurn ? Color.red : Color.blue;
                }

                // The Agent calculates logic and performs the spell...
                currentAgent.TakeTurn(targetAgent);

                // Delay the end of the turn
                yield return new WaitForSeconds(_turnDuration / 2);

                if (!targetAgent.IsAlive) break;
                redTurn = !redTurn;
                if (redTurn)
                {
                    _currentRound++;
                    UpdateRoundUI();
                }
            }
            // ======================

            // End Game =======
            string winner = _redWizard.IsAlive ? _redWizard.Name : _blueWizard.Name;
            DisplayCombatMessage($"GAME OVER! {winner} Wins!");
            Debug.Log($"Winner: {winner}");
        }

        // Update the round counter in the UI
        private void UpdateRoundUI()
        {
            if (_roundDisplay != null)
                _roundDisplay.text = $"Round {_currentRound}";
        }

        // Display a message in the UI
        // Display what spell the user cast in the UI
        public void DisplayCombatMessage(string message)
        {
            if (_spellCastText != null)
                _spellCastText.text = message;
        }


        // private IEnumerator SimulateBattle()
        // {
        //     bool blueStarts = Random.Range(0, 2) == 0;
        //     Agent currentAgent = blueStarts? _blueWizard: _redWizard;
        //     Agent target = blueStarts? _redWizard: _blueWizard;
        //     _turnArrowText.text = currentAgent == _redWizard ? "<=====" : "=====>";
        //     _turnArrowText.color = currentAgent == _redWizard? Color.red: Color.blue;
        //     
        //     yield return new WaitForSeconds(1.5f);
        //     Debug.Log("Battle started!");
        //     _currentRound++;
        //     
        //     while (_redWizard.IsAlive && _blueWizard.IsAlive)
        //     {
        //         _turnArrowText.text = currentAgent == _redWizard ? "<=====" : "=====>";
        //         _turnArrowText.color = currentAgent == _redWizard? Color.red: Color.blue;
        //         
        //         yield return new WaitForSeconds(2f);
        //         
        //         currentAgent.TakeTurn(target);
        //         _roundDisplay.text = $"Round {_currentRound++}";
        //         
        //         
        //         yield return new WaitForSeconds(2f);
        //
        //         (currentAgent, target) = (target, currentAgent);
        //     }
        //     
        //     Debug.Log($"Winner: {(_redWizard.IsAlive ? _redWizard.name: _blueWizard.name)}");
        // }
    }
}