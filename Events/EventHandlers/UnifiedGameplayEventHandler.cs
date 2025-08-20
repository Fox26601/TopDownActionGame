using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using IsometricActionGame.Events;
using IsometricActionGame.Events.EventData;
using IsometricActionGame.UI;
using IsometricActionGame.Quests;
using IsometricActionGame.Items;
using IsometricActionGame.Core.Data;


namespace IsometricActionGame.Events.EventHandlers
{
    /// <summary>
    /// Unified handler for all gameplay-related events
    /// Consolidates combat, inventory, quest, and environment event handling
    /// </summary>
    public class UnifiedGameplayEventHandler : IEventHandler
    {
        private readonly ConsoleDisplay _console;
        private readonly QuestManager _questManager;
        private readonly ContentManager _contentManager;
        private readonly Player _player;
        private readonly LevelSystem _levelSystem;
        private readonly GameEventSystem _eventSystem;
        private readonly Random _random;
        
        public UnifiedGameplayEventHandler(
            ConsoleDisplay console, 
            QuestManager questManager, 
            ContentManager contentManager, 
            Player player, 
            LevelSystem levelSystem)
        {
            _console = console;
            _questManager = questManager;
            _contentManager = contentManager;
            _player = player;
            _levelSystem = levelSystem;
            _eventSystem = GameEventSystem.Instance;
            _random = new Random();
        }
        
        public void Initialize()
        {
            SubscribeToEvents();
        }
        
        public void SubscribeToEvents()
        {
            // Combat events
            _eventSystem.Subscribe<PlayerDamagedEventData>(GameEvents.PLAYER_DAMAGED, OnPlayerDamaged);
            _eventSystem.Subscribe<PlayerHealedEventData>(GameEvents.PLAYER_HEALED, OnPlayerHealed);
            _eventSystem.Subscribe<object>(GameEvents.ENEMY_DEFEATED, OnEnemyDefeated);
            _eventSystem.Subscribe<AttackExecutedEventData>(GameEvents.ATTACK_EXECUTED, OnAttackExecuted);
            _eventSystem.Subscribe<DamageDealtEventData>(GameEvents.DAMAGE_DEALT, OnDamageDealt);
            _eventSystem.Subscribe<ProjectileCreatedEventData>(GameEvents.PROJECTILE_CREATED, OnProjectileCreated);
            _eventSystem.Subscribe<ProjectileDestroyedEventData>(GameEvents.PROJECTILE_DESTROYED, OnProjectileDestroyed);
            
            // Inventory events
            _eventSystem.Subscribe<ItemPickedUpEventData>(GameEvents.ITEM_PICKED_UP, OnItemPickedUp);
            _eventSystem.Subscribe<ItemUsedEventData>(GameEvents.ITEM_USED, OnItemUsed);
            _eventSystem.Subscribe<GoldChangedEventData>(GameEvents.GOLD_CHANGED, OnGoldChanged);
            _eventSystem.Subscribe<ItemDroppedEventData>(GameEvents.ITEM_DROPPED, OnItemDropped);
            
            // Quest events
            _eventSystem.Subscribe<Quest>(GameEvents.QUEST_STARTED, OnQuestStarted);
            _eventSystem.Subscribe<Quest>(GameEvents.QUEST_COMPLETED, OnQuestCompleted);
            _eventSystem.Subscribe<Quest>(GameEvents.QUEST_TURNED_IN, OnQuestTurnedIn);
            _eventSystem.Subscribe<Quest>(GameEvents.QUEST_REFUSED, OnQuestRefused);
            
            // Environment events
            _eventSystem.Subscribe<object>(GameEvents.CHEST_OPENED, OnChestOpened);
            
            // Dialogue events
            _eventSystem.Subscribe<string>(GameEvents.DIALOGUE_STARTED, OnDialogueStarted);
            _eventSystem.Subscribe<object>(GameEvents.DIALOGUE_ENDED, OnDialogueEnded);
            _eventSystem.Subscribe<object>(GameEvents.DIALOGUE_CHOICE_SELECTED, OnDialogueChoiceSelected);
        }
        
