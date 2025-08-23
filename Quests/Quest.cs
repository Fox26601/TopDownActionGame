using System;
using IsometricActionGame.SaveSystem;

namespace IsometricActionGame.Quests
{
    public abstract class Quest : ISaveable
    {
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public bool IsActive { get; protected set; } = false;
        public bool IsCompleted { get; protected set; } = false;
        public bool IsTurnedIn { get; protected set; } = false;
        public bool IsRefused { get; protected set; } = false;

        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestTurnedIn;
        public event Action<Quest> OnQuestRefused;

        protected Quest(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public virtual void Start()
        {
            IsActive = true;
            IsCompleted = false;
            IsTurnedIn = false;
            IsRefused = false;
        }

        public virtual void Refuse()
        {
            if (IsActive && !IsCompleted && !IsTurnedIn)
            {
                IsActive = false;
                IsRefused = true;
                OnQuestRefused?.Invoke(this);
            }
        }

        public virtual void Complete()
        {
            if (IsActive && !IsCompleted)
            {
                IsCompleted = true;
                System.Diagnostics.Debug.WriteLine($"Quest: {Title} completed! IsActive: {IsActive}, IsCompleted: {IsCompleted}");
                OnQuestCompleted?.Invoke(this);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Quest: {Title} cannot be completed. IsActive: {IsActive}, IsCompleted: {IsCompleted}");
            }
        }

        public virtual void TurnIn()
        {
            if (IsCompleted && !IsTurnedIn)
            {
                IsTurnedIn = true;
                IsActive = false;
                OnQuestTurnedIn?.Invoke(this);
            }
        }
        
        /// <summary>
        /// Reset quest after turning in to allow taking it again
        /// </summary>
        public virtual void ResetAfterTurnIn()
        {
            if (IsTurnedIn)
            {
                IsTurnedIn = false;
                IsCompleted = false;
                IsActive = false;
                IsRefused = false;
                System.Diagnostics.Debug.WriteLine($"Quest: {Title} reset after turn in - can be taken again");
            }
        }

        public virtual void Reset()
        {
            IsActive = false;
            IsCompleted = false;
            IsTurnedIn = false;
            IsRefused = false;
        }

        public bool CanBeTaken()
        {
            return !IsActive && !IsTurnedIn;
        }

        public bool CanBeRefused()
        {
            return IsActive && !IsCompleted && !IsTurnedIn;
        }

        public abstract string GetProgressText();
        public abstract void UpdateProgress(object data);
        
        // ISaveable implementation
        public virtual string SaveId => $"Quest_{Title.Replace(" ", "_")}";
        
        public virtual SaveData Serialize()
        {
            var data = new SaveData(SaveId);
            data.SetValue("Title", Title);
            data.SetValue("Description", Description);
            data.SetValue("IsActive", IsActive);
            data.SetValue("IsCompleted", IsCompleted);
            data.SetValue("IsTurnedIn", IsTurnedIn);
            data.SetValue("IsRefused", IsRefused);
            return data;
        }
        
        public virtual void Deserialize(SaveData data)
        {
            if (data == null) return;
            
            Title = data.GetValue<string>("Title", Title);
            Description = data.GetValue<string>("Description", Description);
            IsActive = data.GetValue<bool>("IsActive", IsActive);
            IsCompleted = data.GetValue<bool>("IsCompleted", IsCompleted);
            IsTurnedIn = data.GetValue<bool>("IsTurnedIn", IsTurnedIn);
            IsRefused = data.GetValue<bool>("IsRefused", IsRefused);
        }
    }
} 