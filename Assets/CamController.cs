using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float camSpeed;
    public float zoomSpeed;

    private Vector3 dir;
    private int zoom;
    public float minSize;
    public float maxSize;


    void Start() {

    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) dir.x = -1;
        if (Input.GetKeyDown(KeyCode.D)) dir.x =  1;
        if (Input.GetKeyDown(KeyCode.W)) dir.y =  1;
        if (Input.GetKeyDown(KeyCode.S)) dir.y = -1;

        if (Input.GetKeyUp(KeyCode.A) && dir.x == -1) dir.x = 0;
        if (Input.GetKeyUp(KeyCode.D) && dir.x ==  1) dir.x = 0;
        if (Input.GetKeyUp(KeyCode.W) && dir.y ==  1) dir.y = 0;
        if (Input.GetKeyUp(KeyCode.S) && dir.y == -1) dir.y = 0;

        var pos = GetComponent<Transform>().position;
        pos += Time.deltaTime * camSpeed * dir;
        GetComponent<Transform>().position = pos;


        if (Input.GetKeyDown(KeyCode.Q)) zoom = -1;
        if (Input.GetKeyDown(KeyCode.E)) zoom =  1;

        if (Input.GetKeyUp(KeyCode.Q) && zoom == -1) zoom = 0;
        if (Input.GetKeyUp(KeyCode.E) && zoom ==  1) zoom = 0;

        var sz = GetComponent<Camera>().orthographicSize;
        sz += Time.deltaTime * zoomSpeed * zoom;
        if (sz < minSize) sz = minSize;
        if (sz > maxSize) sz = maxSize;

        GetComponent<Camera>().orthographicSize = sz;
    }
}
