using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{
    private Items currentItem;
    public Inventory inventory;
    public TextMeshProUGUI heroHPText;
    public TextMeshProUGUI enemyHPText;
    public Image headSlot;
    public Image chestSlot;
    public TextMeshProUGUI headDefenseText;
    public TextMeshProUGUI chestDefenseText;

    public TextMeshProUGUI pistolAmmoText;
    public TextMeshProUGUI automaticAmmoText;
    public Button shootButton;
    public Toggle pistolToggle;
    public Toggle automaticToggle;

    public int heroHP = 100;
    private int enemyHP = 100;
    public Items equippedHead;
    public Items equippedChest;

    public GameObject gameOverPanel; // Панель "Game Over"

    public Sprite capIcon;
    public Sprite helmetIcon;
    public Sprite ammo9x18Icon;
    public Sprite ammo545x39Icon;
    public Sprite medkitIcon;
    public Sprite jacketIcon;
    public Sprite vestIcon;

    private void Awake()
    {
        // Находим все текстовые компоненты с тегом "Med"
        TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.CompareTag("Ammo"))
            {
                text.text = $"{currentItem.currentStack}";
            }
        }
    }

    private void Start()
    {
        // Устанавливаем начальное состояние переключателей оружия
        pistolToggle.isOn = false;
        automaticToggle.isOn = false;

        // Обновляем UI сразу при старте
        UpdateUI();
    }

    public void UpdateUI()
    {
        heroHPText.text = heroHP.ToString();
        enemyHPText.text = enemyHP.ToString();

        if (equippedHead != null)
        {
            headSlot.sprite = equippedHead.icon;
            headDefenseText.text = equippedHead.defense.ToString();
        }
        else
        {
            headSlot.sprite = null;
            headDefenseText.text = "0";
        }

        if (equippedChest != null)
        {
            chestSlot.sprite = equippedChest.icon;
            chestDefenseText.text = equippedChest.defense.ToString();
        }
        else
        {
            chestSlot.sprite = null;
            chestDefenseText.text = "0";
        }

        // Обновляем количество патронов в UI
        int pistolAmmoCount = GetAmmoCount(ItemTypes.Ammo, "Патроны 9х18мм");
        int automaticAmmoCount = GetAmmoCount(ItemTypes.Ammo, "Патроны 5.45х39мм");

        pistolAmmoText.text = pistolAmmoCount.ToString();
        automaticAmmoText.text = automaticAmmoCount.ToString();

        Debug.Log("Pistol ammo: " + pistolAmmoCount);
        Debug.Log("Automatic ammo: " + automaticAmmoCount);
    }

    private void GameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Отображаем панель "Game Over"
            Time.timeScale = 0;
        }
    }

    public void OnShootButtonClick()
    {
        // Проверяем, выбран ли пистолет или автомат
        bool isPistol = pistolToggle.isOn;
        bool isAutomatic = automaticToggle.isOn;

        // Предотвращаем стрельбу, если оружие не выбрано
        if (!isPistol && !isAutomatic)
        {
            Debug.Log("No weapon selected. Cannot shoot.");
            return;
        }

        // Определяем параметры оружия
        int weaponDamage = isPistol ? 5 : 9;
        string ammoType = isPistol ? "Патроны 9х18мм" : "Патроны 5.45х39мм";
        int ammoUsed = isPistol ? 1 : 3;

        // Проверяем наличие достаточного количества патронов
        if (GetAmmoCount(ItemTypes.Ammo, ammoType) < ammoUsed)
        {
            Debug.Log("Not enough ammo. Cannot shoot.");
            return;
        }

        // Обновляем HP врага
        enemyHP -= weaponDamage;

        if (enemyHP <= 0)
        {
            enemyHP = 0;
            // Добавляем добычу в инвентарь
            Items loot = GetRandomLoot();
            if (loot != null)
            {
                inventory.AddItem(loot);
                Debug.Log($"Added loot: {loot.itemName}");
            }
        }
        else
        {
            // Герой получает урон
            int damageTaken = 15 - GetEquippedDefense();
            damageTaken = Mathf.Max(damageTaken, 0); // Убедимся, что урон не может быть отрицательным
            heroHP -= damageTaken;

            if (heroHP <= 0)
            {
                heroHP = 0;
                GameOver();
                // Конец игры
                Debug.Log("Game Over!");
            }
        }

        // Потребляем патроны
        ConsumeAmmo(ammoType, ammoUsed);

        // Обновляем UI после стрельбы
        UpdateUI();
    }

    public void OnPistolToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Деактивируем другой переключатель
            automaticToggle.isOn = false;
        }
    }

    public void OnAutomaticToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Деактивируем другой переключатель
            pistolToggle.isOn = false;
        }
    }

    private int GetAmmoCount(ItemTypes itemType, string ammoType)
    {
        return inventory.items
            .Where(item => item.itemType == itemType && item.itemName == ammoType)
            .Sum(item => item.currentStack);
    }

    private void ConsumeAmmo(string ammoType, int amount)
    {
        foreach (var item in inventory.items
            .Where(item => item.itemType == ItemTypes.Ammo && item.itemName == ammoType)
            .OrderBy(item => item.currentStack))
        {
            if (amount <= item.currentStack)
            {
                item.currentStack -= amount;
                if (item.currentStack == 0)
                {
                    inventory.RemoveItem(item);
                }
                break;
            }
            else
            {
                amount -= item.currentStack;
                inventory.RemoveItem(item);
            }
        }
    }

    private int GetEquippedDefense()
    {
        int defense = 0;

        if (equippedHead != null)
        {
            defense += equippedHead.defense;
        }

        if (equippedChest != null)
        {
            defense += equippedChest.defense;
        }

        return defense;
    }

    private Items GetRandomLoot()
    {
        // Определяем возможные расходники
        Items[] consumables =
        {
            new Items("Патроны 9х18мм", ClothingType.None, ItemTypes.Ammo, ammo9x18Icon, 50, 50, 0.01f),
            new Items("Патроны 5.45х39мм", ClothingType.None, ItemTypes.Ammo, ammo545x39Icon, 100, 100, 0.03f),
            new Items("Medkit", ClothingType.None, ItemTypes.Medkit, medkitIcon, 6, 6, 1f, 50)
        };

        // Определяем возможные предметы одежды для головы
        Items[] headClothing =
        {
            new Items("Cap", ClothingType.Head, ItemTypes.Clothing, capIcon, 1, 1, 0.1f, 0, 3),
            new Items("Helmet", ClothingType.Head, ItemTypes.Clothing, helmetIcon, 1, 1, 1f, 0, 10)
        };

        // Определяем возможные предметы одежды для торса
        Items[] chestClothing =
        {
            new Items("Jacket", ClothingType.Chest, ItemTypes.Clothing, jacketIcon, 1, 1, 1f, 0, 3),
            new Items("Bulletproof Vest", ClothingType.Chest, ItemTypes.Clothing, vestIcon, 1, 1, 10f, 0, 10)
        };

        // Объединяем все предметы в один массив
        Items[] allLoot = consumables
            .Concat(headClothing)
            .Concat(chestClothing)
            .ToArray();

        // Если нет предметов, вернуть null
        if (allLoot.Length == 0)
        {
            return null;
        }

        // Выбираем случайный предмет из списка
        int randomIndex = Random.Range(0, allLoot.Length);
        Items randomLoot = allLoot[randomIndex];

        return randomLoot;
    }
}
