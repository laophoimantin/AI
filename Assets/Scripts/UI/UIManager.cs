using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wizardo;

namespace GameUI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Agent _targetAgent;
        [SerializeField] private UIBar _healthBar;
        [SerializeField] private UIBar _manaBar;
        
        
        // Status Pool
        [SerializeField] private GameObject _statusIconPrefab;
        [SerializeField] private Transform _statusIconContainer;
        [SerializeField] private List<GameObject> _iconPool;
        private Dictionary<Type, GameObject> _activeStatusMap = new();
        
        void Start() 
        {
            _targetAgent.OnHealthChanged += UpdateHealthBar;
            _targetAgent.OnManaChanged += UpdateManaBar;
            
            _targetAgent.OnStatusApply += AddStatusIcon;
            _targetAgent.OnStatusRemove += RemoveStatusIcon;
        }
        
        void OnDestroy() 
        {
            _targetAgent.OnHealthChanged -= UpdateHealthBar;
            _targetAgent.OnManaChanged -= UpdateManaBar;
            
            _targetAgent.OnStatusApply -= AddStatusIcon;
            _targetAgent.OnStatusRemove -= RemoveStatusIcon;
        }
        
        
        

        private void UpdateHealthBar(float current, float max) 
        {
            _healthBar.UpdateBar(max, current);
        }

        private void UpdateManaBar(float current, float max)
        {
            _manaBar.UpdateBar(max, current);
        }
        
        
        
        public void AddStatusIcon(StatusEffect status)
        {
            Type t = status.GetType();
            
            if (_activeStatusMap.ContainsKey(t))
                return;
            
            
            GameObject icon = GetIconFromPool();
            icon.GetComponent<Image>().sprite = status.Icon;
            _activeStatusMap[t] = icon;
        }
        
        public void RemoveStatusIcon(StatusEffect status)
        {
            Type t = status.GetType();
            
            if (!_activeStatusMap.ContainsKey(t))
            {
                return;
            }
            
            GameObject icon = _activeStatusMap[t];
            icon.SetActive(false);
            _activeStatusMap.Remove(t);
        }
        
        
        private GameObject GetIconFromPool()
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