        public void UnsubscribeFromEvents()
        {
            // Unsubscribe from all events
            _eventSystem.Unsubscribe<PlayerDamagedEventData>(GameEvents.PLAYER_DAMAGED, OnPlayerDamaged);
            _eventSystem.Unsubscribe<PlayerHealedEventData>(GameEvents.PLAYER_HEALED, OnPlayerHealed);
            _eventSystem.Unsubscribe<object>(GameEvents.ENEMY_DEFEATED, OnEnemyDefeated);
            _eventSystem.Unsubscribe<AttackExecutedEventData>(GameEvents.ATTACK_EXECUTED, OnAttackExecuted);
            _eventSystem.Unsubscribe<DamageDealtEventData>(GameEvents.DAMAGE_DEALT, OnDamageDealt);
            _eventSystem.Unsubscribe<ProjectileCreatedEventData>(GameEvents.PROJECTILE_CREATED, OnProjectileCreated);
            _eventSystem.Unsubscribe<ProjectileDestroyedEventData>(GameEvents.PROJECTILE_DESTROYED, OnProjectileDestroyed);
            
            _eventSystem.Unsubscribe<ItemPickedUpEventData>(GameEvents.ITEM_PICKED_UP, OnItemPickedUp);
            _eventSystem.Unsubscribe<ItemUsedEventData>(GameEvents.ITEM_USED, OnItemUsed);
            _eventSystem.Unsubscribe<GoldChangedEventData>(GameEvents.GOLD_CHANGED, OnGoldChanged);
            _eventSystem.Unsubscribe<ItemDroppedEventData>(GameEvents.ITEM_DROPPED, OnItemDropped);
            
            _eventSystem.Unsubscribe<Quest>(GameEvents.QUEST_STARTED, OnQuestStarted);
            _eventSystem.Unsubscribe<Quest>(GameEvents.QUEST_COMPLETED, OnQuestCompleted);
            _eventSystem.Unsubscribe<Quest>(GameEvents.QUEST_TURNED_IN, OnQuestTurnedIn);
            _eventSystem.Unsubscribe<Quest>(GameEvents.QUEST_REFUSED, OnQuestRefused);
            
            _eventSystem.Unsubscribe<object>(GameEvents.CHEST_OPENED, OnChestOpened);
            
            _eventSystem.Unsubscribe<string>(GameEvents.DIALOGUE_STARTED, OnDialogueStarted);
            _eventSystem.Unsubscribe<object>(GameEvents.DIALOGUE_ENDED, OnDialogueEnded);
            _eventSystem.Unsubscribe<object>(GameEvents.DIALOGUE_CHOICE_SELECTED, OnDialogueChoiceSelected);
        }
        
        #region Combat Event Handlers
        
        private void OnPlayerDamaged(PlayerDamagedEventData data)
        {
            _console?.AddMessage($"Player took {data.Damage} damage!", Color.Red);
            
            if (!string.IsNullOrEmpty(data.AttackerType))
            {
                _console?.AddMessage($"Attacked by {data.AttackerType}!", Color.OrangeRed);
            }
        }
        
        private void OnPlayerHealed(PlayerHealedEventData data)
        {
            _console?.AddMessage($"Player healed for {data.Amount} HP!", Color.Green);
            
            if (!string.IsNullOrEmpty(data.HealSource))
            {
                _console?.AddMessage($"Healed by {data.HealSource}!", Color.LightGreen);
            }
        }
        
