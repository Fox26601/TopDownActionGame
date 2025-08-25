using System;
using System.Collections.Generic;
using System.Linq;
using IsometricActionGame.SaveSystem;

namespace IsometricActionGame.Quests
{
    public class QuestManager : ISaveable
    {
        private List<Quest> _activeQuests;
        private List<Quest> _completedQuests;
        private List<Quest> _refusedQuests;
        private ExtendedKillBatsQuest _extendedKillBatsQuest;
        private SimpleKillBatsQuest _simpleKillBatsQuest;
        private ReturnToMayorQuest _returnToMayorQuest;
        private bool _isPebbleDefeated = false;
        private bool _extendedQuestWasEverCompleted = false;

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests.AsReadOnly();
        public IReadOnlyList<Quest> CompletedQuests => _completedQuests.AsReadOnly();
        public IReadOnlyList<Quest> RefusedQuests => _refusedQuests.AsReadOnly();

        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestTurnedIn;
        public event Action<Quest> OnQuestRefused;
        public event Action<Quest> OnReturnQuestAutoAssigned;
        public event Action<Quest> OnReturnQuestCompleted;
        public event Action OnPebbleDefeated;
        public event Action OnPebbleStateReset;

        public QuestManager()
        {
            // QuestManager constructor
            
            _activeQuests = new List<Quest>();
            _completedQuests = new List<Quest>();
            _refusedQuests = new List<Quest>();
            
            // Create quest instances
            _extendedKillBatsQuest = new ExtendedKillBatsQuest();
            _simpleKillBatsQuest = new SimpleKillBatsQuest();
            
            // Subscribe to quest completion events to automatically move completed quests
            _extendedKillBatsQuest.OnQuestCompleted += (quest) => CompleteQuest(quest);
            _simpleKillBatsQuest.OnQuestCompleted += (quest) => CompleteQuest(quest);
            
            // Constructor completed
        }
        
        public void SubscribeToPebbleEvents()
        {
            var eventSystem = Events.GameEventSystem.Instance;
            if (eventSystem != null)
            {
                // Subscribe to enemy defeated events to track Pebble defeat
                eventSystem.Subscribe<object>(Events.GameEvents.ENEMY_DEFEATED, OnEnemyDefeated);
                // Subscribe to game restart events to reset Pebble state
                eventSystem.Subscribe<object>(Events.GameEvents.GAME_RESTART, OnGameRestarted);
            }
        }
        
        private void OnEnemyDefeated(object data)
        {
            try
            {
                var enemyType = GetPropertyValue<string>(data, "EnemyType");
                
                if (enemyType == "Pebble" && !_isPebbleDefeated)
                {
                    _isPebbleDefeated = true;
                    // Pebble has been defeated
                    OnPebbleDefeated?.Invoke();
                }
            }
            catch
            {
                // Error handling enemy defeated event
            }
        }
        
        private void OnGameRestarted(object data)
        {
            ResetPebbleState();
        }
        
        public void ResetPebbleState()
        {
            if (_isPebbleDefeated)
            {
                _isPebbleDefeated = false;
                // Pebble state reset
                OnPebbleStateReset?.Invoke();
            }
        }

        public void StartKillBatsQuest()
        {
            if (_extendedKillBatsQuest.CanBeTaken())
            {
                _extendedKillBatsQuest.Start();
                _activeQuests.Add(_extendedKillBatsQuest);
                
                // Remove from refused quests if it was previously refused
                if (_refusedQuests.Contains(_extendedKillBatsQuest))
                {
                    _refusedQuests.Remove(_extendedKillBatsQuest);
                }
                
                OnQuestStarted?.Invoke(_extendedKillBatsQuest);
            }
        }

        public void StartSimpleKillBatsQuest()
        {
            if (_simpleKillBatsQuest.CanBeTaken())
            {
                _simpleKillBatsQuest.Start();
                _activeQuests.Add(_simpleKillBatsQuest);
                
                // Remove from refused quests if it was previously refused
                if (_refusedQuests.Contains(_simpleKillBatsQuest))
                {
                    _refusedQuests.Remove(_simpleKillBatsQuest);
                }
                
                OnQuestStarted?.Invoke(_simpleKillBatsQuest);
            }
        }

