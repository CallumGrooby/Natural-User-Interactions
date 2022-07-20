using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New item", menuName = "New Placeable Item")]
public class Items : ScriptableObject
{
    public string itemName;
    public float itemCost;
    public Sprite itemIcon;
    public GameObject itemPrefab;

    private void Awake()
    {
        
    }
}
