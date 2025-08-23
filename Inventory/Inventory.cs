using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using IsometricActionGame.Items;
using IsometricActionGame.Events;

using System;
using System.Collections.Generic;

namespace IsometricActionGame.Inventory
{
    public class Inventory : IInventoryOperations
    {
        public const int INVENTORY_WIDTH = 6;
        public const int INVENTORY_HEIGHT = 4;
        public const int QUICK_ACCESS_SLOTS = 6; // Slots 1-6

        private Item[,] _inventoryGrid;
        private Item[] _quickAccessSlots;
        private InventoryDragManager _dragManager;
        private InventoryRenderer _renderer;
        
        public int Gold { get; private set; } = 0;
        public bool IsOpen { get; private set; } = false;
        public bool IsQuickAccessVisible { get; private set; } = true;

        public event Action<int> OnGoldChanged;
        public event Action OnInventoryChanged;
        public event Action<Item> OnItemDiscarded;



        /// <summary>
        /// Dispose cached textures to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            _renderer?.Dispose();
        }

        public Inventory()
        {
            _inventoryGrid = new Item[INVENTORY_WIDTH, INVENTORY_HEIGHT];
            _quickAccessSlots = new Item[QUICK_ACCESS_SLOTS];
            _dragManager = new InventoryDragManager(this);
            _renderer = new InventoryRenderer();
            _healthPotionAutoAssignmentManager = new HealthPotionAutoAssignmentManager(this);
        }
        
        /// <summary>
        /// Initialize inventory with screen dimensions for proper scaling and positioning
        /// </summary>
        public void Initialize(int screenWidth, int screenHeight)
        {
            _renderer.Initialize(screenWidth, screenHeight);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (Gold >= amount)
            {
                Gold -= amount;
                OnGoldChanged?.Invoke(Gold);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Clear all items from inventory and reset gold
        /// </summary>
        public void Clear()
        {
            // Clear inventory grid
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < INVENTORY_HEIGHT; y++)
                {
                    _inventoryGrid[x, y] = null;
                }
            }
            
            // Clear quick access slots
            for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
            {
                _quickAccessSlots[i] = null;
            }
            
            // Reset gold
            Gold = 0;
            OnGoldChanged?.Invoke(Gold);
            
            // Notify inventory changed
            OnInventoryChanged?.Invoke();
            
            System.Diagnostics.Debug.WriteLine("Inventory cleared");
        }

        public bool AddItem(Item item)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: Inventory.AddItem called for {item.Name}. IsStackable: {item.IsStackable}, Quantity: {item.Quantity}");
            if (item == null) return false;

            // Special handling for health potions - try to auto-assign to quick access first
            if (IsHealthPotion(item))
            {
                System.Diagnostics.Debug.WriteLine($"AddItem: Health potion detected, attempting auto-assignment");
                bool assigned = _healthPotionAutoAssignmentManager.AutoAssignToAnySlot(item);
                System.Diagnostics.Debug.WriteLine($"AddItem: Auto-assignment result: {assigned}");
                if (assigned)
                {
                    System.Diagnostics.Debug.WriteLine($"AddItem: Health potion auto-assigned successfully");
                    return true;
                }
                System.Diagnostics.Debug.WriteLine($"AddItem: Health potion auto-assignment failed, continuing with normal inventory placement");
            }

            // Try to stack with existing items first (but never stack health potions)
            if (item.IsStackable && !IsHealthPotion(item))
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: Item {item.Name} is stackable, trying to stack...");
                // Check inventory grid
                for (int x = 0; x < INVENTORY_WIDTH; x++)
                {
                    for (int y = 0; y < INVENTORY_HEIGHT; y++)
                    {
                        if (_inventoryGrid[x, y] != null && 
                            _inventoryGrid[x, y].GetType() == item.GetType() &&
                            _inventoryGrid[x, y].Quantity < _inventoryGrid[x, y].MaxStackSize)
                        {
                            int spaceInStack = _inventoryGrid[x, y].MaxStackSize - _inventoryGrid[x, y].Quantity;
                            int amountToAdd = Math.Min(item.Quantity, spaceInStack);
                            _inventoryGrid[x, y].Quantity += amountToAdd;
                            item.Quantity -= amountToAdd;
                            
                            if (item.Quantity <= 0)
                            {
                                OnInventoryChanged?.Invoke();
                                System.Diagnostics.Debug.WriteLine($"DEBUG: Successfully stacked {item.Name} in grid slot ({x}, {y})");
                                return true;
                            }
                        }
                    }
                }

