using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;
#endif

public class AnySubstateTranistionController_SMB : StateMachineBehaviour
{
    [SerializeField][HideInInspector] private TransitionInfo[] bakedTransitions;
    [SerializeField][HideInInspector] private string bakedTransitionsJson; // JSON backup for builds
    
    private TransitionInfo[] cachedTransitions; // Runtime cache
    private bool hasValidated = false;
    private bool inTransition;

#if UNITY_EDITOR
    [SerializeField] private List<TransitionConfig> config = new List<TransitionConfig>();

    [ContextMenu("Rebake Transitions")]
    public void ForceBake() 
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogWarning("Cannot bake transitions during play mode!");
            return;
        }
        BakeTransitions();
    }
    
    //for some reason the needed function to grab the direct parent is internal so need to use reflection to find it
    private static AnimatorStateMachine FindParentStateMacihine(AnimatorStateMachine root, AnimatorStateMachine child)
    {
        var type = typeof(AnimatorStateMachine);
        var method_FindParent = type.GetMethod("FindParent", BindingFlags.Instance | BindingFlags.NonPublic);
        var parent = (AnimatorStateMachine)method_FindParent.Invoke(root, new object[] { child });
        return parent;
    }

    public void BakeTransitions()
    {
        var map = config.ToDictionary(c => c.editorReference, c => c.transition);
        var newMap = new Dictionary<AnimatorTransition, TransitionInfo>();

        var contexts = AnimatorController.FindStateMachineBehaviourContext(this);
        foreach (var c in contexts)
        {
            if (!(c.animatorObject is AnimatorStateMachine asm))
            {
                continue;
            }
            var rootMachine = c.animatorController.layers[c.layerIndex].stateMachine;
            var parent = FindParentStateMacihine(rootMachine, asm);

            var allExitTransitions = parent.GetStateMachineTransitions(asm);

            foreach(var t in allExitTransitions)
            {
                if (!map.TryGetValue(t, out var oldInfo))
                {
                    oldInfo = TransitionInfo.Create(t, c.animatorController, c.layerIndex);
                }
                else
                {
                    oldInfo = TransitionInfo.Create(t, c.animatorController, c.layerIndex, oldInfo.crossfadeDuration);
                }

                if(oldInfo.conditons.Length > 0)
                {
                    oldInfo.name = t.GetDisplayName(asm);
                    newMap[t] = oldInfo;
                }
            }
        }

        config = newMap.Select(kvp => new TransitionConfig() 
        { 
            name = kvp.Value.name, 
            editorReference = kvp.Key, 
            transition = kvp.Value
        }).ToList();

        bakedTransitions = newMap.Values.ToArray();
        
        // ✅ Serialize to JSON for reliable build serialization
        var wrapper = new TransitionDataWrapper { transitions = bakedTransitions };
        bakedTransitionsJson = JsonUtility.ToJson(wrapper, false);
        
        EditorUtility.SetDirty(this);
        foreach (var c in contexts)
        {
            if (c.animatorController != null)
            {
                EditorUtility.SetDirty(c.animatorController);
            }
        }

        AssetDatabase.SaveAssets();
        
        Debug.Log($"✓ Baked {bakedTransitions.Length} transitions to JSON ({bakedTransitionsJson.Length} chars)");
    }
#endif

    #region StateMachineBehaviour
    
    private TransitionInfo[] GetTransitions()
    {
        // Return cached if available
        if (cachedTransitions != null) 
            return cachedTransitions;

#if UNITY_EDITOR
        // In editor, prefer the direct array
        cachedTransitions = bakedTransitions;
#else
        // In builds, deserialize from JSON (more reliable)
        if (!string.IsNullOrEmpty(bakedTransitionsJson))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<TransitionDataWrapper>(bakedTransitionsJson);
                cachedTransitions = wrapper.transitions;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize transitions from JSON: {e.Message}");
                cachedTransitions = bakedTransitions; // Fallback to direct array
            }
        }
        else
        {
            // No JSON, try direct array
            cachedTransitions = bakedTransitions;
        }
#endif

        return cachedTransitions;
    }
    
    private void ValidateTransitions(Animator animator)
    {
        if (hasValidated) return;
        hasValidated = true;

        var transitions = GetTransitions();
        
        if (transitions == null)
        {
            Debug.LogError($"[{animator.name}] bakedTransitions is NULL! Serialization completely failed.", animator);
            return;
        }
        
        if (transitions.Length == 0)
        {
            Debug.LogWarning($"[{animator.name}] No transitions baked. Use 'Rebake Transitions' in the inspector.", animator);
            return;
        }
        
        // Validate nested arrays
        for (int i = 0; i < transitions.Length; i++)
        {
            if (transitions[i].conditons == null)
            {
                Debug.LogError($"[{animator.name}] Transition {i} '{transitions[i].name}' has NULL conditions! Nested serialization failed.", animator);
                return;
            }
        }
        
        Debug.Log($"✓ [{animator.name}] Validated {transitions.Length} transitions successfully", animator);
    }
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ValidateTransitions(animator);
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (inTransition)
        {
            inTransition = false;
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (inTransition)
        {
            return;
        }

        var transitions = GetTransitions();
        if (transitions == null || transitions.Length == 0)
        {
            return;
        }

        for (int i = 0; i < transitions.Length; i++)
        {
            var info = transitions[i];

            if (info.TryTransition(animator, stateInfo, false, layerIndex))
            {
                inTransition = true;
                return;
            }
        }
    }
    #endregion

    #region typedefs
    
    // Wrapper class for JSON serialization
    [Serializable]
    [Preserve]
    private class TransitionDataWrapper
    {
        public TransitionInfo[] transitions;
    }

