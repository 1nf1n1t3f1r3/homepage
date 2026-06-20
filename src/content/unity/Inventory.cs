using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Mirror;

public enum EquipmentSlotType
{
    Shield = 0,
    Head = 1,
    Chest = 2,
    Legs = 3,


    // Add more as needed
}


public class Inventory : NetworkBehaviour
{
    // References
    private ItemDatabase itemDatabase; // Reference to ItemDatabase
    private InventoryUI inventoryUI; // Reference to the player's inventory UI
    private InventoryCursorSlotUI inventoryCursorSlotUI;
    private PlayerStats playerStats;
    private PlayerController playerController;
    private PlayerEventManager playerEventManager;

    // Inventory Size Variables
    private int hotbarSize = 10;
    private int inventorySize = 50;

    public int HotbarSize => hotbarSize; // read-only
    public int InventorySize => inventorySize;

    // SyncLists for Inventory & Equipment
    public readonly SyncList<InventorySlot> inventorySlots = new();
    public readonly SyncList<InventorySlot> equipmentSlots = new();


    // Synced CursorSlot Contents
    [SyncVar(hook = nameof(OnCursorSlotChanged))]
    private InventorySlot cursorSlot;

    // Synced Active Item Variable Deduced from Active Slot Index
    [SyncVar(hook = nameof(OnActiveItemIDChanged))]
    private int activeItemID;

    // Local ActiveItem Chain
    private Item activeItem;
    public Item ActiveItem => activeItem;

    private InventorySlot activeSlot;

    private int activeSlotIndex;
    public int ActiveSlotIndex => activeSlotIndex; // read-only from outside

    // Inventory Event System
    public delegate void InventoryChanged();
    public static event InventoryChanged OnInventoryChanged;

    // Inventory UI open/close state 
    private bool inventoryUIOpen = false;
    public bool InventoryUIOpen => inventoryUIOpen;





    // Awake: Initialize References and the Inventory 
    private void Awake()
    {
        // References 
        itemDatabase = ItemDatabase.Instance;
        playerController = GetComponent<PlayerController>();
        playerEventManager = GetComponent<PlayerEventManager>();
        playerStats = GetComponent<PlayerStats>();
        inventoryUI = GetComponent<InventoryUI>();
        inventoryCursorSlotUI = GetComponentInChildren<InventoryCursorSlotUI>();
    }

    // Initialize Inventories on Server Start
    public override void OnStartServer()
    {
        // Initialize inventory with default slots
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(new InventorySlot(0, 0)); // Empty slot at start
        }

        // Initialize equipment slots on server start
        for (int i = 0; i < System.Enum.GetValues(typeof(EquipmentSlotType)).Length; i++)
        {
            equipmentSlots.Add(new InventorySlot(0, 0));
        }

