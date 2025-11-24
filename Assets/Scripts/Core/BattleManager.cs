using System.Collections;
using System.Collections.Generic;
using Spells;
using TMPro;
using UnityEngine;
using Wizardo;

namespace Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }
        
        [Header("Wizards")]
        [SerializeField] private Agent _redWizard;
        [SerializeField] private Agent _blueWizard;
        [SerializeField] private List<BaseSpellSO> _spellBook; // assign in inspector

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _roundDisplay;
        [SerializeField] private TextMeshProUGUI _turnArrowText;
        [SerializeField] private TextMeshProUGUI _spellCastText;
        
        private int _currentRound;


        void Awake()
        {
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
            _redWizard.Initialize(_spellBook);
            _blueWizard.Initialize(_spellBook);
            StartCoroutine(SimulateBattle());
        }
        
        
       
        
        private IEnumerator SimulateBattle()
        {
            bool redTurn = Random.Range(0, 2) == 0;
        
            DisplayCombatMessage("Battle Start!");
            yield return new WaitForSeconds(1.5f);
            
            while (_redWizard.IsAlive && _blueWizard.IsAlive)
            {
                _roundDisplay.text = $"Round {_currentRound}";
            
                Agent currentAgent = redTurn ? _redWizard : _blueWizard;
                Agent targetAgent = redTurn ? _blueWizard : _redWizard;

                if (_turnArrowText != null)
                {
                    _turnArrowText.text = redTurn ? "<=====" : "=====>";
                    _turnArrowText.color = redTurn ? Color.red : Color.blue;
                }

                currentAgent.TakeTurn(targetAgent);
 
                yield return new WaitForSeconds(1.5f);

                if (!targetAgent.IsAlive) break;

                redTurn = !redTurn;
                
                if (redTurn) _currentRound++;
            }
        
            // End Game
            string winner = _redWizard.IsAlive ? _redWizard.Name : _blueWizard.Name;
            DisplayCombatMessage($"GAME OVER! {winner} Wins!");
            Debug.Log($"Winner: {winner}");
        }

        public void DisplayCombatMessage(string message)
        {
            if (_spellCastText != null)
                _spellCastText.text = message;
        }
        
        
        
        
        // private IEnumerator SimulateBattle()
        // {
        //     bool blueStarts = Random.Range(0, 2) == 0;
        //     Agent currentAgent = blueStarts ? _blueWizard : _redWizard;
        //     Agent target   = blueStarts ? _redWizard : _blueWizard;
        //     _turnArrowText.text = currentAgent == _redWizard ? "<=====" : "=====>";
        //     _turnArrowText.color = currentAgent == _redWizard? Color.red : Color.blue;
        //     
        //     yield return new WaitForSeconds(1.5f);
        //     Debug.Log("Battle started!");
        //     _currentRound++;
        //     
        //     while (_redWizard.IsAlive && _blueWizard.IsAlive)
        //     {
        //         _turnArrowText.text = currentAgent == _redWizard ? "<=====" : "=====>";
        //         _turnArrowText.color = currentAgent == _redWizard? Color.red : Color.blue;
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
        //     Debug.Log($"Winner: {(_redWizard.IsAlive ? _redWizard.name : _blueWizard.name)}");
        // }
    }
}