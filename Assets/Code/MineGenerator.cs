using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MineGenerator : TickUpdateComponent
{
    [SerializeField] private Mine m_minePrefab = null;

    private List<Mine> m_mineList = new List<Mine>();

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
        // TODO clamp to ground
        var pos = new Vector3(a_posX, 0f, a_posZ);
        var mine = Instantiate(m_minePrefab, pos, Quaternion.identity);
        mine.Id = a_id;
        m_mineList.Add(mine);
    }

    private void ProcessMines(string a_result) {
        var entries = a_result.Split('\n');
        for (var line = 0; line < entries.Length; ++line) {
            var entry = entries[line].Trim();
            if (string.IsNullOrEmpty(entry))
                continue;
            var tokens = entry.Split(' ');
            if (tokens.Length != 3) {
                Debug.LogError($"Incorrect args from shots page line {line}: expected 3, got {tokens.Length}");
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

            var id = values[0];

            var hasId = false;
            foreach (var other in m_mineList) {
                if (other.Id != id)
                    continue;
                hasId = true;
                break;
            }
            if (hasId)
                return;

            var x = values[1];
            var z = values[2];
            GenerateMine(id, x, z);
        }
    }
}