#if UNITY_EDITOR
    [Serializable]
    [Preserve]
    private class TransitionConfig
    {
        [SerializeField][HideInInspector] public string name;
        [SerializeField][HideInInspector] public AnimatorTransition editorReference;
        [SerializeField] public TransitionInfo transition;
    }
#endif

    [Serializable]
    [Preserve]
    private struct TransitionInfo
    {
        [SerializeField] public string name;
        [SerializeField] public int destinationStateHash;
        [SerializeField] public float crossfadeDuration;
        [SerializeField] public TransitionCondition[] conditons;

#if UNITY_EDITOR
        public static TransitionInfo Create(AnimatorTransition data, AnimatorController controller, int layer, float crossfadeDuration = 0.25f)
        {
            var targetState = data.destinationState ?? data.destinationStateMachine?.defaultState ?? controller.layers[layer].stateMachine.defaultState;

            var conditions = data.conditions.Select(c => TransitionCondition.Create(c, controller));

            return new TransitionInfo()
            {
                name = string.Empty,
                destinationStateHash = targetState.nameHash,
                crossfadeDuration = crossfadeDuration,
                conditons = conditions.ToArray()
            };
        }
#endif

        public bool TryTransition(Animator animator, AnimatorStateInfo stateInfo, bool isExit, int layer)
        {
            if (stateInfo.fullPathHash == destinationStateHash) return false;

            for (int i = 0; i < conditons.Length; i++)
            {
                if (!conditons[i].Evaluate(animator)) return false;
            }

            if (conditons.Length < 1 && !isExit)
                return false;

            for (int i = 0; i < conditons.Length; i++)
                conditons[i].Consume(animator);

            animator.CrossFadeInFixedTime(destinationStateHash, crossfadeDuration, layer);
            return true;
        }
    }

    [Serializable]
    [Preserve]
    private struct TransitionCondition
    {
        /// <summary>
        /// the type of condition to eval, this is a mirror of UnityEditor.Animations.AnimatorConditionMode
        /// </summary>
        public enum Condition
        {
            If = 1,
            IfNot = 2,
            Greater = 3,
            Less = 4,
            Equals = 6,
            NotEqual = 7
        }
        
        public enum DataType { Float = 1, Int = 3, Bool = 4, Trigger = 9 }

        [SerializeField] public DataType dataType;
        [SerializeField] public int paramHash;
        [SerializeField] public string parameter;
        [SerializeField] public Condition mode;
        [SerializeField] public float threshold;

#if UNITY_EDITOR
        public static TransitionCondition Create(AnimatorCondition data, AnimatorController editorAnimator)
        {
            var parameters = editorAnimator.parameters;
            AnimatorControllerParameter parameter = default;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].nameHash == Animator.StringToHash(data.parameter))
                {
                    parameter = parameters[i];
                    break;
                }
            }

            return new TransitionCondition()
            {
                dataType = (DataType)parameter.type,
                paramHash = parameter.nameHash,
                parameter = parameter.name,
                mode = (Condition)data.mode,
                threshold = data.threshold
            };
        }
#endif

        public bool Evaluate(Animator animator)
        {
            switch (dataType)
            {
                case DataType.Float:
                    var floatValue = animator.GetFloat(paramHash);
                    switch (mode)
                    {
                        case Condition.Greater:
                            return floatValue > threshold;
                        case Condition.Less:
                            return floatValue < threshold;
                        case Condition.Equals:
                            return floatValue == threshold;
                        case Condition.NotEqual:
                            return floatValue != threshold;
                        default: return false;
                    }
                    
                case DataType.Int:
                    var intValue = animator.GetInteger(paramHash);
                    switch (mode)
                    {
                        case Condition.Greater:
                            return intValue > threshold;
                        case Condition.Less:
                            return intValue < threshold;
                        case Condition.Equals:
                            return intValue == threshold;
                        case Condition.NotEqual:
                            return intValue != threshold;
                        default: return false;
                    }
                    
                case DataType.Bool:
                    var boolValue = animator.GetBool(paramHash);
                    switch (mode)
                    {
                        case Condition.If:
                            return boolValue;
                        case Condition.IfNot:
                            return !boolValue;
                        default:
                            return false;
                    }
                    
                case DataType.Trigger:
                    var triggerValue = animator.GetBool(paramHash);
                    return triggerValue;
                    
                default: return false;
            }
        }

        public void Consume(Animator animator)
        {
            if (dataType == DataType.Trigger)
            {
                animator.ResetTrigger(paramHash);
            }
        }
    }
    #endregion
}