        private void OnEnemyDefeated(object data)
        {
            var enemyType = GetPropertyValue<string>(data, "EnemyType");
            var position = GetPropertyValue<Vector2>(data, "Position");
            
            _console?.AddMessage($"{enemyType} defeated!", Color.Orange);
            
            // Update quest progress
            if (enemyType == "Bat" || enemyType == "Pebble")
            {
                _questManager?.UpdateQuestProgress(enemyType);
            }
            
            // Update save data
            UpdateEnemyKillCount(enemyType);
            
            // Handle item drops
            HandleEnemyItemDrops(enemyType, position);
        }
        

        
        private void OnAttackExecuted(AttackExecutedEventData data)
        {
            var criticalText = data.IsCritical ? " CRITICAL!" : "";
            _console?.AddMessage($"{data.AttackerType} attacked for {data.Damage} damage{criticalText}!", Color.Yellow);
        }
        
        private void OnDamageDealt(DamageDealtEventData data)
        {
            var criticalText = data.IsCritical ? " CRITICAL!" : "";
            _console?.AddMessage($"{data.AttackerType} dealt {data.Damage} damage to {data.TargetType}{criticalText}!", Color.Yellow);
        }
        
        private void OnProjectileCreated(ProjectileCreatedEventData data)
        {
            _console?.AddMessage($"{data.ProjectileType} projectile created!", Color.Cyan);
        }
        
        private void OnProjectileDestroyed(ProjectileDestroyedEventData data)
        {
            var reason = !string.IsNullOrEmpty(data.DestroyReason) ? $" ({data.DestroyReason})" : "";
            _console?.AddMessage($"{data.ProjectileType} projectile destroyed{reason}!", Color.Gray);
        }
        
        #endregion
        
        #region Inventory Event Handlers
        
        private void OnItemPickedUp(ItemPickedUpEventData data)
        {
            if (data.Quantity > 1)
            {
                _console?.AddMessage($"Picked up {data.Quantity}x {data.Item.Name}!", Color.Yellow);
            }
            else
            {
                _console?.AddMessage($"Picked up {data.Item.Name}!", Color.Yellow);
            }
        }
        
        private void OnItemUsed(ItemUsedEventData data)
        {
            var target = !string.IsNullOrEmpty(data.UseTarget) ? $" on {data.UseTarget}" : "";
            _console?.AddMessage($"Used {data.Item.Name}{target}!", Color.Cyan);
        }
        
        private void OnGoldChanged(GoldChangedEventData data)
        {
            var changeText = data.Change > 0 ? $"+{data.Change}" : data.Change.ToString();
            var reason = !string.IsNullOrEmpty(data.ChangeReason) ? $" ({data.ChangeReason})" : "";
            _console?.AddMessage($"Gold: {data.NewAmount} ({changeText}){reason}!", Color.Gold);
        }
        
        private void OnItemDropped(ItemDroppedEventData data)
        {
            var source = !string.IsNullOrEmpty(data.DropSource) ? $" from {data.DropSource}" : "";
            _console?.AddMessage($"{data.Item.Name} dropped{source}!", Color.Orange);
        }
        
        #endregion
        
        #region Quest Event Handlers
        
        private void OnQuestStarted(Quest quest)
        {
            _console?.AddMessage($"Quest started: {quest?.Title ?? "Unknown"}!", Color.LightBlue);
        }
        
        private void OnQuestCompleted(Quest quest)
        {
            System.Diagnostics.Debug.WriteLine($"UnifiedGameplayEventHandler: Quest completed event received for quest: {quest?.Title}");
            _console?.AddMessage($"Quest completed: {quest?.Title ?? "Unknown"}!", Color.LimeGreen);
            _console?.AddMessage("Return to the Mayor to claim your reward!", Color.Gold);
        }
        
        private void OnQuestTurnedIn(Quest quest)
        {
            _console?.AddMessage($"Quest turned in: {quest?.Title ?? "Unknown"}!", Color.Gold);
        }
        
        private void OnQuestRefused(Quest quest)
        {
            _console?.AddMessage($"Quest refused: {quest?.Title ?? "Unknown"}!", Color.Red);
        }
        
        #endregion
        
        #region Environment Event Handlers
        
