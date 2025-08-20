using Microsoft.Xna.Framework;

namespace IsometricActionGame
{
    public class GameMap
    {
        public const int Width = 25;
        public const int Height = 25;
        private Tile[,] _tiles;

        public GameMap()
        {
            _tiles = new Tile[Width, Height];
            GenerateDefaultMap();
        }

        private void GenerateDefaultMap()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // Border walls
                    if (y == 0 || y == Height - 1 || x == 0 || x == Width - 1)
                        _tiles[x, y] = new Tile(TileType.Wall);

                    // Wall obstacles for interesting terrain (adjusted for smaller map)
                    else if ((x == 18 && y == 10) || 
                             (x == 10 && y == 22) || (x == 22 && y == 12) ||
                             (x == 6 && y == 18) || (x == 19 && y == 6))
                        _tiles[x, y] = new Tile(TileType.Wall);
                    // Default - grass for walkable areas
                    else
                        _tiles[x, y] = new Tile(TileType.Grass);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return _tiles[x, y];
        }

        public bool IsWalkable(Vector2 worldPosition)
        {
            int x = (int)worldPosition.X;
            int y = (int)worldPosition.Y;
            var tile = GetTile(x, y);
            return tile != null && tile.IsWalkable;
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            _tiles[x, y] = new Tile(type);
        }
    }
} 