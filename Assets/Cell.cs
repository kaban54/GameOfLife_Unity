using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int pos;

    void OnMouseDown() {
        GameOfLife.gameRef.ClickCell(pos.x, pos.y);
        // Debug.Log("Cell Clicked");
    }
}
