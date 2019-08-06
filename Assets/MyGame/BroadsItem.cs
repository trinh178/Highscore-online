using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BroadsItem : MonoBehaviour
{
    public Image mAvatar;
    public Text mName;
    public Text mScore;

    void Start()
    {
        //mAvatar = transform.Find("Avatar").GetComponent<Image>();
        //mName = transform.Find("Name").GetComponent<Text>();
        //mScore = transform.Find("Score").GetComponent<Text>();
    }
    
    public void Set(Sprite avatar, string name, int score)
    {
        //mAvatar.sprite = sprite;
        mName.text = name;
        mScore.text = score.ToString();
    }
    
    public void Set(string avatarUrl, string name, int score)
    {
        //mAvatar.sprite = sprite;
        mName.text = name;
        mScore.text = score.ToString();

        StartCoroutine(LoadAvatar(avatarUrl));
    }

    IEnumerator LoadAvatar(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        mAvatar.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }
}
