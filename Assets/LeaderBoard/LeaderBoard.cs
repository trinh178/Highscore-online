using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class LeaderBoard
{
    int localBestScore
    {
        get
        {
            return PlayerPrefs.GetInt("leaderboard_localbestscore", 0);
        }
        set
        {
            PlayerPrefs.SetInt("leaderboard_localbestscore", value);
        }
    }
    bool wasPushedScore
    {
        get
        {
            return bool.Parse(PlayerPrefs.GetString("leaderboard_waspushedscore", "true"));
        }
        set
        {
            PlayerPrefs.SetString("leaderboard_waspushedscore", value.ToString());
        }
    }
    PlayerStat selfPlayer = null;

    public LeaderBoard()
    {
        PlayFabSettings.TitleId = LeaderboardConfig.PlayFabTitleId;
    }

    // API
    public void Init(System.Action<InitResult> callback)
    {
        string logTag = "Init";
        Log(logTag, "Begin");
        _LoginAnonymous(result =>
        {
            if (result.ErrorCode == 0)
            {
                FB.Init(() =>
                {
                    Log("Ok");
                    Log(logTag, "End - Success");
                    callback(new InitResult
                    {
                        ErrorCode = 0
                    });
                });
            }
            else
            {
                Log("Error");
                Log(logTag, "End - Fail");
                callback(new InitResult
                {
                    ErrorCode = 1,
                    ErrorMsg = result.ErrorMsg
                });
            }
        });
    }
    public void LoadLeaderBoard(int top, System.Action<LoadLeaderBoardResult> callback)
    {
        string logTag = "LoadLeaderboard";
        Log(logTag, "Begin");
        Log("LoadLeaderboard");
        Log("_TryPushLocalBestScore");
        _TryPushLocalBestScoreToServer(result =>
        {
            Log("LoadLeaderboardFromServer");
            PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest
            {
                StartPosition = 0,
                MaxResultsCount = top,
                StatisticName = LeaderboardConfig.PlayFabStatisticName,
                //Version = (int)LeaderboardConfig.PlayFabStatisticVersion,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true,
                    ShowAvatarUrl = true
                }
            }, r =>
            {
                Log("Ok");
                Log("Convert to PlayerStat & check include self player");
                bool isIncludeSelfPlayer = false;
                var p = _ConvertToPlayerStat(r.Leaderboard, out isIncludeSelfPlayer);
                Log("Write cache");
                WriteCache_LoadLeaderBoard(p, r2 =>
                {
                    if (r2.ErrorCode == 0)
                    {
                        Log("Ok");
                        Log("Success");
                        Log(logTag, "End - Success");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 0,
                            Players = p,
                            IsIncludeSelfPlayer = isIncludeSelfPlayer
                        });
                    }
                    else
                    {
                        Log("Error");
                        Log("Fail");
                        Log(logTag, "End - Fail - " + r2.ErrorMsg);
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = r2.ErrorMsg,
                            IsIncludeSelfPlayer = isIncludeSelfPlayer
                        });
                    }
                });
            }, e =>
            {
                Log("Read cache");
                ReadCache_LoadLeaderBoard(r =>
                {
                    if (r.ErrorCode == 0)
                    {
                        Log("Ok");
                        Log(logTag, "End - Success");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 0,
                            Players = r.Players,
                            IsIncludeSelfPlayer = r.IsIncludeSelfPlayer
                        });
                    }
                    else
                    {
                        Log("Error");
                        Log(logTag, "End - Fail");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = e.ErrorMessage
                        });
                    }
                });
            });
        });
    }
    public void LoadLeaderBoardAroundSelfPlayer(int range, System.Action<LoadLeaderBoardResult> callback)
    {
        string logTag = "LoadLeaderBoardAroundSelfPlayer";
        Log(logTag, "Begin");
        Log("LoadLeaderboardAroundSelfPlayer");
        Log("Is logged ?");
        if (IsLogged())
        {
            Log("Yes");
            Log("LoadLeaderboardAroundSelfPlayerFromServer");
            PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest
            {
                PlayFabId = selfPlayer.Id,
                MaxResultsCount = 3,
                StatisticName = LeaderboardConfig.PlayFabStatisticName,
                //Version = (int)LeaderboardConfig.PlayFabStatisticVersion,
                ProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true,
                    ShowAvatarUrl = true
                }
            }, r =>
            {
                
                Log("Ok");
                Log("Convert to PlayerStat & check include self player");
                bool isIncludeSelfPlayer;
                var p = _ConvertToPlayerStat(r.Leaderboard, out isIncludeSelfPlayer);
                if (p.Length == 3 && p[2].Id == selfPlayer.Id)
                {
                    p = new PlayerStat[]
                    {
                        p[1], p[2]
                    };
                }
                Log("Write cache");
                WriteCache_LoadLeaderBoardAroundSelfPlayer(p, r2 =>
                {
                    if (r2.ErrorCode == 0)
                    {
                        Log("Ok");
                        Log("Success");
                        Log(logTag, "End - Success");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 0,
                            Players = p,
                            IsIncludeSelfPlayer = isIncludeSelfPlayer
                        });
                    }
                    else
                    {
                        Log("Error");
                        Log("Fail");
                        Log(logTag, "End - Fail");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = r2.ErrorMsg
                        });
                    }
                });
            }, e =>
            {
                Log("Read cache");
                ReadCache_LoadLeaderBoardAroundSelfPlayer(r =>
                {
                    if (r.ErrorCode == 0)
                    {
                        Log("Ok");
                        Log(logTag, "End - Success");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 0,
                            Players = r.Players,
                            IsIncludeSelfPlayer = r.IsIncludeSelfPlayer
                        });
                    }
                    else
                    {
                        Log("Error");
                        Log(logTag, "End - Fail");
                        callback(new LoadLeaderBoardResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = e.ErrorMessage
                        });
                    }
                });
            });
        }
        else
        {
            Log("No");
            Log("Fail");
            Log(logTag, "End - Fail");
            callback(new LoadLeaderBoardResult
            {
                ErrorCode = 1,
                ErrorMsg = "No login"
            });
        }
    }
    public void LoginWithFacebook(System.Action<LoginWithFacebookResult> callback)
    {
        string logTag = "LoginWithFacebook";
        Log(logTag, "Begin");
        Log("LoginWithFacebook");
        _AuthenFacebook(result => {
            if (result.ErrorCode == 0)
            {
                Log("Login PlayFab");
                PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest
                {
                    AccessToken = result.AccessToken,
                    CreateAccount = true
                }, r =>
                {
                    Log("Update name");
                    PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
                    {
                        DisplayName = result.Name
                    }, r2 =>
                    {
                        Log("Update avatar");
                        PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest
                        {
                            ImageUrl = result.AvatarUrl
                        }, r3 =>
                        {
                            Log("Success");
                            _SyncScore(r4 =>
                            {
                                if (r4.ErrorCode == 0)
                                {
                                    Log("_TryPushLocalBestScore");
                                    _TryPushLocalBestScoreToServer(r5 =>
                                    {
                                        selfPlayer = new PlayerStat
                                        {
                                            Id = r.PlayFabId,
                                            AvatarUrl = result.AvatarUrl,
                                            Name = result.Name
                                        };
                                        Log(logTag, "End - Success");
                                        callback(new LoginWithFacebookResult
                                        {
                                            ErrorCode = 0
                                        });
                                    });
                                }
                                else
                                {
                                    Log(logTag, "End - Fail");
                                    callback(new LoginWithFacebookResult
                                    {
                                        ErrorCode = 1,
                                        ErrorMsg = r4.ErrorMsg
                                    });
                                }
                            });
                        }, e3 =>
                        {
                            Log("Logout PlayFab");
                            PlayFabClientAPI.ForgetAllCredentials();
                            Log("Logout facebook");
                            FB.LogOut();
                            Log("Fail - " + e3.ErrorMessage);
                            Log(logTag, "End - Fail");
                            callback(new LoginWithFacebookResult
                            {
                                ErrorCode = 1,
                                ErrorMsg = e3.ErrorMessage
                            });
                        });
                    }, e2 =>
                    {
                        Log("Logout PlayFab");
                        PlayFabClientAPI.ForgetAllCredentials();
                        Log("Logout facebook");
                        FB.LogOut();
                        Log("Fail - " + e2.ErrorMessage);
                        Log(logTag, "End - Fail");
                        callback(new LoginWithFacebookResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = e2.ErrorMessage
                        });
                    });
                }, e =>
                {
                    Log("Fail - " + e.ErrorMessage);
                    Log("Logout facebook");
                    FB.LogOut();
                    Log(logTag, "End - Fail");
                    callback(new LoginWithFacebookResult
                    {
                        ErrorCode = 1,
                        ErrorMsg = e.ErrorMessage
                    });
                });
            }
            else
            {
                Log(logTag, "End - Fail");
                callback(new LoginWithFacebookResult
                {
                    ErrorCode = 1
                });
            }
        });
    }
    public bool IsLogged()
    {
        Log("IsLogged");
        return PlayFabClientAPI.IsClientLoggedIn() && FB.IsInitialized && FB.IsLoggedIn;
    }
    public void Logout()
    {
        localBestScore = 0;
        wasPushedScore = true;
        selfPlayer = null;
        Log("Logout");
        Log("Logout facebook");
        FB.LogOut();
        Log("Logout playfab");
        PlayFabClientAPI.ForgetAllCredentials();
        _LoginAnonymous(result =>
        {
            if (result.ErrorCode == 0)
            {
                Log("Ok");
                Log("Success");
            }
            else
            {
                Log("Error");
                Log("Fail");
            }
        });
    }
    public PlayerStat GetSelfPlayer()
    {
        Log("GetSelfPlayer");
        return selfPlayer;
    }
    public int GetLocalBestScore()
    {
        Log("GetLocalBestScore");
        return localBestScore;
    }
    public void AddScore(int score)
    {
        Log("AddScore");
        Log("Is score > localBestScore");
        if (score > localBestScore)
        {
            localBestScore = score;
            wasPushedScore = false;
            _TryPushLocalBestScoreToServer(r =>
            {
                if (r.ErrorCode == 0)
                {
                    Log("Success");
                }
                else
                {
                    Log("Error");
                    Log("Success");
                }
            });
        }
        else
        {
            Log("Success");
        }
    }
    public void AddScore(int score, System.Action<AddScoreResult> callback)
    {
        string logTag = "AddScore";
        Log(logTag, "Begin");
        Log("AddScore");
        Log("Is score > localBestScore");
        if (score > localBestScore)
        {
            localBestScore = score;
            wasPushedScore = false;
            _TryPushLocalBestScoreToServer(r =>
            {
                if (r.ErrorCode == 0)
                {
                    Log("Success");
                    Log(logTag, "End - Success");
                    callback(new AddScoreResult
                    {
                        ErrorCode = 0
                    });
                }
                else
                {
                    Log("Error");
                    Log("Success");
                    Log(logTag, "End - Success");
                    callback(new AddScoreResult
                    {
                        ErrorCode = 0
                    });
                }
            });
        }
        else
        {
            Log("Success");
            Log(logTag, "End - Success");
            callback(new AddScoreResult
            {
                ErrorCode = 0
            });
        }
    }


    private void _LoginAnonymous(System.Action<LoginAnonymousResult> callback)
    {
        Log("_LoginAnonymous"); 
        Log("LoginWithCustomID");
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = LeaderboardConfig.PlayFabAnonymousCustomID,
            TitleId = LeaderboardConfig.PlayFabTitleId,
            CreateAccount = true
        }, result =>
        {
            Log("Ok");
            Log("Success");
            callback(new LoginAnonymousResult
            {
                ErrorCode = 0
            });
        }, e =>
        {
            Log("Error");
            Log("Fail");
            callback(new LoginAnonymousResult
            {
                ErrorCode = 1,
                ErrorMsg = e.ErrorMessage
            });
        });
    }
    private void _PushScoreToServer(int score, System.Action<_PushScoreToServerResult> callback)
    {
        Log("_PushScoreToServer");
        Log("Is logged ?");
        if (IsLogged())
        {
            Log("Yes");
            Log("Push to PF server");
            PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>()
                {
                    new StatisticUpdate
                    {
                        StatisticName = LeaderboardConfig.PlayFabStatisticName,
                        Value = score,
                        //Version = LeaderboardConfig.PlayFabStatisticVersion
                    }
                }
            }, r =>
            {
                Log("OK");
                Log("Success");
                callback(new _PushScoreToServerResult
                {
                    ErrorCode = 0
                });
            }, e =>
            {
                Log("Error");
                Log("Fail");
                callback(new _PushScoreToServerResult
                {
                    ErrorCode = 1,
                    ErrorMsg = e.ErrorMessage
                });
            });
        }
        else
        {
            Log("No");
            Log("Fail");
            callback(new _PushScoreToServerResult
            {
                ErrorCode = 1,
                ErrorMsg = "No login"
            });
        }
    }
    private void _TryPushLocalBestScoreToServer(System.Action<_TryPushLocalBestScoreToServerResult> callback)
    {
        Log("_TryPushLocalBestScoreToServer");
        Log("Is logged ?");
        if (IsLogged())
        {
            Log("Was pushed score ?");
            if (wasPushedScore)
            {
                Log("Success");
                callback(new _TryPushLocalBestScoreToServerResult
                {
                    ErrorCode = 0,
                });
            }
            else
            {
                _PushScoreToServer(localBestScore, r =>
                {
                    if (r.ErrorCode == 0)
                    {
                        Log("Yes");
                        wasPushedScore = true;
                        Log("Success");
                        callback(new _TryPushLocalBestScoreToServerResult
                        {
                            ErrorCode = 0,
                        });
                    }
                    else
                    {
                        Log("No");
                        Log("Fail");
                        callback(new _TryPushLocalBestScoreToServerResult
                        {
                            ErrorCode = 1,
                        });
                    }
                });
            }
        }
        else
        {
            Log("No");
            Log("Success");
            callback(new _TryPushLocalBestScoreToServerResult
            {
                ErrorCode = 0,
            });
        }
    }
    private PlayerStat[] _ConvertToPlayerStat(List<PlayerLeaderboardEntry> players, out bool isIncludeSelfPlayer)
    {
        isIncludeSelfPlayer = false;
        PlayerStat[] p = new PlayerStat[players.Count];
        if (IsLogged())
        {
            int i = 0;
            foreach (PlayerLeaderboardEntry player in players)
            {
                p[i] = new PlayerStat
                {
                    Id = player.PlayFabId,
                    AvatarUrl = (player.Profile.AvatarUrl == null || player.Profile.AvatarUrl == "") ? "" : player.Profile.AvatarUrl,
                    Name = (player.DisplayName==null || player.DisplayName=="")?"Nonamne":player.DisplayName,
                    Score = player.StatValue,
                    Rank = player.Position + 1,
                    Rel = RelType.Stranger
                };
                if (i == 0)
                {
                    if (p[i].Id == selfPlayer.Id)
                    {
                        p[i].Rel = RelType.Self;
                        isIncludeSelfPlayer = true;
                        //
                        if (p[i].Score == 0)
                        {
                            isIncludeSelfPlayer = false;
                            p = new PlayerStat[0];
                            break;
                        }
                    }
                }
                else
                {
                    if (p[i].Id == selfPlayer.Id)
                    {
                        p[i].Rel = RelType.Self;
                        p[i - 1].Rel = RelType.NextRank;
                        isIncludeSelfPlayer = true;
                        //
                        if (p[i].Score == 0)
                        {
                            isIncludeSelfPlayer = false;
                            p = new PlayerStat[0];
                            break;
                        }
                    }
                    else if (p[i - 1].Id == selfPlayer.Id)
                    {
                        p[i - 1].Rel = RelType.Self;
                        p[i].Rel = RelType.NextRank;
                        isIncludeSelfPlayer = true;
                    }
                }
                i++;    
            }
        }
        else
        {
            int i = 0;
            foreach (PlayerLeaderboardEntry player in players)
            {
                p[i] = new PlayerStat
                {
                    Id = player.PlayFabId,
                    AvatarUrl = (player.Profile.AvatarUrl == null || player.Profile.AvatarUrl == "") ? "" : player.Profile.AvatarUrl,
                    Name = (player.DisplayName == null || player.DisplayName == "") ? "Nonamne" : player.DisplayName,
                    Score = player.StatValue,
                    Rank = player.Position + 1,
                    Rel = RelType.Undefined
                };
                i++;
            }
        }
        return p;
    }


    // Cache
    private void WriteCache_LoadLeaderBoard(PlayerStat[] players, System.Action<WriteCacheResult> callback)
    {
        Log("WriteCache_LoadLeaderBoard");
        try
        {
            WriteCache(LeaderboardConfig._LoadLeaderBoard_CachePath, players);
            callback(new WriteCacheResult
            {
                ErrorCode = 0
            });
        }
        catch (IOException e)
        {
            callback(new WriteCacheResult
            {
                ErrorCode = 1,
                ErrorMsg = e.Message
            });
        }
    }
    private void ReadCache_LoadLeaderBoard(System.Action<ReadCacheResult> callback)
    {
        Log("ReadCache_LoadLeaderBoard");
        try
        {
            bool isIncludeSelfPlayer = false;
            var p = (PlayerStat[])ReadCache(LeaderboardConfig._LoadLeaderBoard_CachePath);
            if (IsLogged())
            {
                foreach (PlayerStat pp in p)
                {
                    if (pp.Id == selfPlayer.Id)
                    {
                        isIncludeSelfPlayer = true;
                        break;
                    }
                }
            }
            callback(new ReadCacheResult
            {
                ErrorCode = 0,
                Players = p,
                IsIncludeSelfPlayer = isIncludeSelfPlayer
            });
        }
        catch (IOException e)
        {
            callback(new ReadCacheResult
            {
                ErrorCode = 1,
                ErrorMsg = e.Message
            });
        }
    }
    private void WriteCache_LoadLeaderBoardAroundSelfPlayer(PlayerStat[] players, System.Action<WriteCacheResult> callback)
    {
        Log("WriteCache_LoadLeaderBoardAroundSelfPlayer");
        try
        {
            WriteCache(LeaderboardConfig._LoadLeaderBoardAroundSelfPlayer_CachePath, players);
            callback(new WriteCacheResult
            {
                ErrorCode = 0
            });
        }
        catch (IOException e)
        {
            callback(new WriteCacheResult
            {
                ErrorCode = 1,
                ErrorMsg = e.Message
            });
        }
    }
    private void ReadCache_LoadLeaderBoardAroundSelfPlayer(System.Action<ReadCacheResult> callback)
    {
        Log("ReadCache_LoadLeaderBoardAroundSelfPlayer");
        try
        {
            bool isIncludeSelfPlayer = false;
            var p = (PlayerStat[])ReadCache(LeaderboardConfig._LoadLeaderBoardAroundSelfPlayer_CachePath);
            if (IsLogged())
            {
                foreach (PlayerStat pp in p)
                {
                    if (pp.Id == selfPlayer.Id)
                    {
                        isIncludeSelfPlayer = true;
                        break;
                    }
                }
            }
            callback(new ReadCacheResult
            {
                ErrorCode = 0,
                Players = p,
                IsIncludeSelfPlayer = isIncludeSelfPlayer
            });
        }
        catch (IOException e)
        {
            callback(new ReadCacheResult
            {
                ErrorCode = 1,
                ErrorMsg = e.Message
            });
        }
    }
    private void WriteCache(string path, object o)
    {
        var binaryFormatter = new BinaryFormatter();
        using (var fileStream = File.Create(Application.temporaryCachePath + "/" + path))
        {
            binaryFormatter.Serialize(fileStream, o);
        }
    }
    private object ReadCache(string path)
    {
        string p = Application.temporaryCachePath + "/" + path;
        if (File.Exists(p))
        {
            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Open(p, FileMode.Open))
            {
                return binaryFormatter.Deserialize(fileStream);
            }
        }
        else
        {
            throw new System.Exception("File doesn't not exist !");
        }
    }

    // Log
    private void Log(string log)
    {
        //Debug.Log("Leaderboard: " + log);
    }
    private void Log(string tag, string log)
    {
        Debug.Log("Leaderboard: " + tag + ": " + log);
    }


    //
    // Authen
    private void _AuthenFacebook(System.Action<_AuthenFacebookResult> callback)
    {
        Log("_AuthenFacebook");
        if (!FB.IsInitialized)
        {
            Log("Uninitialized");
            callback(new _AuthenFacebookResult
            {
                ErrorCode = 1
            });
            return;
        }

        if (FB.IsLoggedIn)
            FB.LogOut();

        // Login to facebook
        Log("Login to facebook");
        FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email", "user_friends" }, result =>
        {
            if (result == null || string.IsNullOrEmpty(result.Error))
            {
                Log("Facebook API get name");
                FB.API(AccessToken.CurrentAccessToken.UserId, HttpMethod.GET, fbResult =>
                {
                    if (fbResult == null || string.IsNullOrEmpty(fbResult.Error))
                    {
                        Log("Success");
                        callback(new _AuthenFacebookResult
                        {
                            ErrorCode = 0,
                            AvatarUrl = "https://graph.facebook.com/" + AccessToken.CurrentAccessToken.UserId + "/picture?type=square",
                            Name = fbResult.ResultDictionary["name"].ToString(),
                            AccessToken = AccessToken.CurrentAccessToken.TokenString
                        });
                    }
                    else
                    {
                        Log("Fail");
                        callback(new _AuthenFacebookResult
                        {
                            ErrorCode = 1,
                            ErrorMsg = fbResult.Error
                        });
                    }
                });
            }
            else
            {
                Log("Fail - " + result.Error);
                callback(new _AuthenFacebookResult
                {
                    ErrorCode = 1,
                    ErrorMsg = result.Error
                });
            }
        });
    }


    private void _SyncScore(System.Action<_RetriveServerBestScoreResult> callback)
    {
        // Sync between localBestScore and serverBestScore
        Log("_SyncScore");
        _RetriveServerBestScore(result =>
        {
            if (result.ErrorCode == 0)
            {
                if (localBestScore > result.ServerBestScore)
                {
                    _PushScoreToServer(localBestScore, r =>
                    {
                        if (r.ErrorCode == 0)
                        {
                            wasPushedScore = true;
                            callback(new _RetriveServerBestScoreResult
                            {
                                ErrorCode = 0
                            });
                        }
                        else
                        {
                            wasPushedScore = false;
                            callback(new _RetriveServerBestScoreResult
                            {
                                ErrorCode = 1,
                                ErrorMsg = r.ErrorMsg
                            });
                        }
                    });
                } else
                {
                    localBestScore = result.ServerBestScore;
                    callback(new _RetriveServerBestScoreResult
                    {
                        ErrorCode = 0
                    });
                }
            }
            else
            {
                callback(new _RetriveServerBestScoreResult
                {
                    ErrorCode = 1,
                    ErrorMsg = result.ErrorMsg
                });
            }
        });
    }
    private void _RetriveServerBestScore(System.Action<_RetriveServerBestScoreResult> callback)
    {
        Log("_RetriveServerBestScore");
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest
        {
            StatisticNameVersions = new List<StatisticNameVersion>() { new StatisticNameVersion
                {
                    StatisticName = LeaderboardConfig.PlayFabStatisticName,
                    //Version = 2
                }
            }
        }, r =>
        {
            Log("OK");
            if (r.Statistics.Count > 0)
            {
                callback(new _RetriveServerBestScoreResult
                {
                    ErrorCode = 0,
                    ServerBestScore = r.Statistics[0].Value
                });
            }
            else
            {
                callback(new _RetriveServerBestScoreResult
                {
                    ErrorCode = 0,
                    ServerBestScore = 0
                });
            }
            
        }, e =>
        {
            Log("Error");
            callback(new _RetriveServerBestScoreResult
            {
                ErrorCode = 1,
                ErrorMsg = e.ErrorMessage
            });
        });
    }
}
