using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Toolbox/Game/GameData")]
public class GameData : ScriptableObject
{
    #region Script Parameters

    [SerializeField]
    private int nbPlayers = 4;

    [SerializeField]
    private float[] playersSpeed;

    #endregion Script Parameters
}