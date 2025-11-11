using System.Collections;
using System.Collections.Generic;
using Spellbook;
using TMPro;
using UnityEngine;
using Wizardo;
using LumbiniPark;

namespace Core
{
    public class BattleManager : MonoBehaviour
    {
        #region Private Fields

        [SerializeField] private Agent _redWizard;
        [SerializeField] private Agent _blueWizard;
        [SerializeField] private List<SpellSO> _spellBook; // assign in inspector

        [SerializeField] private TextMeshProUGUI _roundDisplay;
        private int _currentRound;

        #endregion


        void Start()
        {
            _redWizard.Initialize(_spellBook);
            _blueWizard.Initialize(_spellBook);
            
            StartCoroutine(SimulateBattle());
        }
        

        void Update()
        {
            
        }

        private IEnumerator SimulateBattle()
        {
            bool blueStarts = Random.Range(0, 2) == 0;
            Agent current = blueStarts ? _blueWizard : _redWizard;
            Agent other   = blueStarts ? _redWizard : _blueWizard;
            

            yield return new WaitForSeconds(1.5f);
            Debug.Log("Battle started!");
            _currentRound++;
            
            while (_redWizard.IsAlive && _blueWizard.IsAlive)
            {
                current.TakeTurn(current, other);
                _roundDisplay.text = $"Round {_currentRound++}";
                yield return new WaitForSeconds(2f);

                (current, other) = (other, current);
            }
            Debug.Log($"Winner: {(_redWizard.IsAlive ? _redWizard.name : _blueWizard.name)}");
        }
    }
}