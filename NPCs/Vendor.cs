using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Items;
using IsometricActionGame.Dialogue;
using IsometricActionGame.Events;
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
            try
            {
                System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Called with player gold: {player?.Inventory?.Gold ?? 0}");
                base.OnInteract(player);

                if (_dialogueManager != null && !_dialogueManager.IsDialogueActive)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Starting vendor dialogue");
                    
                    // Check if player has enough gold
                    if (player.Inventory.Gold >= HEALTH_POTION_PRICE)
                    {
                        System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Player has enough gold, offering to sell");
                        
                        // Create dialogue choices for buying
                        var choices = new List<DialogueChoice>
                        {
                            new DialogueChoice("Yes, I'll buy a health potion", () => 
                            {
                                System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Player chose to buy health potion");
                                PurchaseHealthPotion(player);
                            }),
                            new DialogueChoice("No, thank you", () => 
                            {
                                System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Player declined purchase");
                                SetDialogue(new List<string>
                                {
                                    "No problem! Feel free to come back anytime.",
                                    "I'll be here if you change your mind."
                                });
                            })
                        };
                        
                        var dialogueLines = new List<string>
                        {
                            "Welcome to my shop, traveler!",
                            "I sell health potions for 5 gold each.",
                            "Would you like to buy one?"
                        };
                        
                        SetDialogueWithPendingChoices(dialogueLines, choices);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Player doesn't have enough gold");
                        
                        // Player doesn't have enough gold
                        var choices = new List<DialogueChoice>
                        {
                            new DialogueChoice("I'll come back when I have more gold", () => 
                            {
                                SetDialogue(new List<string>
                                {
                                    "Of course! Take your time.",
                                    "I'll be here when you have enough gold."
                                });
                            })
                        };
                        
                        var dialogueLines = new List<string>
                        {
                            "Welcome to my shop, traveler!",
                            "I sell health potions for 5 gold each.",
                            $"You currently have {player.Inventory.Gold} gold, but you need {HEALTH_POTION_PRICE} gold for a potion.",
                            "What would you like to do?"
                        };
                        
                        SetDialogueWithPendingChoices(dialogueLines, choices);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Vendor.OnInteract: Stack trace: {ex.StackTrace}");
                
                // Fallback dialogue
                SetDialogue(new List<string>
                {
                    "Welcome to my shop!",
                    "I'm having some technical difficulties right now.",
                    "Please try again later."
                });
            }
        }
        
        /// <summary>
        /// Handles the purchase of a health potion using event system
        /// </summary>
        private void PurchaseHealthPotion(Player player)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Starting purchase process");
                
                // Check if player has enough gold
                if (player.Inventory.Gold < HEALTH_POTION_PRICE)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Player doesn't have enough gold");
                    SetDialogue(new List<string>
                    {
                        "I'm sorry, but you don't have enough gold for a health potion.",
                        $"You need {HEALTH_POTION_PRICE} gold, but you only have {player.Inventory.Gold} gold."
                    });
                    return;
                }
                
                // Spend gold using event system
                int oldGold = player.Inventory.Gold;
                bool goldSpent = player.Inventory.SpendGold(HEALTH_POTION_PRICE);
                
                if (!goldSpent)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Failed to spend gold");
                    SetDialogue(new List<string>
                    {
                        "I'm sorry, but there was an error processing your payment.",
                        "Please try again."
                    });
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Gold spent successfully. Old: {oldGold}, New: {player.Inventory.Gold}");
                
                // Create health potion
                var potion = ItemFactory.CreateHealthPotion();
                if (potion == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Failed to create health potion");
                    // Refund gold
                    player.Inventory.AddGold(HEALTH_POTION_PRICE);
                    SetDialogue(new List<string>
                    {
                        "I'm sorry, but I'm out of health potions right now.",
                        "Your gold has been refunded.",
                        "Please try again later."
                    });
                    return;
                }
                
                // Load content for the potion
                try
                {
                    potion.LoadContent(player.Content);
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Health potion content loaded successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Failed to load potion content: {ex.Message}");
                    // Continue anyway, potion will use fallback texture
                }
                
                // Add potion to inventory
                bool added = player.Inventory.AddItem(potion);
                if (added)
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Health potion added to inventory successfully");
                    
                    // Publish purchase event
                    UnifiedEventSystem.Instance?.PublishGoldChanged(oldGold, player.Inventory.Gold, "Vendor Purchase");
                    
                    SetDialogue(new List<string>
                    {
                        "Thank you for your purchase!",
                        "The health potion has been added to your inventory.",
                        "Come back anytime for more potions!"
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Failed to add potion to inventory, refunding gold");
                    
                    // Refund gold if inventory is full
                    player.Inventory.AddGold(HEALTH_POTION_PRICE);
                    
                    SetDialogue(new List<string>
                    {
                        "I'm sorry, but your inventory is full.",
                        "Please make some space and try again.",
                        "Your gold has been refunded."
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Exception occurred: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Vendor.PurchaseHealthPotion: Stack trace: {ex.StackTrace}");
                
                // Refund gold in case of error
                player.Inventory.AddGold(HEALTH_POTION_PRICE);
                
                SetDialogue(new List<string>
                {
                    "I'm sorry, but there was an error processing your purchase.",
                    "Your gold has been refunded.",
                    "Please try again later."
                });
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