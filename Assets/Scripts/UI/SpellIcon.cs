using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    public class SpellIcon : MonoBehaviour
    {
        [SerializeField] private Image _iconDisplay;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private TextMeshProUGUI _cooldownText;


        public void UpdateIcon(Sprite spellIcon)
        {
            _iconDisplay.sprite = spellIcon;
        }

        public void UpdateCooldown(int maxCooldown, int currentCooldown)
        {
            if (currentCooldown <= 0)
            {
                _cooldownOverlay.fillAmount = 0;
                _cooldownText.text = string.Empty;
                return;
            }
            
            float fillRatio = (float)currentCooldown / maxCooldown;
            _cooldownOverlay.fillAmount = fillRatio;
            _cooldownText.text = currentCooldown.ToString();
        }
    }
}