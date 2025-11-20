using UnityEngine;
using Wizardo;

namespace GameUI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Agent _targetAgent;
        [SerializeField] private UIBar _healthBar;
        [SerializeField] private UIBar _manaBar;

        void Start() 
        {
            _targetAgent.OnHealthChanged += UpdateHealthBar;
            _targetAgent.OnManaChanged += UpdateManaBar;
        }
        
        void OnDestroy() 
        {
            _targetAgent.OnHealthChanged -= UpdateHealthBar;
            _targetAgent.OnManaChanged += UpdateManaBar;
        }

        void UpdateHealthBar(float current, float max) 
        {
            _healthBar.UpdateBar(max, current);
        }

        void UpdateManaBar(float current, float max)
        {
            _manaBar.UpdateBar(max, current);
        }
    }
}