        private void OnChestOpened(object data)
        {
            var containedKey = GetPropertyValue<bool>(data, "ContainedKey");
            if (containedKey)
            {
                _console?.AddMessage("Chest opened! Key found!", Color.Gold);
            }
            else
            {
                _console?.AddMessage("Chest opened!", Color.SaddleBrown);
            }
        }
        

        
        #endregion
        
        #region Dialogue Event Handlers
        
        private void OnDialogueStarted(string npcName)
        {
            _console?.AddMessage($"Talking to {npcName}!", Color.LightGreen);
        }
        
        private void OnDialogueEnded(object data = null)
        {
            _console?.AddMessage("Conversation ended!", Color.Gray);
        }
        
        private void OnDialogueChoiceSelected(object choiceData)
        {
            var choiceIndex = GetPropertyValue<int>(choiceData, "ChoiceIndex");
            var choiceText = GetPropertyValue<string>(choiceData, "ChoiceText");
            _console?.AddMessage($"Choice selected: {choiceIndex + 1} - {choiceText}!", Color.Cyan);
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateEnemyKillCount(string enemyType)
        {
            if (_levelSystem != null)
            {
                var currentEnemyKillCounts = _levelSystem.GetCurrentEnemyKillCounts();
                if (currentEnemyKillCounts.ContainsKey(enemyType))
                {
                    currentEnemyKillCounts[enemyType]++;
                }
                else
                {
                    currentEnemyKillCounts[enemyType] = 1;
                }
                
                var totalPlayTime = _levelSystem.GetCurrentTotalPlayTime();
                _levelSystem.SetSaveData(currentEnemyKillCounts, totalPlayTime);
                
                System.Diagnostics.Debug.WriteLine($"UnifiedGameplayEventHandler: Updated kill count for {enemyType}: {currentEnemyKillCounts[enemyType]}");
                _console?.AddMessage($"Updated kill count for {enemyType}: {currentEnemyKillCounts[enemyType]}", Color.Cyan);
            }
        }
        
        private void HandleEnemyItemDrops(string enemyType, Vector2 position)
        {
            if (_player == null || _contentManager == null) return;
            
            // Drop gold with probability
            if (_random.NextDouble() < GameConstants.Probability.GOLD_DROP_CHANCE)
            {
                var gold = ItemFactory.CreateGold(1);
                gold.LoadContent(_contentManager);
                _player.Inventory.AddItem(gold);
                _console?.AddMessage("Gold dropped!", Color.Yellow);
            }
            
            // Drop health potion from Bat with 50% probability
            if (enemyType == "Bat" && _random.NextDouble() < GameConstants.Probability.HEALTH_POTION_DROP_CHANCE)
            {
                var potion = ItemFactory.CreateHealthPotion();
                potion.LoadContent(_contentManager);
                
                var droppedPotion = new DroppedItem(position, potion, GameConstants.SpriteScale.HEALTH_POTION_SCALE);
                droppedPotion.LoadContent(_contentManager);
                
                // Note: ITEM_DROPPED event is now handled by UnifiedEventSystem
                // The dropped item should be added to the game world by the calling system
                
                _console?.AddMessage("Health potion dropped on ground!", Color.Green);
            }
            
            // Create chest when Pebble is defeated
            if (enemyType == "Pebble" && _contentManager != null)
            {
                System.Diagnostics.Debug.WriteLine("UnifiedGameplayEventHandler: Pebble defeated - creating chest");
                var chest = new Chest(position);
                chest.LoadContent(_contentManager);
                
                // Publish event to notify Game1 to add the chest to interactables
                _eventSystem?.Publish(GameEvents.CHEST_CREATED, new { Chest = chest, Position = position });
                
                _console?.AddMessage("Chest appeared!", Color.Gold);
            }
        }
        
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null) return default(T);
            
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.PropertyType == typeof(T))
                {
                    return (T)property.GetValue(obj);
                }
            }
            catch
            {
                // Ignore reflection errors
            }
            
            return default(T);
        }
        
        #endregion
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}
