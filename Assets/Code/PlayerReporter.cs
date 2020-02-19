using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PlayerReporter : TickUpdateComponent
{
    static public PlayerReporter instance = null;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI m_playerInfoText = null;
    [SerializeField] private TextMeshProUGUI m_registeringText = null;

    [Header("Game")]
    [SerializeField] private Vector3 m_goal;

    private readonly string BASE_URL = "http://104.237.13.63/asym/unity.php";

    public int CurTime => Mathf.CeilToInt(m_connectTime + timeElapsed);
    public int Id {
        get; private set;
    }

    private Rigidbody m_body = null;
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
                return "-- 000";

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
                {"id", Id.ToString() },
                {"x", transform.position.x.ToString("F2") },
                {"z", transform.position.z.ToString("F2") },
                {"speed", normVel2.magnitude.ToString("F2") },
                {"head_x", normVel2.x.ToString("F2") },
                {"head_z", normVel2.y.ToString("F2") },
                {"time", CurTime.ToString() },
                {"heading", Heading }
            }
        };

        var updateServer = AsymWeb.instance.ProcessRequest(request);
        StartCoroutine(updateServer);
    }

    private void Awake() {
        if (instance != null)
            Destroy(instance.gameObject);
        instance = this;
        m_body = GetComponent<Rigidbody>();
    }

    protected override void Start() {
        base.Start();

        var min = -100f;
        var max = 100f;

        var x = Random.Range(min, max);
        var y = transform.position.y;
        var z = Random.Range(min, max);
        m_goal = new Vector3(x, y, z);

        x = Random.Range(min, max);
        y = transform.position.y;
        z = Random.Range(min, max);
        transform.position = new Vector3(x, y, z);

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
        if (m_playerInfoText != null) {
            var x = transform.position.x;
            var z = transform.position.z;
            var distance = Vector3.Distance(transform.position, m_goal);
            m_playerInfoText.text = $"({x:00.0}, {z:00.0})\n{distance:000}m\n{Heading}";
        }
    }

    private void Register(string a_result) {
        var tokens = a_result.Split(' ');
        if (int.TryParse(tokens[0], out var id) == false) {
            Debug.LogError($"Got non-int for user ID: {id}");
            return;
        }
        Id = id;

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
