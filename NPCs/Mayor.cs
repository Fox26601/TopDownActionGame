using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using IsometricActionGame.Core.Graphics;
using IsometricActionGame.Quests;
using IsometricActionGame.Dialogue;
using IsometricActionGame.Items;
using System.Collections.Generic;
using System;

namespace IsometricActionGame.NPCs
{
    public class Mayor : NPC
    {
        private QuestManager _questManager;
        private ExtendedKillBatsQuest _extendedKillBatsQuest;
        
        // Animation for Mayor (6 frames in one row)
        private AnimatedSprite _idleAnimation;
        private AnimatedSprite _currentAnimation;

        // Override sprite size properties to use animation
        public override int SpriteWidth => _currentAnimation?.CurrentFrameWidth ?? 32;
        public override int SpriteHeight => _currentAnimation?.CurrentFrameHeight ?? 32;

        public Mayor(Vector2 position, DialogueManager dialogueManager, QuestManager questManager) : base(position, dialogueManager)
        {
            _questManager = questManager;
            _extendedKillBatsQuest = questManager.GetExtendedKillBatsQuest();
            
            System.Diagnostics.Debug.WriteLine($"Mayor created with QuestManager: {_questManager != null}");
            System.Diagnostics.Debug.WriteLine($"ExtendedKillBatsQuest: {_extendedKillBatsQuest != null}");
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                var mayorTexture = content.Load<Texture2D>("Mayor_Idle");
                
                // Mayor animation: 6 frames in one row, only idle
                _idleAnimation = new AnimatedSprite(mayorTexture, GameConstants.SpriteFrames.NPC_IDLE_FRAMES, 1, GameConstants.SpriteFrames.NPC_IDLE_FRAMES, 0, GameConstants.Animation.MAYOR_FRAME_TIME, true);
                
                System.Diagnostics.Debug.WriteLine($"Mayor idle animation created: {_idleAnimation != null}");
                
                _currentAnimation = _idleAnimation;
                System.Diagnostics.Debug.WriteLine("Mayor animation loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load Mayor animation: {ex.Message}");
                Texture = null; // Use inherited Texture property
            }
        }

        public override void OnInteract(Player player)
        {
            base.OnInteract(player);

            System.Diagnostics.Debug.WriteLine($"Mayor OnInteract called. QuestManager is null: {_questManager == null}, ExtendedKillBatsQuest is null: {_extendedKillBatsQuest == null}");

            // Check if quest manager is properly initialized
            if (_questManager == null)
            {
                SetDialogue(new List<string>
                {
                    "Hello! I am the mayor of this village.",
                    "Excuse me, I have problems with the quest system."
                });
                return;
            }
            
            if (_extendedKillBatsQuest == null)
            {
                SetDialogue(new List<string>
                {
                    "Hello! I am the mayor of this village.",
                    "Excuse me, I have problems with the quest system."
                });
                return;
            }

            // Use the proper dialogue with choices approach
            var dialogueLines = new List<string>
            {
                "Welcome, brave adventurer! I am the mayor of this village.",
                "We have a problem with bats terrorizing our people.",
                "Would you help us by killing 5 bats? I'll reward you with 5 gold coins."
            };
            
            // Use dynamically generated choices
            var choices = GetMayorMainChoices();
            
            // Use the method that shows choices after dialogue completion
            SetDialogueWithPendingChoices(dialogueLines, choices);
        }
        
