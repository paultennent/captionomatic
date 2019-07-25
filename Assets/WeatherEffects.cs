using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEffects : MonoBehaviour
{
    float killTime=0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(killTime>0)
        {
            killTime-=Time.deltaTime;
            if(killTime<0)
            {
                snow.active=false;
            }
        }
    }
    
    public GameObject snow;
    
    public void StartSnow(string str,OscMessage msg)
    {
        snow.active=true;
        var em=snow.GetComponent<ParticleSystem>().emission;
        em.enabled=true;
        killTime=0;
    }
    
    public void StopSnow(string str,OscMessage msg)
    {
        var em=snow.GetComponent<ParticleSystem>().emission;
        em.enabled=false;
        killTime=5f;
    }
}