                // Check quick access slots
                for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
                {
                    if (_quickAccessSlots[i] != null && 
                        _quickAccessSlots[i].GetType() == item.GetType() &&
                        _quickAccessSlots[i].Quantity < _quickAccessSlots[i].MaxStackSize)
                    {
                        int spaceInStack = _quickAccessSlots[i].MaxStackSize - _quickAccessSlots[i].Quantity;
                        int amountToAdd = Math.Min(item.Quantity, spaceInStack);
                        _quickAccessSlots[i].Quantity += amountToAdd;
                        item.Quantity -= amountToAdd;
                        
                        if (item.Quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            System.Diagnostics.Debug.WriteLine($"DEBUG: Successfully stacked {item.Name} in quick access slot {i}");
                            return true;
                        }
                    }
                }
            }

            // Find empty slot in inventory grid
            System.Diagnostics.Debug.WriteLine($"DEBUG: Looking for empty slot in inventory grid for {item.Name}...");
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < INVENTORY_HEIGHT; y++)
                {
                    if (_inventoryGrid[x, y] == null)
                    {
                        _inventoryGrid[x, y] = item;
                        OnInventoryChanged?.Invoke();
                        System.Diagnostics.Debug.WriteLine($"DEBUG: Successfully added {item.Name} to grid slot ({x}, {y})");
                        return true;
                    }
                }
            }

            // If inventory is full, try quick access slots
            System.Diagnostics.Debug.WriteLine($"DEBUG: Inventory grid full, trying quick access slots for {item.Name}...");
            for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
            {
                if (_quickAccessSlots[i] == null)
                {
                    _quickAccessSlots[i] = item;
                    OnInventoryChanged?.Invoke();
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Successfully added {item.Name} to quick access slot {i}");
                    return true;
                }
            }

            System.Diagnostics.Debug.WriteLine($"DEBUG: Failed to add {item.Name} - no space available");
            return false; // No space available
        }
        
        /// <summary>
        /// Checks if the item is a health potion
        /// </summary>
        private bool IsHealthPotion(Item item)
        {
            return item is Items.HealthPotion;
        }
        


        /// <summary>
        /// Debug method to log the state of all quick access slots
        /// </summary>
        private void LogQuickAccessSlotsState(string context)
        {
            System.Diagnostics.Debug.WriteLine($"=== Quick Access Slots State ({context}) ===");
            for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
            {
                var item = _quickAccessSlots[i];
                System.Diagnostics.Debug.WriteLine($"Slot {i}: {item?.Name ?? "null"} (Quantity: {item?.Quantity ?? 0})");
            }
            System.Diagnostics.Debug.WriteLine("==========================================");
        }

        /// <summary>
        /// Checks if the inventory contains an item of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of item to check for.</typeparam>
        /// <returns>True if an item of the specified type is found, otherwise false.</returns>
        public bool HasItem<T>() where T : Item
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: Checking inventory for item type.");
            // Check inventory grid
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < INVENTORY_HEIGHT; y++)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: Checking grid slot ({x}, {y}). Item: {(_inventoryGrid[x, y]?.Name ?? "None")}, Type: {(_inventoryGrid[x, y]?.GetType().Name ?? "None")}");
                    if (_inventoryGrid[x, y] is T)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: Found match in grid at ({x}, {y}).");
                        return true;
                    }
                }
            }

            // Check quick access slots
            for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: Checking quick access slot ({i}). Item: {(_quickAccessSlots[i]?.Name ?? "None")}, Type: {(_quickAccessSlots[i]?.GetType().Name ?? "None")}");
                if (_quickAccessSlots[i] is T)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: Found match in quick access at ({i}).");
                    return true;
                }
            }
            System.Diagnostics.Debug.WriteLine($"DEBUG: HasItem<{typeof(T).Name}>: No match found.");
            return false;
        }

        /// <summary>
        /// Removes the first found item of the specified type from the inventory.
        /// </summary>
        /// <typeparam name="T">The type of item to remove.</typeparam>
        /// <returns>True if an item was removed, otherwise false.</returns>
        public bool RemoveItem<T>() where T : Item
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Attempting to remove item.");
            // Check inventory grid
            for (int x = 0; x < INVENTORY_WIDTH; x++)
            {
                for (int y = 0; y < INVENTORY_HEIGHT; y++)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Checking grid slot ({x}, {y}). Item: {(_inventoryGrid[x, y]?.Name ?? "None")}, Type: {(_inventoryGrid[x, y]?.GetType().Name ?? "None")}");
                    if (_inventoryGrid[x, y] is T)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Removing item from grid at ({x}, {y}).");
                        _inventoryGrid[x, y] = null;
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }

            // Check quick access slots
            for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Checking quick access slot ({i}). Item: {(_quickAccessSlots[i]?.Name ?? "None")}, Type: {(_quickAccessSlots[i]?.GetType().Name ?? "None")}");
                if (_quickAccessSlots[i] is T)
                {
                    System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Removing item from quick access at ({i}).");
                    _quickAccessSlots[i] = null;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
            System.Diagnostics.Debug.WriteLine($"DEBUG: RemoveItem<{typeof(T).Name}>: Item not found.");
            return false;
        }

        public Item GetItem(int x, int y)
        {
            if (x >= 0 && x < INVENTORY_WIDTH && y >= 0 && y < INVENTORY_HEIGHT)
            {
                return _inventoryGrid[x, y];
            }
            return null;
        }

        public Item GetQuickAccessItem(int slot)
        {
            System.Diagnostics.Debug.WriteLine($"GetQuickAccessItem: Called with slot={slot}");
            
            if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
            {
                var item = _quickAccessSlots[slot];
                System.Diagnostics.Debug.WriteLine($"GetQuickAccessItem: Slot {slot} contains: {item?.Name ?? "null"}");
                return item;
            }
            
            System.Diagnostics.Debug.WriteLine($"GetQuickAccessItem: Invalid slot index {slot}");
            return null;
        }

        public void RemoveItem(int x, int y)
        {
            if (x >= 0 && x < INVENTORY_WIDTH && y >= 0 && y < INVENTORY_HEIGHT)
            {
                _inventoryGrid[x, y] = null;
                OnInventoryChanged?.Invoke();
            }
        }

        public void RemoveQuickAccessItem(int slot)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Called with slot={slot}");
                
                if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
                {
                    var item = _quickAccessSlots[slot];
                    System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Removing item: {item?.Name ?? "null"} from slot {slot}");
                    
                    _quickAccessSlots[slot] = null;
                    System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Slot {slot} cleared");
                    
                    OnInventoryChanged?.Invoke();
                    System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: OnInventoryChanged invoked");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Invalid slot index {slot}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"RemoveQuickAccessItem: Stack trace: {ex.StackTrace}");
            }
        }

        public void PlaceItem(int x, int y, Item item)
        {
            if (x >= 0 && x < INVENTORY_WIDTH && y >= 0 && y < INVENTORY_HEIGHT)
            {
                _inventoryGrid[x, y] = item;
                OnInventoryChanged?.Invoke();
            }
        }

        public bool PlaceQuickAccessItem(int slot, Item item)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Called with slot={slot}, item={item?.Name ?? "null"}");
                System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Item details - IsUsable: {item?.IsUsable}, Quantity: {item?.Quantity}, Type: {item?.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Item hash code: {item?.GetHashCode()}");
                
                if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
                {
                    // Only allow usable items in quick access slots
                    if (item != null && item.IsUsable)
                    {
                        System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Item is valid, placing in slot {slot}");
                        _quickAccessSlots[slot] = item;
                        System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Successfully placed item in slot {slot}");
                        
                        // OnInventoryChanged temporarily disabled to prevent conflicts during auto-assignment
                        
                        return true;
                    }
                    else
                    {
                        // Item cannot be placed in quick access (e.g., keys)
                        System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Cannot place {item?.Name} in quick access slot - not usable");
                        System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Item is null: {item == null}, IsUsable: {item?.IsUsable}");
                        return false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Invalid slot index {slot}");
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"PlaceQuickAccessItem: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool UseQuickAccessItem(int slot, Player player)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Called with slot={slot}");
                LogQuickAccessSlotsState("Before UseQuickAccessItem");
                
                var item = GetQuickAccessItem(slot);
                System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Got item: {item?.Name ?? "null"}");
                
                if (item != null && item.Use(player))
                {
                    System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Item.Use() returned true");
                    
                    // Store item type before modifying quantity
                    bool isHealthPotion = item is Items.HealthPotion;
                    System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: isHealthPotion={isHealthPotion}");
                    
                    item.Quantity--;
                    System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Quantity after decrement: {item.Quantity}");
                    
                    // Ensure quantity doesn't go below 0
                    if (item.Quantity < 0)
                    {
                        item.Quantity = 0;
                        System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Corrected negative quantity to 0");
                    }
                    
                    if (item.Quantity <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Quantity <= 0, removing item");
                        RemoveQuickAccessItem(slot);
                        LogQuickAccessSlotsState("After RemoveQuickAccessItem");
                        
                        // If it was a health potion, try to auto-assign another one to the same slot
                        if (isHealthPotion)
                        {
                            System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Calling AutoAssignToSpecificSlot");
                            bool assigned = _healthPotionAutoAssignmentManager.AutoAssignToSpecificSlot(slot);
                            System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: AutoAssignToSpecificSlot returned {assigned}");
                            LogQuickAccessSlotsState("After AutoAssignToSpecificSlot");
                        }
                    }
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Item.Use() returned false or item is null");
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"UseQuickAccessItem: Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Get current drag state for rendering
        /// </summary>
        public InventoryDragState GetDragState()
        {
            return _dragManager.DragState;
        }

        public void ToggleInventory()
        {
            IsOpen = !IsOpen;
            
            // Publish inventory state change events
            if (IsOpen)
            {
                GameEventSystem.Instance?.Publish<object>(GameEvents.INVENTORY_OPENED, null);
            }
            else
            {
                _dragManager.CancelDrag(); // Cancel any ongoing drag when closing inventory
                GameEventSystem.Instance?.Publish<object>(GameEvents.INVENTORY_CLOSED, null);
            }
        }

        public void Update(GameTime gameTime, Player player, bool isDialogueActive)
        {
            // Only handle inventory-specific logic when inventory is open
            if (!IsOpen)
            {
                System.Diagnostics.Debug.WriteLine($"Inventory.Update: Inventory is closed, skipping update");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Inventory.Update: Processing mouse input for drag and drop");
            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            // Handle mouse input for drag and drop
            HandleMouseInput(mouse, keyboard, gameTime);
        }

        private void HandleMouseInput(MouseState mouse, KeyboardState keyboard, GameTime gameTime)
        {
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            var dragState = _dragManager.DragState;

            // Handle mouse button states
            bool leftButtonPressed = mouse.LeftButton == ButtonState.Pressed;
            bool leftButtonJustPressed = leftButtonPressed && _previousMouseState.LeftButton == ButtonState.Released;
            
            // Track previous mouse state for proper drag detection
            bool leftButtonJustReleased = _previousMouseState.LeftButton == ButtonState.Pressed && mouse.LeftButton == ButtonState.Released;
            _previousMouseState = mouse;

            // Start drag operation (Left Click)
            if (leftButtonJustPressed && !dragState.IsActive)
            {
                System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Attempting to start drag at position {mousePos}");
                
                // Check if clicking on inventory grid
                var inventorySlot = _renderer.GetInventorySlotAtPosition(mousePos);
                if (inventorySlot.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Starting drag from inventory slot ({inventorySlot.Value.X}, {inventorySlot.Value.Y})");
                    bool started = _dragManager.StartDragFromGrid(inventorySlot.Value.X, inventorySlot.Value.Y, mousePos);
                    System.Diagnostics.Debug.WriteLine($"HandleMouseInput: StartDragFromGrid returned {started}");
                }
                else
                {
                    // Check if clicking on quick access slot
                    var quickAccessSlot = _renderer.GetQuickAccessSlotAtPosition(mousePos);
                    if (quickAccessSlot.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Starting drag from quick access slot {quickAccessSlot.Value}");
                        bool started = _dragManager.StartDragFromQuickAccess(quickAccessSlot.Value, mousePos);
                        System.Diagnostics.Debug.WriteLine($"HandleMouseInput: StartDragFromQuickAccess returned {started}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HandleMouseInput: No valid slot found at position {mousePos}");
                    }
                }
            }

            // Drop operation (Left Click release)
            if (leftButtonJustReleased && dragState.IsActive)
            {
                System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Attempting to drop at position {mousePos}");
                bool dropSuccessful = false;

                // Check if dropping on inventory grid
                var inventorySlot = _renderer.GetInventorySlotAtPosition(mousePos);
                if (inventorySlot.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Dropping to inventory slot ({inventorySlot.Value.X}, {inventorySlot.Value.Y})");
                    dropSuccessful = _dragManager.DropToGrid(inventorySlot.Value.X, inventorySlot.Value.Y);
                }
                else
                {
                    // Check if dropping on quick access slot
                    var quickAccessSlot = _renderer.GetQuickAccessSlotAtPosition(mousePos);
                    if (quickAccessSlot.HasValue)
                    {
                        System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Dropping to quick access slot {quickAccessSlot.Value}");
                        dropSuccessful = _dragManager.DropToQuickAccess(quickAccessSlot.Value);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"HandleMouseInput: No valid drop target found at position {mousePos}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Drop operation returned {dropSuccessful}");

                // If drop was not successful, cancel the drag
                if (!dropSuccessful && dragState.IsActive)
                {
                    System.Diagnostics.Debug.WriteLine($"HandleMouseInput: Drop failed, canceling drag");
                    _dragManager.CancelDrag();
                }
            }

            // Update drag position
            if (dragState.IsActive)
            {
                dragState.DragOffset = mousePos;
            }
        }



        private void DiscardDraggedItem()
        {
            var dragState = _dragManager.DragState;
            if (!dragState.IsValid()) return;

            // Item is discarded - notify game to drop it on ground

            OnItemDiscarded?.Invoke(dragState.DraggedItem);
            OnInventoryChanged?.Invoke();
            
            // Complete the drag without returning item to source (since it's discarded)
            _dragManager.CompleteDrag();
        }

        // These methods are now handled by the renderer
        // TODO: Move to renderer if needed for drag and drop
        
        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Always draw quick access panel in game mode, inventory panel only when open
            if (IsQuickAccessVisible)
            {
                _renderer.DrawQuickAccess(spriteBatch, font, this);
            }
            
            if (!IsOpen) return;

            _renderer.DrawInventory(spriteBatch, font, this);
        }
        
        /// <summary>
        /// Draw only the quick access panel (used when inventory is closed)
        /// </summary>
        public void DrawQuickAccessOnly(SpriteBatch spriteBatch, SpriteFont font)
        {
            if (IsQuickAccessVisible)
            {
                _renderer.DrawQuickAccess(spriteBatch, font, this);
            }
        }
        
        /// <summary>
        /// Hide quick access panel (called when menu, pause, or other UI panels are opened)
        /// </summary>
        public void HideQuickAccess()
        {
            IsQuickAccessVisible = false;
        }
        
        /// <summary>
        /// Show quick access panel (called when returning to gameplay)
        /// </summary>
        public void ShowQuickAccess()
        {
            IsQuickAccessVisible = true;
        }

        /// <summary>
        /// Unified health potion auto-assignment system using drag & drop logic
        /// </summary>
        private class HealthPotionAutoAssignmentManager
        {
            private readonly Inventory _inventory;
            private bool _isAutoAssigning = false; // Protection against recursion
            
            public HealthPotionAutoAssignmentManager(Inventory inventory)
            {
                _inventory = inventory;
            }
            
            /// <summary>
            /// Auto-assigns health potion to a specific slot (for consumption replacement) using drag & drop logic
            /// </summary>
            public bool AutoAssignToSpecificSlot(int slotIndex)
            {
                try
                {
                    // Protection against recursion
                    if (_isAutoAssigning)
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Recursion detected, skipping AutoAssignToSpecificSlot");
                        return false;
                    }
                    
                    _isAutoAssigning = true;
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: AutoAssignToSpecificSlot called with slotIndex={slotIndex}");
                    
                    // Validate slot index
                    if (slotIndex < 0 || slotIndex >= Inventory.QUICK_ACCESS_SLOTS)
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Invalid slot index: {slotIndex}");
                        return false;
                    }
                    
                    // Find health potion in inventory using drag & drop logic
                    var healthPotion = FindHealthPotionInInventory();
                    if (healthPotion == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: No health potion found in inventory");
                        return false;
                    }
                    
                    // Use drag & drop logic to move health potion to quick access
                    return MoveHealthPotionToQuickAccess(healthPotion, slotIndex, true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Exception in AutoAssignToSpecificSlot: {ex.Message}");
                    return false;
                }
                finally
                {
                    _isAutoAssigning = false;
                }
            }
            
            /// <summary>
            /// Auto-assigns health potion to any empty slot (for pickup) using drag & drop logic
            /// </summary>
            public bool AutoAssignToAnySlot(Item healthPotion)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: AutoAssignToAnySlot called with healthPotion: {healthPotion?.Name ?? "null"}");
                    
                    // Protection against recursion
                    if (_isAutoAssigning)
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Recursion detected, skipping AutoAssignToAnySlot");
                        return false;
                    }
                    
                    _isAutoAssigning = true;
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: AutoAssignToAnySlot called");
                    
                    // Check if any slot already has a health potion
                    if (HasHealthPotionInQuickAccess())
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Already has health potion in quick access");
                        return false;
                    }
                    
                    // Find empty slot
                    int emptySlot = FindEmptyQuickAccessSlot();
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Found empty slot: {emptySlot}");
                    if (emptySlot == -1)
                    {
                        System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: No empty slots available");
                        return false;
                    }
                    
                    // Use drag & drop logic to move health potion to quick access
                    return MoveHealthPotionToQuickAccess(healthPotion, emptySlot, false);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"HealthPotionAutoAssignmentManager: Exception in AutoAssignToAnySlot: {ex.Message}");
                    return false;
                }
                finally
                {
                    _isAutoAssigning = false;
                }
            }
            
            /// <summary>
            /// Moves health potion to quick access slot using simplified logic
            /// </summary>
            private bool MoveHealthPotionToQuickAccess(Item healthPotion, int slotIndex, bool isFromInventory)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"MoveHealthPotionToQuickAccess: Moving health potion to slot {slotIndex}");
                    
                    // Place the health potion directly in quick access slot
                    bool placed = _inventory.PlaceQuickAccessItem(slotIndex, healthPotion);
                    if (placed)
                    {
                        // If moving from inventory, remove the original
                        if (isFromInventory && healthPotion is HealthPotion)
                        {
                            RemoveHealthPotionFromInventory((HealthPotion)healthPotion);
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"MoveHealthPotionToQuickAccess: Successfully moved health potion to slot {slotIndex}");
                        return true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"MoveHealthPotionToQuickAccess: Failed to place health potion in slot {slotIndex}");
                    }
                    
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MoveHealthPotionToQuickAccess: Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"MoveHealthPotionToQuickAccess: Stack trace: {ex.StackTrace}");
                    return false;
                }
            }
            
            /// <summary>
            /// Finds a health potion in the inventory grid
            /// </summary>
            private HealthPotion FindHealthPotionInInventory()
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"FindHealthPotionInInventory: Searching for health potion in inventory");
                    for (int x = 0; x < Inventory.INVENTORY_WIDTH; x++)
                    {
                        for (int y = 0; y < Inventory.INVENTORY_HEIGHT; y++)
                        {
                            var item = _inventory._inventoryGrid[x, y];
                            if (item is HealthPotion healthPotion)
                            {
                                System.Diagnostics.Debug.WriteLine($"FindHealthPotionInInventory: Found health potion at ({x}, {y})");
                                return healthPotion;
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"FindHealthPotionInInventory: No health potion found in inventory");
                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FindHealthPotionInInventory: Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"FindHealthPotionInInventory: Stack trace: {ex.StackTrace}");
                    return null;
                }
            }
            
            /// <summary>
            /// Removes a specific health potion from inventory without triggering events
            /// </summary>
            private void RemoveHealthPotionFromInventory(HealthPotion healthPotion)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Looking for health potion to remove");
                    for (int x = 0; x < Inventory.INVENTORY_WIDTH; x++)
                    {
                        for (int y = 0; y < Inventory.INVENTORY_HEIGHT; y++)
                        {
                            var item = _inventory._inventoryGrid[x, y];
                            if (item == healthPotion)
                            {
                                System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Found health potion at ({x}, {y}), removing directly");
                                // Remove directly without triggering events to avoid conflicts
                                _inventory._inventoryGrid[x, y] = null;
                                System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Health potion removed successfully");
                                return;
                            }
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Health potion not found in inventory");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Exception occurred: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"RemoveHealthPotionFromInventory: Stack trace: {ex.StackTrace}");
                }
            }
            
            /// <summary>
            /// Checks if any quick access slot contains a health potion
            /// </summary>
            private bool HasHealthPotionInQuickAccess()
            {
                for (int i = 0; i < Inventory.QUICK_ACCESS_SLOTS; i++)
                {
                    var item = _inventory.GetQuickAccessItem(i);
                    if (item is HealthPotion)
                    {
                        return true;
                    }
                }
                return false;
            }
            
            /// <summary>
            /// Finds an empty quick access slot
            /// </summary>
            private int FindEmptyQuickAccessSlot()
            {
                for (int i = 0; i < Inventory.QUICK_ACCESS_SLOTS; i++)
                {
                    if (_inventory.GetQuickAccessItem(i) == null)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }
        
        // Instance of the unified auto-assignment manager
        private readonly HealthPotionAutoAssignmentManager _healthPotionAutoAssignmentManager;
        private MouseState _previousMouseState = Mouse.GetState(); // Track previous mouse state for drag detection


    }
} 
