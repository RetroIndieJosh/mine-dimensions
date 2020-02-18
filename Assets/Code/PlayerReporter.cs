using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerReporter : TickUpdateComponent
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_registeringText = null;

    private readonly string BASE_URL = "http://104.237.13.63/asym/unity.php";

    private int m_id = -1;
    private bool m_isRegistered = false;
    private int m_connectTime = 0;

    protected override void TickUpdate() {
        if (!m_isRegistered)
            return;

        var x = transform.position.x;
        var z = transform.position.z;
        var time = Mathf.Ceil(m_connectTime + Time.time);
        var url = BASE_URL + $"?action=add&id={m_id}&x={x}&z={z}&time={time}";
        var updateServer = AsymWeb.instance.Request(url);
        StartCoroutine(updateServer);
    }

    protected override void Start() {
        base.Start();

        if( m_registeringText != null )
            m_registeringText.enabled = true;

        var url = BASE_URL + "?action=user&name=bob";
        var login = AsymWeb.instance.Request(url, Register);
        StartCoroutine(login);
    }

    private void Register(string a_result) {
        var tokens = a_result.Split(' ');
        if (int.TryParse(tokens[0], out var id) == false) {
            Debug.LogError($"Got non-int for user ID: {id}");
            return;
        }
        m_id = id;

        if (int.TryParse(tokens[1], out var time) == false) {
            Debug.LogError($"Got non-int for user start time: {time}");
            return;
        }
        m_connectTime = time;
        m_isRegistered = true;
        Debug.Log($"ID: {id} / Start: {time}");

        if( m_registeringText != null )
            m_registeringText.enabled = false;
    }
}
