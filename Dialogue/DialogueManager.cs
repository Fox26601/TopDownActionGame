using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using IsometricActionGame.UI;
using IsometricActionGame.Events;
using IsometricActionGame.Input.Handlers;
using System.Linq;

namespace IsometricActionGame.Dialogue
{
    public class DialogueChoice
    {
        public string Text { get; set; }
        public Action OnSelected { get; set; }
        public bool IsSelected { get; set; }

        public DialogueChoice(string text, Action onSelected)
        {
            Text = text;
            OnSelected = onSelected;
            IsSelected = false;
        }
    }

    public class DialogueManager : IDisposable
    {
        private List<string> _dialogueLines;
        private int _currentLineIndex;
        private bool _isDialogueActive;
        private string _currentDisplayText;
        private string _fullText;
        private SpriteFont _font;
        private Vector2 _dialoguePosition;
        private Rectangle _dialogueBackground;
        
        // Enhanced features
        private bool _isShowingChoices = false;
        private List<DialogueChoice> _currentChoices;
        private int _selectedChoiceIndex = 0;
        private bool _dialogueComplete = false;
        private bool _hasPendingChoices = false;
        private bool _canDialogueBeClosed = true; // New flag to control ESC key closing
        private bool _awaitingChoiceInput = false; // New flag to delay choice input processing
        
        // Architectural components
        private IDialoguePositioningStrategy _positioningStrategy;
        private IDialogueInputHandler _inputHandler;
        private Input.Handlers.DialogueInputHandler _unifiedInputHandler;

        // Cached textures for performance
        private Texture2D _backgroundTexture;
        private Texture2D _borderTexture;
        private bool _texturesInitialized = false;
        private bool _disposed = false;

        public bool IsDialogueActive => _isDialogueActive;
    

        public DialogueManager()
        {
            _dialogueLines = new List<string>();
            _dialoguePosition = new Vector2(GameConstants.UILayout.DIALOGUE_POSITION_X, GameConstants.UILayout.DIALOGUE_POSITION_Y);
            _dialogueBackground = new Rectangle(90, 490, 1100, 120);
            _currentChoices = new List<DialogueChoice>();
            _dialogueComplete = false;
            _hasPendingChoices = false;
            _currentDisplayText = string.Empty; // Initialize to prevent null
            _fullText = string.Empty; // Initialize to prevent null
            
            // Initialize architectural components
            _positioningStrategy = new QuickAccessAwareDialoguePositioning();
            _inputHandler = new NumberKeyDialogueInputHandler();
            System.Diagnostics.Debug.WriteLine("DialogueManager: Constructor called.");
        }
        
        /// <summary>
        /// Set unified input handler for dialogue system
        /// </summary>
        public void SetInputHandler(Input.Handlers.DialogueInputHandler inputHandler)
        {
            _unifiedInputHandler = inputHandler;
        }
        
        /// <summary>
        /// Initialize dialogue manager with screen dimensions for scaling
        /// </summary>
        public void Initialize(int screenWidth, int screenHeight, GraphicsDevice graphicsDevice)
        {
            // Initialize UI scaling
            UIScalingManager.Initialize(screenWidth, screenHeight);
            
    
            UpdateDialogueLayout();

            // Initialize textures here as GraphicsDevice is available
            InitializeTextures(graphicsDevice);
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Initialized with {screenWidth}x{screenHeight}");
        }
        
        /// <summary>
        /// Sets new dialogue lines to be displayed, ending any current dialogue.
        /// </summary>
        /// <param name="dialogueLines">The list of strings representing the dialogue lines.</param>
        public void SetDialogue(List<string> dialogueLines)
        {
            StartDialogue(dialogueLines);
            System.Diagnostics.Debug.WriteLine($"DialogueManager: SetDialogue called with {dialogueLines.Count} lines.");
        }

