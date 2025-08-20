using Microsoft.Xna.Framework;
using IsometricActionGame.Events.EventData;
using IsometricActionGame.Items;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Event data for item pickup events
    /// </summary>
    public class ItemPickedUpEventData : BaseEventData
    {
        public Item Item { get; }
        public int Quantity { get; }
        public Vector2 PickupPosition { get; }
        
        public ItemPickedUpEventData(Item item, int quantity, Vector2 pickupPosition) : base("Inventory")
        {
            Item = item;
            Quantity = quantity;
            PickupPosition = pickupPosition;
        }
    }
    
    /// <summary>
    /// Event data for item discard events
    /// </summary>
    public class ItemDiscardedEventData : BaseEventData
    {
        public Item Item { get; }
        public Vector2 DiscardPosition { get; }
        public string Reason { get; }
        
        public ItemDiscardedEventData(Item item, Vector2 discardPosition, string reason = null) : base("Inventory")
        {
            Item = item;
            DiscardPosition = discardPosition;
            Reason = reason;
        }
    }
    
    /// <summary>
    /// Event data for item drop events
    /// </summary>
    public class ItemDroppedEventData : BaseEventData
    {
        public Item Item { get; }
        public Vector2 DropPosition { get; }
        public string DropSource { get; }
        
        public ItemDroppedEventData(Item item, Vector2 dropPosition, string dropSource = null) : base("Inventory")
        {
            Item = item;
            DropPosition = dropPosition;
            DropSource = dropSource;
        }
    }
    
    /// <summary>
    /// Event data for item use events
    /// </summary>
    public class ItemUsedEventData : BaseEventData
    {
        public Item Item { get; }
        public Vector2 UsePosition { get; }
        public string UseTarget { get; }
        
        public ItemUsedEventData(Item item, Vector2 usePosition, string useTarget = null) : base("Inventory")
        {
            Item = item;
            UsePosition = usePosition;
            UseTarget = useTarget;
        }
    }
    
    /// <summary>
    /// Event data for gold change events
    /// </summary>
    public class GoldChangedEventData : BaseEventData
    {
        public int OldAmount { get; }
        public int NewAmount { get; }
        public int Change { get; }
        public string ChangeReason { get; }
        
        public GoldChangedEventData(int oldAmount, int newAmount, string changeReason = null) : base("Inventory")
        {
            OldAmount = oldAmount;
            NewAmount = newAmount;
            Change = newAmount - oldAmount;
            ChangeReason = changeReason;
        }
    }
    
    /// <summary>
    /// Event data for inventory state change events
    /// </summary>
    public class InventoryChangedEventData : BaseEventData
    {
        public int TotalItems { get; }
        public int TotalWeight { get; }
        public bool IsFull { get; }
        
        public InventoryChangedEventData(int totalItems, int totalWeight, bool isFull) : base("Inventory")
        {
            TotalItems = totalItems;
            TotalWeight = totalWeight;
            IsFull = isFull;
        }
    }
}


