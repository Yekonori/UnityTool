using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameData))]
public class GameDataInspector : Editor
{
    #region Fields

    private const int PLAYER_MAX = 4;

    private SerializedProperty _nbPlayersProperty = null;
    private GUIContent _nbPlayersGUIContent = null;

    private SerializedProperty _playersSpeedProperty = null;
    private GUIContent[] _playerSpeedGUIContentArr = null;

    #endregion Fields

    #region Unity Methods

    private void OnEnable()
    {
        // NbPlayers
        _nbPlayersProperty = serializedObject.FindProperty("nbPlayers");
        _nbPlayersGUIContent = new GUIContent(_nbPlayersProperty.displayName);

        // PlayersSpeed
        _playersSpeedProperty = serializedObject.FindProperty("playersSpeed");
        if (_playersSpeedProperty.arraySize != PLAYER_MAX)
        {
            serializedObject.Update();
            _playersSpeedProperty.arraySize = PLAYER_MAX;
            serializedObject.ApplyModifiedProperties();
        }

        for (int i = 0; i < PLAYER_MAX; i++)
        {
            _playerSpeedGUIContentArr[i] = new GUIContent($"Player {i + 1} Speed");
        }
    }

    private void OnDisable()
    {
        _nbPlayersProperty = null;
        _nbPlayersGUIContent = null;

        _playersSpeedProperty = null;
        _playerSpeedGUIContentArr = null;
    }

    #endregion Unity Methods

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            _nbPlayersProperty.intValue = EditorGUILayout.IntSlider(_nbPlayersGUIContent, _nbPlayersProperty.intValue, 1, 4);

            for (int i = 0; i < _nbPlayersProperty.intValue; i++)
            {
                SerializedProperty playerSpeedProperty = _playersSpeedProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(playerSpeedProperty, _playerSpeedGUIContentArr[i]);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}