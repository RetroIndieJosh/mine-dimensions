using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerReporter : MonoBehaviour
{
    private readonly string BASE_URL = "http://104.237.13.63/asym/unity.php";

    [SerializeField] private float m_tickSec = 10.0f;

    private int m_id = -1;
    private bool m_isRegistered = false;

    private void Start() {
        StartCoroutine(Login());
    }

    private void Update() {
        if (!m_isRegistered)
            return;
    }

    private IEnumerator Login() {
        var url = BASE_URL + "?action=user&name=bob";
        var www = UnityWebRequest.Get( url.ToString() );
        yield return www.SendWebRequest();

        if ( www.isNetworkError || www.isHttpError ) {
            Debug.LogError( $"Network error: {www.error}" );
            yield break;
        }

        var result = www.downloadHandler.text.Trim();
        if (int.TryParse(result, out var id) == false) {
            Debug.LogError($"Got non-int from user add: {result}");
            yield break;
        }
        m_id = id;
        Debug.Log($"ID: {id}");

        /*
        if ( result.StartsWith( "ERR" ) ) {
            url.Error( result );
            yield break;
        }
        */
    }
}
