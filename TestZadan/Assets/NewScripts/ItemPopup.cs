using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopup : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI defenseText; // Поле для отображения защиты
    public TextMeshProUGUI weightText;  // Поле для отображения веса
    public Image itemImage; // Поле для изображения
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
        itemImage.sprite = item.icon; // Установка спрайта изображения

        // Обновляем текст защиты и веса
        defenseText.text = $"+{item.defense}";
        weightText.text = $"{item.weight} кг";

        actionButton1.onClick.RemoveAllListeners();
        actionButton2.onClick.RemoveAllListeners();

        // Setup buttons based on item type
        switch (item.itemType)
        {
            case ItemTypes.Ammo:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Купить";
                actionButton1.onClick.AddListener(() => { FillAmmo(); ClosePopup(); });
                break;
            case ItemTypes.Medkit:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Лечить";
                actionButton1.onClick.RemoveAllListeners();
                actionButton1.onClick.AddListener(() => { UseMedkit(); ClosePopup(); });
                break;
            case ItemTypes.Clothing:
                actionButton1.GetComponentInChildren<TextMeshProUGUI>().text = "Экипировать";
                actionButton1.onClick.AddListener(() => { EquipClothing(); ClosePopup(); });
                break;
        }

        actionButton2.GetComponentInChildren<TextMeshProUGUI>().text = "Удалить";
        actionButton2.onClick.AddListener(RemoveItem);

        gameObject.SetActive(true); // Show the popup
    }

    private void FillAmmo()
    {
        if (currentItem.itemType == ItemTypes.Ammo)
        {
            // Найти предмет с таким же названием
            Items ammoItem = inventory.items
                .FirstOrDefault(item => item.itemType == ItemTypes.Ammo && item.itemName == currentItem.itemName);

            if (ammoItem != null)
            {
                // Заполнить стак патронов
                ammoItem.currentStack = ammoItem.maxStack;
                inventory.UpdateItemsList();
            }
            else
            {
                // Если патроны не найдены, возможно создать новый объект патронов
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

            // Обновляем текст с актуальным количеством патронов
            UpdateAmmoText();
        }
    }

    private void UpdateAmmoText()
    {
        // Находим все текстовые компоненты с тегом "Ammo"
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("Ammo"))
            {
                // Определяем тип патронов, который должен отображаться на этом текстовом компоненте
                string ammoType = GetAmmoTypeFromTag(text);

                // Обновляем текст с актуальным количеством патронов
                Items ammoItem = inventory.items
                    .FirstOrDefault(item => item.itemType == ItemTypes.Ammo && item.itemName == ammoType);

                if (ammoItem != null)
                {
                    text.text = $"{ammoItem.currentStack}";
                }
                else
                {
                    text.text = "0"; // Если патронов нет в инвентаре
                }
            }
        }
    }

    private string GetAmmoTypeFromTag(TextMeshProUGUI text)
    {
        // Возвращаем тип патронов в зависимости от тега текстового компонента
        switch (text.name)
        {
            case "PistolAmmoText":
                return "Патроны 9х18мм";
            case "AutomaticAmmoText":
                return "Патроны 5.45х39мм";
            default:
                return ""; // Возвращаем пустую строку, если тег не распознан
        }
    }

    private void UseMedkit()
    {
        // Находим аптечку в инвентаре
        Items medkitItem = inventory.items
            .FirstOrDefault(item => item.itemType == ItemTypes.Medkit);

        if (medkitItem != null && medkitItem.currentStack > 0)
        {
            // Восстанавливаем HP героя
            battleController.heroHP = Mathf.Min(battleController.heroHP + medkitItem.hpRestore, 100);
            medkitItem.currentStack--;

            // Проверяем, если количество аптечек стало 0 или меньше
            if (medkitItem.currentStack <= 0)
            {
                inventory.RemoveItem(medkitItem);
            }
            else
            {
                inventory.UpdateItemsList();
            }

            // Обновляем текст, который отображает количество аптечек
            UpdateMedkitText();

            // Обновляем UI после использования аптечки
            battleController.UpdateUI();
        }
    }

    private void UpdateMedkitText()
    {
        // Находим все текстовые компоненты с тегом "Med"
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("Med"))
            {
                if (text.gameObject.name == "MedkitCountText")
                {
                    // Обновляем текст с актуальным количеством аптечек
                    Items medkitItem = inventory.items
                        .FirstOrDefault(item => item.itemType == ItemTypes.Medkit);

                    if (medkitItem != null)
                    {
                        text.text = $"{medkitItem.currentStack}";
                    }
                    else
                    {
                        text.text = "0"; // Если аптечек нет в инвентаре
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
            battleController.UpdateUI(); // Обновляем UI после экипировки
            inventory.UpdateUI(); // Обновляем UI инвентаря
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