using System;
using IsometricActionGame.SaveSystem;

namespace IsometricActionGame.Quests
{
    /// <summary>
    /// Automatic quest that is assigned when a main quest is completed
    /// This quest represents the task to return to the Mayor for reward
    /// </summary>
    public class ReturnToMayorQuest : Quest
    {
        private Quest _originalQuest;
        private int _goldReward;
        private string _questType;
        private bool _hasPebbleObjective;
        
        public Quest OriginalQuest => _originalQuest;
        public int GoldReward => _goldReward;
        public string QuestType => _questType;
        public bool HasPebbleObjective => _hasPebbleObjective;
        
        public ReturnToMayorQuest(Quest originalQuest) 
            : base("Return to Mayor", $"Return to the Mayor to claim your reward for: {originalQuest?.Title ?? "Unknown Quest"}")
        {
            _originalQuest = originalQuest ?? throw new ArgumentNullException(nameof(originalQuest));
            
            // Extract reward information from original quest
            if (originalQuest is ExtendedKillBatsQuest extendedQuest)
            {
                _goldReward = extendedQuest.GoldReward;
                _questType = "ExtendedKillBatsQuest";
                _hasPebbleObjective = extendedQuest.HasPebbleObjective;
            }
            else if (originalQuest is SimpleKillBatsQuest simpleQuest)
            {
                _goldReward = simpleQuest.GoldReward;
                _questType = "SimpleKillBatsQuest";
                _hasPebbleObjective = false;
            }
            else
            {
                _goldReward = 0;
                _questType = originalQuest.GetType().Name;
                _hasPebbleObjective = false;
            }
        }
        
        public override string GetProgressText()
        {
            return "Return to the Mayor to claim your reward";
        }
        
        public override void UpdateProgress(object data)
        {
            // This quest is completed when player interacts with Mayor
            // Progress is handled externally when Mayor dialogue is triggered
        }
        
        public override void Complete()
        {
            // Call base implementation which handles the event properly
            base.Complete();
        }
        
        public override void TurnIn()
        {
            // Call base implementation which handles the event properly
            base.TurnIn();
        }
        
        // ISaveable implementation
        public override string SaveId => $"ReturnToMayorQuest_{_originalQuest?.Title?.Replace(" ", "_") ?? "Unknown"}";
        
        public override SaveData Serialize()
        {
            var data = base.Serialize();
            data.SetValue("OriginalQuestId", _originalQuest?.SaveId ?? "");
            data.SetValue("GoldReward", _goldReward);
            data.SetValue("QuestType", _questType);
            data.SetValue("HasPebbleObjective", _hasPebbleObjective);
            return data;
        }
        
        public override void Deserialize(SaveData data)
        {
            base.Deserialize(data);
            _goldReward = data.GetValue<int>("GoldReward", 0);
            _questType = data.GetValue<string>("QuestType", "Unknown");
            _hasPebbleObjective = data.GetValue<bool>("HasPebbleObjective", false);

            // This is a limitation of the current save system
        }
    }
}
