using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedComponent : MonoBehaviour
{
    protected float timeElapsed {
        get; private set;
    }

    protected void ResetTimer() {
        timeElapsed = 0f;
    }
    
    protected virtual void Start() {
        ResetTimer();
    }

    protected virtual void Update() {
        timeElapsed += Time.deltaTime;
    }
}
