using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public Transform broads;
    public BroadsItem broadsItemPrefab;
    public InputField score;

    LeaderBoard leaderBoard;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.LogError("Add score 101");
            LeaderBoardUI.AddScore(101);
            
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.LogError("Add score 1001");
            LeaderBoardUI.AddScore(1001);
            
        }
    }
}
