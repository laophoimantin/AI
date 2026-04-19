using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wizardo;
using StatusEffects;
using TMPro;

namespace GameUI
{
    /// <summary>
    /// Manages the UI of the game.
    /// Displays the health and mana bars and displays the status effects of the target.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Agent _targetAgent;
        [SerializeField] private UIBar _healthBar;
        [SerializeField] private UIBar _manaBar;
        [SerializeField] private TextMeshProUGUI _personalityText;

        // Status Pool
        // Overkill for nothing, but it works, maybe a good practice?
        [SerializeField] private StatusIconUI _statusIconPrefab;
        [SerializeField] private Transform _statusIconContainer;

        void OnEnable()
        {
            if (_targetAgent == null) return;

            _targetAgent.OnPersonalityChanged += UpdatePersonalityText;
            _targetAgent.OnHealthChanged += UpdateHealthBar;
            _targetAgent.OnManaChanged += UpdateManaBar;

            _targetAgent.OnStatusApply += AddStatusIcon;
            _targetAgent.OnStatusRemove += RemoveStatusIcon;
        }

        void OnDisable()
        {
            if (_targetAgent == null) return;

            _targetAgent.OnPersonalityChanged -= UpdatePersonalityText;
            _targetAgent.OnHealthChanged -= UpdateHealthBar;
            _targetAgent.OnManaChanged -= UpdateManaBar;

            _targetAgent.OnStatusApply -= AddStatusIcon;
            _targetAgent.OnStatusRemove -= RemoveStatusIcon;
        }

        // Update the health and mana bars
        private void UpdateHealthBar(float current, float max)
        {
            _healthBar.UpdateBar(max, current);
        }

        private void UpdateManaBar(float current, float max)
        {
            _manaBar.UpdateBar(max, current);
        }

        public void UpdatePersonalityText(PersonalitySO personality)
        {
            if (personality == null) return;
            _personalityText.text = $"{personality.Name}: {personality.Description}";
        }

        // Display the status effects (poisoned, shield)
        public void AddStatusIcon(StatusEffect status)
        {
            foreach (Transform child in _statusIconContainer)
            {
                var existingIcon = child.GetComponent<StatusIconUI>();
                if (existingIcon != null && existingIcon.StatusType == status.GetType())
                {
                    return;
                }
            }

            StatusIconUI newIcon = Instantiate(_statusIconPrefab, _statusIconContainer);
            newIcon.Setup(status);
        }

        public void RemoveStatusIcon(StatusEffect status)
        {
            foreach (Transform child in _statusIconContainer)
            {
                var icon = child.GetComponent<StatusIconUI>();
                if (icon != null && icon.StatusType == status.GetType())
                {
                    Destroy(child.gameObject);
                    return;
                }
            }
        }
    }
}