        private void AcceptQuest(Player player)
        {
            // Add safety checks to prevent null reference exceptions
            if (_questManager == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: _questManager is null in AcceptQuest");
                SetDialogue(new List<string> { "Sorry, problems with the quest system." });
                return;
            }
            
            if (_extendedKillBatsQuest == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: _extendedKillBatsQuest is null in AcceptQuest");
                SetDialogue(new List<string> { "Sorry, problems with the quest system." });
                return;
            }
            
            _questManager.StartKillBatsQuest();
            
            var dialogueLines = new List<string>
            {
                "Thank you for accepting the quest!",
                "Please kill 5 bats and return to me for your reward.",
                $"Current progress: {_extendedKillBatsQuest.GetProgressText()}"
            };
            
            // After accepting the quest, present updated main choices
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices()); // Use _dialogueManager instance
        }

        private void RefuseQuest()
        {
            _questManager.RefuseKillBatsQuest();
            
            var dialogueLines = new List<string>
            {
                "I understand. Maybe next time.",
                "If you change your mind, just come back to me."
            };
            
            // After refusing the quest, end the dialogue
            SetDialogue(dialogueLines); // Just display the refusal message, no more choices
            ExitDialogue(); // Explicitly end dialogue
        }

        private void AskForMoreMoney(Player player)
        {
            // Add safety checks to prevent null reference exceptions
            if (_questManager == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: _questManager is null in AskForMoreMoney");
                SetDialogue(new List<string> { "Sorry, problems with the quest system." });
                return;
            }
            
            if (_extendedKillBatsQuest == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: _extendedKillBatsQuest is null in AskForMoreMoney");
                SetDialogue(new List<string> { "Sorry, problems with the quest system." });
                return;
            }
            
            // Increase the reward and add Pebble objective
            _extendedKillBatsQuest.IncreaseReward(3); // Add 3 more gold
            _extendedKillBatsQuest.AddPebbleObjective(); // Add Pebble as additional objective
            _questManager.StartKillBatsQuest();
            
            var dialogueLines = new List<string>
            {
                "Alright, you drive a hard bargain! I will increase the reward to 8 gold coins.",
                "But there is one more task - defeat the Pebble that also threatens the village.",
                "This is my final offer. Kill 5 bats and defeat the Pebble.",
                $"Current progress: {_extendedKillBatsQuest.GetProgressText()}"
            };

            var choices = GetMayorMainChoices();
            
            // Continue dialogue with updated information and return to main choices
            _dialogueManager.ContinueDialogue(dialogueLines, choices); // Use _dialogueManager instance
        }

        private void AskAboutVillage()
        {
            var dialogueLines = new List<string>
            {
                "Our village is called Isometric. We have lived here for many generations.",
                "It used to be peaceful here, but recently bats and Pebble appeared.",
                "They are terrorizing our residents, and we cannot cope with them ourselves.",
                "That is why we are looking for a brave hero to help us."
            };
            
            // After asking about the village, present updated main choices
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices()); // Use _dialogueManager instance
        }

        private void ExitDialogue()
        {
            // Ensure dialogue can be closed after interaction is complete
            _dialogueManager.SetCanDialogueBeClosed(true);
            _dialogueManager.EndDialogue();
        }

        public override void Update(GameTime gameTime)
        {
            // Update current animation
            _currentAnimation?.Update(gameTime);
            
            // Update layer depth for proper isometric rendering
            UpdateLayerDepth();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var screenPos = GridHelper.WorldToScreen(WorldPosition);
            
            if (_currentAnimation != null)
            {
                System.Diagnostics.Debug.WriteLine($"Drawing Mayor with animation at {screenPos}, LayerDepth: {LayerDepth}");
                _currentAnimation.Draw(spriteBatch, screenPos, Color.White, 0f, SpriteEffects.None, LayerDepth, Scale);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Drawing Mayor with debug rectangle at {screenPos}, LayerDepth: {LayerDepth}");
                // Debug rectangle for Mayor
                var debugTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                debugTexture.SetData(new[] { Color.Blue });
                var debugRect = new Rectangle((int)screenPos.X - GameConstants.Items.DEBUG_SMALL_RECTANGLE_OFFSET, (int)screenPos.Y - GameConstants.Items.DEBUG_SMALL_RECTANGLE_OFFSET, GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE, GameConstants.Items.DEBUG_SMALL_RECTANGLE_SIZE);
                spriteBatch.Draw(debugTexture, debugRect, null, Color.Blue, 0f, Vector2.Zero, SpriteEffects.None, LayerDepth);
            }
        }

        /// <summary>
        /// Dynamically generates the main dialogue choices for the Mayor based on quest status.
        /// </summary>
        private List<DialogueChoice> GetMayorMainChoices()
        {
            var choices = new List<DialogueChoice>();

            if (_extendedKillBatsQuest == null)
            {
                // Fallback if quest is not initialized
                choices.Add(new DialogueChoice("1. Goodbye.", () => ExitDialogue()));
                return choices;
            }
            
            if (!_extendedKillBatsQuest.IsActive && !_extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn)
            {
                // Quest is available
                choices.Add(new DialogueChoice("1. Accept Quest", () => AcceptQuest(_currentPlayer)));
                choices.Add(new DialogueChoice("2. Refuse", () => RefuseQuest()));
                choices.Add(new DialogueChoice("3. Ask for more gold", () => AskForMoreMoney(_currentPlayer)));
                choices.Add(new DialogueChoice("4. Ask about the village", () => AskAboutVillage()));
            }
            else if (_extendedKillBatsQuest.IsActive)
            {
                // Quest is active
                choices.Add(new DialogueChoice("1. What is my quest progress?", () => ShowQuestProgress()));
                choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
            }
            else if (_extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn)
            {
                // Quest is completed, but not turned in
                choices.Add(new DialogueChoice("1. Turn in Quest", () => TurnInQuest()));
                choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
            }
            else if (_extendedKillBatsQuest.IsTurnedIn)
            {
                // Quest is turned in
                choices.Add(new DialogueChoice("1. Any new quests?", () => NoNewQuests()));
                choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
            }
            else if (_extendedKillBatsQuest.IsRefused)
            {
                // Quest was refused, can be taken again
                choices.Add(new DialogueChoice("1. Accept Quest (reconsider)", () => AcceptQuest(_currentPlayer)));
                choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
            }

            return choices;
        }

        // Helper method for quest progress (new)
        private void ShowQuestProgress()
        {
            if (_extendedKillBatsQuest == null)
            {
                SetDialogue(new List<string> { "Sorry, I cannot check quest progress right now." });
                return;
            }

            var dialogueLines = new List<string>
            {
                $"Your current progress: {_extendedKillBatsQuest.GetProgressText()}"
            };
            
            // After showing progress, return to the main options
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices()); // Use _dialogueManager instance
        }

        // Helper method for no new quests (new)
        private void NoNewQuests()
        {
            var dialogueLines = new List<string>
            {
                "No new quests available at the moment, brave adventurer."
            };
            
            // After showing this, return to the main options
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices()); // Use _dialogueManager instance
        }

        // Helper method for turning in quest (new)
        private void TurnInQuest()
        {
            if (_extendedKillBatsQuest != null && _extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn)
            {
                _questManager?.TurnInQuest(_extendedKillBatsQuest);
                
                // Grant reward to player
                if (_currentPlayer != null)
                {
                    var goldReward = _extendedKillBatsQuest.GoldReward;
                    var gold = ItemFactory.CreateGold(goldReward);
                    _currentPlayer.Inventory.AddItem(gold);
                    
                    var dialogueLines = new List<string>
                    {
                        "Thank you for completing the quest! Here is your reward.",
                        $"You received {goldReward} gold coins."
                    };

                    // After turning in quest, return to the main options
                    _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
                }
                else
                {
                    var dialogueLines = new List<string>
                    {
                        "Thank you for completing the quest! Here is your reward.",
                        $"You received {_extendedKillBatsQuest.GoldReward} gold coins."
                    };

                    // After turning in quest, return to the main options
                    _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
                }
            }
            else
            {
                SetDialogue(new List<string> { "You have no completed quest to turn in." });
            }
        }

    }
} 