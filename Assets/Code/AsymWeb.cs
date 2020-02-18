using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Request
{
    public string page = "unity.php";
    public Dictionary<string, string> args = new Dictionary<string, string>();

    public override string ToString() {
        var str = $"{page}?";
        foreach (var arg in args)
            str += $"{arg.Key}={arg.Value}&";
        return str.Substring(0, str.Length - 1);
    }
}

public class AsymWeb : MonoBehaviour
{
    static public AsymWeb instance = null;

    [SerializeField, Tooltip("Must end with /")] private string m_baseUrl = "";

    private void Awake() {
        instance = this;
    }

    public IEnumerator ProcessRequest(Request a_request, System.Action<string> a_callback = null) {
        var url = m_baseUrl + a_request.ToString();
        Debug.Log($"URL: {url}");
        var www = UnityWebRequest.Get( url.ToString() );
        yield return www.SendWebRequest();

        if ( www.isNetworkError || www.isHttpError ) {
            Debug.LogError( $"Network error: {www.error}" );
            yield break;
        }

        var result = www.downloadHandler.text.Trim();
        if (a_callback == null)
            yield break;
        a_callback.Invoke(result);
    }
}
