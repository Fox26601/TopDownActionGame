using System;
using System.Collections.Generic;

namespace IsometricActionGame.Quests
{
    public class QuestObjective
    {
        public string Name { get; set; }
        public string EnemyType { get; set; }
        public int Required { get; set; }
        public int Current { get; set; }
        public bool IsOptional { get; set; }

        public QuestObjective(string name, string enemyType, int required, bool isOptional = false)
        {
            Name = name;
            EnemyType = enemyType;
            Required = required;
            Current = 0;
            IsOptional = isOptional;
        }

        public bool IsCompleted => Current >= Required;
        public string GetProgressText() => $"{Name}: {Current}/{Required}";
    }

    public class ExtendedKillBatsQuest : Quest
    {
        private List<QuestObjective> _objectives;
        public int GoldReward { get; private set; }
        public bool HasPebbleObjective { get; private set; }

        public ExtendedKillBatsQuest(int batKills = 5, int goldReward = 5) 
            : base("Bat Extermination", "Kill bats to protect the village")
        {
            GoldReward = goldReward;
            _objectives = new List<QuestObjective>
            {
                new QuestObjective("Kill Bats", "Bat", batKills, false)
            };
            HasPebbleObjective = false;
        }

        public void AddPebbleObjective()
        {
            if (!HasPebbleObjective)
            {
                _objectives.Add(new QuestObjective("Kill Pebble", "Pebble", 1, true));
                HasPebbleObjective = true;
                Description = "Kill bats and defeat the Pebble to protect the village";
            }
        }

        public override string GetProgressText()
        {
            var progressTexts = new List<string>();
            foreach (var objective in _objectives)
            {
                progressTexts.Add(objective.GetProgressText());
            }
            return string.Join(" | ", progressTexts);
        }

        public override void UpdateProgress(object data)
        {
            if (data is string enemyType)
            {
                System.Diagnostics.Debug.WriteLine($"ExtendedKillBatsQuest: UpdateProgress called with enemyType: {enemyType}");
                
                foreach (var objective in _objectives)
                {
                    if (objective.EnemyType == enemyType)
                    {
                        objective.Current++;
                        System.Diagnostics.Debug.WriteLine($"ExtendedKillBatsQuest: Updated {objective.Name} progress: {objective.Current}/{objective.Required}");
                        break;
                    }
                }

                // Check if all required objectives are completed
                bool allRequiredCompleted = true;
                foreach (var objective in _objectives)
                {
                    if (!objective.IsOptional && !objective.IsCompleted)
                    {
                        allRequiredCompleted = false;
                        System.Diagnostics.Debug.WriteLine($"ExtendedKillBatsQuest: Required objective {objective.Name} not completed: {objective.Current}/{objective.Required}");
                        break;
                    }
                }

                if (allRequiredCompleted)
                {
                    System.Diagnostics.Debug.WriteLine("ExtendedKillBatsQuest: All required objectives completed! Calling Complete()");
                    Complete();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            foreach (var objective in _objectives)
            {
                objective.Current = 0;
            }
            HasPebbleObjective = false;
            // Remove Pebble objective if it was added
            _objectives.RemoveAll(obj => obj.EnemyType == "Pebble");
        }

        public void IncreaseReward(int additionalGold)
        {
            GoldReward += additionalGold;
        }


    }
}

