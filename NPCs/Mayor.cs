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
using System.Linq;

namespace IsometricActionGame.NPCs
{
    public class Mayor : NPC
    {
        private QuestManager _questManager;
        private ExtendedKillBatsQuest _extendedKillBatsQuest;
        private SimpleKillBatsQuest _simpleKillBatsQuest;
        
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
            _simpleKillBatsQuest = questManager.GetSimpleKillBatsQuest();
            
            System.Diagnostics.Debug.WriteLine($"Mayor created with QuestManager: {_questManager != null}");
            System.Diagnostics.Debug.WriteLine($"ExtendedKillBatsQuest: {_extendedKillBatsQuest != null}");
            System.Diagnostics.Debug.WriteLine($"SimpleKillBatsQuest: {_simpleKillBatsQuest != null}");
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

            // Complete return quest if player has one active
            _questManager.CompleteReturnQuest();

            // Check if player has completed return quest to turn in
            var returnQuest = _questManager.GetReturnQuest();
            if (returnQuest != null && returnQuest.IsCompleted && !returnQuest.IsTurnedIn)
            {
                // Show return quest reward dialogue
                ShowReturnQuestRewardDialogue(returnQuest);
                return;
            }

            // Check if player has completed quest to turn in
            if (_questManager.HasCompletedQuestToTurnIn())
            {
                // Show reward dialogue
                ShowRewardDialogue();
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
            
            // Check if extended quest was already completed - if so, use simple quest
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            if (extendedQuestWasCompleted)
            {
                // Use simple quest for repeat attempts
                if (_simpleKillBatsQuest == null)
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: _simpleKillBatsQuest is null in AcceptQuest");
                    SetDialogue(new List<string> { "Sorry, problems with the quest system." });
                    return;
                }
                
                _questManager.StartSimpleKillBatsQuest();
                
                var dialogueLines = new List<string>
                {
                    "Thank you for accepting the quest again!",
                    "Please kill 5 bats and return to me for your reward.",
                    $"Current progress: {_simpleKillBatsQuest.GetProgressText()}"
                };
                
                // After accepting the quest, present updated main choices
                _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
            }
            else
            {
                // Use extended quest for first time
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
                _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
            }
        }

