using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AngleKeys : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
            {
                // editing name of preset
                // in input field, ignore keyboard
                return;
            }
            if (Input.GetKey("-"))
            {
                Camera.main.fieldOfView += Time.deltaTime * 5.0f;
            }
            else if (Input.GetKey("="))
            {
                Camera.main.fieldOfView -= Time.deltaTime * 5.0f;
            }
            else if (Input.GetKey("q"))
            {
                GetComponent<Transform>().position += new Vector3(0, Time.deltaTime * 1f, 0);
            }
            else if (Input.GetKey("e"))
            {
                GetComponent<Transform>().position -= new Vector3(0, Time.deltaTime * 1f, 0);
            }
            else if (Input.GetKey("w"))
            {
                GetComponent<Transform>().position += new Vector3(0, 0, Time.deltaTime * 1f);
            }
            else if (Input.GetKey("s"))
            {
                GetComponent<Transform>().position -= new Vector3(0, 0, Time.deltaTime * 1f);
            }
            else if (Input.GetKey("a"))
            {
                GetComponent<Transform>().position -= new Vector3(Time.deltaTime * 1f, 0, 0);
            }
            else if (Input.GetKey("d"))
            {
                GetComponent<Transform>().position += new Vector3(Time.deltaTime * 1f, 0, 0);
            }
            else if (Input.GetKey("up"))
            {
                GetComponent<Transform>().rotation *= Quaternion.Euler(Time.deltaTime * 30f, 0, 0);
            }
            else if (Input.GetKey("down"))
            {
                GetComponent<Transform>().rotation *= Quaternion.Euler(-Time.deltaTime * 30f, 0, 0);
            }
            else if (Input.GetKey("left"))
            {
                GetComponent<Transform>().rotation *= Quaternion.Euler(0, -Time.deltaTime * 30f, 0);
            }
            else if (Input.GetKey("right"))
            {
                GetComponent<Transform>().rotation *= Quaternion.Euler(0, Time.deltaTime * 30f, 0);
            }
            // }else if(Input.GetKey("["))
            // {
            // GetComponent<Transform>().rotation*=Quaternion.Euler(0,0,-Time.deltaTime*30f);
            // }else if(Input.GetKey("]"))
            // {
            // GetComponent<Transform>().rotation*=Quaternion.Euler(0,0,Time.deltaTime*30f);
            // }
        }
    }
}
