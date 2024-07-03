using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopup : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI defenseText; // ���� ��� ����������� ������
    public TextMeshProUGUI weightText;  // ���� ��� ����������� ����
    public Image itemImage; // ���� ��� �����������
    public Button actionButton1;
    public Button actionButton2;

    private Items currentItem;
    private Inventory inventory;
    private BattleController battleController;

    public void Initialize(Items item, Inventory inventory, BattleController battleController)
    {
        currentItem = item;
        this.inventory = inventory;
        this.battleController = battleController;

        itemNameText.text = item.itemName;
        itemImage.sprite = item.icon; // ��������� ������� �����������

        // ��������� ����� ������ � ����
        defenseText.text = $"+{item.defense}";
        weightText.text = $"{item.weight} ��";

        actionButton1.onClick.RemoveAllListeners();
        actionButton2.onClick.RemoveAllListeners();

        // Setup buttons based on item type
        switch (item.itemType)
        {
            case ItemTypes.Ammo:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "������";
                actionButton1.onClick.AddListener(() => { FillAmmo(); ClosePopup(); });
                break;
            case ItemTypes.Medkit:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "������";
                actionButton1.onClick.RemoveAllListeners();
                actionButton1.onClick.AddListener(() => { UseMedkit(); ClosePopup(); });
                break;
            case ItemTypes.Clothing:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "�����������";
                actionButton1.onClick.AddListener(() => { EquipClothing(); ClosePopup(); });
                break;
        }

        actionButton2.GetComponentInChildren<TextMeshProUGUI>().text = "�������";
        actionButton2.onClick.AddListener(RemoveItem);

        gameObject.SetActive(true); // Show the popup
    }

    private void FillAmmo()
    {
        if (currentItem.itemType == ItemTypes.Ammo)
        {
            // ����� ������� � ����� �� ���������
            Items ammoItem = inventory.items
                .FirstOrDefault(item => item.itemType == ItemTypes.Ammo && item.itemName == currentItem.itemName);

            if (ammoItem != null)
            {
                // ��������� ���� ��������
                ammoItem.currentStack = ammoItem.maxStack;
                inventory.UpdateItemsList();
            }
            else
            {
                // ���� ������� �� �������, �������� ������� ����� ������ ��������
                Items newAmmoItem = new Items(
                    currentItem.itemName,
                    ClothingType.None,
                    ItemTypes.Ammo,
                    currentItem.icon,
                    currentItem.currentStack,
                    currentItem.maxStack,
                    currentItem.weight);

                inventory.AddItem(newAmmoItem);
                inventory.UpdateItemsList();
            }

            // ��������� ����� � ���������� ����������� ��������
            UpdateAmmoText();
        }
    }

    private void UpdateAmmoText()
    {
        // ������� ��� ��������� ���������� � ����� "Ammo"
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("Ammo"))
            {
                // ���������� ��� ��������, ������� ������ ������������ �� ���� ��������� ����������
                string ammoType = GetAmmoTypeFromTag(text);

                // ��������� ����� � ���������� ����������� ��������
                Items ammoItem = inventory.items
                    .FirstOrDefault(item => item.itemType == ItemTypes.Ammo && item.itemName == ammoType);

                if (ammoItem != null)
                {
                    text.text = $"{ammoItem.currentStack}";
                }
                else
                {
                    text.text = "0"; // ���� �������� ��� � ���������
                }
            }
        }
    }

    private string GetAmmoTypeFromTag(TextMeshProUGUI text)
    {
        // ���������� ��� �������� � ����������� �� ���� ���������� ����������
        switch (text.name)
        {
            case "PistolAmmoText":
                return "������� 9�18��";
            case "AutomaticAmmoText":
                return "������� 5.45�39��";
            default:
                return ""; // ���������� ������ ������, ���� ��� �� ���������
        }
    }

    private void UseMedkit()
    {
        // ������� ������� � ���������
        Items medkitItem = inventory.items
            .FirstOrDefault(item => item.itemType == ItemTypes.Medkit);

        if (medkitItem != null && medkitItem.currentStack > 0)
        {
            // ��������������� HP �����
            battleController.heroHP = Mathf.Min(battleController.heroHP + medkitItem.hpRestore, 100);
            medkitItem.currentStack--;

            // ���������, ���� ���������� ������� ����� 0 ��� ������
            if (medkitItem.currentStack <= 0)
            {
                inventory.RemoveItem(medkitItem);
            }
            else
            {
                inventory.UpdateItemsList();
            }

            // ��������� �����, ������� ���������� ���������� �������
            UpdateMedkitText();

            // ��������� UI ����� ������������� �������
            battleController.UpdateUI();
        }
    }

    private void UpdateMedkitText()
    {
        // ������� ��� ��������� ���������� � ����� "Med"
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("Med"))
            {
                if (text.gameObject.name == "MedkitCountText")
                {
                    // ��������� ����� � ���������� ����������� �������
                    Items medkitItem = inventory.items
                        .FirstOrDefault(item => item.itemType == ItemTypes.Medkit);

                    if (medkitItem != null)
                    {
                        text.text = $"{medkitItem.currentStack}";
                    }
                    else
                    {
                        text.text = "0"; // ���� ������� ��� � ���������
                    }
                }
            }
        }
    }

    private void EquipClothing()
    {
        if (currentItem.itemType == ItemTypes.Clothing)
        {
            if (currentItem.clothingType == ClothingType.Head)
            {
                Items previousHead = battleController.equippedHead;
                battleController.equippedHead = currentItem;
                if (previousHead != null)
                {
                    inventory.AddItem(previousHead);
                }
                inventory.RemoveItem(currentItem);
            }
            else if (currentItem.clothingType == ClothingType.Chest)
            {
                Items previousChest = battleController.equippedChest;
                battleController.equippedChest = currentItem;
                if (previousChest != null)
                {
                    inventory.AddItem(previousChest);
                }
                inventory.RemoveItem(currentItem);
            }
            battleController.UpdateUI(); // ��������� UI ����� ����������
            inventory.UpdateUI(); // ��������� UI ���������
        }
    }

    private void RemoveItem()
    {
        inventory.RemoveItem(currentItem);
        gameObject.SetActive(false); // Hide the popup
    }

    private void ClosePopup()
    {
        gameObject.SetActive(false); // Close the popup
    }

}