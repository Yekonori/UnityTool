using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerInspector : Editor
{
    #region Fields

    private static readonly string[] _excludedProperties = new string[]
    {
        "m_Script",
        "gameData"
    };

    #endregion Fields

    #region Unity Methods

    private void OnEnable()
    {
        SerializedProperty gameDataProperty = serializedObject.FindProperty("gameData");

        if (gameDataProperty.objectReferenceValue == null)
        {
            serializedObject.Update();
            gameDataProperty.objectReferenceValue = FindGameDataInProject();
            serializedObject.ApplyModifiedProperties();
        }
    }

    #endregion Unity Methods

    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(serializedObject, _excludedProperties);

        EditorGUILayout.Space(200);

        if (GUILayout.Button("Edit Game Data"))
        {
            Selection.activeObject = serializedObject.FindProperty("gameData").objectReferenceValue;
        }
    }

    private GameData FindGameDataInProject()
    {
        string[] fileGuidsArr = AssetDatabase.FindAssets($"t:{typeof(GameData)}");

        if (fileGuidsArr.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(fileGuidsArr[0]);
            return AssetDatabase.LoadAssetAtPath<GameData>(assetPath);
        }

        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        AssetDatabase.CreateAsset(gameData, "Assets/Settings/GameData.asset");
        AssetDatabase.SaveAssets();
        return gameData;
    }
}