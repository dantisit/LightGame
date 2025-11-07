using System;
using UnityEngine;

namespace Light_and_controller.Scripts.AbilitiesSystem
{
    public enum UnlockBehavior
    {
        UnlockAtLevel,      // Unlocks at specific level and stays unlocked
        AlwaysUnlocked,     // Always active, unlocked from start
        TemporaryUnlock     // Unlocks at level but can be locked again
    }
    
    public abstract class LevelAbility : ScriptableObject
    {
        public SceneName unlockAtLevel;
        public UnlockBehavior unlockBehavior = UnlockBehavior.UnlockAtLevel;
        
        public abstract void Activate(GameObject target);
        public abstract void Deactivate(GameObject target);
    }
}