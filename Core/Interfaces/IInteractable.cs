using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    public interface IInteractable
    {
        float InteractionRadius { get; }
        void OnInteract(Player player);
    }
} 