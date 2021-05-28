using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Script Parameters

    [SerializeField]
    private GameData gameData = null;

    #endregion Script Parameters

    #region Fields

    private const string GAMEOBJECT_NAME = "GameManager";

    #endregion Fields

    #region Unity Methods

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (gameObject.name != GAMEOBJECT_NAME)
        {
            gameObject.name = GAMEOBJECT_NAME;
        }
    }

#endif

    #endregion Unity Methods
}