        /// <summary>
        /// Sets new dialogue lines to be displayed, and queues choices to be shown after all lines are displayed.
        /// This is useful for dialogues that lead to a decision point.
        /// </summary>
        /// <param name="dialogueLines">The list of strings representing the dialogue lines.</param>
        /// <param name="choices">The list of DialogueChoice objects to be presented after dialogue lines are complete.</param>
        public void SetDialogueWithPendingChoices(List<string> dialogueLines, List<DialogueChoice> choices)
        {
            StartDialogue(dialogueLines);
            SetPendingChoices(choices);
            System.Diagnostics.Debug.WriteLine($"DialogueManager: SetDialogueWithPendingChoices called with {dialogueLines.Count} lines and {choices.Count} choices.");
        }

        /// <summary>
        /// Update dialogue layout for current screen resolution
        /// </summary>
        private void UpdateDialogueLayout()
        {
            try
            {
                var (screenWidth, screenHeight) = UIScalingManager.GetScreenDimensions();
                
                // Check if UIScalingManager is properly initialized (not zero dimensions)
                if (screenWidth <= 0 || screenHeight <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("DialogueManager: UIScalingManager not initialized, using default layout");
                    return; // Keep default layout
                }
                
                // Scale dialogue panel dimensions
                int panelWidth = UIScalingManager.ScaleValue(GameConstants.UI.DIALOGUE_PANEL_WIDTH);
                int panelHeight = UIScalingManager.ScaleValue(GameConstants.UI.DIALOGUE_PANEL_HEIGHT);
                
                // Use positioning strategy to calculate optimal position (safe null check)
                if (_positioningStrategy != null)
                {
                    _dialoguePosition = _positioningStrategy.CalculateDialoguePosition(screenWidth, screenHeight, panelWidth, panelHeight);
                    _dialogueBackground = _positioningStrategy.GetDialogueBackground(_dialoguePosition, panelWidth, panelHeight);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DialogueManager: Positioning strategy is null, using default layout");
                    // Keep default layout if positioning strategy is null
                }
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Layout updated to Panel: {_dialogueBackground.Width}x{_dialogueBackground.Height} at {_dialogueBackground.X},{_dialogueBackground.Y}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Error updating layout, using defaults: {ex.Message}");
                // Keep default values if something goes wrong
            }
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                _font = content.Load<SpriteFont>("messageFont");
                System.Diagnostics.Debug.WriteLine("Dialogue font loaded successfully");
            }
            catch (Exception ex)
            {
                _font = null;
                System.Diagnostics.Debug.WriteLine($"Failed to load dialogue font: {ex.Message}");
            }
            // Initialize textures is now called in Initialize method
        }

        private void InitializeTextures(GraphicsDevice graphicsDevice)
        {
            if (_texturesInitialized) return;
            
            // Create cached textures
            _backgroundTexture = new Texture2D(graphicsDevice, 1, 1);
            _backgroundTexture.SetData(new[] { Color.Black });
            
            _borderTexture = new Texture2D(graphicsDevice, 1, 1);
            _borderTexture.SetData(new[] { Color.White });
            
            _texturesInitialized = true;
        }

        public void StartDialogue(List<string> dialogueLines)
        {
            if (_disposed) return;
            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: StartDialogue called with {dialogueLines?.Count ?? 0} lines. Font is null: {_font == null}, Textures Initialized: {_texturesInitialized}");
            
            // Clear any existing state
            EndDialogue();
            
            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: No dialogue lines provided");
                // If no lines, dialogue is not active, can be closed
                _isDialogueActive = false;
                _canDialogueBeClosed = true;
                return;
            }
            
            _dialogueLines = new List<string>(dialogueLines);
            _currentLineIndex = 0;
            _isDialogueActive = true;
            _isShowingChoices = false;
            _fullText = _dialogueLines[0];
            _currentDisplayText = _fullText;
            _selectedChoiceIndex = 0;
            _dialogueComplete = false;
            _hasPendingChoices = false;
            _awaitingChoiceInput = false; // Reset this flag when dialogue starts
            
            // By default, dialogue can be closed, unless choices are pending
            _canDialogueBeClosed = true; 
            
