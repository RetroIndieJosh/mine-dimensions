using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private SphereCollider m_killSphere = null;
    [SerializeField] private float m_explodeTime = 2f;
    [SerializeField] private float m_explodeScale = 4f;

    public int Id {
        get; set;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log($"Mine #{Id} triggered");
        var request = new Request() {
            args = new Dictionary<string, string>() {
                {"action", "deactivate" },
                {"id", Id.ToString() },
                {"target", PlayerReporter.instance.Id.ToString() }
            }
        };
        var report = AsymWeb.instance.ProcessRequest(request);
        StartCoroutine(report);
        StartCoroutine(Explode());
    }

    private IEnumerator Explode() {
        var collider = GetComponent<SphereCollider>();
        collider.enabled = false;

        var originalScale = m_killSphere.transform.localScale;
        var targetScale = Vector3.one * collider.radius * 2f * m_explodeScale;
        var timeElapsed = 0f;
        while (timeElapsed < m_explodeTime) {
            var t = timeElapsed / m_explodeTime;
            m_killSphere.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
