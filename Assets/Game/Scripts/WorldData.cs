using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldData", menuName = "Scriptable Objects/WorldData")]
public class WorldData : ScriptableObject
{
    public Level[] scenes;
}

[Serializable]
public struct Level
{
    public string levelName;
    public int moneyWhenLoose;
    public int moneyWhenWin;
}