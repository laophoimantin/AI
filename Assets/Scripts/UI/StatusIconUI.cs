using StatusEffects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconUI : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    public System.Type StatusType { get; private set; }

    public void Setup(StatusEffect status)
    {
        StatusType = status.GetType();
        _iconImage.sprite = status.Icon;
    }
}
