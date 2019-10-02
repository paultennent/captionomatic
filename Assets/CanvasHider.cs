using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHider : MonoBehaviour
{

    Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H)){
            canvas.enabled = !canvas.enabled;
        }
    }
}
