using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    public Text mRank;
    public Image mAvatar;
    public Text mName;
    public Text mScore;

    public void Set(PlayerStat playerStat)
    {
        mRank.text = playerStat.Rank.ToString();
        ImageHelper.LoadImage(playerStat.AvatarUrl, r =>
        {
            if (r.ErrorCode == 0)
            {
                mAvatar.sprite = Sprite.Create(r.texture, new Rect(0, 0, r.texture.width, r.texture.height), new Vector2(0, 0));
            }
        });
        mName.text = playerStat.Name.ToString();
        mScore.text = playerStat.Score.ToString();
    }
}
