using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlots : MonoBehaviour
{
    public Items item;
    public Inventory inventory;
    private DragDrop dragDropComponent;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        dragDropComponent = GetComponentInChildren<DragDrop>(true);
        if (dragDropComponent != null)
        {
            item = dragDropComponent.item;
        }
    }

    public void AddItem(Items newItem)
    {
        item = newItem;
    }

    public void ClearSlot()
    {
        item = null;
    }

    public void RemoveItem(Items itemToRemove)
    {
        if (item != null && item == itemToRemove)
        {
            ClearSlot();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragDrop droppedItem = eventData.pointerDrag.GetComponent<DragDrop>();
        if (droppedItem != null)
        {
            Items itemToMove = droppedItem.item;
            AddItem(itemToMove);
            InventorySlots previousSlot = droppedItem.GetComponentInParent<InventorySlots>();
            if (previousSlot != null)
            {
                previousSlot.RemoveItem(itemToMove);
            }
            if (inventory != null)
            {
                inventory.UpdateItemsList();
            }
        }
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (item != null)
    //    {
    //        // Instantiate and initialize popup
    //        GameObject popup = Instantiate(itemPopupPrefab, transform.root);
    //        ItemPopup itemPopup = popup.GetComponent<ItemPopup>();
    //        if (itemPopup != null)
    //        {
    //            itemPopup.Initialize(item, inventory, FindObjectOfType<BattleController>());
    //        }
    //    }
    //}
}
