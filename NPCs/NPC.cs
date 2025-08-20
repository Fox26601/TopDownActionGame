using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Core.Helpers;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Dialogue;
using IsometricActionGame.UI;
using System.Collections.Generic;

namespace IsometricActionGame.NPCs
{
    public abstract class NPC : IEntity, IInteractable, IUpdateable
    {
        // Public properties from IEntity
        public Vector2 WorldPosition { get; protected set; }
        public Vector2 FacingDirection { get; protected set; } // Added for IEntity
        public float BaseHitboxRadius { get; protected set; } // Added for IEntity
        public Texture2D Texture { get; protected set; }
        public virtual int SpriteWidth => Texture?.Width ?? 0;
        public virtual int SpriteHeight => Texture?.Height ?? 0;
        public float LayerDepth { get; protected set; }
        public float Scale { get; protected set; }

        // Properties for IInteractable
        public float InteractionRadius { get; protected set; }

        protected DialogueManager _dialogueManager;
        protected SpriteFont _dialogueFont; // Assuming NPCs will use a font for their dialogue
        
        // Reference to player for dynamic depth calculation
        protected Player _currentPlayer;

        public NPC(Vector2 position, DialogueManager dialogueManager)
        {
            WorldPosition = position;
            _dialogueManager = dialogueManager;
            LayerDepth = GameConstants.Graphics.NPC_LAYER_DEPTH;
            Scale = GameConstants.Graphics.DEFAULT_SPRITE_SCALE;
            InteractionRadius = GameConstants.Interaction.NPC_INTERACTION_RADIUS;
            FacingDirection = Vector2.Zero; // Default value, will be updated by specific NPCs
            BaseHitboxRadius = GameConstants.Combat.DEFAULT_NPC_HITBOX_RADIUS; // Default value
        }

        public abstract void LoadContent(ContentManager content);
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch spriteBatch);

        public virtual void OnInteract(Player player)
        {
            // Store reference to current player for depth calculation
            _currentPlayer = player;
            
            // Default interaction logic for NPCs
            // This can be overridden by specific NPC types
        }

        /// <summary>
        /// Updates the layer depth based on position for proper grid rendering
        /// </summary>
        protected void UpdateLayerDepth()
        {
            // Use the grid depth calculation
            LayerDepth = GridHelper.CalculateLayerDepth(WorldPosition);
            System.Diagnostics.Debug.WriteLine($"UpdateLayerDepth: {this.GetType().Name} at {WorldPosition} - Depth: {LayerDepth:F3}");
        }

        /// <summary>
        /// Sets the player reference for dynamic depth calculation
        /// </summary>
        public void SetPlayerReference(Player player)
        {
            System.Diagnostics.Debug.WriteLine($"SetPlayerReference: {this.GetType().Name} - Setting player reference to {player?.WorldPosition}");
            _currentPlayer = player;
            UpdateLayerDepth(); // Update depth immediately
        }

        protected void SetDialogue(List<string> dialogueLines)
        {
            _dialogueManager?.SetDialogue(dialogueLines);
        }

        protected void SetDialogueWithPendingChoices(List<string> dialogueLines, List<DialogueChoice> choices)
        {
            _dialogueManager?.SetDialogueWithPendingChoices(dialogueLines, choices);
        }
        
        /// <summary>
        /// Set the NPC position
        /// </summary>
        public void SetPosition(Vector2 position)
        {
            WorldPosition = position;
        }
    }
}
