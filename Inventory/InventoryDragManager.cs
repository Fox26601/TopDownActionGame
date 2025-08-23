using Microsoft.Xna.Framework;
using IsometricActionGame.Items;

namespace IsometricActionGame.Inventory
{
    /// <summary>
    /// Manages drag and drop operations for the inventory
    /// Implements clean separation of concerns for drag and drop logic
    /// </summary>
    public class InventoryDragManager
    {
        private readonly IInventoryOperations _inventory;
        private readonly InventoryDragState _dragState;

        public InventoryDragManager(IInventoryOperations inventory)
        {
            _inventory = inventory ?? throw new System.ArgumentNullException(nameof(inventory));
            _dragState = new InventoryDragState();
        }

        /// <summary>
        /// Gets the current drag state
        /// </summary>
        public InventoryDragState DragState => _dragState;

        /// <summary>
        /// Starts a drag operation from inventory grid
        /// </summary>
        /// <param name="x">X coordinate in inventory grid</param>
        /// <param name="y">Y coordinate in inventory grid</param>
        /// <param name="mousePosition">Current mouse position</param>
        /// <returns>True if drag operation started successfully</returns>
        public bool StartDragFromGrid(int x, int y, Vector2 mousePosition)
        {
            System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: Called with coordinates ({x}, {y})");
            
            var item = _inventory.GetItem(x, y);
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: No item found at ({x}, {y})");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: Found item: {item.Name}");

            // Create a copy of the item for dragging
            var draggedItem = ItemFactory.CreateItemInstance(item);
            if (draggedItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: Failed to create item instance");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: Created dragged item: {draggedItem.Name}");

            _dragState.StartDrag(draggedItem, mousePosition, new Point(x, y), false);
            
            // Remove the original item from the grid
            _inventory.RemoveItem(x, y);
            
            System.Diagnostics.Debug.WriteLine($"StartDragFromGrid: Successfully started drag operation");
            return true;
        }

        /// <summary>
        /// Starts a drag operation from quick access slot
        /// </summary>
        /// <param name="slot">Quick access slot index</param>
        /// <param name="mousePosition">Current mouse position</param>
        /// <returns>True if drag operation started successfully</returns>
        public bool StartDragFromQuickAccess(int slot, Vector2 mousePosition)
        {
            System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: Called with slot {slot}");
            
            var item = _inventory.GetQuickAccessItem(slot);
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: No item found in slot {slot}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: Found item: {item.Name}");

            // Create a copy of the item for dragging
            var draggedItem = ItemFactory.CreateItemInstance(item);
            if (draggedItem == null)
            {
                System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: Failed to create item instance");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: Created dragged item: {draggedItem.Name}");

            _dragState.StartDrag(draggedItem, mousePosition, new Point(slot, 0), true);
            
            // Remove the original item from quick access
            _inventory.RemoveQuickAccessItem(slot);
            
            System.Diagnostics.Debug.WriteLine($"StartDragFromQuickAccess: Successfully started drag operation");
            return true;
        }

