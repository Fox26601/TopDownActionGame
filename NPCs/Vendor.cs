using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Items;
using IsometricActionGame.Dialogue;

using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using System.Collections.Generic;
using System;

namespace IsometricActionGame.NPCs
{
    public class Vendor : NPC
    {
        private const int HEALTH_POTION_PRICE = 5;
        
        // Animation for Vendor (6 frames in one row)
        private AnimatedSprite _idleAnimation;
        private AnimatedSprite _currentAnimation;
        
        // Override sprite size properties to use animation
        public override int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public override int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 32;

        public Vendor(Vector2 position, DialogueManager dialogueManager) : base(position, dialogueManager)
        {
            System.Diagnostics.Debug.WriteLine($"Vendor created at position: {position}");
            
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Vendor LoadContent called");
                var vendorTexture = content.Load<Texture2D>("Vendor_Idle");
                System.Diagnostics.Debug.WriteLine($"Vendor texture loaded: {vendorTexture != null}, Size: {vendorTexture?.Width}x{vendorTexture?.Height}");
                
                // Vendor animation: 6 frames in one row, only idle
                _idleAnimation = new AnimatedSprite(vendorTexture, GameConstants.SpriteFrames.NPC_IDLE_FRAMES, 1, GameConstants.SpriteFrames.NPC_IDLE_FRAMES, 0, GameConstants.Animation.IDLE_FRAME_TIME, true);
                
                System.Diagnostics.Debug.WriteLine($"Vendor idle animation created: {_idleAnimation != null}");
                
                _currentAnimation = _idleAnimation;
                System.Diagnostics.Debug.WriteLine("Vendor animation loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Vendor animation: {ex.Message}");
                Texture = null; // Use inherited Texture property
            }
        }

        public override void OnInteract(Player player)
        {
            base.OnInteract(player);

            // Initial dialogue logic for vendor
            if (_dialogueManager != null && !_dialogueManager.IsDialogueActive)
            {
                // If player has enough gold for a potion, offer to sell. Otherwise, just a greeting.
                if (player.Inventory.Gold >= HEALTH_POTION_PRICE)
                {
                    SetDialogue(new List<string>
                    {
                        "Welcome to my shop, traveler!",
                        "I sell health potions for 5 gold each.",
                        "Would you like to buy one?"
                    });
                }
                else
                {
                    SetDialogue(new List<string>
                    {
                        "Welcome to my shop, traveler!",
                        "I sell health potions for 5 gold each.",
                        "You don't have enough gold for a potion right now, but feel free to look around!"
                    });
                }
            }

            if (player.Inventory.SpendGold(HEALTH_POTION_PRICE))
            {
                var potion = ItemFactory.CreateHealthPotion();
                potion.LoadContent(player.Content);
                
                if (player.Inventory.AddItem(potion))
                {
                    SetDialogue(new List<string>
                    {
                        "Thank you for your purchase!",
                        "The health potion has been added to your inventory.",
                        "Come back anytime!"
                    });
                }
                else
                {
                    // Refund if inventory is full
                    player.Inventory.AddGold(HEALTH_POTION_PRICE);
                    SetDialogue(new List<string>
                    {
                        "I'm sorry, but your inventory is full.",
                        "Please make some space and try again.",
                        "Your gold has been refunded."
                    });
                }
            }
            else
            {
                // Show choices when player doesn't have enough gold
                var choices = new List<DialogueChoice>
                {
                    new DialogueChoice("Okay, I'll come back later", () => SetDialogue(new List<string>
                    {
                        "Of course! Take your time.",
                        "I'll be here when you have enough gold."
                    }))
                };
                
                var dialogueLines = new List<string>
                {
                    "I'm sorry, but you don't have enough gold.",
                    "Health potions cost 5 gold each.",
                    "What would you like to do?"
                };
                
                SetDialogueWithPendingChoices(dialogueLines, choices);
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Update current animation
            if (_currentAnimation != null)
            {
                _currentAnimation.Update(gameTime);
                System.Diagnostics.Debug.WriteLine($"Vendor animation updated - Current frame: {_currentAnimation.CurrentFrame}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Vendor has no current animation!");
            }
            
            // Update layer depth for proper isometric rendering
            UpdateLayerDepth();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            System.Diagnostics.Debug.WriteLine($"Vendor Draw called at {screenPos}, LayerDepth: {LayerDepth}, Animation: {_currentAnimation != null}");
            
            if (_currentAnimation != null)
            {
                _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, LayerDepth, Scale);
                System.Diagnostics.Debug.WriteLine("Vendor animation drawn successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Vendor drawing debug rectangle");
                // Debug rectangle for Vendor
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Orange });
                var debugRect = new Rectangle((int)screenPos.X - GameConstants.Items.DEBUG_SMALL_RECTANGLE_OFFSET, (int)screenPos.Y - GameConstants.Items.DEBUG_SMALL_RECTANGLE_OFFSET, GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE, GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE);
                spriteBatch.Draw(debugTexture, debugRect, null, Color.Orange, 0f, Vector2.Zero, SpriteEffects.None, LayerDepth);
            }
        }
    }
} 