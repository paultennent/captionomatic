using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    Quaternion startRot;
    Vector3 startPos;

    bool spinning = false;
    Vector3 axis;
    float rate;
    Vector3 hub;

    float curAngle;

    // Start is called before the first frame update
    void Start()
    {
        startRot = transform.rotation;
        startPos = transform.position;
        hub = GetComponent<Renderer>().bounds.center;
    }

    // Update is called once per frame
    void Update()
    {
        if (spinning)
        {
            transform.RotateAround(hub, axis, rate * Time.deltaTime);
            curAngle += rate * Time.deltaTime;
            if(curAngle > 360f)
            {
                curAngle -= 360f;
            }
        }
    }

    public void startSpinning(string s, OscMessage m)
    {
        curAngle = 0f;
        float rate = 1;
        if (m.values.Count > 0)
        {
            float.TryParse(m.values[0].ToString(), out rate);
        }
        axis = transform.up;
        this.rate = rate;
        spinning = true;
    }

    public void stopSpinning(string s, OscMessage m)
    {
        float rate = 1;
        if (m.values.Count > 0)
        {   
            float.TryParse(m.values[0].ToString(), out rate);
        }

        spinning = false;
        StartCoroutine(RotateAround(gameObject, hub, axis, 360-curAngle, rate));
    }

    IEnumerator RotateAround(GameObject gameobject, Vector3 point, Vector3 axis, float angle, float inTimeSecs)
    {
        float currentTime = 0.0f;
        float angleDelta = angle / inTimeSecs; //how many degress to rotate in one second
        float ourTimeDelta = 0;
        while (currentTime < inTimeSecs)
        {
            currentTime += Time.deltaTime;
            ourTimeDelta = Time.deltaTime;
            //Make sure we dont spin past the angle we want.
            if (currentTime > inTimeSecs)
                ourTimeDelta -= (currentTime - inTimeSecs);
            gameObject.transform.RotateAround(point, axis, angleDelta * ourTimeDelta);
            yield return null;
        }
        transform.rotation = startRot;
        transform.position = startPos;
    }


}
