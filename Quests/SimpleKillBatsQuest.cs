using System;

namespace IsometricActionGame.Quests
{
    /// <summary>
    /// Simple repeatable quest to kill 5 bats without any additional objectives
    /// Used for repeat quest attempts after the extended quest was completed
    /// </summary>
    public class SimpleKillBatsQuest : Quest
    {
        private int _batKills;
        private int _requiredBats;
        public int GoldReward { get; private set; }

        public SimpleKillBatsQuest(int requiredBats = 5, int goldReward = 5) 
            : base("Simple Bat Extermination", "Kill bats to protect the village")
        {
            _requiredBats = requiredBats;
            _batKills = 0;
            GoldReward = goldReward;
        }

        public override string GetProgressText()
        {
            return $"Kill Bats: {_batKills}/{_requiredBats}";
        }

        public override void UpdateProgress(object data)
        {
            if (data is string enemyType && enemyType == "Bat")
            {
                _batKills++;
                System.Diagnostics.Debug.WriteLine($"SimpleKillBatsQuest: Bat killed, progress: {_batKills}/{_requiredBats}");
                
                if (_batKills >= _requiredBats)
                {
                    Complete();
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            _batKills = 0;
        }

        /// <summary>
        /// Reset quest after turning in to allow taking it again
        /// </summary>
        public override void ResetAfterTurnIn()
        {
            if (IsTurnedIn)
            {
                base.ResetAfterTurnIn();
                _batKills = 0;
                System.Diagnostics.Debug.WriteLine($"SimpleKillBatsQuest: Reset after turn in - can be taken again");
            }
        }

        public override string SaveId => "SimpleKillBatsQuest";

        public override SaveSystem.SaveData Serialize()
        {
            var data = base.Serialize();
            data.SetValue("BatKills", _batKills);
            data.SetValue("RequiredBats", _requiredBats);
            data.SetValue("GoldReward", GoldReward);
            return data;
        }

        public override void Deserialize(SaveSystem.SaveData data)
        {
            base.Deserialize(data);
            _batKills = data.GetValue<int>("BatKills");
            _requiredBats = data.GetValue<int>("RequiredBats");
            GoldReward = data.GetValue<int>("GoldReward");
        }
    }
}

