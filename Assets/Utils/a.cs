using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class a : MonoBehaviour
{
    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Load");
        ImageHelper.Init();
        ImageHelper.LoadImage(
            "",
            r =>
            {
                if (r.ErrorCode == 0)
                {
                    Debug.Log("OK");
                    image.sprite = Sprite.Create(r.texture, new Rect(0, 0, r.texture.width, r.texture.height), new Vector2(0, 0));
                }
                else
                {
                    Debug.Log("Error");
                }
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
