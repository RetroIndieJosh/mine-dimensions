using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : TimedComponent
{
    [SerializeField] private float m_expandTimeSec = 0.5f;
    [SerializeField] private float m_expandSpeed = 0.1f;
    [SerializeField] private int m_explodeTime = int.MaxValue;

    protected override void Update() {
        base.Update();
        if (timeElapsed >= m_explodeTime)
            Explode();
    }

    private IEnumerator Expand() {
        var expandTimeElapsed = 0f;
        while (expandTimeElapsed < m_expandTimeSec) {
            transform.localScale += Vector3.one * m_expandSpeed * Time.deltaTime;
            expandTimeElapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private void Explode() {
        StartCoroutine(Expand());
    }
}