        // Initialize cursor slot
        cursorSlot = new InventorySlot(0, 0);
    }

    // Subscribe
    public override void OnStartClient()
    {
        base.OnStartClient();

        inventorySlots.Callback += OnInventorySlotChanged;
        equipmentSlots.Callback += OnEquipmentSlotChanged;

        // Initialize UI once slots are ready
        if (isLocalPlayer)
        {
            inventoryUI.Initialize();
            inventoryUI.RefreshAllSlots();
        }

        else
        {
            inventoryUI.DisableForRemotePlayer();
        }
    }

    // Unsubscribe
    public override void OnStopClient()
    {
        inventorySlots.Callback -= OnInventorySlotChanged;
        equipmentSlots.Callback -= OnEquipmentSlotChanged;
    }

    // Validity Check
    private bool IsValidItemInSlot(InventorySlot slot)
    {
        // Empty slot is always valid
        if (slot.id == 0 && slot.count == 0) return true;

        // Lookup item
        Item item = itemDatabase.GetItemByID(slot.id);

        if (item == null)
        {
            Debug.LogWarning($"[Inventory] Invalid item ID: {slot.id}");
            return false;
        }

        // Validate stack count
        if (slot.count < 0 || slot.count > item.maxStack)
        {
            Debug.LogWarning($"[Inventory] Invalid stack count {slot.count} for item '{item.name}' (max: {item.maxStack})");
            return false;
        }

        return true;
    }

    // Assuming you know which EquipmentSlotType you want to check against
    private bool CanEquipItemInSlot(InventorySlot itemSlot, int equipmentSlotIndex)
    {
        if (itemSlot.IsEmpty) return false; // Return if not holding anything

        Item item = itemDatabase.GetItemByID(itemSlot.id);
        if (item == null) return false;

        EquipmentSlotType slotType = (EquipmentSlotType)equipmentSlotIndex;
        bool isValid = IsItemValidForSlot(item, slotType);

        if (!isValid)
        {
            Debug.Log($"[CanEquipItemInSlot] Item '{item.name}' (type {item.type}) is not valid for EquipmentSlot '{slotType}'.");
        }

        return isValid;
    }

    // Map EquipmentSlotType to ItemType 
    private bool IsItemValidForSlot(Item item, EquipmentSlotType slotType)
    {
        switch (slotType)
        {
            case EquipmentSlotType.Shield:
                return item.type == ItemType.Shield;
            case EquipmentSlotType.Head:
                return item.type == ItemType.Helmet;
            case EquipmentSlotType.Chest:
                return item.type == ItemType.Chestpiece;
            case EquipmentSlotType.Legs:
                return item.type == ItemType.Greaves;
            default:
                return false;
        }
    }


    // shared logic
    private void OnCursorSlotChanged(InventorySlot oldSlot, InventorySlot newSlot)
    {
        UpdateActiveItemChain();
        inventoryCursorSlotUI.SetSlotUI(cursorSlot);

        if (isLocalPlayer && inventoryCursorSlotUI != null)
        {
            inventoryCursorSlotUI.SetSlotUI(newSlot);
        }
    }

    // shared logic
    private void OnActiveItemIDChanged(int oldID, int newID)
    {
        UpdateActiveItemChain();
    }


    private void OnInventorySlotChanged(SyncList<InventorySlot>.Operation op, int index, InventorySlot oldItem, InventorySlot newSlot)
    {
        switch (op)
        {
            case SyncList<InventorySlot>.Operation.OP_ADD:     // Add Slot
                Debug.Log("Added slot at " + index);

                // UI Updates
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshAllSlots(); // or RefreshSlot(index) later
                }

                break;
            case SyncList<InventorySlot>.Operation.OP_REMOVEAT:        // Remove Slot
                Debug.Log("Removed slot at " + index);

                // UI Updates
                if (inventoryUI != null)
                {
                    inventoryUI.RefreshAllSlots(); // or RefreshSlot(index) later
                }

                break;
            case SyncList<InventorySlot>.Operation.OP_SET:     // Set Slot (Its Contents) 
                if (!IsValidItemInSlot(newSlot))
                {
                    Debug.LogError($"[Inventory] Invalid slot data at index {index}, reverting to empty.");
                    inventorySlots[index] = new InventorySlot(0, 0); // Reset to empty
                }



                else
                {
                    Debug.Log($"Slot {index} updated");
                }

                // UI Updates
                inventoryUI?.RefreshAllSlots(); // or RefreshSlot(index) later
                break;

        }
    }

    private void OnEquipmentSlotChanged(SyncList<InventorySlot>.Operation op, int index, InventorySlot oldItem, InventorySlot newSlot)
    {
        switch (op)
        {
            case SyncList<InventorySlot>.Operation.OP_ADD:
                Debug.Log("[Equipment] Added slot at " + index);
                inventoryUI?.RefreshAllSlots(); // or RefreshEquipmentSlot(index);
                break;

            case SyncList<InventorySlot>.Operation.OP_REMOVEAT:
                Debug.Log("[Equipment] Removed slot at " + index);
                inventoryUI?.RefreshAllSlots();
                break;

            case SyncList<InventorySlot>.Operation.OP_SET:
                if (!IsValidItemInSlot(newSlot))
                {
                    Debug.LogError($"[Equipment] Invalid item in slot {index}, resetting.");
                    equipmentSlots[index] = InventorySlot.Empty;
                }
                else
                {
                    Debug.Log($"[Equipment] Slot {index} updated");
                }

                inventoryUI?.RefreshAllSlots();
                break;
        }
    }



    // Update Stats
    private void UpdateActiveItemChain()
    {
        if (cursorSlot.id != 0)
        {
            activeSlot = cursorSlot;
            activeItem = itemDatabase.GetItemByID(cursorSlot.id);
        }
        else if (activeSlotIndex >= 0 && activeSlotIndex < inventorySlots.Count)
        {
            activeSlot = inventorySlots[activeSlotIndex];
            activeItem = itemDatabase.GetItemByID(activeSlot.id);
        }
        else // Error
        {
            activeSlot = new InventorySlot(0, 0);
            activeItem = null;

            Debug.Log("[UpdateActiveItemChain] Error");
        }

        // 🔁 Sync the ID for visibility across network. Only the server should write to SyncVars
        if (isServer)
        {
            activeItemID = activeItem != null ? activeItem.id : 0;
        }

        // Update the player stats
        playerStats.RefreshAllStats();

        // Visual Updates

        // UI Updates
    }

    // Try Get Slot by UISlotInfo
    private bool TryGetSlot(UISlotInfo info, out InventorySlot slot)
    {
        slot = default;

        if (info.Type == UISlotInfo.SlotType.Inventory)
        {
            if (info.Index < 0 || info.Index >= inventorySlots.Count) return false;
            slot = inventorySlots[info.Index];
            return true;
        }
        else if (info.Type == UISlotInfo.SlotType.Equipment)
        {
            if (info.Index < 0 || info.Index >= equipmentSlots.Count) return false;
            slot = equipmentSlots[info.Index];
            return true;
        }

        return false;
    }

    // Atomic Operations. Set All Slots (Inventory/Equipment)
    private bool SetSlot(UISlotInfo info, InventorySlot newSlot)
    {
        if (info.Type == UISlotInfo.SlotType.Inventory)
        {
            if (info.Index < 0 || info.Index >= inventorySlots.Count) return false;
            SetInventorySlot(info.Index, newSlot);
            return true;
        }
        else if (info.Type == UISlotInfo.SlotType.Equipment)
        {
            if (info.Index < 0 || info.Index >= equipmentSlots.Count) return false;
            return SetEquipmentSlot(info.Index, newSlot); // ← updated to return bool
        }
        else
        {
            return false;
        }
    }

    // Set Inventory Slots Only
    private void SetInventorySlot(int index, InventorySlot slot)
    {
        if (!IsValidItemInSlot(slot))
        {
            Debug.LogError($"[SetInventorySlot] Attempted to set invalid slot at index {index}. Resetting to empty.");
            inventorySlots[index] = InventorySlot.Empty;
        }

        else
        {
            inventorySlots[index] = slot;
        }

        OnInventoryChanged?.Invoke(); // Event for UI updates
    }

    // Set Equipment Slots
    private bool SetEquipmentSlot(int index, InventorySlot slot)
    {
        // Always allow Clearing
        if (slot.IsEmpty)
        {
            equipmentSlots[index] = InventorySlot.Empty;
            OnInventoryChanged?.Invoke();
            playerStats.RefreshAllStats();
            return true;
        }

        if (!IsValidItemInSlot(slot))
        {
            Debug.Log($"[SetEquipmentSlot] Attempted to set an invalid Equipment Slot at index {index}. Resetting to empty.");
            equipmentSlots[index] = InventorySlot.Empty;
            return false;
        }

        if (!CanEquipItemInSlot(slot, index))
        {
            Debug.Log($"[SetEquipmentSlot] Cannot equip item {slot.id} in slot {index}");
            return false;
        }

        equipmentSlots[index] = slot;
        OnInventoryChanged?.Invoke();
        playerStats.RefreshAllStats();
        return true;
    }

    [Server]
    private void SetCursorSlot(InventorySlot slot)
    {
        if (!IsValidItemInSlot(slot))
        {
            cursorSlot = InventorySlot.Empty;
        }

        else
        {
            if (slot.count == 0)
            {
                cursorSlot = InventorySlot.Empty;
            }

            else
            {
                cursorSlot = slot;
            }

        }

        // Updates via SyncVar hook will already handle UpdateActiveItemChain() and any visual updates
        OnInventoryChanged?.Invoke();
    }





    // Merge Into a Slot depending on how many of itemId we're trying to place there
    private int MergeIntoSlot(UISlotInfo slotInfo, int itemID, int remaining)
    {
        var item = itemDatabase.GetItemByID(itemID);
        if (item == null || remaining <= 0) return remaining;

        if (!TryGetSlot(slotInfo, out InventorySlot slot)) return remaining;

        if (slot.id == itemID && slot.count < item.maxStack)
        {
            int space = item.maxStack - slot.count;
            int toAdd = Mathf.Min(space, remaining);

            SetSlot(slotInfo, new InventorySlot(itemID, slot.count + toAdd));
            return remaining - toAdd;
        }

        return remaining;
    }

    // Merge into the CursorSlot
    private void MergeIntoCursorSlot(UISlotInfo slotInfo)
    {
        InventorySlot slot;

        // Get the source slot based on slot type
        if (slotInfo.Type == UISlotInfo.SlotType.Inventory)
        {
            if (slotInfo.Index < 0 || slotInfo.Index >= inventorySlots.Count) return;
            slot = inventorySlots[slotInfo.Index];
        }
        else if (slotInfo.Type == UISlotInfo.SlotType.Equipment)
        {
            if (slotInfo.Index < 0 || slotInfo.Index >= equipmentSlots.Count) return;
            slot = equipmentSlots[slotInfo.Index];
        }
        else return;

        // Defensive checks
        if (slot.IsEmpty || cursorSlot.IsEmpty || slot.id != cursorSlot.id) return;
        var item = itemDatabase.GetItemByID(slot.id);
        if (item == null) return;

        // Continue
        int maxStack = item.maxStack;
        int space = maxStack - cursorSlot.count;
        int toMove = Mathf.Min(space, slot.count);

        if (toMove <= 0) return;

        // Update cursor and source slot
        if (SetSlot(slotInfo, new InventorySlot(slot.id, slot.count - toMove)))
        {
            cursorSlot = new InventorySlot(slot.id, cursorSlot.count + toMove);
        }
    }








    // Inventory Operations
    public void FullPickUpFromSlotToCursorSlot(UISlotInfo slotInfo)
    {
        InventorySlot slot;
        if (!TryGetSlot(slotInfo, out slot)) return;
        if (slot.count == 0) return;

        // Move Slot to Empty CursorSlot
        if (cursorSlot.IsEmpty)
        {
            bool cleared = SetSlot(slotInfo, InventorySlot.Empty);
            if (cleared)
            {
                SetCursorSlot(slot);
            }
            else
            {
                Debug.Log($"[FullPickUpFromSlotToCursorSlot] Failed to clear slot {slotInfo.Type} {slotInfo.Index}");
            }

        }

        // Item in CursorSlot and Slot are the Same. Attempt Merge
        else if (cursorSlot.count != 0 && slot.id == cursorSlot.id)
        {
            MergeIntoCursorSlot(slotInfo);
        }

        // Holding a different Item in CursorSlot. Swap
        else
        {
            InventorySlot oldCursor = cursorSlot;
            bool swapped = SetSlot(slotInfo, oldCursor);
            if (swapped)
            {
                SetCursorSlot(slot);
            }
            else
            {
                Debug.Log($"[FullPickUpFromSlotToCursorSlot] Failed to Swap slot {slotInfo.Type} {slotInfo.Index}");
            }
        }
    }

    // Pick Up One at a Time. Note we're not using the 'Merge' Function, but just doing -1 and +1. It's easier here. 
    public void SinglePickUpFromSlotToCursorSlot(UISlotInfo slotInfo)
    {
        InventorySlot slot;
        if (!TryGetSlot(slotInfo, out slot)) return;
        if (slot.IsEmpty || slot.count < 1) return;

        // Cursor is Empty — start a new stack
        if (cursorSlot.IsEmpty)
        {
            int newCount = slot.count - 1;
            bool cleared = SetSlot(slotInfo, newCount > 0 ? new InventorySlot(slot.id, newCount) : InventorySlot.Empty);
            if (cleared)
            {
                SetCursorSlot(new InventorySlot(slot.id, 1));
            }
        }

        // Cursor has same Item and is not full — add 1
        else if (cursorSlot.id == slot.id)
        {
            var item = itemDatabase.GetItemByID(slot.id);
            if (item == null) return;

            if (cursorSlot.count < item.maxStack)
            {
                int newCount = slot.count - 1;
                bool cleared = SetSlot(slotInfo, newCount > 0 ? new InventorySlot(slot.id, newCount) : InventorySlot.Empty);
                if (cleared)
                {
                    SetCursorSlot(new InventorySlot(cursorSlot.id, cursorSlot.count + 1));
                }
            }
        }
    }



    // Place 
    public void FullPlacementFromCursorSlotIntoSlot(UISlotInfo slotInfo)
    {
        if (cursorSlot.IsEmpty) return;

        InventorySlot targetSlot;
        if (!TryGetSlot(slotInfo, out targetSlot)) return;

        // Place Into Empty Slot
        if (targetSlot.IsEmpty)
        {
            if (SetSlot(slotInfo, cursorSlot))
            {
                SetCursorSlot(InventorySlot.Empty);
            }
        }

        // Merge into Slot, if it has the same Item. MergeIntoSlot has Defensive Checks for maxStack, Null etc. 
        else if (targetSlot.id == cursorSlot.id)
        {
            int beforeCount = cursorSlot.count;
            int remaining = MergeIntoSlot(slotInfo, cursorSlot.id, cursorSlot.count);
            int toMove = beforeCount - remaining;

            // Update CursorSlot with Reduced Count. SetSlot handled by MergeIntoSlot. 
            SetCursorSlot(new InventorySlot(cursorSlot.id, remaining));
        }

        // Swap if neither Empty nor the same Item
        else
        {
            var temp = targetSlot;
            if (SetSlot(slotInfo, cursorSlot))
            {
                SetCursorSlot(temp);
            }
        }
    }








    // General Item Additions without CursorSlot
    private int TryAddItemToInventory(int itemID, int count)
    {
        var item = itemDatabase.GetItemByID(itemID);
        if (item == null || count <= 0) return 0;

        int remaining = count;

        // 1. Try to merge into existing partial stacks, where MergeIntoSlot checks if any of these Slots contain the same Item
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slotInfo = new UISlotInfo { Type = UISlotInfo.SlotType.Inventory, Index = i };
            remaining = MergeIntoSlot(slotInfo, itemID, remaining);
            if (remaining == 0)
            {
                Debug.Log($"[TryAddItemToInventory] Merged Count {count} * ID {itemID}");
                return count;
            }
        }

        // 2. Try to place into empty Slots. First check if we're still holding any after the Merge Attempt
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];

            if (slot.count == 0)
            {

                int toAdd = Mathf.Min(item.maxStack, remaining);
                inventorySlots[i] = new InventorySlot(itemID, toAdd);
                remaining -= toAdd;

                if (remaining == 0)
                {
                    Debug.Log($"[TryAddItemToInventory] Added Count {count} * ID {itemID} to Empty Slot");
                    return count;
                }
            }
        }

        // 3. If we got here, not everything fit. Currently simply returning False
        // Will want to Update to allow Partial Pick-Ups
        // Including Updating the Source Stack with its new Count information in order to prevent Duplication
        int added = count - remaining;
        Debug.Log($"[TryAddItemToInventory] Only added {added} of {count} * {item.name} (ID {itemID}).");
        return added;
    }





    // General Item Removals. (Crafting, Buying Etc.)
    private bool RemoveItemFromInventory(int itemID, int count)
    {
        int remaining = count;

        // Check CursorSlot First
        int totalAvailable = 0;
        if (cursorSlot.id == itemID)
        {
            totalAvailable += cursorSlot.count;
        }

        // Loop over Inventory Slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].id == itemID)
                totalAvailable += inventorySlots[i].count;
        }

        if (totalAvailable < count) return false;

        // Remove from cursorSlot first, if applicable
        if (cursorSlot.id == itemID)
        {
            int toRemove = Mathf.Min(cursorSlot.count, remaining);

            int newCursorCount = cursorSlot.count - toRemove;
            cursorSlot = newCursorCount > 0 ? new InventorySlot(itemID, newCursorCount) : InventorySlot.Empty;
            inventoryCursorSlotUI.SetSlotUI(cursorSlot); // UI update

            remaining -= toRemove;
        }

        // Now remove from inventory slots
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (slot.id == itemID)
            {
                int toRemove = Mathf.Min(slot.count, remaining);

                int newCount = slot.count - toRemove;
                inventorySlots[i] = newCount > 0 ? new InventorySlot(itemID, newCount) : InventorySlot.Empty;

                remaining -= toRemove;

                if (remaining <= 0) break;
            }
        }

        OnInventoryChanged?.Invoke(); // Notify other systems
        return true;
    }



    // Middle Mouse Button (Scroll Wheel) Click on the Inventory UI. Select a Slot
    public void SelectSlotIndex(int index)
    {
        if (index < 0 || index >= inventorySlots.Count) return;

        activeSlotIndex = index;
        UpdateActiveItemChain();
        inventoryUI.SetActiveSlotIndex(index);
    }

    // Open / Close the Inventory
    public void ToggleInventory()
    {
        if (!inventoryUIOpen)
        {
            Debug.Log("[ToggleInventory] Received with !inventoryUIOpen");
            inventoryUIOpen = true;
            inventoryUI.ToggleInventoryUI(true);

        }

        else if (inventoryUIOpen)
        {
            Debug.Log("[ToggleInventory] Received with inventoryUIOpen");
            inventoryUIOpen = false;
            inventoryUI.ToggleInventoryUI(false);

        }
    }


    public void SortInventory()
    {
        itemDatabase = ItemDatabase.Instance;

        // Step 1: Copy current inventory into a working list
        List<InventorySlot> sorted = new(inventorySlots);

        // Step 2: Extract and merge non-hotbar items
        List<InventorySlot> toSort = new();
        for (int i = 10; i < sorted.Count; i++)
        {
            if (!sorted[i].IsEmpty)
                toSort.Add(sorted[i]);
        }

        List<InventorySlot> merged = MergeStackables(toSort);

        // Step 3: Sort merged stack by ItemType then name
        merged.Sort((a, b) =>
        {
            var itemA = itemDatabase.GetItemByID(a.id);
            var itemB = itemDatabase.GetItemByID(b.id);
            if (itemA == null || itemB == null) return 0;

            int typeCompare = itemA.type.CompareTo(itemB.type);
            if (typeCompare != 0) return typeCompare;

            return string.Compare(itemA.name, itemB.name, System.StringComparison.Ordinal);
        });

        // Step 4: Write only changed slots
        for (int i = 10; i < inventorySlots.Count; i++)
        {
            InventorySlot newSlot = (i - 10 < merged.Count) ? merged[i - 10] : InventorySlot.Empty;
            if (!inventorySlots[i].Equals(newSlot))
            {
                inventorySlots[i] = newSlot;
            }
        }

        // Step 5: Refresh UI
        OnInventoryChanged?.Invoke();
        Debug.Log("✅ Inventory sorted with diff-based update.");
    }




    private List<InventorySlot> MergeStackables(List<InventorySlot> slots)
    {
        Dictionary<int, int> idToCount = new();

        foreach (var slot in slots)
        {
            if (idToCount.ContainsKey(slot.id))
            {
                idToCount[slot.id] += slot.count;
            }

            else
            {
                idToCount[slot.id] = slot.count;
            }

        }

        List<InventorySlot> result = new();

        foreach (var kvp in idToCount)
        {
            var item = itemDatabase.GetItemByID(kvp.Key);
            if (item == null) continue;

            int total = kvp.Value;

            while (total > 0)
            {
                int toTake = Mathf.Min(item.maxStack, total);
                result.Add(new InventorySlot(item.id, toTake));
                total -= toTake;
            }
        }

        return result;
    }



    // Left Click on the Inventory UI. Either Holding something in CursorSlot or Not. Pick Up or Place a Full Stack.
    [Command]
    public void InventoryLeftClick(UISlotInfo slotInfo)
    {
        // Checks for Inventory/Equipment/Invalid
        InventorySlot slot;
        if (slotInfo.Type == UISlotInfo.SlotType.Inventory)
        {
            if (slotInfo.Index < 0 || slotInfo.Index >= inventorySlots.Count) return;
            slot = inventorySlots[slotInfo.Index];
        }
        else if (slotInfo.Type == UISlotInfo.SlotType.Equipment)
        {
            if (slotInfo.Index < 0 || slotInfo.Index >= equipmentSlots.Count) return;
            slot = equipmentSlots[slotInfo.Index];
        }
        else return;

        // Operations
        if (cursorSlot.IsEmpty && !slot.IsEmpty)
        {
            FullPickUpFromSlotToCursorSlot(slotInfo);
        }
        else if (!cursorSlot.IsEmpty && (slot.IsEmpty || slot.id == cursorSlot.id))
        {
            FullPlacementFromCursorSlotIntoSlot(slotInfo);
        }
        else if (!cursorSlot.IsEmpty && !slot.IsEmpty && cursorSlot.id != slot.id)
        {
            FullPickUpFromSlotToCursorSlot(slotInfo);
        }
    }



    // Right Click on the Inventory UI. Either Holding something in CursorSlot or Not. Pick Up or Place a Single Unit
    [Command]
    public void InventoryRightClick(UISlotInfo slotInfo)
    {
        if (!TryGetSlot(slotInfo, out InventorySlot slot)) return;

        // Pick Up One from Stack. If CursorSlot is Empty, Take One. Merge if Same ID between CursorSlot/TargetSlot && CursorSlot < maxStack
        if (!slot.IsEmpty)
        {
            SinglePickUpFromSlotToCursorSlot(slotInfo);
        }
    }











    // Debug Logic
    public void HandleDebugCommands()
    {
        // Print Inventory Contents
        if (Input.GetKeyDown(KeyCode.Y))
        {
            PrintInventory();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            CmdGiveItem(1, 1); // Give 1 of item ID 1
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            CmdGiveItem(2, 30); // Give 30 of item ID 2
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            CmdGiveItem(3, 30); // Give 30 of item ID 3
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            CmdGiveItem(4, 1); // Give 1 of item ID 4
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            CmdRemoveitem(1, 1); // Remove 1 of item ID 1
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            CmdRemoveitem(2, 1); // Remove 1 of item ID 2
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            CmdRemoveitem(3, 16); // Remove 16 of item ID 3
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CmdRemoveitem(4, 1); // Remove 16 of item ID 4
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            CmdSortInventory();
        }


    }

    [Command]
    public void CmdSortInventory()
    {
        SortInventory();
    }



    // Debug Commands to Give Items
    [Command]
    public void CmdGiveItem(int itemID, int count)
    {
        Debug.Log($"[CmdDebugGiveItem] Called with itemID: {itemID}, count: {count}");
        TryAddItemToInventory(itemID, count);
    }

    // Debug Commands to Remove Items
    [Command]
    public void CmdRemoveitem(int itemID, int count)
    {
        Debug.Log($"[CmdRemoveitem] Called with itemID: {itemID}, count: {count}");
        RemoveItemFromInventory(itemID, count);
    }

    // Debug Print out the entire Inventory/Equipment Contents (Check if it's a UI or Inventory Problem)
    public void PrintInventory()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            var slot = inventorySlots[i];
            if (!slot.IsEmpty)
            {
                Debug.Log($"Slot {i}: ItemID {slot.id}, Count {slot.count}");
            }
            else
            {
                Debug.Log($"Slot {i}: EMPTY");
            }
        }

        for (int i = 0; i < equipmentSlots.Count; i++)
        {
            var slot = equipmentSlots[i];
            var slotName = ((EquipmentSlotType)i).ToString();
            if (!slot.IsEmpty)
            {
                Debug.Log($"Equipment Slot {slotName} ({i}): ItemID {slot.id}, Count {slot.count}");
            }
            else
            {
                Debug.Log($"Equipment Slot {slotName} ({i}): EMPTY");
            }
        }

        Debug.Log($"activeItem = {activeItem}. activeSlot = {activeSlot} activeSlotIndex = {activeSlotIndex}");
    }
}




