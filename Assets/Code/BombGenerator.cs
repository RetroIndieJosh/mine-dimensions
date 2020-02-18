using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BombGenerator : TickUpdateComponent
{
    [SerializeField] private Bomb m_bombPrefab = null;

    class BombInfo
    {
        public int id;
        public Vector3 pos;
        public int time;
        public bool fired = false;

        public override string ToString() {
            return $"#{id} ({pos.x}, {pos.z}) @{time}";
        }
    }

    List<BombInfo> m_bombInfoList = new List<BombInfo>();

    protected override void Update() {
        base.Update();
        foreach (var info in m_bombInfoList) {
            if (PlayerReporter.instance.CurTime > info.time)
                GenerateBomb(info);
        }
        m_bombInfoList.RemoveAll(i => i.fired);
    }

    protected override void TickUpdate() {
        var request = new Request() {
            args = new Dictionary<string, string>() {
                {"action", "shots" },
                {"time", PlayerReporter.instance.CurTime.ToString() }
            }
        };
        var getBombs = AsymWeb.instance.ProcessRequest(request, ProcessBombs);
        StartCoroutine(getBombs);
    }

    private void GenerateBomb(BombInfo a_info) {
        Debug.Log($"Generate bomb {a_info}");
        Instantiate(m_bombPrefab, a_info.pos, Quaternion.identity);
    }

    private void ProcessBombs(string a_result) {
        var entries = a_result.Split('\n');
        for (var line = 0; line < entries.Length; ++line) {
            var entry = entries[line].Trim();
            if (string.IsNullOrEmpty(entry))
                continue;
            var tokens = entry.Split(' ');
            if (tokens.Length != 4) {
                Debug.LogError($"Incorrect args from shots page line {line}: expected 4, got {tokens.Length}");
                continue;
            }

            var values = new int[tokens.Length];
            for( var i = 0; i < tokens.Length; ++i) {
                if (int.TryParse(tokens[i], out var v) == false) {
                    Debug.LogError($"Unable to parse #{i} on line {line} from shots page as int ({tokens[i]})");
                    continue;
                }
                values[i] = v;
            }

            var info = new BombInfo {
                time = Mathf.CeilToInt(values[0] + TickLengthSec),
                id = values[1],
                // TODO find floor and put it there
                pos = new Vector3(values[2], 0, values[3])
            };

            if (info.time < PlayerReporter.instance.CurTime) {
                Debug.Log($"Found bomb firing at {info.time} but it's too late - now is {PlayerReporter.instance.CurTime}");
                continue;
            }
            var hasId = false;
            foreach (var other in m_bombInfoList) {
                if (other.id != info.id)
                    continue;
                hasId = true;
                break;
            }
            if (hasId)
                return;

            Debug.Log($"Add bomb {info.ToString()}");
            m_bombInfoList.Add(info);
        }
    }
}
