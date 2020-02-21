using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        var player = collision.gameObject.GetComponent<PlayerReporter>();
        if (player == null)
            return;
        PlayerReporter.instance.Die();
    }
}
