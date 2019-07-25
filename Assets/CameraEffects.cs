using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraEffects : MonoBehaviour
{
    public GameObject fader;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SetBrightness(string s,OscMessage m)
    {
        if(m.values.Count>0)
        {
            print("WOOO"+s);
            float b=255;
            float.TryParse(m.values[0].ToString(),out b);
            fader.GetComponent<Image>().material.color =new Color(0,0,0,1f-(b/255.0f));


        }
    }
}