        private void RefuseQuest()
        {
            // Check if extended quest was already completed - if so, refuse simple quest
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            if (extendedQuestWasCompleted)
            {
                _questManager.RefuseSimpleKillBatsQuest();
            }
            else
            {
                _questManager.RefuseKillBatsQuest();
            }
            
            var dialogueLines = new List<string>
            {
                "I understand. Maybe next time.",
                "If you change your mind, just come back to me."
            };
            
            // After refusing the quest, end the dialogue
            SetDialogue(dialogueLines);
            ExitDialogue();
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

            if (_extendedKillBatsQuest == null || _simpleKillBatsQuest == null)
            {
                // Fallback if quest is not initialized
                choices.Add(new DialogueChoice("1. Goodbye.", () => ExitDialogue()));
                return choices;
            }
            
            // Check if extended quest was already completed
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            if (extendedQuestWasCompleted)
            {
                // Use simple quest logic for repeat attempts
                if (!_simpleKillBatsQuest.IsActive && !_simpleKillBatsQuest.IsCompleted && !_simpleKillBatsQuest.IsTurnedIn)
                {
                    // Simple quest is available
                    choices.Add(new DialogueChoice("1. Accept Quest", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Refuse", () => RefuseQuest()));
                    choices.Add(new DialogueChoice("3. Ask about the village", () => AskAboutVillage()));
                }
                else if (_simpleKillBatsQuest.IsActive)
                {
                    // Simple quest is active
                    choices.Add(new DialogueChoice("1. What is my quest progress?", () => ShowQuestProgress()));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_simpleKillBatsQuest.IsCompleted && !_simpleKillBatsQuest.IsTurnedIn)
                {
                    // Simple quest is completed, but not turned in
                    choices.Add(new DialogueChoice("1. Turn in Quest", () => TurnInQuest()));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_simpleKillBatsQuest.IsTurnedIn)
                {
                    // Simple quest is turned in - can be taken again
                    choices.Add(new DialogueChoice("1. Take the quest again", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_simpleKillBatsQuest.IsRefused)
                {
                    // Simple quest was refused, can be taken again
                    choices.Add(new DialogueChoice("1. Accept Quest (reconsider)", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
            }
            else
            {
                // Use extended quest logic for first time
                if (!_extendedKillBatsQuest.IsActive && !_extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn)
                {
                    // Extended quest is available
                    choices.Add(new DialogueChoice("1. Accept Quest", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Refuse", () => RefuseQuest()));
                    
                    // Only show "Ask for more gold" option if Pebble is not defeated
                    if (!_questManager.IsPebbleDefeated())
                    {
                        choices.Add(new DialogueChoice("3. Ask for more gold", () => AskForMoreMoney(_currentPlayer)));
                    }
                    
                    choices.Add(new DialogueChoice("4. Ask about the village", () => AskAboutVillage()));
                }
                else if (_extendedKillBatsQuest.IsActive)
                {
                    // Extended quest is active
                    choices.Add(new DialogueChoice("1. What is my quest progress?", () => ShowQuestProgress()));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn)
                {
                    // Extended quest is completed, but not turned in
                    choices.Add(new DialogueChoice("1. Turn in Quest", () => TurnInQuest()));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_extendedKillBatsQuest.IsTurnedIn)
                {
                    // Extended quest is turned in - can be taken again
                    choices.Add(new DialogueChoice("1. Take the quest again", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
                else if (_extendedKillBatsQuest.IsRefused)
                {
                    // Extended quest was refused, can be taken again
                    choices.Add(new DialogueChoice("1. Accept Quest (reconsider)", () => AcceptQuest(_currentPlayer)));
                    choices.Add(new DialogueChoice("2. Can I ask another question?", () => AskAboutVillage()));
                    choices.Add(new DialogueChoice("3. Goodbye.", () => ExitDialogue()));
                }
            }

            return choices;
        }

        // Helper method for quest progress (new)
        private void ShowQuestProgress()
        {
            // Check if extended quest was already completed - if so, show simple quest progress
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            Quest currentQuest = extendedQuestWasCompleted ? _simpleKillBatsQuest : _extendedKillBatsQuest;
            
            if (currentQuest == null)
            {
                SetDialogue(new List<string> { "Sorry, I cannot check quest progress right now." });
                return;
            }

            var dialogueLines = new List<string>
            {
                $"Your current progress: {currentQuest.GetProgressText()}"
            };
            
            // After showing progress, return to the main options
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
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
            // Check if extended quest was already completed - if so, turn in simple quest
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            Quest currentQuest = extendedQuestWasCompleted ? _simpleKillBatsQuest : _extendedKillBatsQuest;
            
            if (currentQuest != null && currentQuest.IsCompleted && !currentQuest.IsTurnedIn)
            {
                _questManager?.TurnInQuest(currentQuest);
                
                // Get reward information for dialogue
                var goldReward = GetQuestGoldReward(currentQuest);
                var dialogueLines = new List<string>
                {
                    extendedQuestWasCompleted ? "Thank you for completing the quest again! Here is your reward." : "Thank you for completing the quest! Here is your reward.",
                    $"You received {goldReward} gold coins."
                };

                // After turning in quest, return to the main options
                _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
            }
            else
            {
                SetDialogue(new List<string> { "You have no completed quest to turn in." });
            }
        }

        /// <summary>
        /// Helper method to get gold reward from quest
        /// </summary>
        private int GetQuestGoldReward(Quest quest)
        {
            if (quest is ExtendedKillBatsQuest extendedQuest)
            {
                // Extended quest can have different rewards based on whether Pebble objective was added
                return extendedQuest.GoldReward;
            }
            else if (quest is SimpleKillBatsQuest simpleQuest)
            {
                // Simple quest always has 5 gold reward
                return simpleQuest.GoldReward;
            }
            else
                return 0;
        }
        
        /// <summary>
        /// Show reward dialogue when player has completed quest
        /// </summary>
        private void ShowRewardDialogue()
        {
            // Check if extended quest was already completed - if so, check simple quest
            bool extendedQuestWasCompleted = _questManager.WasExtendedQuestEverCompleted();
            
            Quest currentQuest = extendedQuestWasCompleted ? _simpleKillBatsQuest : _extendedKillBatsQuest;
            
            if (currentQuest == null || !currentQuest.IsCompleted || currentQuest.IsTurnedIn)
            {
                SetDialogue(new List<string> { "You have no completed quest to turn in." });
                return;
            }
            
            var dialogueLines = new List<string>();
            
            // Different dialogue based on quest type
            if (currentQuest is ExtendedKillBatsQuest extendedQuest && extendedQuest.HasPebbleObjective)
            {
                dialogueLines.Add("Ah, my brave hero! You have returned victorious!");
                dialogueLines.Add("You have not only defeated the bats, but also vanquished the mighty Pebble!");
                dialogueLines.Add("The village is forever grateful for your heroic deeds.");
                dialogueLines.Add("Here is your well-deserved reward for completing this challenging quest.");
            }
            else if (currentQuest is SimpleKillBatsQuest)
            {
                dialogueLines.Add("Welcome back, brave adventurer!");
                dialogueLines.Add("I can see from your triumphant expression that you have completed the quest again.");
                dialogueLines.Add("The bats have been defeated once more, and our village is safe.");
                dialogueLines.Add("Here is your reward for your continued service.");
            }
            else
            {
                dialogueLines.Add("Welcome back, brave adventurer!");
                dialogueLines.Add("I can see from your triumphant expression that you have completed the quest.");
                dialogueLines.Add("The bats have been defeated, and our village is safe once more.");
                dialogueLines.Add("Here is your reward for your excellent work.");
            }
            
            // Add reward information
            var goldReward = GetQuestGoldReward(currentQuest);
            dialogueLines.Add($"You will receive {goldReward} gold coins for your efforts.");
            
            // Turn in the quest (this will also reset it for taking again and publish reward event)
            _questManager?.TurnInQuest(currentQuest);
            
            // Show completion message with hint about taking the quest again
            dialogueLines.Add("Thank you for your service to our village!");
            dialogueLines.Add("If you wish to help us again, you can take this quest once more.");
            
            // Continue with main dialogue options
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
        }

        /// <summary>
        /// Show reward dialogue when player has completed return quest
        /// </summary>
        private void ShowReturnQuestRewardDialogue(ReturnToMayorQuest returnQuest)
        {
            var dialogueLines = new List<string>();
            
            // Different dialogue based on quest type
            if (returnQuest.HasPebbleObjective)
            {
                dialogueLines.Add("Ah, my brave hero! You have returned to claim your reward!");
                dialogueLines.Add("You have not only defeated the bats, but also vanquished the mighty Pebble!");
                dialogueLines.Add("The village is forever grateful for your heroic deeds.");
                dialogueLines.Add("Here is your well-deserved reward for completing this challenging quest.");
            }
            else if (returnQuest.QuestType == "SimpleKillBatsQuest")
            {
                dialogueLines.Add("Welcome back, brave adventurer!");
                dialogueLines.Add("You have returned to claim your reward for completing the quest again.");
                dialogueLines.Add("The bats have been defeated once more, and our village is safe.");
                dialogueLines.Add("Here is your reward for your continued service.");
            }
            else
            {
                dialogueLines.Add("Welcome back, brave adventurer!");
                dialogueLines.Add("You have returned to claim your reward.");
                dialogueLines.Add("The bats have been defeated, and our village is safe once more.");
                dialogueLines.Add("Here is your reward for your excellent work.");
            }
            
            // Add reward information
            var goldReward = returnQuest.GoldReward;
            dialogueLines.Add($"You will receive {goldReward} gold coins for your efforts.");
            
            // Turn in the return quest (this will publish reward event)
            _questManager?.TurnInQuest(returnQuest);
            
            // Show completion message with hint about taking the quest again
            dialogueLines.Add("Thank you for your service to our village!");
            dialogueLines.Add("If you wish to help us again, you can take this quest once more.");
            
            // Continue with main dialogue options
            _dialogueManager.ContinueDialogue(dialogueLines, GetMayorMainChoices());
        }

    }
} 