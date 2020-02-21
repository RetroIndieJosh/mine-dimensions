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
    [SerializeField] private float m_minPos = -100f;
    [SerializeField] private float m_maxPos = 100f;
    [SerializeField] private GameObject m_goalPrefab = null;

    private readonly string BASE_URL = "http://104.237.13.63/asym/unity.php";

    public int CurTime => Mathf.CeilToInt(m_connectTime + timeElapsed);
    public int Id {
        get; private set;
    }

    private Rigidbody m_body = null;
    private int m_connectTime = 0;
    private Vector3 m_goalPos = Vector3.zero;
    private GameObject m_goal = null;
    private bool m_isRegistered = false;
    private int m_score = 0;
    private int m_highScore = 0;

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

    private void ResetPos() {
        var pos = Vector3.zero;
        pos.x = Random.Range(m_minPos, m_maxPos);
        pos.z = Random.Range(m_minPos, m_maxPos);
        pos.y = GetFloor(pos);
        transform.position = pos;
    }

    public void Die() {
        m_score = 0;
        m_isRegistered = false;
        TryRegister();
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
        TryRegister();
    }

    private void TryRegister() {
        GenerateGoal();
        ResetPos();

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
        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, m_minPos, m_maxPos);
        pos.z = Mathf.Clamp(pos.z, m_minPos, m_maxPos);
        transform.position = pos;

        base.Update();
        if (m_playerInfoText != null) {
            var x = transform.position.x;
            var z = transform.position.z;
            var distance = Vector3.Distance(transform.position, m_goalPos);
            m_playerInfoText.text = $"ID: {Id}\n{x:000.0}, {z:000.0}\n{distance:000}m\n{Heading}\n"
                + $"Score: {m_score:000} ({m_highScore:000})";
        }

        if (Vector3.Distance(transform.position, m_goalPos) < 1f) {
            ++m_score;
            if (m_score > m_highScore) {
                m_highScore = m_score;
                var request = new Request() {
                    args = {
                        { "action", "score" },
                        {"id", Id.ToString() },
                        {"score", m_score.ToString() }
                    }
                };
                var updateScore = AsymWeb.instance.ProcessRequest(request);
                StartCoroutine(updateScore);
            }
            Destroy(m_goal);
            GenerateGoal();
        }
    }

    private float GetFloor(Vector3 pos) {
        Physics.Raycast(pos, Vector3.down, out var hit);
        return hit.point.y;
    }

    private void GenerateGoal() {
        if (m_goal != null)
            Destroy(m_goal);
        m_goalPos = Vector3.zero;
        m_goalPos.x = Random.Range(m_minPos, m_maxPos);
        m_goalPos.z = Random.Range(m_minPos, m_maxPos);
        m_goalPos.y = GetFloor(m_goalPos);
        m_goal = Instantiate(m_goalPrefab, m_goalPos, Quaternion.identity);
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
