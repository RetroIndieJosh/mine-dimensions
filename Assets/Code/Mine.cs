using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    public int Id {
        get; set;
    }

    private Explosion m_explosion = null;

    private void Awake() {
        m_explosion = GetComponent<Explosion>();
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Mine #{Id} triggered");
        PlayerReporter.instance.ResetScore();
        var request = new Request() {
            args = new Dictionary<string, string>() {
                {"action", "deactivate" },
                {"id", Id.ToString() },
                {"target", PlayerReporter.instance.Id.ToString() }
            }
        };
        var report = AsymWeb.instance.ProcessRequest(request, (str) => { Destroy(gameObject); });
        StartCoroutine(report);
    }
}
