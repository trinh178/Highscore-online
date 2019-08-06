using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStat
{
    public string Id;
    public string AvatarUrl;
    public string Name;
    public int Score;
    public int Rank;
    public RelType Rel;
}

public enum RelType
{
    Undefined,
    Stranger,
    NextRank,
    Friend,
    Self
}
