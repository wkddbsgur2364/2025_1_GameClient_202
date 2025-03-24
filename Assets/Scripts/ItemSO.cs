using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/item")]

public class ItemSO : ScriptableObject
{
    //���� GameDB Excel ���� ������ ������ ���� �Ѵ�.
    public int id;
    public string itemName;
    public string nameEng;
    public string description;
    public ItemType itemType;
    public int price;
    public int power;
    public int level;
    public bool isStackable;
    public Sprite icon;               //���� ����� ��������Ʈ ����

    public override string ToString()
    {
        return $"[{id}] {itemName} ({itemType})-����: {price}���, �Ӽ�: {power}";
    }
    public string DisplayName
    {
        get { return string.IsNullOrEmpty(nameEng) ? itemName : nameEng; }
    }
}

    