        public void RefuseKillBatsQuest()
        {
            if (_extendedKillBatsQuest.CanBeRefused())
            {
                _extendedKillBatsQuest.Refuse();
                _activeQuests.Remove(_extendedKillBatsQuest);
                _refusedQuests.Add(_extendedKillBatsQuest);
                OnQuestRefused?.Invoke(_extendedKillBatsQuest);
            }
        }

        public void RefuseSimpleKillBatsQuest()
        {
            if (_simpleKillBatsQuest.CanBeRefused())
            {
                _simpleKillBatsQuest.Refuse();
                _activeQuests.Remove(_simpleKillBatsQuest);
                _refusedQuests.Add(_simpleKillBatsQuest);
                OnQuestRefused?.Invoke(_simpleKillBatsQuest);
            }
        }

        public void UpdateQuestProgress(string enemyType)
        {
            foreach (var quest in _activeQuests)
            {
                quest.UpdateProgress(enemyType);
            }
        }

        public void CompleteQuest(Quest quest)
        {
            System.Diagnostics.Debug.WriteLine($"QuestManager: CompleteQuest called for quest: {quest?.Title}");
            System.Diagnostics.Debug.WriteLine($"QuestManager: Quest in active quests: {_activeQuests.Contains(quest)}, Quest is completed: {quest?.IsCompleted}");
            
            if (_activeQuests.Contains(quest) && quest.IsCompleted)
            {
                _activeQuests.Remove(quest);
                _completedQuests.Add(quest);
                System.Diagnostics.Debug.WriteLine($"QuestManager: Moved quest {quest.Title} from active to completed");
                System.Diagnostics.Debug.WriteLine($"QuestManager: Publishing OnQuestCompleted event for quest: {quest.Title}");
                
                // Mark extended quest as ever completed if this is an extended quest
                if (quest == _extendedKillBatsQuest)
                {
                    _extendedQuestWasEverCompleted = true;
                    System.Diagnostics.Debug.WriteLine("QuestManager: Extended quest marked as ever completed");
                }
                
                OnQuestCompleted?.Invoke(quest);
                
                // Auto-assign return quest if this is not already a return quest
                if (quest != _returnToMayorQuest)
                {
                    AutoAssignReturnQuest(quest);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"QuestManager: Cannot complete quest {quest?.Title}. In active: {_activeQuests.Contains(quest)}, IsCompleted: {quest?.IsCompleted}");
            }
        }

        public void TurnInQuest(Quest quest)
        {
            // Check if quest is completed and not turned in, regardless of which list it's in
            if (quest.IsCompleted && !quest.IsTurnedIn)
            {
                // If quest is still in active quests, move it to completed first
                if (_activeQuests.Contains(quest))
                {
                    _activeQuests.Remove(quest);
                    _completedQuests.Add(quest);
                }
                
                quest.TurnIn();
                OnQuestTurnedIn?.Invoke(quest);
                
                // Publish quest reward event with quest-specific data
                PublishQuestRewardEvent(quest);
                
                // Reset quest after turning in to allow taking it again
                ResetQuestAfterTurnIn(quest);
            }
        }
        
        private void PublishQuestRewardEvent(Quest quest)
        {
            int goldReward = 0;
            string questType = quest.GetType().Name;
            bool hasPebbleObjective = false;
            
            // Handle ReturnToMayorQuest specially
            if (quest is ReturnToMayorQuest returnQuest)
            {
                goldReward = returnQuest.GoldReward;
                questType = returnQuest.QuestType;
                hasPebbleObjective = returnQuest.HasPebbleObjective;
            }
            else if (quest is ExtendedKillBatsQuest extendedQuest)
            {
                goldReward = extendedQuest.GoldReward;
                hasPebbleObjective = extendedQuest.HasPebbleObjective;
            }
            else if (quest is SimpleKillBatsQuest simpleQuest)
            {
                goldReward = simpleQuest.GoldReward;
            }
            
            // Publish the reward event through the unified event system
            Events.UnifiedEventSystem.Instance?.PublishQuestRewardGranted(quest, goldReward, questType, hasPebbleObjective);
        }
        
