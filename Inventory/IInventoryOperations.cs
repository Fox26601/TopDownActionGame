using Microsoft.Xna.Framework;
using IsometricActionGame.Items;

namespace IsometricActionGame.Inventory
{
    /// <summary>
    /// Interface for inventory operations
    /// Defines contract for inventory manipulation methods
    /// </summary>
    public interface IInventoryOperations
    {
        /// <summary>
        /// Adds an item to the inventory
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if item was successfully added</returns>
        bool AddItem(Item item);

        /// <summary>
        /// Removes an item from the inventory grid
        /// </summary>
        /// <param name="x">X coordinate in inventory grid</param>
        /// <param name="y">Y coordinate in inventory grid</param>
        void RemoveItem(int x, int y);

        /// <summary>
        /// Removes an item from quick access slot
        /// </summary>
        /// <param name="slot">Quick access slot index</param>
        void RemoveQuickAccessItem(int slot);

        /// <summary>
        /// Gets an item from inventory grid
        /// </summary>
        /// <param name="x">X coordinate in inventory grid</param>
        /// <param name="y">Y coordinate in inventory grid</param>
        /// <returns>The item at specified position or null</returns>
        Item GetItem(int x, int y);

        /// <summary>
        /// Gets an item from quick access slot
        /// </summary>
        /// <param name="slot">Quick access slot index</param>
        /// <returns>The item in specified slot or null</returns>
        Item GetQuickAccessItem(int slot);

        /// <summary>
        /// Places an item in inventory grid
        /// </summary>
        /// <param name="x">X coordinate in inventory grid</param>
        /// <param name="y">Y coordinate in inventory grid</param>
        /// <param name="item">The item to place</param>
        void PlaceItem(int x, int y, Item item);

        /// <summary>
        /// Places an item in quick access slot
        /// </summary>
        /// <param name="slot">Quick access slot index</param>
        /// <param name="item">The item to place</param>
        /// <returns>True if item was successfully placed</returns>
        bool PlaceQuickAccessItem(int slot, Item item);
    }
}
