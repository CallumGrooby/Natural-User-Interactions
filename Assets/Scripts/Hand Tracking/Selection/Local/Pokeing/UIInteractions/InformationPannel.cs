using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InformationPannel : MonoBehaviour
{
    public static InformationPannel instance;

    [SerializeField]
    TextMeshProUGUI txtName;
    [SerializeField]
    TextMeshProUGUI txtCost;
    [SerializeField]
    Image imgIcon;

    private void Awake()
    {
        instance = this;
    }

    public void UpdatePannel(string itemName, string itemCost, Sprite itemIcon)
    {
        if (txtName == null || txtCost == null || imgIcon == null)
            return;

        txtName.text = itemName.ToString();
        txtCost.text = itemCost.ToString();
        imgIcon.sprite = itemIcon;
    }
}
