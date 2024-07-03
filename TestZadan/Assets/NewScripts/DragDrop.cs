using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private Canvas canvas;
    public Items item; // Ссылка на Item

    public GameObject itemPopupPrefab; // Префаб попап окна
    private GameObject currentPopup; // Ссылка на текущее окно

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        RectTransform slotTransform = GetSlotFromPointerEvent(eventData);
        InventorySlots newSlot = slotTransform != null ? slotTransform.GetComponent<InventorySlots>() : null;

        if (newSlot != null && newSlot.GetComponentInChildren<DragDrop>() == null)
        {
            InventorySlots oldSlot = originalParent.GetComponent<InventorySlots>();
            if (oldSlot != null)
            {
                oldSlot.RemoveItem(item);
            }

            newSlot.AddItem(item);
            transform.SetParent(newSlot.transform);
            transform.localPosition = Vector3.zero;
        }
    }

    private RectTransform GetSlotFromPointerEvent(PointerEventData eventData)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = eventData.position
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (var result in results)
        {
            RectTransform slot = result.gameObject.GetComponent<RectTransform>();
            if (slot != null && slot.CompareTag("InventorySlot"))
            {
                return slot;
            }
        }

        return null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null)
        {
            ShowItemPopup();
        }
    }

    private void ShowItemPopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }

        currentPopup = Instantiate(itemPopupPrefab, canvas.transform);
        ItemPopup itemPopup = currentPopup.GetComponent<ItemPopup>();

        if (itemPopup != null)
        {
            itemPopup.Initialize(item, FindObjectOfType<Inventory>(), FindObjectOfType<BattleController>());
        }
    }
}
