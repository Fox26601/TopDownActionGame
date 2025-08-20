using Microsoft.Xna.Framework;
using IsometricActionGame.Items;

namespace IsometricActionGame.Inventory
{
    /// <summary>
    /// Represents the state of a drag and drop operation in the inventory
    /// Implements State pattern for clean drag and drop logic
    /// </summary>
    public class InventoryDragState
    {
        public Item DraggedItem { get; private set; }
        public Vector2 DragOffset { get; set; }
        public Point? SourcePosition { get; private set; }
        public bool IsQuickAccessSource { get; private set; }
        public bool IsActive { get; private set; }

        /// <summary>
        /// Starts a new drag operation
        /// </summary>
        /// <param name="item">The item being dragged</param>
        /// <param name="mousePosition">Current mouse position</param>
        /// <param name="sourcePosition">Source slot position</param>
        /// <param name="isQuickAccess">Whether the source is quick access slot</param>
        public void StartDrag(Item item, Vector2 mousePosition, Point sourcePosition, bool isQuickAccess)
        {
            if (item == null) return;

            DraggedItem = item;
            DragOffset = mousePosition;
            SourcePosition = sourcePosition;
            IsQuickAccessSource = isQuickAccess;
            IsActive = true;
        }

        /// <summary>
        /// Completes the drag operation
        /// </summary>
        public void CompleteDrag()
        {
            IsActive = false;
            DraggedItem = null;
            SourcePosition = null;
            IsQuickAccessSource = false;
        }

        /// <summary>
        /// Cancels the drag operation
        /// </summary>
        public void CancelDrag()
        {
            IsActive = false;
            DraggedItem = null;
            SourcePosition = null;
            IsQuickAccessSource = false;
        }

        /// <summary>
        /// Checks if the drag operation is valid
        /// </summary>
        /// <returns>True if the drag operation is valid</returns>
        public bool IsValid()
        {
            return IsActive && DraggedItem != null;
        }
    }
}
