using System;
using IsometricActionGame.Quests;

namespace IsometricActionGame.Events.EventData
{
    /// <summary>
    /// Event data for quest-related events with reward information
    /// </summary>
    public class QuestEventArgs : BaseEventData
    {
        public Quest Quest { get; }
        public int GoldReward { get; }
        public string QuestType { get; }
        public bool HasPebbleObjective { get; }

        public QuestEventArgs(Quest quest, int goldReward, string questType = null, bool hasPebbleObjective = false)
        {
            Quest = quest ?? throw new ArgumentNullException(nameof(quest));
            GoldReward = goldReward;
            QuestType = questType ?? quest.GetType().Name;
            HasPebbleObjective = hasPebbleObjective;
        }

        public string EventType => "QuestEvent";
    }
} 