        private void ResetQuestAfterTurnIn(Quest quest)
        {
            if (quest == _extendedKillBatsQuest)
            {
                // Reset the extended quest to allow taking it again
                _extendedKillBatsQuest.ResetAfterTurnIn();
                
                // Remove from completed quests if it's there
                if (_completedQuests.Contains(_extendedKillBatsQuest))
                {
                    _completedQuests.Remove(_extendedKillBatsQuest);
                }
                
                System.Diagnostics.Debug.WriteLine("QuestManager: Extended quest reset after turn in - can be taken again");
            }
            else if (quest == _simpleKillBatsQuest)
            {
                // Reset the simple quest to allow taking it again
                _simpleKillBatsQuest.ResetAfterTurnIn();
                
                // Remove from completed quests if it's there
                if (_completedQuests.Contains(_simpleKillBatsQuest))
                {
                    _completedQuests.Remove(_simpleKillBatsQuest);
                }
                
                System.Diagnostics.Debug.WriteLine("QuestManager: Simple quest reset after turn in - can be taken again");
            }
        }

        public ExtendedKillBatsQuest GetExtendedKillBatsQuest()
        {
            System.Diagnostics.Debug.WriteLine($"QuestManager.GetExtendedKillBatsQuest: Returning {_extendedKillBatsQuest != null}");
            return _extendedKillBatsQuest;
        }

        public SimpleKillBatsQuest GetSimpleKillBatsQuest()
        {
            System.Diagnostics.Debug.WriteLine($"QuestManager.GetSimpleKillBatsQuest: Returning {_simpleKillBatsQuest != null}");
            return _simpleKillBatsQuest;
        }

        public bool CanTakeKillBatsQuest()
        {
            return _extendedKillBatsQuest.CanBeTaken();
        }

        public bool CanTakeSimpleKillBatsQuest()
        {
            return _simpleKillBatsQuest.CanBeTaken();
        }

        public bool CanRefuseKillBatsQuest()
        {
            return _extendedKillBatsQuest.CanBeRefused();
        }

        public bool CanRefuseSimpleKillBatsQuest()
        {
            return _simpleKillBatsQuest.CanBeRefused();
        }
        
        // Automatically assign return quest when main quest is completed
        private void AutoAssignReturnQuest(Quest originalQuest)
        {
            if (_returnToMayorQuest != null)
            {
                // Remove existing return quest if any
                _activeQuests.Remove(_returnToMayorQuest);
                _completedQuests.Remove(_returnToMayorQuest);
            }
            
            _returnToMayorQuest = new ReturnToMayorQuest(originalQuest);
            _returnToMayorQuest.Start();
            _activeQuests.Add(_returnToMayorQuest);
            
            System.Diagnostics.Debug.WriteLine($"QuestManager: Auto-assigned return quest for: {originalQuest.Title}");
            OnReturnQuestAutoAssigned?.Invoke(_returnToMayorQuest);
        }
        
        // Complete the return quest when player interacts with Mayor
        public void CompleteReturnQuest()
        {
            if (_returnToMayorQuest != null && _returnToMayorQuest.IsActive && !_returnToMayorQuest.IsCompleted)
            {
                _returnToMayorQuest.Complete();
                _activeQuests.Remove(_returnToMayorQuest);
                _completedQuests.Add(_returnToMayorQuest);
                
                System.Diagnostics.Debug.WriteLine($"QuestManager: Return quest completed");
                OnReturnQuestCompleted?.Invoke(_returnToMayorQuest);
            }
        }
        
        // Get the current return quest if any
        public ReturnToMayorQuest GetReturnQuest()
        {
            return _returnToMayorQuest;
        }
        
        // Check if player has a completed quest that needs to be turned in
        public bool HasCompletedQuestToTurnIn()
        {
            return (_extendedKillBatsQuest != null && _extendedKillBatsQuest.IsCompleted && !_extendedKillBatsQuest.IsTurnedIn) ||
                   (_simpleKillBatsQuest != null && _simpleKillBatsQuest.IsCompleted && !_simpleKillBatsQuest.IsTurnedIn);
        }
        
        // Check if Pebble is defeated (for extended quest availability)
        public bool IsPebbleDefeated()
        {
            return _isPebbleDefeated;
        }
        
        // Check if extended quest was ever completed (for simple quest availability)
        public bool WasExtendedQuestEverCompleted()
        {
            return _extendedQuestWasEverCompleted;
        }
        
