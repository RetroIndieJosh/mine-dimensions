using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombGenerator : TickUpdateComponent
{
    [SerializeField] private Bomb m_bombPrefab = null;

    protected override void TickUpdate() {
        //var getBombs = AsymWeb.instance.Request();
    }

    private void GenerateBomb(float a_x, float a_z) {
        // TODO find floor and put it there
        Instantiate(m_bombPrefab, new Vector3(a_x, 0f,a_z), Quaternion.identity);
    }

    private void ProcessBombs(string a_result) {
    }
}
