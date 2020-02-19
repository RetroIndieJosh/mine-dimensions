using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MineGenerator : TickUpdateComponent
{
    [SerializeField] private Mine m_minePrefab = null;

    private Dictionary<int, Mine> m_mineDict = new Dictionary<int, Mine>();

    protected override void TickUpdate() {
        var request = new Request() {
            args = new Dictionary<string, string>() {
                {"action", "mines" },
            }
        };
        var getMines = AsymWeb.instance.ProcessRequest(request, ProcessMines);
        StartCoroutine(getMines);
    }

    private void GenerateMine(int a_id, float a_posX, float a_posZ) {
        var pos = new Vector3(a_posX, 100f, a_posZ);
        // TODO clamp to ground
        if (Physics.Raycast(pos, Vector3.down, out var hit) == false)
            return;
        Debug.Log($"Ground position: {hit.point}");
        pos.y = hit.point.y;
        Debug.Log($"Generate mine {a_id} at {pos}");
        var mine = Instantiate(m_minePrefab, pos, Quaternion.identity);
        mine.Id = a_id;
        m_mineDict.Add(a_id, mine);
    }

    private void ProcessMine(string a_entry) {
        if (string.IsNullOrEmpty(a_entry))
            return;
        var tokens = a_entry.Split(' ');
        if (tokens.Length != 4) {
            Debug.LogError($"Incorrect args from shots page line '{a_entry}': expected 4, got {tokens.Length}");
            return;
        }

        var values = new int[tokens.Length];
        for (var i = 0; i < tokens.Length; ++i) {
            if (int.TryParse(tokens[i], out var v) == false) {
                Debug.LogError($"Unable to parse value {i} on line '{a_entry}' from shots page as int ({tokens[i]})");
                continue;
            }
            values[i] = v;
        }

        var id = values[0];

        var isActive = values[1];
        if (isActive == 0) {
            if (m_mineDict.ContainsKey(id)) {
                Debug.Log($"Remove mine {id}");
                Destroy(m_mineDict[id].gameObject);
                m_mineDict.Remove(id);
            }
            return;
        }

        if (m_mineDict.ContainsKey(id))
            return;

        var x = values[2];
        var z = values[3];
        GenerateMine(id, x, z);
    }

    private void ProcessMines(string a_result) {
        var entries = a_result.Split('\n');
        for (var line = 0; line < entries.Length; ++line) {
            var entry = entries[line].Trim();
            ProcessMine(entry);
        }
    }
}
