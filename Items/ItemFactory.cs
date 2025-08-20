using System;
using Microsoft.Xna.Framework.Content;

namespace IsometricActionGame.Items
{
    /// <summary>
    /// Factory class for creating and cloning items
    /// Implements Factory pattern for centralized item management
    /// </summary>
    public static class ItemFactory
    {
        /// <summary>
        /// Creates a new instance of an item based on the original item type
        /// </summary>
        /// <param name="originalItem">The original item to clone</param>
        /// <returns>A new instance of the same item type with copied properties</returns>
        public static Item CreateItemInstance(Item originalItem)
        {
            if (originalItem == null)
                return null;

            try
            {
                Item newInstance = originalItem.Clone();
                if (newInstance != null)
                {
                    // Ensure the new instance has the same quantity as the original
                    newInstance.Quantity = originalItem.Quantity;
                    return newInstance;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cloning item {originalItem.GetType().Name}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Creates a new item of the specified type
        /// </summary>
        /// <typeparam name="T">The type of item to create</typeparam>
        /// <param name="quantity">The quantity for the new item</param>
        /// <returns>A new item instance</returns>
        public static T CreateItem<T>(int quantity = 1) where T : Item, new()
        {
            try
            {
                var item = new T();
                item.Quantity = quantity;
                return item;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating item of type {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a health potion with default heal amount
        /// </summary>
        /// <param name="quantity">The quantity for the health potion</param>
        /// <returns>A new HealthPotion instance</returns>
        public static HealthPotion CreateHealthPotion(int quantity = 1)
        {
            try
            {
                var potion = new HealthPotion();
                potion.Quantity = quantity;
                return potion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating HealthPotion: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates gold with specified amount
        /// </summary>
        /// <param name="amount">The amount of gold</param>
        /// <returns>A new Gold instance</returns>
        public static Gold CreateGold(int amount = 1)
        {
            try
            {
                var gold = new Gold(amount);
                return gold;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating Gold: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a health potion with custom heal amount
        /// </summary>
        /// <param name="healAmount">Amount of health to restore</param>
        /// <param name="quantity">Quantity of potions</param>
        /// <returns>A new HealthPotion instance</returns>
        public static HealthPotion CreateHealthPotion(int healAmount, int quantity = 1)
        {
            try
            {
                var potion = new HealthPotion(healAmount);
                potion.Quantity = quantity;
                return potion;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating HealthPotion: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a key item
        /// </summary>
        /// <param name="quantity">Quantity of keys</param>
        /// <returns>A new KeyItem instance</returns>
        public static KeyItem CreateKeyItem(int quantity = 1)
        {
            try
            {
                var key = new KeyItem();
                key.Quantity = quantity;
                return key;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating KeyItem: {ex.Message}");
                return null;
            }
        }
    }
}
