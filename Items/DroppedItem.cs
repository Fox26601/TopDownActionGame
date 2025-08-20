using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Items;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using System; // Added for Math class

namespace IsometricActionGame.Items
{
    public class DroppedItem : IInteractable, IDrawable
    {
        public Vector2 WorldPosition { get; set; }
        public float LayerDepth => GridHelper.CalculateLayerDepth(WorldPosition); // Use grid-based depth calculation
        public Vector2 FacingDirection => Vector2.Zero; // Items don't face any direction
        
        // All these properties are now taken from the wrapped _item
        public float Scale => _item.ItemScale;
        public float InteractionRadius => _item.InteractionRadius;
        public int SpriteWidth => _item.SpriteWidth;
        public int SpriteHeight => _item.SpriteHeight;

        private Item _item;

        public bool IsPickedUp { get; private set; } = false;
        public event Action<DroppedItem> OnPickedUp; // New event for when the item is picked up

        public Item Item => _item;

        public DroppedItem(Vector2 position, Item item, float multiplier = GameConstants.Graphics.DEFAULT_SPRITE_SCALE)
        {
            WorldPosition = position;
            _item = item;
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem constructor - Item: {_item.Name}, Original scale: {_item.ItemScale}, Multiplier: {multiplier}");
            _item.ApplyScaleMultiplier(multiplier); // Use the new method
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem constructor - After multiplier, scale: {_item.ItemScale}");
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem.LoadContent called for {_item.Name}");
            _item.LoadContent(content);
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem.LoadContent completed - Icon: {_item.Icon != null}, Icon size: {(_item.Icon?.Width ?? 0)}x{(_item.Icon?.Height ?? 0)}");
        }

        public void Update(GameTime gameTime)
        {
            // Items on ground don't need much updating
        }

        public bool Interact(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem.Interact called for {_item.Name}. IsPickedUp: {IsPickedUp}, WorldPosition: {WorldPosition}, PlayerPosition: {player.WorldPosition}, Distance: {Vector2.Distance(WorldPosition, player.WorldPosition)}, InteractionRadius: {InteractionRadius}");

            if (IsPickedUp) return false;

            // Try to add item to player's inventory
            System.Diagnostics.Debug.WriteLine($"DEBUG: About to add {_item.Name} to inventory. Item type: {_item.GetType().Name}, IsStackable: {_item.IsStackable}, Quantity: {_item.Quantity}");
            bool addedToInventory = player.Inventory.AddItem(_item);
            System.Diagnostics.Debug.WriteLine($"DEBUG: Attempting to add {_item.Name} to inventory. Success: {addedToInventory}");

            if (addedToInventory)
            {
                IsPickedUp = true;
                OnPickedUp?.Invoke(this); // Invoke the event
                System.Diagnostics.Debug.WriteLine($"DEBUG: {_item.Name} successfully picked up.");
                return true; // Successfully picked up
            }
            
            System.Diagnostics.Debug.WriteLine($"DEBUG: {_item.Name} could not be picked up. Inventory full?");
            return false; // Inventory is full
        }

        public void OnInteract(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem.OnInteract called for {_item.Name}. Current Position: {WorldPosition}");
            Interact(player);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsPickedUp || _item.Icon == null) 
            {
                System.Diagnostics.Debug.WriteLine($"DEBUG: DroppedItem.Draw skipped - IsPickedUp: {IsPickedUp}, Icon null: {_item.Icon == null}, Item: {_item.Name}");
                return;
            }

            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            // Center the sprite properly by using the item's icon center as origin
            Vector2 origin = new Vector2(_item.Icon.Width / 2f, _item.Icon.Height / 2f);
            var drawPos = screenPos;
            
            System.Diagnostics.Debug.WriteLine($"DEBUG: Drawing DroppedItem {_item.Name} at world pos {WorldPosition}, screen pos {screenPos}, icon size: {_item.Icon.Width}x{_item.Icon.Height}, ItemScale: {_item.ItemScale}, layerDepth: {LayerDepth}");
            spriteBatch.Draw(_item.Icon, drawPos, null, Color.White, 0f, origin, _item.ItemScale, SpriteEffects.None, LayerDepth);
        }
    }
}
