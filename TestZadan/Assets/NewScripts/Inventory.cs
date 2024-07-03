using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Items> items = new List<Items>();
    public InventorySlots[] slots;

    public Sprite capIcon;
    public Sprite helmetIcon;
    public Sprite ammo9x18Icon;
    public Sprite ammo545x39Icon;
    public Sprite medkitIcon;
    public Sprite jacketIcon;
    public Sprite vestIcon;

    private void Start()
    {
        InitializeSlots(); // �������������� �����
        InitializeInventory(); // ��������� ���������
        UpdateUI(); // ��������� UI ����� �������������
    }

    private void InitializeSlots()
    {
        // �������� ��� ���������� InventorySlots � �������� ��������
        slots = GetComponentsInChildren<InventorySlots>();

        // �������� ������ �� Inventory � �����
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.inventory = this;
                Debug.Log("Slot initialized: " + slot.name);
            }
            else
            {
                Debug.LogError("Slot is null.");
            }
        }
    }

    private void InitializeInventory()
    {
        // ������� ��������
        // ������� ��������
        items.Add(new Items("Cap", ClothingType.Head, ItemTypes.Clothing, capIcon, 1, 1, 0.1f, 0, 3));
        items.Add(new Items("Helmet", ClothingType.Head, ItemTypes.Clothing, helmetIcon, 1, 1, 1f, 0, 10));
        items.Add(new Items("Jacket", ClothingType.Chest, ItemTypes.Clothing, jacketIcon, 1, 1, 1f, 0, 3));
        items.Add(new Items("Bulletproof Vest", ClothingType.Chest, ItemTypes.Clothing, vestIcon, 1, 1, 10f, 0, 10));
        items.Add(new Items("Medkit", ClothingType.None, ItemTypes.Medkit, medkitIcon, 6, 6, 1f, 50));
        items.Add(new Items("������� 9�18��", ClothingType.None, ItemTypes.Ammo, ammo9x18Icon, 50, 50, 0.01f));
        items.Add(new Items("������� 5.45�39��", ClothingType.None, ItemTypes.Ammo, ammo545x39Icon, 100, 100, 0.03f));

        Debug.Log("Inventory initialized with items:");
        foreach (var item in items)
        {
            Debug.Log($"Item: {item.itemName}, Stack: {item.currentStack}");
        }

        // ���������� �������� � �����, ������� �������� ��������� DragDrop
        List<InventorySlots> slotsWithDragDrop = new List<InventorySlots>();
        foreach (var slot in slots)
        {
            if (slot != null && slot.GetComponentInChildren<DragDrop>() != null)
            {
                slotsWithDragDrop.Add(slot);
            }
        }

        int itemIndex = 0;
        foreach (var slot in slotsWithDragDrop)
        {
            if (itemIndex < items.Count)
            {
                Items itemToAdd = items[itemIndex];
                slot.AddItem(itemToAdd);
                Debug.Log($"Item {itemToAdd.itemName} added to slot: {slot.name}");
                itemIndex++;
            }
            else
            {
                Debug.LogWarning("No more items to add.");
                break; // ��������� ����, ���� ��� �������� ��� ������������
            }
        }

        // ���������� �����, ���� ��������� �������� ������, ��� ������
        if (itemIndex < items.Count)
        {
            Debug.LogWarning($"Remaining items not assigned: {items.Count - itemIndex}");
        }
    }

    public void UpdateUI()
    {
        // ����������, ��� ������ ������ �� ������ � �� null
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("Inventory slots are not assigned or empty.");
            return;
        }

        // ������� ��� ����� ����� �����������
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.ClearSlot();
            }
            else
            {
                Debug.LogError("Slot is null.");
            }
        }

        // ������������ ���������� �������� �� ������
        List<InventorySlots> slotsWithDragDrop = new List<InventorySlots>();
        foreach (var slot in slots)
        {
            if (slot != null && slot.GetComponentInChildren<DragDrop>() != null)
            {
                slotsWithDragDrop.Add(slot);
            }
        }

        int itemIndex = 0;
        for (int i = 0; i < slotsWithDragDrop.Count; i++)
        {
            if (itemIndex < items.Count)
            {
                slotsWithDragDrop[i].AddItem(items[itemIndex]);
                Debug.Log($"Item {items[itemIndex].itemName} added to slot {i}");
                itemIndex++;
            }
            else
            {
                Debug.LogWarning($"Not enough items to fill all slots. Remaining slots: {slotsWithDragDrop.Count - i}");
                break;
            }
        }

        // ���������� �����, ���� ��������� �������� ������, ��� ������
        if (itemIndex < items.Count)
        {
            Debug.LogWarning($"Remaining items not assigned: {items.Count - itemIndex}");
        }
    }

    public void UpdateItemsList()
    {
        // �������� items ������ � ����������� �� �������� ��������� ������
        items.Clear();
        foreach (var slot in slots)
        {
            if (slot != null && slot.item != null)
            {
                items.Add(slot.item); // ��������� ������ ��������� � ���������
                Debug.Log($"Item {slot.item.itemName} added to items list.");
            }
        }
        UpdateUI(); // ��������� UI ����� ��������� ������
    }

    public void AddItem(Items item)
    {
        items.Add(item);
        UpdateItemsList();
    }

    public void RemoveItem(Items item)
    {
        items.Remove(item);
        UpdateItemsList();
    }
}
