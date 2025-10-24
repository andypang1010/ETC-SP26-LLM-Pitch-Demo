using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceCanvasUIManager : MonoBehaviour
{
    public GameObject target;
    GameObject worldSpaceCanvas;

    void Start () 
    {
        worldSpaceCanvas = GameObject.Find("WORLD SPACE CANVAS");
        target = transform.root.gameObject;
    }

    void Update()
    {
        if (target == null) {
            Destroy(gameObject);
        }

        else {
            if (target.CompareTag("Enemy")) {
                transform.position = target.transform.position + 0.9f * Vector3.up;
            }
            else {
                transform.position = target.transform.position;
            }
        }

        transform.LookAt(Camera.main.transform, Vector3.up);
        transform.SetParent(worldSpaceCanvas.transform);
    }
}