        // Helper method to extract property values from anonymous objects
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

        public string GetActiveQuestProgress()
        {
            if (_activeQuests.Count > 0)
            {
                return _activeQuests[0].GetProgressText();
            }
            return "No active quests";
        }


        
        // Clear all quests and reset their states
        public void ClearQuests()
        {
            _activeQuests.Clear();
            _completedQuests.Clear();
            _refusedQuests.Clear();
            _returnToMayorQuest = null;
            
            // Reset quest states to allow taking them again
            if (_extendedKillBatsQuest != null)
            {
                _extendedKillBatsQuest.Reset();
            }
            if (_simpleKillBatsQuest != null)
            {
                _simpleKillBatsQuest.Reset();
            }
            
            // Reset quest completion history after death/restart
            _extendedQuestWasEverCompleted = false;
            _isPebbleDefeated = false;
        }
        
        // Add a quest to the active quests
        public void AddQuest(Quest quest)
        {
            if (quest != null && !_activeQuests.Contains(quest))
            {
                _activeQuests.Add(quest);
                OnQuestStarted?.Invoke(quest);
            }
        }
        
        // Get all quests for save system
        public List<Quest> GetAllQuests()
        {
            var allQuests = new List<Quest>();
            allQuests.AddRange(_activeQuests);
            allQuests.AddRange(_completedQuests);
            allQuests.AddRange(_refusedQuests);
            allQuests.Add(_extendedKillBatsQuest);
            if (_returnToMayorQuest != null)
            {
                allQuests.Add(_returnToMayorQuest);
            }
            return allQuests;
        }
        
        // Get quest by ID for save system
        public Quest GetQuestById(string questId)
        {
            var allQuests = GetAllQuests();
            return allQuests.FirstOrDefault(q => q is ISaveable saveable && saveable.SaveId == questId);
        }
        
        // ISaveable implementation
        public string SaveId => "QuestManager";
        
        public SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            
            // Save quest lists
            var activeQuestIds = _activeQuests.Where(q => q is ISaveable).Select(q => ((ISaveable)q).SaveId).ToList();
            var completedQuestIds = _completedQuests.Where(q => q is ISaveable).Select(q => ((ISaveable)q).SaveId).ToList();
            var refusedQuestIds = _refusedQuests.Where(q => q is ISaveable).Select(q => ((ISaveable)q).SaveId).ToList();
            
            data.SetValue("ActiveQuestIds", activeQuestIds);
            data.SetValue("CompletedQuestIds", completedQuestIds);
            data.SetValue("RefusedQuestIds", refusedQuestIds);
            data.SetValue("ExtendedQuestWasEverCompleted", _extendedQuestWasEverCompleted);
            data.SetValue("IsPebbleDefeated", _isPebbleDefeated);
            
            return data;
        }
        
        public void Deserialize(SaveData data)
        {
            if (data == null) return;
            
            // Clear current quest lists
            _activeQuests.Clear();
            _completedQuests.Clear();
            _refusedQuests.Clear();
            
            // Restore quest lists
            var activeQuestIds = data.GetValue<List<string>>("ActiveQuestIds", new List<string>());
            var completedQuestIds = data.GetValue<List<string>>("CompletedQuestIds", new List<string>());
            var refusedQuestIds = data.GetValue<List<string>>("RefusedQuestIds", new List<string>());
            
            // Restore quest flags
            _extendedQuestWasEverCompleted = data.GetValue<bool>("ExtendedQuestWasEverCompleted", false);
            _isPebbleDefeated = data.GetValue<bool>("IsPebbleDefeated", false);
            
            // Restore quests to appropriate lists
            foreach (var questId in activeQuestIds)
            {
                var quest = GetQuestById(questId);
                if (quest != null)
                {
                    _activeQuests.Add(quest);
                }
            }
            
            foreach (var questId in completedQuestIds)
            {
                var quest = GetQuestById(questId);
                if (quest != null)
                {
                    _completedQuests.Add(quest);
                }
            }
            
            foreach (var questId in refusedQuestIds)
            {
                var quest = GetQuestById(questId);
                if (quest != null)
                {
                    _refusedQuests.Add(quest);
                }
            }
        }
    }
} 