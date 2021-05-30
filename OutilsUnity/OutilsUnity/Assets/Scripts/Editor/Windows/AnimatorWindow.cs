using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AnimatorWindow : EditorWindow
{
    #region Fields

    private List<Animator> _animators = new List<Animator>();
    private List<AnimationClip> _animationsClipsOnAnimator = new List<AnimationClip>();

    private Animator _selectedAnimator = null;
    private AnimationClip _selectedClip = null;

    private string _searchString;

    private float _lastEditorTime = 0f;
    private bool _isSimulatingAnimation = false;
    private float _animationTimer = 0f;

    private bool _canFocusOnAnimation = false;

    private bool _sampleLoopMode = false;
    private int _sampleLoopDelay = 0;

    private bool _sampleStepMode = false;
    private float _animationClipSampleStep = 0f;

    private bool _sampleSpeedMode = false;
    private float _animationClipSampleSpeed = 1f;

    private float _windowWidth = 0f;

    #region Style

    private const string START_SUBTITLE = "<color=white><i>";
    private const string END_SUBTITLE = "</i></color>";

    private const string START_TEXT = "<color=white>";
    private const string END_TEXT = "</color>";

    #endregion Style

    #endregion Fields

    [MenuItem("Custom Window/Animator Window")]
    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(AnimatorWindow));
        window.minSize = new Vector2(650, 650);
    }

    #region Unity Methods

    private void Awake()
    {
        FindAnimatorsInScene();

        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneLoaded();
        EditorSceneManager.sceneClosing += SceneClosing;
        EditorSceneManager.sceneOpened += SceneOpened;
    }

    private void OnValidate()
    {
        FindAnimatorsInScene();
    }

    private void OnPlayModeStateChange(PlayModeStateChange modeState)
    {
        if (modeState == PlayModeStateChange.ExitingEditMode)
        {
            StopAnimSimulation();
        }
    }

    private UnityAction<Scene, Scene> OnSceneLoaded()
    {
        FindAnimatorsInScene();

        return null;
    }

    private void SceneClosing(Scene scene, bool removingscene)
    {
        StopAnimSimulation();
        ResetAll();
    }

    private void SceneOpened(Scene scene, OpenSceneMode mode)
    {
        ResetAll();
        FindAnimatorsInScene();
    }

    #endregion Unity Methods

    private void OnGUI()
    {
        _windowWidth = position.width;

        DisplayAnimators();
        DisplayAnimations();
        DisplayAnimationsOptions();
        DisplayAnimationsButtons();
    }

    private void DrawSeparatorLine()
    {
        EditorGUILayout.Space(10);
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);
    }

    private void ResetAll()
    {
        _animators = new List<Animator>();
        _animationsClipsOnAnimator = new List<AnimationClip>();

        _selectedAnimator = null;
        _selectedClip = null;

        _searchString = "";

        _lastEditorTime = 0f;
        _isSimulatingAnimation = false;
        _animationTimer = 0f;

        _canFocusOnAnimation = false;

        ResetOptions();

        _windowWidth = 0f;
    }

    private void ResetOptions()
    {
        _sampleLoopMode = false;
        _sampleLoopDelay = 0;

        _sampleStepMode = false;
        _animationClipSampleStep = 0f;

        _sampleSpeedMode = false;
        _animationClipSampleSpeed = 0f;
    }

    #region Animator

    private void FindAnimatorsInScene()
    {
        ResetAll();

        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject go in rootGameObjects)
        {
            Animator[] toConcat = go.GetComponentsInChildren<Animator>(includeInactive: true);
            foreach (Animator animator in toConcat)
            {
                _animators.Add(animator);
            }
        }
    }

    private void DisplayAnimators()
    {
        if (_animators != null)
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            {
                _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    _searchString = "";
                    GUI.FocusControl(null);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Animator List", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            GUILayout.BeginVertical();
            {
                foreach (var animator in _animators)
                {
                    if (animator != null)
                    {
                        if (!string.IsNullOrEmpty(_searchString))
                        {
                            if (animator.name.ToLowerInvariant().Contains(_searchString.ToLowerInvariant()))
                            {
                                DisplayAnimatorButton(animator);
                            }
                            else
                            {
                                if (_selectedAnimator == animator)
                                {
                                    _selectedAnimator = null;
                                    _selectedClip = null;
                                    ResetOptions();
                                }
                            }
                        }
                        else
                        {
                            DisplayAnimatorButton(animator);
                        }
                    }
                }
            }
            GUILayout.EndVertical();
        }
    }

    private void DisplayAnimatorButton(Animator animator)
    {
        if (GUILayout.Button($"{animator.name}"))
        {
            EditorGUIUtility.PingObject(animator);

            SceneView.FrameLastActiveSceneView();
            Selection.activeGameObject = animator.gameObject;
            SceneView.FrameLastActiveSceneView();

            if (_selectedAnimator != animator)
            {
                _selectedClip = null;

                ResetOptions();
            }

            _selectedAnimator = animator;
            _animationsClipsOnAnimator = animator.runtimeAnimatorController.animationClips.ToList();
        }
    }

    #endregion Animator

    #region Animations

    private void ListAllAnimations()
    {
        int animationIndex = 0;

        foreach (AnimationClip animationClip in _animationsClipsOnAnimator)
        {
            if (GUILayout.Button($"Animation Clip {++animationIndex} : {animationClip.name}"))
            {
                _selectedClip = animationClip;

                ResetOptions();
            }
        }
    }

    private void DisplayAnimations()
    {
        if (_selectedAnimator != null)
        {
            DrawSeparatorLine();

            GUILayout.Label("Animations List", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            GUIStyle style = new GUIStyle();
            style.richText = true;

            EditorGUILayout.Space(5);
            GUILayout.Label($"{START_SUBTITLE}Selected Animator :{END_SUBTITLE} {START_TEXT + _selectedAnimator.name + END_TEXT}", style);
            EditorGUILayout.Space(5);
            GUILayout.Label($"{START_SUBTITLE}Selected Animation Clip :{END_SUBTITLE} {(_selectedClip != null ? START_TEXT + _selectedClip.name + END_TEXT : "<color=red>none</color>")}", style);
            EditorGUILayout.Space(5);

            EditorGUILayout.Space(5);
            GUILayout.Label($"{START_SUBTITLE}Animation Clip Duration :{END_SUBTITLE} {(_selectedClip != null ? START_TEXT + _selectedClip.length + "s" + END_TEXT : "")}", style);
            EditorGUILayout.Space(5);
            GUILayout.Label($"{START_SUBTITLE}Animation Clip Loop ?{END_SUBTITLE} {(_selectedClip != null ? START_TEXT + _selectedClip.isLooping + END_TEXT : "")}", style);
            EditorGUILayout.Space(5);

            ListAllAnimations();
        }
    }

    private void DisplayAnimationsOptions()
    {
        if (!Application.isPlaying && _selectedClip != null)
        {
            DrawSeparatorLine();

            GUILayout.Label("Animations Options", EditorStyles.boldLabel);

            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(_windowWidth/3));
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        _sampleLoopMode = GUILayout.Toggle(_sampleLoopMode, "Loop Options");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        if (_sampleLoopMode)
                        {
                            GUILayout.Label("Delay : ");
                            _sampleLoopDelay = EditorGUILayout.IntSlider(_sampleLoopDelay, 0, 30);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(_windowWidth / 3));
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        _sampleStepMode = GUILayout.Toggle(_sampleStepMode, "Step Option");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        if (_sampleStepMode)
                        {
                            GUILayout.Label("Frame : ");
                            _animationClipSampleStep = EditorGUILayout.Slider(_animationClipSampleStep, 0f, _selectedClip.length);

                            Selection.activeGameObject = _selectedAnimator.gameObject;
                            SceneView.lastActiveSceneView.FrameSelected(true, true);
                            _selectedClip.SampleAnimation(_selectedAnimator.gameObject, _animationClipSampleStep);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(_windowWidth / 3));
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        _sampleSpeedMode = GUILayout.Toggle(_sampleSpeedMode, "Speed Option");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(_windowWidth / 3));
                    {
                        if (_sampleSpeedMode)
                        {
                            GUILayout.Label("Speed : ");
                            _animationClipSampleSpeed = EditorGUILayout.Slider(_animationClipSampleSpeed, 0.1f, 5f);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
    
    private void DisplayAnimationsButtons()
    {
        if (!Application.isPlaying && _selectedClip != null)
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button($"Play {_selectedClip.name}"))
                {
                    if (!_isSimulatingAnimation)
                    {
                        StartAnimSimulation();
                    }
                    else
                    {
                        Debug.LogWarning("An animation is already currently in play !");
                    }
                }

                if (GUILayout.Button($"Stop {_selectedClip.name}"))
                {
                    if (_isSimulatingAnimation)
                    {
                        StopAnimSimulation();
                    }
                    else
                    {
                        Debug.LogWarning("You need to start an animation first !");
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public void StartAnimSimulation()
    {
        AnimationMode.StartAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
        _lastEditorTime = Time.realtimeSinceStartup;
        _isSimulatingAnimation = true;
        _canFocusOnAnimation = true;
        _sampleStepMode = false;
    }

    public void StopAnimSimulation()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        _isSimulatingAnimation = false;
        _canFocusOnAnimation = false;
        _animationTimer = 0f;
    }

    private void OnEditorUpdate()
    {
        if (null == _selectedClip) return;

        _animationTimer = Time.realtimeSinceStartup - _lastEditorTime;

        if (_sampleSpeedMode)
        {
            _animationTimer *= _animationClipSampleSpeed;
        }

        if (_animationTimer >= _selectedClip.length)
        {
            if (_sampleLoopMode)
            {
                //StartAnimSimulation();
                _lastEditorTime = Time.realtimeSinceStartup + _sampleLoopDelay;
            }
            else
            {
                StopAnimSimulation();

                Selection.activeGameObject = _selectedAnimator.gameObject;
                SceneView.lastActiveSceneView.FrameSelected(true, true);
            }
        }
        else
        {
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.SampleAnimationClip(_selectedAnimator.gameObject, _selectedClip, _animationTimer);

                if (_canFocusOnAnimation)
                {
                    Selection.activeGameObject = _selectedAnimator.gameObject;
                    SceneView.lastActiveSceneView.FrameSelected(true, true);
                    _canFocusOnAnimation = false;
                }
            }
        }
    }

    #endregion Animations
}