        /// <summary>
        /// Drops the dragged item to inventory grid
        /// </summary>
        /// <param name="x">X coordinate in inventory grid</param>
        /// <param name="y">Y coordinate in inventory grid</param>
        /// <returns>True if drop operation completed successfully</returns>
        public bool DropToGrid(int x, int y)
        {
            System.Diagnostics.Debug.WriteLine($"DropToGrid: Called with coordinates ({x}, {y})");
            
            if (!_dragState.IsValid())
            {
                System.Diagnostics.Debug.WriteLine($"DropToGrid: Invalid drag state");
                return false;
            }

            var targetItem = _inventory.GetItem(x, y);
            
            // Check if dropping on the same slot we dragged from
            if (_dragState.SourcePosition.HasValue && 
                !_dragState.IsQuickAccessSource && 
                _dragState.SourcePosition.Value.X == x && 
                _dragState.SourcePosition.Value.Y == y)
            {
                // Return item to original position
                _inventory.PlaceItem(x, y, _dragState.DraggedItem);
                _dragState.CompleteDrag();
                return true;
            }

            // Handle stacking (but never stack health potions)
            if (targetItem != null && 
                targetItem.GetType() == _dragState.DraggedItem.GetType() && 
                targetItem.IsStackable &&
                !(_dragState.DraggedItem is Items.HealthPotion))
            {
                return HandleStacking(x, y, targetItem);
            }

            // Handle swapping or placing in empty slot
            if (targetItem == null)
            {
                _inventory.PlaceItem(x, y, _dragState.DraggedItem);
            }
            else
            {
                // Swap items
                _inventory.PlaceItem(x, y, _dragState.DraggedItem);
                
                // Return the target item to source if possible
                if (_dragState.SourcePosition.HasValue)
                {
                    if (_dragState.IsQuickAccessSource)
                    {
                        _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, targetItem);
                    }
                    else
                    {
                        _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, targetItem);
                    }
                }
            }

            _dragState.CompleteDrag();
            return true;
        }

        /// <summary>
        /// Drops the dragged item to quick access slot
        /// </summary>
        /// <param name="slot">Quick access slot index</param>
        /// <returns>True if drop operation completed successfully</returns>
        public bool DropToQuickAccess(int slot)
        {
            if (!_dragState.IsValid()) return false;

            var targetItem = _inventory.GetQuickAccessItem(slot);
            
            // Check if dropping on the same slot we dragged from
            if (_dragState.SourcePosition.HasValue && 
                _dragState.IsQuickAccessSource && 
                _dragState.SourcePosition.Value.X == slot)
            {
                // Return item to original position
                _inventory.PlaceQuickAccessItem(slot, _dragState.DraggedItem);
                _dragState.CompleteDrag();
                return true;
            }

            // Check if the dragged item can be placed in quick access (must be usable)
            if (!_dragState.DraggedItem.IsUsable)
            {
                // Item cannot be placed in quick access, return to source
                if (_dragState.SourcePosition.HasValue)
                {
                    if (_dragState.IsQuickAccessSource)
                    {
                        _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                    }
                    else
                    {
                        _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                    }
                }
                _dragState.CompleteDrag();
                return false;
            }

            // Handle stacking (but never stack health potions)
            if (targetItem != null && 
                targetItem.GetType() == _dragState.DraggedItem.GetType() && 
                targetItem.IsStackable &&
                !(_dragState.DraggedItem is Items.HealthPotion))
            {
                return HandleQuickAccessStacking(slot, targetItem);
            }

            // Handle swapping or placing in empty slot
            if (targetItem == null)
            {
                if (!_inventory.PlaceQuickAccessItem(slot, _dragState.DraggedItem))
                {
                    // Failed to place item, return to source
                    if (_dragState.SourcePosition.HasValue)
                    {
                        if (_dragState.IsQuickAccessSource)
                        {
                            _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                        }
                        else
                        {
                            _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                        }
                    }
                    _dragState.CompleteDrag();
                    return false;
                }
            }
            else
            {
                // Swap items
                if (!_inventory.PlaceQuickAccessItem(slot, _dragState.DraggedItem))
                {
                    // Failed to place item, return to source
                    if (_dragState.SourcePosition.HasValue)
                    {
                        if (_dragState.IsQuickAccessSource)
                        {
                            _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                        }
                        else
                        {
                            _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                        }
                    }
                    _dragState.CompleteDrag();
                    return false;
                }
                
                // Return the target item to source if possible
                if (_dragState.SourcePosition.HasValue)
                {
                    if (_dragState.IsQuickAccessSource)
                    {
                        _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, targetItem);
                    }
                    else
                    {
                        _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, targetItem);
                    }
                }
            }

            _dragState.CompleteDrag();
            return true;
        }

        /// <summary>
        /// Completes the drag operation without returning item to source
        /// Used for discard operations where item should not be returned
        /// </summary>
        public void CompleteDrag()
        {
            if (!_dragState.IsValid()) return;
            _dragState.CompleteDrag();
        }

        /// <summary>
        /// Cancels the current drag operation and returns item to source
        /// </summary>
        public void CancelDrag()
        {
            if (!_dragState.IsValid()) return;

            // Return item to original position
            if (_dragState.SourcePosition.HasValue)
            {
                if (_dragState.IsQuickAccessSource)
                {
                    _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                }
                else
                {
                    _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                }
            }

            _dragState.CancelDrag();
        }

        /// <summary>
        /// Handles stacking logic for inventory grid
        /// </summary>
        private bool HandleStacking(int x, int y, Item targetItem)
        {
            int spaceInStack = targetItem.MaxStackSize - targetItem.Quantity;
            int amountToAdd = System.Math.Min(_dragState.DraggedItem.Quantity, spaceInStack);
            
            targetItem.Quantity += amountToAdd;
            _dragState.DraggedItem.Quantity -= amountToAdd;

            if (_dragState.DraggedItem.Quantity <= 0)
            {
                // Item completely stacked
                _dragState.CompleteDrag();
            }
            else
            {
                // Return remaining items to source
                if (_dragState.SourcePosition.HasValue)
                {
                    if (_dragState.IsQuickAccessSource)
                    {
                        _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                    }
                    else
                    {
                        _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                    }
                }
                _dragState.CompleteDrag();
            }

            return true;
        }

        /// <summary>
        /// Handles stacking logic for quick access slots
        /// </summary>
        private bool HandleQuickAccessStacking(int slot, Item targetItem)
        {
            int spaceInStack = targetItem.MaxStackSize - targetItem.Quantity;
            int amountToAdd = System.Math.Min(_dragState.DraggedItem.Quantity, spaceInStack);
            
            targetItem.Quantity += amountToAdd;
            _dragState.DraggedItem.Quantity -= amountToAdd;

            if (_dragState.DraggedItem.Quantity <= 0)
            {
                // Item completely stacked
                _dragState.CompleteDrag();
            }
            else
            {
                // Return remaining items to source
                if (_dragState.SourcePosition.HasValue)
                {
                    if (_dragState.IsQuickAccessSource)
                    {
                        _inventory.PlaceQuickAccessItem(_dragState.SourcePosition.Value.X, _dragState.DraggedItem);
                    }
                    else
                    {
                        _inventory.PlaceItem(_dragState.SourcePosition.Value.X, _dragState.SourcePosition.Value.Y, _dragState.DraggedItem);
                    }
                }
                _dragState.CompleteDrag();
            }

            return true;
        }
    }
}
