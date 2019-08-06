using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Result
{
    
}

public class Error
{
    public int ErrorCode;
    public string ErrorMsg;
}

public class InitResult : Error
{

}

public class LoginAnonymousResult : Error
{

}

public class LoadLeaderBoardResult : Error
{
    public PlayerStat[] Players;
    public bool IsIncludeSelfPlayer = false;
}

public class LoginWithFacebookResult : Error
{
    
}

public class PushScoreResult : Error
{

}

public class _AuthenFacebookResult : Error
{
    public string AccessToken;
    public string Name;
    public string AvatarUrl;
}
public class _AuthenPlayFabWithFacebook : Error
{
    
}
public class _CheckScore : Error
{

}

public class _RetriveServerBestScoreResult : Error
{
    public int ServerBestScore;
}
public class _SyncScoreResult : Error
{
    
}
public class _PushScoreToServerResult : Error
{

}

public class _TryPushLocalBestScoreToServerResult : Error
{

}

public class WriteCacheResult : Error
{

}
public class ReadCacheResult : Error
{
    public PlayerStat[] Players;
    public bool IsIncludeSelfPlayer;
}

public class AddScoreResult : Error
{

}