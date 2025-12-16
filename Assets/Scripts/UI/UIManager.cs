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
        [SerializeField] private TextMeshProUGUI _personalityDes;
        
        // Status Pool
        // Overkill for nothing, but it works, maybe a good practice?
        [SerializeField] private GameObject _statusIconPrefab;
        [SerializeField] private Transform _statusIconContainer;
        [SerializeField] private List<GameObject> _iconPool;
        private Dictionary<Type, GameObject> _activeStatusMap = new();
        
        void Start() 
        {
            _targetAgent.OnPersonalityChanged += UpdatePersonalityText;
            _targetAgent.OnHealthChanged += UpdateHealthBar;
            _targetAgent.OnManaChanged += UpdateManaBar;
            
            _targetAgent.OnStatusApply += AddStatusIcon;
            _targetAgent.OnStatusRemove += RemoveStatusIcon;
        }
        
        void OnDestroy() 
        {
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
            _personalityText.text = personality.Name;
            _personalityDes.text = personality.Description;
        }

        // Display the status effects (poisoned, shield)
        public void AddStatusIcon(StatusEffect status)
        {
            Type type = status.GetType();
            
            if (_activeStatusMap.ContainsKey(type)) // Already exists
                return;
            
            GameObject icon = GetIconFromPool(); // Get a game object
            icon.GetComponent<Image>().sprite = status.Icon; // Set the icon sprite
            _activeStatusMap[type] = icon; // Add to the dictionary
        }
        
        public void RemoveStatusIcon(StatusEffect status)
        {
            Type type = status.GetType();
            
            if (!_activeStatusMap.ContainsKey(type)) // Doesn't exist
                return;
            
            GameObject icon = _activeStatusMap[type]; // Get the game object that represents the status
            icon.SetActive(false); // Hide it
            _activeStatusMap.Remove(type); // Remove from the dictionary
        }
        
        // Icon Object pooling 
        // Get icon from the pool or create a new one if none are available

        private GameObject GetIconFromPool() // Overkill for nothing, but it works, maybe a good practice?
        {
            foreach (GameObject icon in _iconPool)
            {
                if (!icon.activeInHierarchy)
                {
                    icon.SetActive(true);
                    icon.transform.SetAsLastSibling(); 
                    return icon;
                }
            }
            GameObject newIcon = Instantiate(_statusIconPrefab, _statusIconContainer);
            _iconPool.Add(newIcon);
            return newIcon;
        }
    }
}