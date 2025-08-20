using Microsoft.Xna.Framework;
using IsometricActionGame;
using IsometricActionGame.NPCs;
using IsometricActionGame.Core.Data;
using IsometricActionGame.Dialogue;
using IsometricActionGame.Quests;

namespace IsometricActionGame.Core.Factories
{
    public static class EntityFactory
    {
        public static Player CreatePlayer(GameMap gameMap = null)
        {
            return new Player(GameConstants.World.PLAYER_START_POSITION, gameMap);
        }

        public static Bat CreateBat()
        {
            return new Bat(GameConstants.World.BAT_START_POSITION, GameConstants.World.BAT_INITIAL_DIRECTION);
        }

        public static Pebble CreatePebble(float shootCooldown = 0, int damage = 0)
        {
            return new Pebble(GameConstants.World.PEBBLE_START_POSITION, shootCooldown, damage);
        }

        public static Mayor CreateMayor(DialogueManager dialogueManager = null, QuestManager questManager = null)
        {
            return new Mayor(GameConstants.World.MAYOR_POSITION, dialogueManager, questManager);
        }

        public static Vendor CreateVendor(DialogueManager dialogueManager = null)
        {
            return new Vendor(GameConstants.World.VENDOR_POSITION, dialogueManager);
        }

        public static Door CreateDoor()
        {
            return new Door(GameConstants.World.DOOR_POSITION);
        }

        public static Chest CreateChest(Vector2 position)
        {
            return new Chest(position);
        }
    }
}
