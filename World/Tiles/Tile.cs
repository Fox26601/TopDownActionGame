namespace IsometricActionGame
{
    public class Tile
    {
        public TileType Type { get; set; }
        public bool IsWalkable { get; set; }
        public float MovementCost { get; set; }

        public Tile(TileType type)
        {
            Type = type;
            switch (type)
            {
                case TileType.Wall:
                    IsWalkable = false;
                    MovementCost = float.MaxValue;
                    break;

                default:
                    IsWalkable = true;
                    MovementCost = 1f;
                    break;
            }
        }
    }
} 