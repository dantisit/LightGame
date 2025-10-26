using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System;

namespace Core._.UI
{
    [CustomEditor(typeof(TweenableBase), true)]
    public class TweenableEditor : Editor
    {
        private float previewTime = 0f;
        private bool isPreviewing = false;
        private bool isLooping = false;
        private bool playBackwards = false;
        private double lastUpdateTime = 0;
        
        void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            Selection.selectionChanged += OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            Undo.undoRedoPerformed += OnUndoRedo;
            
            CleanupPreview();
        }
        
        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            Selection.selectionChanged -= OnSelectionChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            Undo.undoRedoPerformed -= OnUndoRedo;
            
            CleanupPreview();
        }
        
        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || 
                state == PlayModeStateChange.EnteredPlayMode)
            {
                CleanupPreview();
            }
        }
        
        void OnSelectionChanged()
        {
            CleanupPreview();
        }
        
        void OnBeforeAssemblyReload()
        {
            CleanupPreview();
        }
        
        void OnUndoRedo()
        {
            if (isPreviewing)
            {
                CleanupPreview();
            }
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            TweenableBase tweenableTarget = (TweenableBase)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);
            
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Preview is disabled in Play Mode.", MessageType.Info);
                return;
            }
            
            isLooping = EditorGUILayout.Toggle("Loop", isLooping);
            playBackwards = EditorGUILayout.Toggle("Play Backwards", playBackwards);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button(isPreviewing ? "Stop" : "Play"))
            {
                if (isPreviewing) StopPreview();
                else StartPreview();
            }
            
            if (GUILayout.Button("Reset"))
            {
                ResetPreview();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Initialize tween if needed (with auto-kill disabled)
            if (tweenableTarget.Tween == null || !tweenableTarget.Tween.IsActive())
            {
                InitializeTween(tweenableTarget);
            }
            
            // Preview slider
            if (tweenableTarget.Tween != null)
            {
                EditorGUI.BeginChangeCheck();
                previewTime = EditorGUILayout.Slider("Preview Time", previewTime, 0f, tweenableTarget.Tween.Duration());
                if (EditorGUI.EndChangeCheck())
                {
                    try
                    {
                        tweenableTarget.Tween.Goto(previewTime);
                        SceneView.RepaintAll();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error during preview: {e.Message}");
                        CleanupPreview();
                    }
                }
            }
        }
        
        void InitializeTween(TweenableBase tweenableTarget)
        {
            try
            {
                tweenableTarget.Tween?.Kill();
                tweenableTarget.Tween = tweenableTarget.CreateTween();
                
                // Key: Don't auto-kill so we can rewind later
                tweenableTarget.Tween.SetAutoKill(false);
                tweenableTarget.Tween.SetLoops(1);
                tweenableTarget.Tween.Pause(); // Start paused
            }
            catch (Exception e)
            {
                Debug.LogError($"Error initializing tween: {e.Message}");
            }
        }
        
        void StartPreview()
        {
            try
            {
                TweenableBase tweenableTarget = (TweenableBase)target;
                
                // Ensure tween is initialized
                InitializeTween(tweenableTarget);
                
                isPreviewing = true;
                previewTime = playBackwards ? tweenableTarget.Tween.Duration() : 0f;
                lastUpdateTime = EditorApplication.timeSinceStartup;
                EditorApplication.update += UpdatePreview;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error starting preview: {e.Message}");
                CleanupPreview();
            }
        }
        
        void StopPreview()
        {
            isPreviewing = false;
            EditorApplication.update -= UpdatePreview;
            
            // Revert to initial state using DOTween
            RevertTween();
        }
        
        void ResetPreview()
        {
            try
            {
                StopPreview();
                SceneView.lastActiveSceneView?.Repaint();
                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error resetting preview: {e.Message}");
                CleanupPreview();
            }
        }
        
        void UpdatePreview()
        {
            if (!isPreviewing) return;
            
            try
            {
                var tweenableTarget = (TweenableBase)target;
                var tween = tweenableTarget?.Tween;
                if (tween == null || !tween.IsActive()) return;
                
                double currentTime = EditorApplication.timeSinceStartup;
                float deltaTime = (float)(currentTime - lastUpdateTime);
                lastUpdateTime = currentTime;
                
                if (playBackwards)
                {
                    previewTime -= deltaTime;
                    if (previewTime < 0f)
                    {
                        previewTime = isLooping ? tween.Duration() : 0f;
                       // if (!isLooping) StopPreview();
                    }
                }
                else
                {
                    previewTime += deltaTime;
                    if (previewTime > tween.Duration())
                    {
                        previewTime = isLooping ? 0f : tween.Duration();
                       // if (!isLooping) StopPreview();
                    }
                }
                
                tween.GotoWithCallbacks(previewTime);
                SceneView.lastActiveSceneView?.Repaint();
                Repaint();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during preview update: {e.Message}");
                CleanupPreview();
            }
        }
        
        void RevertTween()
        {
            try
            {
                TweenableBase tweenableTarget = (TweenableBase)target;
                if (tweenableTarget != null && tweenableTarget.Tween != null && tweenableTarget.Tween.IsActive())
                {
                    // Rewind reverts all tweened values to their starting state
                    tweenableTarget.Tween.Rewind();
                    previewTime = 0f;
                    SceneView.lastActiveSceneView?.Repaint();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reverting tween: {e.Message}");
            }
        }
        
        void CleanupPreview()
        {
            try
            {
                isPreviewing = false;
                EditorApplication.update -= UpdatePreview;
                
                // Revert using DOTween
                RevertTween();
                
                // Kill the tween
                TweenableBase tweenableTarget = (TweenableBase)target;
                if (tweenableTarget != null && tweenableTarget.Tween != null)
                {
                    tweenableTarget.Tween.Kill();
                    tweenableTarget.Tween = null;
                }
                
                previewTime = 0f;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during cleanup: {e.Message}");
            }
        }
    }
}