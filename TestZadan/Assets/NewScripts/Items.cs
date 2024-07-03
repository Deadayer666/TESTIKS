using UnityEngine;

public enum ItemTypes
{
    Ammo,
    Medkit,
    Clothing
}

public enum ClothingType
{
    Head,
    Chest,
    None
}

[System.Serializable]
public class Items
{
    public string itemName;
    public ItemTypes itemType;
    public ClothingType clothingType;
    public Sprite icon;
    public int maxStack;
    public int currentStack;
    public float weight;
    public int hpRestore;
    public int defense;
    public Items(string name, ClothingType clothingType, ItemTypes type, Sprite icon, int maxStack, int currentStack, float weight, int hpRestore = 0, int defense = 0)
    {
        this.itemName = name;
        this.itemType = type;
        this.clothingType = clothingType;
        this.icon = icon;
        this.maxStack = maxStack;
        this.currentStack = currentStack;
        this.weight = weight;
        this.hpRestore = hpRestore;
        this.defense = defense;
    }

}
