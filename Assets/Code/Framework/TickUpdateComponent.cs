﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickUpdateComponent : TimedComponent
{
    [SerializeField] private float m_tickLengthSec = 1f;

    protected virtual void TickUpdate() { }

    protected override void Update() {
        base.Update();
        if (timeElapsed < m_tickLengthSec)
            return;
        TickUpdate();
        ResetTimer();
    }
}
