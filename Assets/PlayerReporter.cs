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
    private int m_startTimeServer = 0;

    private float m_timeElapsed = 0f;

    private void Start() {
        StartCoroutine(Login());
    }

    private void Update() {
        if (!m_isRegistered)
            return;

        m_timeElapsed += Time.deltaTime;
        if (m_timeElapsed < m_tickSec)
            return;

        var x = transform.position.x;
        var z = transform.position.z;
        var time = Mathf.Ceil(m_startTimeServer + Time.time);
        var url = BASE_URL + $"?action=add&id={m_id}&x={x}&z={z}&time={time}";
        StartCoroutine(UpdateServer(url));

        m_timeElapsed = 0f;
    }

    private IEnumerator Login() {
        var url = BASE_URL + "?action=user&name=bob";
        var www = UnityWebRequest.Get( url.ToString() );
        yield return www.SendWebRequest();

        if ( www.isNetworkError || www.isHttpError ) {
            Debug.LogError( $"Network error: {www.error}" );
            yield break;
        }

        var result = www.downloadHandler.text.Trim().Split(' ');

        if (int.TryParse(result[0], out var id) == false) {
            Debug.LogError($"Got non-int for user ID: {id}");
            yield break;
        }
        m_id = id;

        if (int.TryParse(result[1], out var time) == false) {
            Debug.LogError($"Got non-int for user start time: {time}");
            yield break;
        }
        m_startTimeServer = time;
        m_isRegistered = true;
        Debug.Log($"ID: {id} / Start: {time}");
    }

    private IEnumerator UpdateServer(string url) {
        var www = UnityWebRequest.Get( url.ToString() );
        yield return www.SendWebRequest();

        if ( www.isNetworkError || www.isHttpError ) {
            Debug.LogError( $"Network error: {www.error}" );
            yield break;
        }

        var result = www.downloadHandler.text.Trim();
        Debug.Log($"Update result: {result}");
    }
}
