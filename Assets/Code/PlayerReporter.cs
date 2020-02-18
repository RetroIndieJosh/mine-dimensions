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

            var degAngle = (Mathf.Rad2Deg * angle) + 180f;

            return $"{dir,2} {degAngle:000}";
        }
    }

    protected override void TickUpdate() {
        if (!m_isRegistered)
            return;

        var vel2 = new Vector2(m_body.velocity.x, m_body.velocity.z);
        var normVel2 = vel2.normalized;
        var request = new Request {
            args = new Dictionary<string, string>() {
                {"action", "add" },
                {"id", m_id.ToString() },
                {"x", transform.position.x.ToString("F2") },
                {"z", transform.position.z.ToString("F2") },
                {"speed", normVel2.magnitude.ToString("F2") },
                {"head_x", normVel2.x.ToString("F2") },
                {"head_z", normVel2.y.ToString("F2") },
                {"time", Mathf.Ceil(m_connectTime + Time.time).ToString() },
                {"heading", Heading }
            }
        };

        var updateServer = AsymWeb.instance.ProcessRequest(request);
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
        var request = new Request {
            args = new Dictionary<string, string>() {
                {"action", "user" },
                {"name", "bob" }
            }
        };
        var login = AsymWeb.instance.ProcessRequest(request, Register);
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
