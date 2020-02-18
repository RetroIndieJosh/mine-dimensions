using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AsymWeb : MonoBehaviour
{
    static public AsymWeb instance = null;

    public string result {
        get; private set;
    }

    private void Awake() {
        instance = this;
    }

    public IEnumerator Request(string a_url, System.Action<string> a_callback = null) {
        result = null;

        var www = UnityWebRequest.Get( a_url.ToString() );
        yield return www.SendWebRequest();

        if ( www.isNetworkError || www.isHttpError ) {
            Debug.LogError( $"Network error: {www.error}" );
            yield break;
        }

        result = www.downloadHandler.text.Trim();
        if (a_callback == null)
            yield break;
        a_callback.Invoke(result);
    }
}
