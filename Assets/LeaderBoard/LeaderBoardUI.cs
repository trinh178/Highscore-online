using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class LeaderBoardUI : MonoBehaviour
{
    private void Awake()
    {
        if (leaderBoardUI == null)
        {
            Debug.LogError("Init");
            ImageHelper.Init();
            leaderBoardUI = this;
            leaderBoardUI.Init();
        }
        DontDestroyOnLoad(transform.gameObject);
    }

    private static LeaderBoardUI leaderBoardUI = null;


    public Transform globalBroad;
    public Transform friendBroad;
    public PlayerStatUI playerStatPrefab;
    public PlayerStatUI selfPlayerStatPrefab;
    public PlayerStatUI friendPlayerStatPrefab;
    public PlayerStatUI nextrankPlayerStatPrefab;
    private LeaderBoard leaderboard;
    public Transform leftNotLogged;
    public Text leftNotLogged_score;
    public Transform leftLogged;
    public Text leftLogged_score;
    public Image leftLogged_avatar;
    private bool isRefeshingLeaderboard = false;

    // API
    public static void Show(bool show)
    {
        leaderBoardUI._Show(show);
    }
    public static bool IsShowing()
    {
        return leaderBoardUI._IsShowing();
    }
    public static void AddScore(int score)
    {
        leaderBoardUI._AddScore(score);
    }
    public void Init()
    {
        Log("Init");
        leaderboard = new LeaderBoard();
        leaderboard.Init(result =>
        {
            if (result.ErrorCode == 0)
            {
                RefeshStateUI();
                RefeshLeaderboard();
            }
            else
            {
                ShowMessenger(result.ErrorMsg);
            }
        });
    }
    public void _Show(bool show)
    {
        gameObject.SetActive(show);
    }
    public bool _IsShowing()
    {
        return gameObject.activeSelf;
    }
    public void _AddScore(int score)
    {
        leaderboard.AddScore(score, r =>
        {
            RefeshStateUI();
            Debug.LogWarning("Refesh");
            RefeshLeaderboard();
        });
    }

    private void RefeshLeaderboard()
    {
        if (isRefeshingLeaderboard)
        {
            return;
        }
        isRefeshingLeaderboard = true;
        Log("RefeshLeaderboard");
        Log("IsLogged ?");
        if (leaderboard.IsLogged())
        {
            Log("Yes");
            Log("Retrieve top 10");
            leaderboard.LoadLeaderBoard(10, r =>
            {
                if (r.ErrorCode == 0)
                {
                    Log("Ok");
                    Log("Is include self player ?");
                    if (r.IsIncludeSelfPlayer)
                    {
                        Log("Yes");
                        Log("Render");
                        RenderLeaderboard(r.Players);
                        isRefeshingLeaderboard = false;
                    }
                    else
                    {
                        Log("No");
                        Log("Retrieve around player <1>");
                        leaderboard.LoadLeaderBoardAroundSelfPlayer(1, r2 =>
                        {
                            if (r2.ErrorCode == 0)
                            {
                                Log("Ok");
                                Log("Around player list > 0");
                                if (r2.Players.Length > 0)
                                {
                                    Log("Yes");
                                    Log("Combine");
                                    if (r2.Players.Length == 3)
                                    {
                                        r.Players[9] = r2.Players[2];
                                        r.Players[8] = r2.Players[1];
                                        r.Players[7] = r2.Players[0];
                                    }
                                    else
                                    {
                                        r.Players[9] = r2.Players[1];
                                        r.Players[8] = r2.Players[0];
                                    }
                                    Log("Render");
                                    RenderLeaderboard(r.Players);
                                    isRefeshingLeaderboard = false;
                                }
                                else
                                {
                                    Log("No");
                                    Log("Render");
                                    RenderLeaderboard(r.Players);
                                    isRefeshingLeaderboard = false;
                                }
                            }
                            else
                            {
                                Log("Error");
                                Log("Fail");
                                isRefeshingLeaderboard = false;
                            }
                        });
                    }
                }
                else
                {
                    Log("Error");
                    Log("Fail");
                    isRefeshingLeaderboard = false;
                }
            });
        }
        else
        {
            Log("No");
            Log("Retrieve top 10");
            leaderboard.LoadLeaderBoard(10, r =>
            {
                if (r.ErrorCode == 0)
                {
                    //Debug.LogWarning(r.Players.Length);
                    //Debug.LogWarning(r.Players[0].Id);
                    //Debug.LogWarning(r.Players[1].Id);
                    Log("Ok");
                    RenderLeaderboard(r.Players);
                    isRefeshingLeaderboard = false;
                }
                else
                {
                    Log("Error");
                    Log("Fail");
                    isRefeshingLeaderboard = false;
                }
            });
        }
    }
    private void RefeshStateUI()
    {
        Log("RefeshStateUI");
        Log("Is Logged ?");
        if (leaderboard.IsLogged())
        {
            Log("Yes");
            leftNotLogged.gameObject.SetActive(false);
            leftLogged.gameObject.SetActive(true);
            leftLogged_score.text = leaderboard.GetLocalBestScore().ToString();
            ImageHelper.LoadImage(leaderboard.GetSelfPlayer().AvatarUrl, r => {
                if (r.ErrorCode == 0)
                {
                    leftLogged_avatar.sprite = Sprite.Create(r.texture, new Rect(0, 0, r.texture.width, r.texture.height), new Vector2(0, 0));
                }
            });
        }
        else
        {
            Log("No");
            leftNotLogged.gameObject.SetActive(true);
            leftLogged.gameObject.SetActive(false);
            leftNotLogged_score.text = leaderboard.GetLocalBestScore().ToString();
        }
    }
    // Event
    public void LoginWithFacebook()
    {
        ShowLoading(true);
        leaderboard.LoginWithFacebook(r => {
            if (r.ErrorCode == 0)
            {
                ShowLoading(false);
            }
            else
            {
                ShowMessenger(r.ErrorMsg);
                ShowLoading(false);
            }
            RefeshStateUI();
            RefeshLeaderboard();
        });
    }

    // UI
    private void ClearPlayerRender()
    {
        foreach (Transform child in globalBroad)
        {
            Destroy(child.gameObject);
        }
    }
    private void AddPlayerRender(PlayerStat playerStat)
    {
        PlayerStatUI prefab;
        switch (playerStat.Rel)
        {
            case RelType.Self:
                prefab = selfPlayerStatPrefab;
                break;
            case RelType.Friend:
                prefab = friendPlayerStatPrefab;
                break;
            case RelType.NextRank:
                prefab = nextrankPlayerStatPrefab;
                break;
            default:
                prefab = playerStatPrefab;
                break;
        }
        PlayerStatUI playerStatUI = Instantiate(prefab, globalBroad);
        playerStatUI.Set(playerStat);
    }
    private void RenderLeaderboard(PlayerStat[] players)
    {
        Log("RenderLeaderboard");
        ClearPlayerRender();
        foreach (PlayerStat p in players)
        {
            AddPlayerRender(p);
        }
    }

    // Messenger
    public Transform messenger;
    public Text msg;
    private void ShowMessenger(string msg)
    {
        this.msg.text = msg;
        messenger.gameObject.SetActive(true);
    }
    // Loading
    public Transform loading;
    private void ShowLoading(bool show)
    {
        loading.gameObject.SetActive(show);
    }

    // Log
    private void Log(string log)
    {
        Debug.Log("LeaderBoardUI : " + log);
    }
    private void Log(string tag, string log)
    {
        Debug.Log("Leaderboard: " + tag + ": " + log);
    }
}
