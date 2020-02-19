using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [SerializeField] private float m_speed = 1f;

    void Update() {
        transform.Rotate(Vector3.up, Time.deltaTime * m_speed);
    }
}
