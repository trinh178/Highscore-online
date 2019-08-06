using System.Collections;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine.Networking;

public class ImageHelper : MonoBehaviour
{
    // Config
    private const string CACHE_PATH = "/image__/";

    //
    private static ImageHelper instance = null;
    public static void Init()
    {
        if (instance == null)
        {
            instance = new GameObject().AddComponent<ImageHelper>();
        }
    }
    public static void LoadImage(string url, System.Action<ImageLoadCallback> callback)
    {
        if (instance == null)
        {
            callback(new ImageLoadCallback
            {
                ErrorCode = 1,
                ErrorMsg = "You must initialize to use!"
            });
        }
        else
        {
            instance.Load(url, callback);
        }
    }

    private Hashtable memoryData = new Hashtable();

    public void Load(string url, System.Action<ImageLoadCallback> callback)
    {
        string key = GetKey(url);
        // In memory ?
        Texture2D tex;
        if (CheckoutInMemory(key, out tex))
        {
            // Yes
            // Response - Ok
            callback(new ImageLoadCallback
            {
                ErrorCode = 0,
                texture = tex
            });
        }
        else
        {
            // On disk ?
            if (CheckoutOnDisk(key, out tex))
            {
                // Yes
                // Save on memory
                SaveInMemory(key, tex);
                // Response - Ok
                callback(new ImageLoadCallback
                {
                    ErrorCode = 0,
                    texture = tex
                });
            }
            else
            {
                // No
                // Fetch from network
                StartCoroutine(Fetch(url, r =>
                {
                    if (r.ErrorCode == 0)
                    {
                        // Ok
                        // Save on disk
                        SaveOnDisk(key, r.texture);
                        // Save in memory
                        SaveInMemory(key, r.texture);
                        // Response - Ok
                        callback(new ImageLoadCallback
                        {
                            ErrorCode = 0,
                            texture = r.texture
                        });
                    }
                    else
                    {
                        // Error
                        // Response - Error
                        callback(new ImageLoadCallback
                        {
                            ErrorCode = 1,
                            ErrorMsg = r.ErrorMsg
                        });
                    }
                }));
            }
        }
    }

    private IEnumerator Fetch(string url, System.Action<ImageLoadCallback> callback)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.error == null)
        {
            callback(new ImageLoadCallback
            {
                ErrorCode = 0,
                texture = DownloadHandlerTexture.GetContent(www)
            });
        }
        else
        {
            callback(new ImageLoadCallback
            {
                ErrorCode = 1,
                ErrorMsg = www.error
            });
        }
    }

    private bool CheckoutInMemory(string key)
    {
        return memoryData.ContainsKey(key);
    }
    private bool CheckoutInMemory(string key, out Texture2D tex)
    {
        if (memoryData.ContainsKey(key))
        {
            tex = memoryData[key] as Texture2D;
            return true;
        }
        else
        {
            tex = null;
            return false;
        }
    }
    private bool CheckoutOnDisk(string key)
    {
        string p = Application.temporaryCachePath + CACHE_PATH + key;
        return (File.Exists(p));
    }
    private bool CheckoutOnDisk(string key, out Texture2D tex)
    {
        string p = Application.temporaryCachePath + CACHE_PATH + key;
        if (File.Exists(p))
        {
            var binaryFormatter = new BinaryFormatter();

            tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(p));
            return true;
        }
        else
        {
            tex = null;
            return false;
        }
    }
    private void SaveInMemory(string key, Texture2D tex)
    {
        memoryData.Add(key, tex);
    }
    private void SaveOnDisk(string key, Texture2D tex)
    {
        var binaryFormatter = new BinaryFormatter();
        string p = Application.temporaryCachePath + CACHE_PATH + key;
        using (var fileStream = File.Create(p))
        {
            var a = tex.EncodeToPNG();
            fileStream.Write(a, 0, a.Length);
        }
    }

    //
    private string GetKey(string url)
    {
        return MD5Hash(NormalizeUrl(url));
    }
    public string NormalizeUrl(string url)
    {
        return url;
    }
    public string MD5Hash(string input)
    {
        StringBuilder hash = new StringBuilder();
        MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
        byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

        for (int i = 0; i < bytes.Length; i++)
        {
            hash.Append(bytes[i].ToString("x2"));
        }
        return hash.ToString();
    }
}

public class ImageLoadCallback : Error
{
    public Texture2D texture;
}

public class LoadThreadParam
{
    public string url;
    public System.Action<ImageLoadCallback> callback;
}
