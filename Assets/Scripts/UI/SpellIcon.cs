using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellIcon : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Image _fillImage;
    [SerializeField] private TextMeshProUGUI _cooldownText;


    public void UpdateIcon(Sprite spellIcon)
    {
        _image.sprite = spellIcon;
    }

    public void UpdateCooldown(int cooldown)
    {
    //    if (cooldown > 0)
    //    {
    //        cooldown--;
    //        _fillImage.fillAmount = (float)currentCooldown / maxCooldown;
    //        _cooldownText.text = currentCooldown.ToString();
    //    }

    //    else
    //    {
    //        _fillImage.fillAmount = 0;
    //        _cooldownText.text = string.Empty;
    //    }
    }
}