            // Publish DIALOGUE_STARTED event (safe check for event system)
            GameEventSystem.Instance?.Publish(GameEvents.DIALOGUE_STARTED, _currentDisplayText);
            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Dialogue started with {_dialogueLines.Count} lines");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Initial display text: '{_currentDisplayText}'");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: State after StartDialogue: Active={_isDialogueActive}, ShowingChoices={_isShowingChoices}, HasPending={_hasPendingChoices}, CanBeClosed={_canDialogueBeClosed}, AwaitingInput={_awaitingChoiceInput}");
        }

        public void ShowChoices(List<DialogueChoice> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("ShowChoices called with null or empty choices");
                EndDialogue(); // End dialogue if no choices are provided
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"ShowChoices called with {choices.Count} choices");
            _isShowingChoices = true;
            _currentChoices = new List<DialogueChoice>(choices);
            _selectedChoiceIndex = 0;
            _hasPendingChoices = false; // Clear pending flag since we're showing choices now
            _awaitingChoiceInput = true; // Set to true to delay input processing for one frame
            _unifiedInputHandler?.Reset(); // Reset input states when showing choices
            
            System.Diagnostics.Debug.WriteLine($"Choices displayed: {_currentChoices.Count}");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: State after ShowChoices: Active={_isDialogueActive}, ShowingChoices={_isShowingChoices}, HasPending={_hasPendingChoices}, CanBeClosed={_canDialogueBeClosed}, AwaitingInput={_awaitingChoiceInput}");
        }

        /// <summary>
        /// Set choices to be shown after dialogue completion
        /// </summary>
        public void SetPendingChoices(List<DialogueChoice> choices)
        {
            if (choices == null || choices.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: SetPendingChoices called with null or empty choices");
                _hasPendingChoices = false; // Ensure flag is false
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: SetPendingChoices called with {choices.Count} choices. Font is null: {_font == null}, Textures Initialized: {_texturesInitialized}");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Current state - isDialogueActive: {_isDialogueActive}, isShowingChoices: {_isShowingChoices}, hasPendingChoices: {_hasPendingChoices}");
            
            _currentChoices = new List<DialogueChoice>(choices);
            _hasPendingChoices = true;
            

            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Pending choices set successfully. hasPendingChoices: {_hasPendingChoices}, choicesCount: {_currentChoices.Count}");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Choice texts: {string.Join(", ", _currentChoices.Select(c => $"'{c.Text}'"))}");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: State after SetPendingChoices: Active={_isDialogueActive}, ShowingChoices={_isShowingChoices}, HasPending={_hasPendingChoices}, CanBeClosed={_canDialogueBeClosed}, AwaitingInput={_awaitingChoiceInput}");
        }

        /// <summary>
        /// Appends new dialogue lines to the current dialogue and sets new pending choices.
        /// This allows for continuous dialogue flow without restarting the dialogue.
        /// </summary>
        /// <param name="newDialogueLines">New lines to append to the current dialogue.</param>
        /// <param name="newChoices">New choices to be presented after the appended lines.</param>
        public void ContinueDialogue(List<string> newDialogueLines, List<DialogueChoice> newChoices)
        {
            if (_disposed) return;
            if (!_isDialogueActive)
            {
                // If dialogue is not active, start a new one with the provided content
                StartDialogue(newDialogueLines);
                if (newChoices != null && newChoices.Count > 0)
                {
                    SetPendingChoices(newChoices);
                }
                System.Diagnostics.Debug.WriteLine("DialogueManager: ContinueDialogue started a new dialogue as none was active.");
                return;
            }

            // Append new dialogue lines to the existing ones
            if (newDialogueLines != null && newDialogueLines.Count > 0)
            {
                // If dialogue was already complete, reset currentLineIndex to start of new lines
                if (_currentLineIndex >= _dialogueLines.Count)
                {
                    _currentLineIndex = _dialogueLines.Count; // Set to the index where new lines begin
                    _currentDisplayText = newDialogueLines[0]; // Display the first new line
                }

                _dialogueLines.AddRange(newDialogueLines);
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Appended {newDialogueLines.Count} new dialogue lines.");
            }
            
            // Set new pending choices
            if (newChoices != null && newChoices.Count > 0)
            {
                SetPendingChoices(newChoices);
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Set {newChoices.Count} new pending choices for continued dialogue.");
            }
            else
            {
                // If no new choices, ensure pending choices flag is false
                _hasPendingChoices = false;
                // If dialogue is complete and no new choices, allow it to be closed.
                if (_dialogueComplete) _canDialogueBeClosed = true; 
            }
            

            _dialogueComplete = false;
            _isShowingChoices = false;
            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Dialogue continued. Total lines: {_dialogueLines.Count}, HasPendingChoices: {_hasPendingChoices}");
        }

        public void Update(GameTime gameTime)
        {
            if (_disposed || !_isDialogueActive) 
            {
                return;
            }
            
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Update start. Active={_isDialogueActive}, ShowingChoices={_isShowingChoices}, HasPending={_hasPendingChoices}, CanBeClosed={_canDialogueBeClosed}, AwaitingInput={_awaitingChoiceInput}, LineIndex={_currentLineIndex}, LinesCount={_dialogueLines.Count}");
            
            // Validate state
            if (_dialogueLines == null || _dialogueLines.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: No dialogue lines, ending dialogue");
                EndDialogue();
                return;
            }
            
            // Safety check: prevent infinite dialogue - but allow pending choices
            if (_currentLineIndex >= _dialogueLines.Count && !_hasPendingChoices && !_isShowingChoices)
            {
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Dialogue completed, currentLineIndex: {_currentLineIndex}, linesCount: {_dialogueLines.Count}, hasPendingChoices: {_hasPendingChoices}, isShowingChoices: {_isShowingChoices}");
                EndDialogue();
                return;
            }

            var keyboard = Keyboard.GetState();
            
            // Handle ESC key to close dialogue (safe null check)
            // Only allow closing if _canDialogueBeClosed is true AND input handler confirms ESC press
            if (_canDialogueBeClosed && _unifiedInputHandler != null && _unifiedInputHandler.ShouldCloseDialogue(keyboard))
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: ESC pressed, ending dialogue");
                EndDialogue();
                return;
            }

            if (_isShowingChoices)
            {
                // If choices just became active, skip input processing for one frame
                if (_awaitingChoiceInput)
                {
                    System.Diagnostics.Debug.WriteLine("DialogueManager: Skipping choice input for one frame (awaitingChoiceInput is true)");
                    _awaitingChoiceInput = false; // Allow input processing next frame
                    // IMPORTANT: We still need to update the input handler's previous state
                    // so that 'just pressed' calculations are accurate for the next frame.
                    _unifiedInputHandler?.UpdatePreviousKeyboardState(keyboard);
                    return;
                }
                System.Diagnostics.Debug.WriteLine("DialogueManager: Updating choices.");
                UpdateChoices(keyboard);
                // Update previous keyboard state after processing choices
                _unifiedInputHandler?.UpdatePreviousKeyboardState(keyboard);
                return;
            }

            // Don't handle progression if dialogue is complete and we have pending choices
            if (_dialogueComplete && _hasPendingChoices) 
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: Dialogue complete and has pending choices, skipping progression.");
                // Update previous keyboard state even if no progression is handled
                _unifiedInputHandler?.UpdatePreviousKeyboardState(keyboard);
                return;
            }

            // Handle dialogue progression using input handler (safe null check)
            if (_unifiedInputHandler != null && _unifiedInputHandler.HandleDialogueProgression(keyboard))
            {
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Progression input detected, currentLineIndex: {_currentLineIndex}, linesCount: {_dialogueLines.Count}");
                _currentLineIndex++;
                if (_currentLineIndex >= _dialogueLines.Count)
                {
                    // Dialogue is complete, check if we have pending choices
                    System.Diagnostics.Debug.WriteLine($"DialogueManager: Dialogue lines completed. Checking pending choices: hasPendingChoices={_hasPendingChoices}, currentChoices={_currentChoices?.Count ?? 0}");
                    if (_hasPendingChoices && _currentChoices != null && _currentChoices.Count > 0)
                    {
                        // Show choices instead of ending dialogue
                        System.Diagnostics.Debug.WriteLine($"DialogueManager: Dialogue completed, showing {_currentChoices.Count} pending choices");
                        _isShowingChoices = true;
                        _hasPendingChoices = false; // Reset hasPendingChoices once choices are active
                        _dialogueComplete = true;
                        _awaitingChoiceInput = true; // Set this flag to true to delay input for next frame
                        
                        // When choices are active, dialogue cannot be closed by ESC
                        
                        // Reset current display text to the last dialogue line for better UX
                        _currentDisplayText = _dialogueLines[_dialogueLines.Count - 1];
                        
                        // Don't call OnDialogueEnded here - wait until choice is selected
                    }
                    else
                    {
                        // Dialogue lines completed, no pending choices.
                        // Mark dialogue as complete, but don't end it yet. Wait for explicit close input.
                        System.Diagnostics.Debug.WriteLine("DialogueManager: Dialogue lines completed, no pending choices. Awaiting explicit close.");
                        _dialogueComplete = true;
                        _canDialogueBeClosed = true; // Allow ESC to close when dialogue is complete without choices
                        
                        // Also, next SPACE/ENTER press will close the dialogue
                        // No need to publish DIALOGUE_ENDED here yet
                    }
                }
                else
                {
                    _currentDisplayText = _dialogueLines[_currentLineIndex];
                    System.Diagnostics.Debug.WriteLine($"DialogueManager: Advanced to line {_currentLineIndex}: {_currentDisplayText}");
                }
            }
            else if (_dialogueComplete && !_isShowingChoices && _unifiedInputHandler != null && _unifiedInputHandler.HandleDialogueProgression(keyboard))
            {
                // If dialogue is complete (all lines shown, no choices) and Space/Enter is pressed, close it.
                System.Diagnostics.Debug.WriteLine("DialogueManager: Explicit close input detected after dialogue completion.");
                GameEventSystem.Instance.Publish<object>(GameEvents.DIALOGUE_ENDED, null);
                EndDialogue();
            }
            
            // Always update the previous keyboard state at the end of the update cycle
            _unifiedInputHandler?.UpdatePreviousKeyboardState(keyboard);
        }

        private void UpdateChoices(KeyboardState keyboard)
        {
            System.Diagnostics.Debug.WriteLine("DialogueManager: UpdateChoices called.");
            
            // Handle choice selection using input handler (safe null check)
            if (_unifiedInputHandler == null)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: Input handler is null in UpdateChoices");
                return;
            }
            
            int selectedChoice = _unifiedInputHandler.HandleChoiceSelection(keyboard, _currentChoices.Count);
            System.Diagnostics.Debug.WriteLine($"DialogueManager: HandleChoiceSelection returned: {selectedChoice}");
            
            if (selectedChoice >= 0)
            {
                // Direct number key selection (1-4)
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Choice {selectedChoice + 1} selected via number key");
                var choice = _currentChoices[selectedChoice];
                
                // Publish DIALOGUE_CHOICE_SELECTED event (safe check for event system)
                GameEventSystem.Instance?.Publish(GameEvents.DIALOGUE_CHOICE_SELECTED, new { ChoiceIndex = selectedChoice, ChoiceText = choice.Text });
                
                choice.OnSelected?.Invoke();
            }
            else if (selectedChoice == -2)
            {
                // Previous choice (Up arrow)
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Navigating to previous choice from {_selectedChoiceIndex}");
                // Clear previous selection if any
                if (_selectedChoiceIndex >= 0 && _selectedChoiceIndex < _currentChoices.Count)
                {
                    _currentChoices[_selectedChoiceIndex].IsSelected = false;
                }
                _selectedChoiceIndex = (_selectedChoiceIndex - 1 + _currentChoices.Count) % _currentChoices.Count;
                _currentChoices[_selectedChoiceIndex].IsSelected = true;
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Moved to previous choice: {_selectedChoiceIndex}");
            }
            else if (selectedChoice == -3)
            {
                // Next choice (Down arrow)
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Navigating to next choice from {_selectedChoiceIndex}");
                // Clear previous selection if any
                if (_selectedChoiceIndex >= 0 && _selectedChoiceIndex < _currentChoices.Count)
                {
                    _currentChoices[_selectedChoiceIndex].IsSelected = false;
                }
                _selectedChoiceIndex = (_selectedChoiceIndex + 1) % _currentChoices.Count;
                _currentChoices[_selectedChoiceIndex].IsSelected = true;
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Moved to next choice: {_selectedChoiceIndex}");
            }
            else if (selectedChoice == -1)
            {
                // Confirm current selection (Enter/Space)
                // Check if a choice is actually selected
                if (_selectedChoiceIndex >= 0 && _selectedChoiceIndex < _currentChoices.Count && _currentChoices[_selectedChoiceIndex].IsSelected)
                {
                    System.Diagnostics.Debug.WriteLine($"DialogueManager: Confirming current choice {_selectedChoiceIndex + 1}");
                    var choice = _currentChoices[_selectedChoiceIndex];
                    
                    // Publish DIALOGUE_CHOICE_SELECTED event (safe check for event system)
                    GameEventSystem.Instance?.Publish(GameEvents.DIALOGUE_CHOICE_SELECTED, new { ChoiceIndex = _selectedChoiceIndex, ChoiceText = choice.Text });
                    
                    choice.OnSelected?.Invoke();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DialogueManager: No choice selected for confirmation");
                }
            }
            else if (selectedChoice == -10)
            {
                // No input detected
                System.Diagnostics.Debug.WriteLine("DialogueManager: No choice input detected.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Unexpected choice value: {selectedChoice}");
            }
        }

        public void EndDialogue()
        {
            System.Diagnostics.Debug.WriteLine("=== DIALOGUE END ===");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: Ending dialogue. isDialogueActive: {_isDialogueActive}, isShowingChoices: {_isShowingChoices}, hasPendingChoices: {_hasPendingChoices}");
            
            // If we are showing choices, and ESC is pressed, we want to allow it to close.
            // If we're ending dialogue programmatically (e.g., after a choice), then it should always end.
            // The _canDialogueBeClosed flag is handled by the calling NPC (Mayor/Vendor) and choice logic.
            
            _isDialogueActive = false;
            _isShowingChoices = false;
            _currentLineIndex = 0;
            _currentDisplayText = "";
            _fullText = "";
            _selectedChoiceIndex = 0;
            _currentChoices?.Clear();
            _dialogueComplete = false; // Reset the flag
            _hasPendingChoices = false; // Reset pending choices flag
            _canDialogueBeClosed = true; // Always allow closing after dialogue fully ends
            _awaitingChoiceInput = false; // Reset this flag when dialogue ends
            _unifiedInputHandler?.Reset(); // Reset input states when dialogue ends
            System.Diagnostics.Debug.WriteLine("=== DIALOGUE ENDED ===");
            System.Diagnostics.Debug.WriteLine($"DialogueManager: State after EndDialogue: Active={_isDialogueActive}, ShowingChoices={_isShowingChoices}, HasPending={_hasPendingChoices}, CanBeClosed={_canDialogueBeClosed}, AwaitingInput={_awaitingChoiceInput}");
        }

        /// <summary>
        /// Allows external control over whether the dialogue can be closed by ESC key.
        /// This is primarily for NPCs like Mayor that have forced dialogue choices.
        /// </summary>
        /// <param name="canClose">True if dialogue can be closed, false otherwise.</param>
        public void SetCanDialogueBeClosed(bool canClose)
        {
            _canDialogueBeClosed = canClose;
            System.Diagnostics.Debug.WriteLine($"DialogueManager: SetCanDialogueBeClosed to {canClose}");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_disposed || !_isDialogueActive) 
            {
                return;
            }
            
            if (_font == null)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: Draw skipped, font is null.");
                return;
            }
            
            if (_dialogueLines == null || _dialogueLines.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("DialogueManager: Draw skipped, no dialogue lines.");
                return;
            }

            try
            {
                
                // Check if textures are initialized
                if (!_texturesInitialized)
                {
                    InitializeTextures(spriteBatch.GraphicsDevice);
                    System.Diagnostics.Debug.WriteLine($"DialogueManager: Initialized textures in Draw. BackgroundTexture is null: {_backgroundTexture == null}");
                }

                
                // Use cached background texture
                if (_backgroundTexture != null)
                {
                    // Draw dialogue background
                    spriteBatch.Draw(_backgroundTexture, _dialogueBackground, Color.Black);
                    
                    // Draw white border
                    var borderRect = new Rectangle(_dialogueBackground.X - 2, _dialogueBackground.Y - 2, 
                                                 _dialogueBackground.Width + 4, _dialogueBackground.Height + 4);
                    spriteBatch.Draw(_backgroundTexture, borderRect, Color.White);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("DialogueManager: Background texture is null in Draw method.");
                }

                // Draw the dialogue text (safe check for null)
                if (!string.IsNullOrEmpty(_currentDisplayText))
                {
                    spriteBatch.DrawString(_font, _currentDisplayText, _dialoguePosition, Color.White);
                }
                
                // Draw continue indicator only if not showing choices
                if (!_isShowingChoices)
                {
                    var continueText = "Press SPACE or ENTER to continue...";
                    var continuePos = _dialoguePosition + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, GameConstants.UILayout.DIALOGUE_CONTINUE_Y_OFFSET);
                    spriteBatch.DrawString(_font, continueText, continuePos, Color.Yellow);
                    
                    // Draw ESC indicator
                    // Only show ESC indicator if dialogue can be closed by ESC
                    if (_canDialogueBeClosed)
                    {
                        var escText = "Press ESC to close";
                        var escPos = _dialoguePosition + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, GameConstants.UILayout.DIALOGUE_ESC_Y_OFFSET);
                        spriteBatch.DrawString(_font, escText, escPos, Color.Gray);
                    }
                }
                else if (_dialogueComplete && !_isShowingChoices)
                {
                    // When dialogue is complete and no choices are showing, prompt to close
                    var closeText = "Press SPACE or ENTER to close...";
                    var closePos = _dialoguePosition + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, GameConstants.UILayout.DIALOGUE_CONTINUE_Y_OFFSET);
                    spriteBatch.DrawString(_font, closeText, closePos, Color.Yellow);

                    // Show ESC indicator if dialogue can be closed by ESC
                    if (_canDialogueBeClosed)
                    {
                        var escText = "Press ESC to close";
                        var escPos = _dialoguePosition + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, GameConstants.UILayout.DIALOGUE_ESC_Y_OFFSET);
                        spriteBatch.DrawString(_font, escText, escPos, Color.Gray);
                    }
                }
                
                // Draw choices BELOW the dialogue window if showing choices
                if (_isShowingChoices && _currentChoices != null && _currentChoices.Count > 0)
                {
                    DrawChoices(spriteBatch, _dialoguePosition + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, GameConstants.UILayout.DIALOGUE_CHOICES_Y_OFFSET));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DialogueManager: Error in Draw method: {ex.Message}");
            }
        }

        private void DrawChoices(SpriteBatch spriteBatch, Vector2 dialoguePos)
        {
            var choiceStartPos = dialoguePos;
            
            for (int i = 0; i < _currentChoices.Count; i++)
            {
                var choice = _currentChoices[i];
                var color = choice.IsSelected ? Color.Yellow : Color.White;
                var pos = choiceStartPos + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, i * GameConstants.UILayout.DIALOGUE_CHOICE_SPACING);
                
                // Draw choice number
                var numberText = $"{i + 1}. ";
                spriteBatch.DrawString(_font, numberText, pos, Color.Cyan);
                pos.X += _font.MeasureString(numberText).X;
                
                // Draw selection indicator
                if (choice.IsSelected)
                {
                    var indicatorText = "> ";
                    spriteBatch.DrawString(_font, indicatorText, pos, Color.Yellow);
                    pos.X += _font.MeasureString(indicatorText).X;
                }
                
                spriteBatch.DrawString(_font, choice.Text, pos, color);
            }

            // Draw instruction
            var instructionText = "Press 1-4 for direct selection or use ARROWS + ENTER/SPACE";
            var instructionPos = dialoguePos + GameConstants.UILayout.ZERO_OFFSET + new Vector2(0, _currentChoices.Count * GameConstants.UILayout.DIALOGUE_CHOICE_SPACING + GameConstants.UILayout.DIALOGUE_INSTRUCTION_OFFSET);
            spriteBatch.DrawString(_font, instructionText, instructionPos, Color.Cyan);
        }



        public void Dispose()
        {
            if (_disposed) return;

            _backgroundTexture?.Dispose();
            _borderTexture?.Dispose();
            // Note: SpriteFont doesn't need to be disposed

            _disposed = true;
        }
    }
} 