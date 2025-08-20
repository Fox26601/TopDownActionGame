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

            // Try to stack with existing items first
            if (item.IsStackable)
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
            if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
            {
                return _quickAccessSlots[slot];
            }
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
            if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
            {
                _quickAccessSlots[slot] = null;
                OnInventoryChanged?.Invoke();
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
            if (slot >= 0 && slot < QUICK_ACCESS_SLOTS)
            {
                // Only allow usable items in quick access slots
                if (item != null && item.IsUsable)
                {
                    _quickAccessSlots[slot] = item;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
                else
                {
                    // Item cannot be placed in quick access (e.g., keys)
                    System.Diagnostics.Debug.WriteLine($"Cannot place {item?.Name} in quick access slot - not usable");
                    return false;
                }
            }
            return false;
        }

        public bool UseQuickAccessItem(int slot, Player player)
        {
            var item = GetQuickAccessItem(slot);
            if (item != null && item.Use(player))
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                {
                    RemoveQuickAccessItem(slot);
                }
                return true;
            }
            return false;
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
            var keyboard = Keyboard.GetState();

            // Always handle quick access hotkeys (1-6) - only when dialogue is not active
            if (!isDialogueActive)
            {
                for (int i = 0; i < QUICK_ACCESS_SLOTS; i++)
                {
                    if (keyboard.IsKeyDown(Keys.D1 + i))
                    {
                        UseQuickAccessItem(i, player);
                    }
                }
            }

            // Only handle inventory-specific logic when inventory is open
            if (!IsOpen) return;

            var mouse = Mouse.GetState();

            // Handle mouse input for drag and drop
            HandleMouseInput(mouse, keyboard, gameTime);
        }

        private void HandleMouseInput(MouseState mouse, KeyboardState keyboard, GameTime gameTime)
        {
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y);
            var dragState = _dragManager.DragState;

            // Handle mouse button states
            bool leftButtonPressed = mouse.LeftButton == ButtonState.Pressed;
            bool leftButtonJustPressed = leftButtonPressed && 
                (keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl));

            // Start drag operation
            if (leftButtonJustPressed && !dragState.IsActive)
            {
                // Check if clicking on inventory grid
                var inventorySlot = _renderer.GetInventorySlotAtPosition(mousePos);
                if (inventorySlot.HasValue)
                {
                    _dragManager.StartDragFromGrid(inventorySlot.Value.X, inventorySlot.Value.Y, mousePos);
                }
                else
                {
                    // Check if clicking on quick access slot
                    var quickAccessSlot = _renderer.GetQuickAccessSlotAtPosition(mousePos);
                    if (quickAccessSlot.HasValue)
                    {
                        _dragManager.StartDragFromQuickAccess(quickAccessSlot.Value, mousePos);
                    }
                }
            }

            // Drop operation
            if (leftButtonJustPressed && dragState.IsActive)
            {
                bool dropSuccessful = false;

                // Check if dropping on inventory grid
                var inventorySlot = _renderer.GetInventorySlotAtPosition(mousePos);
                if (inventorySlot.HasValue)
                {
                    dropSuccessful = _dragManager.DropToGrid(inventorySlot.Value.X, inventorySlot.Value.Y);
                }
                else
                {
                    // Check if dropping on quick access slot
                    var quickAccessSlot = _renderer.GetQuickAccessSlotAtPosition(mousePos);
                    if (quickAccessSlot.HasValue)
                    {
                        dropSuccessful = _dragManager.DropToQuickAccess(quickAccessSlot.Value);
                    }
                }

                // If drop was not successful, cancel the drag
                if (!dropSuccessful && dragState.IsActive)
                {
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
            _renderer.DrawQuickAccess(spriteBatch, font, this);
            
            if (!IsOpen) return;

            _renderer.DrawInventory(spriteBatch, font, this);
        }


    }
} 
