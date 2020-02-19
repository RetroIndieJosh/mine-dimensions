using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float m_expandTimeSec = 0.5f;
    [SerializeField] private float m_expandSpeed = 0.1f;

    private bool m_isExploded = false;

    private IEnumerator Expand() {
        var expandTimeElapsed = 0f;
        while (expandTimeElapsed < m_expandTimeSec) {
            transform.localScale += Vector3.one * m_expandSpeed * Time.deltaTime;
            expandTimeElapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Explode() {
        if (m_isExploded)
            return;
        m_isExploded = true;
        StartCoroutine(Expand());
    }
}
