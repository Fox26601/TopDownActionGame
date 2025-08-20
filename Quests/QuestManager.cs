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

        public IReadOnlyList<Quest> ActiveQuests => _activeQuests.AsReadOnly();
        public IReadOnlyList<Quest> CompletedQuests => _completedQuests.AsReadOnly();
        public IReadOnlyList<Quest> RefusedQuests => _refusedQuests.AsReadOnly();

        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestTurnedIn;
        public event Action<Quest> OnQuestRefused;

        public QuestManager()
        {
            _activeQuests = new List<Quest>();
            _completedQuests = new List<Quest>();
            _refusedQuests = new List<Quest>();
            _extendedKillBatsQuest = new ExtendedKillBatsQuest();
            
            // Subscribe to quest completion events to automatically move completed quests
            _extendedKillBatsQuest.OnQuestCompleted += (quest) => CompleteQuest(quest);
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
                OnQuestCompleted?.Invoke(quest);
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
            }
        }

        public ExtendedKillBatsQuest GetExtendedKillBatsQuest()
        {
            return _extendedKillBatsQuest;
        }

        public bool CanTakeKillBatsQuest()
        {
            return _extendedKillBatsQuest.CanBeTaken();
        }

        public bool CanRefuseKillBatsQuest()
        {
            return _extendedKillBatsQuest.CanBeRefused();
        }

        public string GetActiveQuestProgress()
        {
            if (_activeQuests.Count > 0)
            {
                return _activeQuests[0].GetProgressText();
            }
            return "No active quests";
        }


        
        /// <summary>
        /// Clear all quests
        /// </summary>
        public void ClearQuests()
        {
            _activeQuests.Clear();
            _completedQuests.Clear();
            _refusedQuests.Clear();
        }
        
        /// <summary>
        /// Add a quest to the active quests
        /// </summary>
        public void AddQuest(Quest quest)
        {
            if (quest != null && !_activeQuests.Contains(quest))
            {
                _activeQuests.Add(quest);
                OnQuestStarted?.Invoke(quest);
            }
        }
        
        /// <summary>
        /// Get all quests for save system
        /// </summary>
        public List<Quest> GetAllQuests()
        {
            var allQuests = new List<Quest>();
            allQuests.AddRange(_activeQuests);
            allQuests.AddRange(_completedQuests);
            allQuests.AddRange(_refusedQuests);
            allQuests.Add(_extendedKillBatsQuest);
            return allQuests;
        }
        
        /// <summary>
        /// Get quest by ID for save system
        /// </summary>
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