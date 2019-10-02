using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using OpenCvSharp;

public class ClickMover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        points=new Vector2[targetPoints.Length];
        foreach(Renderer r in mouseMarker.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled=false;
        }
        targetGroups=new Transform[selectables.Length];
        linkParents=new Transform[selectables.Length];
        for(int c=0;c<selectables.Length;c++)
        {
            linkParents[c]=null;
            targetGroups[c]=selectables[c].Find("targets");
            GetShapeMaterials(selectables[c],"shape");
            SetShapeMaterial(selectables[c],"shape",blackoutMaterial);
            // if this is a child of a 'faces' node - it is just one face - when we set it up we move the whole object (the faces node owner)
            if(selectables[c].parent!=null && selectables[c].parent.parent!=null && selectables[c].parent.name=="faces")
            {
                linkParents[c]=selectables[c].parent.parent;
            }            
        }        
    }

    void Awake()
    {
    }
    
    void GetShapeMaterials(Transform t,string name)
    {
        foreach(Transform ch in t)
        {
            if(ch.name!=name)
            {
                GetShapeMaterials(ch,name);
            }else
            {
                foreach(Renderer r in ch.GetComponentsInChildren<Renderer>())
                {
                    shapeMaterials[r]=r.material;
                }
            }
        }
    }
    
    void SetShapeMaterial(Transform t,string name,Material m)
    {
        foreach(Transform ch in t)
        {
            if(ch.name!=name)
            {
                SetShapeMaterial(ch,name,m);
            }else
            {
                foreach(Renderer r in ch.GetComponentsInChildren<Renderer>())
                {
                   r.material=m;
                }
            }
        }

    }
    
    void ResetShapeMaterials(Transform t,string name)
    {
        foreach(Transform ch in t)
        {
            if(ch.name!=name)
            {
                ResetShapeMaterials(ch,name);
            }else
            {
                foreach(Renderer r in ch.GetComponentsInChildren<Renderer>())
                {
                   r.material=shapeMaterials[r];
                }
            }
        }
    }

    public GameObject debugDroplet;
    
    public Material lineMaterial;
    public Material blackoutMaterial;
    
    public Camera mainCamera;

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;


    public int pointNum=0;
    
    Vector2 []points=new Vector2[4];
    
    Transform []targetPoints=new Transform[4];
    Dictionary<Renderer,Material> shapeMaterials=new Dictionary<Renderer,Material>();
    
    public Camera selectionCamera;
    
    public Transform[]selectables;
    Transform[]targetGroups; 
    public Transform[] linkParents;
    
    public Transform selectionMarker;
    public Transform mouseMarker;

    public float moveRate = 1f;
    public float rotateRate = 10f;

    Transform selected=null;
    int selectedIndex=-1;

    void SetShapeLayer(Transform t,string name,int layer)
    {
        foreach(Transform ch in t)
        {
            if(ch.name!=name)
            {
                SetShapeLayer(ch,name,layer);
            }else
            {
                foreach (Transform trans in ch.GetComponentsInChildren<Transform>(true))
                {
                    trans.gameObject.layer = layer;                
                }            
            }
        }
    }

    void SetSelection(int index)
    {
        
        if(selected!=null)
        {

            foreach (Transform trans in selected.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = 0;
            
            }            
            if(linkParents[selectedIndex]!=null)
            {
                SetShapeMaterial(linkParents[selectedIndex],"shape",blackoutMaterial);
            }else
            {
                SetShapeMaterial(selected,"shape",blackoutMaterial);
            }
        }
        selectedIndex=index;
        if(index==-1)
        {
            selected=null;
        }else
        {
            selected=selectables[index];
        }
        if(selected!=null)
        {
            SetShapeLayer(selected,"shape",8);
            if(linkParents[selectedIndex]!=null)
            {
                ResetShapeMaterials(linkParents[selectedIndex],"shape");
            }else
            {
                ResetShapeMaterials(selected,"shape");
            }
            foreach (Transform trans in selected.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = 8;
            }
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach(Renderer r in selected.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(r.bounds);
            }

            selectionCamera.GetComponent<Transform>().parent=selected;
            selectionCamera.GetComponent<Transform>().localPosition=new Vector3(0,0.05f,-5);
            selectionCamera.GetComponent<Transform>().localRotation=Quaternion.identity;
            selectionCamera.enabled=true;
            foreach(Renderer r in selectionMarker.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled=true;
            }
            foreach(Renderer r in mouseMarker.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled=true;
            }
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }else
        {
            foreach(Renderer r in selectionMarker.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled=false;
            }
            foreach(Renderer r in mouseMarker.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled=false;
            }
            
            // find the path
            


            selectionCamera.enabled=false;
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }
    }

    void MapSelectedObject()
    {
        Transform moveTransform=selected;
        if(linkParents[selectedIndex]!=null)
        {
            moveTransform=linkParents[selectedIndex];
        }
        
        moveTransform.position=Vector3.zero;
        moveTransform.rotation=Quaternion.identity;
        // convert to opencv types

        int numPoints=0;
        for(int c=0;c<targetPoints.Length;c++)
        {
            if(points[c].x>-900 || points[c].y>-900)
            {
                numPoints+=1;
            }
        }
        Point3f[] targets=new Point3f[numPoints];
        Point2f[] pts=new Point2f[numPoints];
        pointNum=0;
        
        for(int c=0;c<targetPoints.Length;c++)
        {
            if(points[c].x>-900 || points[c].y>-900)
            {
                targets[pointNum]=new Point3f(targetPoints[c].position.x,targetPoints[c].position.y,targetPoints[c].position.z);
                pts[pointNum]=new Point2f(points[c].x,points[c].y);
                pointNum+=1;
            }                    
        }
        double[,] cameraMatrix=new double[3,3];
        
//                Mat rVec=new Mat(3,1, MatType.CV_64FC1,0);
//                Mat tranVec=new Mat(3,1, MatType.CV_64FC1,0);
//                Mat cameraMatrix = new Mat(3, 3, MatType.CV_64FC1,0);
        float focalLen=Camera.FieldOfViewToFocalLength(mainCamera.fieldOfView,(float)(mainCamera.pixelHeight));
        float cx=mainCamera.pixelWidth*0.5f;
        float cy=mainCamera.pixelHeight*0.5f;
        cameraMatrix[0,2]=cx;
        cameraMatrix[1,2]=cy;
        cameraMatrix[0,0]=focalLen;
        cameraMatrix[1,1]=focalLen;
        
        double[] rVec=new double[3];
        double[] tranVec=new double[3];
        double[,]rotMatrix=new double[3,3];
        double[] distMatrix={0,0,0,0};
        
        Cv2.SolvePnP(targets,pts,cameraMatrix,distMatrix,ref rVec,ref tranVec);
        
       
        Cv2.Rodrigues(rVec,out rotMatrix);
        
        
        Matrix4x4 unityRotation=new Matrix4x4();
        Vector3 unityTranslation=new Vector3((float)tranVec[0],(float)tranVec[1],(float)tranVec[2]);
        for(int r=0;r<3;r++)
        {
            for(int c=0;c<3;c++)
            {
                unityRotation[r,c]=(float)rotMatrix[r,c];
            }                    
        }
        unityRotation[3,3]=1;
        Quaternion qRot=unityRotation.rotation;
        moveTransform.position=unityTranslation;
        moveTransform.rotation=qRot;
        
        
        print(unityRotation+ ":"+unityTranslation);
        
        
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if( EventSystem.current.currentSelectedGameObject!=null && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>()!=null)
        {
            // editing name of preset
            // in input field, ignore keyboard
            return;
        }
        // mouse position corrected for multi-displays
        Vector3 pos=Input.mousePosition; 
        if(Display.displays.Length>1 && Display.displays[1].active)
        {
            if(pos.x>Display.displays[0].renderingWidth)
            {
                pos.x-=Display.displays[0].renderingWidth;
                pos.y/=((float)Display.displays[0].renderingHeight/(float)Display.displays[1].renderingHeight);
            }
        }
        if(selected!=null)
        {
            float depth=mouseMarker.position.z;
            
            mouseMarker.position=mainCamera.ViewportToWorldPoint(new Vector3(pos.x/mainCamera.pixelWidth,pos.y/mainCamera.pixelHeight,depth));
        }
        for(int c=0;c<selectables.Length && c<targetGroups.Length;c++)
        {
            if(Input.GetKeyDown(""+(c+1)))
            {
                SetSelection(c);
                targetPoints=new Transform[targetGroups[c].childCount];
                for(int d=0;d<targetPoints.Length;d++)
                {
                    targetPoints[d]=targetGroups[c].GetChild(d);
                }
                points=new Vector2[targetPoints.Length];
                pointNum=0;
            }
        }
        if(Input.GetKeyDown("0"))
        {
            SetSelection(-1);
        }
        if(targetPoints.Length!=points.Length)
        {
            points=new Vector2[targetPoints.Length];
            pointNum=0;
        }
        
        if(selectionMarker!=null && selected!=null && pointNum<targetPoints.Length)
        {
            selectionMarker.position=targetPoints[pointNum].position;
        }

        if(Input.GetKeyDown("x") && selected!=null)
        {
            points[pointNum]=new Vector2(-999f,-999f);
            pointNum+=1;
            if(pointNum==targetPoints.Length)
            {
                MapSelectedObject();
                
                pointNum=0;
                SetSelection(-1);

            }
        }

        if(Input.GetKeyDown(KeyCode.Delete) && selected!=null)
        {
            // this flat is offstage
            Transform moveTransform=selected;
            if(linkParents[selectedIndex]!=null)
            {
                moveTransform=linkParents[selectedIndex];
            }
            
            moveTransform.position=new Vector3(0,0,-999);
            moveTransform.rotation=Quaternion.identity;
            
        }

        //allow for nudging
        if (Input.GetKey(KeyCode.LeftShift) && selected != null)
        {
            Transform moveTransform = selected;
            if (linkParents[selectedIndex] != null)
            {
                moveTransform = linkParents[selectedIndex];
            }

            //rotate if control is held
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    moveTransform.Rotate(moveTransform.right * Time.deltaTime * rotateRate);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    moveTransform.Rotate(-moveTransform.right * Time.deltaTime * rotateRate);
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    moveTransform.Rotate(moveTransform.up * Time.deltaTime * rotateRate);
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    moveTransform.Rotate(-moveTransform.up * Time.deltaTime * rotateRate);
                }
                if (Input.GetKey(KeyCode.Comma))
                {
                    moveTransform.Rotate(-moveTransform.forward * Time.deltaTime * rotateRate);
                }
                if (Input.GetKey(KeyCode.Period))
                {
                    moveTransform.Rotate(moveTransform.forward * Time.deltaTime * rotateRate);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    moveTransform.Translate(moveTransform.up * Time.deltaTime * moveRate);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    moveTransform.Translate(-moveTransform.up * Time.deltaTime * moveRate);
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    moveTransform.Translate(-moveTransform.right * Time.deltaTime * moveRate);
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    moveTransform.Translate(moveTransform.right * Time.deltaTime * moveRate);
                }
                if (Input.GetKey(KeyCode.Comma))
                {
                    moveTransform.Translate(moveTransform.forward * Time.deltaTime * moveRate);
                }
                if (Input.GetKey(KeyCode.Period))
                {
                    moveTransform.Translate(-moveTransform.forward * Time.deltaTime * moveRate);
                }
            }

        }


        
        //print(mainCamera.ViewportToScreenPoint(new Vector3(0,0,0))+":"+Input.mousePosition+":"+mainCamera.ScreenToViewportPoint(Input.mousePosition));
        if(Input.GetMouseButtonDown(0) && selected!=null)
        {
            points[pointNum]=pos;
            if(debugDroplet)
            {
                GameObject newObj=Instantiate(debugDroplet);
                newObj.transform.position=mainCamera.ScreenToWorldPoint(new Vector3(pos.x,pos.y,0.5f));
            }
            pointNum+=1;
            if(pointNum==targetPoints.Length)
            {
                MapSelectedObject();
                
                pointNum=0;
                SetSelection(-1);
                
                
            }
        }
    }
}
