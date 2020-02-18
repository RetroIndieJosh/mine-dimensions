using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerReporter : TickUpdateComponent
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_headingText = null;
    [SerializeField] private TextMeshProUGUI m_registeringText = null;

    private readonly string BASE_URL = "http://104.237.13.63/asym/unity.php";

    private Rigidbody m_body = null;
    private int m_id = -1;
    private bool m_isRegistered = false;
    private int m_connectTime = 0;

    enum Direction
    {
        E = 0,
        NE = 1,
        N = 2,
        NW = 3,
        W = 4,
        SW = 5,
        S = 6,
        SE = 7
    }

    private string Heading {
        get {
            var vel2 = new Vector2(m_body.velocity.x, m_body.velocity.z).normalized;
            if (vel2.magnitude < Mathf.Epsilon)
                return "--";

            var angle = Mathf.Atan2(vel2.y, vel2.x);
            var octant = Mathf.RoundToInt((8 * angle / (2 * Mathf.PI)) + 8) % 8;
            var dir = (Direction)octant;

            return $"{dir,2}";
        }
    }

    protected override void TickUpdate() {
        if (!m_isRegistered)
            return;

        var x = transform.position.x.ToString("F2");
        var z = transform.position.z.ToString("F2");
        var vel2 = new Vector2(m_body.velocity.x, m_body.velocity.z);
        var speed = vel2.magnitude.ToString("F2");
        var normVel2 = vel2.normalized;
        var headingX = normVel2.x.ToString("F2");
        var headingZ = normVel2.y.ToString("F2");
        var time = Mathf.Ceil(m_connectTime + Time.time);
        var url = BASE_URL + $"?action=add&id={m_id}&x={x}&z={z}&time={time}&heading='{Heading}'&head_x={headingX}&head_z={headingZ}&speed={speed}";
        var updateServer = AsymWeb.instance.Request(url);
        StartCoroutine(updateServer);
    }

    private void Awake() {
        m_body = GetComponent<Rigidbody>();
    }

    protected override void Start() {
        base.Start();

        if( m_registeringText != null )
            m_registeringText.enabled = true;

        var url = BASE_URL + "?action=user&name=bob";
        var login = AsymWeb.instance.Request(url, Register);
        StartCoroutine(login);
    }

    protected override void Update() {
        base.Update();
        if (m_headingText != null)
            m_headingText.text = Heading;
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
