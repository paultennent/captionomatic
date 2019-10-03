using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MediaArea : MonoBehaviour
{
    public string mediaName="";
    
    public float targetW=1.0f;
    public float targetH=1.0f;
    public bool matchAspect=true;
    public bool expand=true;
    public bool loop=false;
    string oldMediaName="XXXXX";
	

    public int axisU=0; // x -> U
    public bool flipU=false;
    public int axisV=1; // y ->V
    public bool flipV=false;
    public bool remapUVs=false;
    public bool autoMask=false;
    [Range(0, 1f)]
    public float edgeBlur = 0;
    float lastEdgeBlur = 0;

    public RenderTexture generatedMask;
    public Camera renderCamera;
    
    public Texture2D maskTexture;

    float [,] edgeDistanceMap;
    
    //RenderTexture videoRT;

    public float userFadeTime = 0f;

    private Texture2D blackout;
    private RenderTexture renderTex;
    private RenderTexture renderTex2;

    VideoPlayer vp;
    VideoPlayer vp2;
    Transform videoPlayerChild;
    Transform videoPlayerChild2;

    public Vector2 texSize = new Vector2(1920, 1080);

    public bool ignoreEdgeBlur = false;

    public VideoAspectRatio videoAspectRatio = VideoAspectRatio.Stretch;

    // Start is called before the first frame update
    void Start()
    {

        float scalar = 2048f/targetW;
        texSize = new Vector2(2048f, targetH * scalar);

        edgeDistanceMap = new float[512,512];
        if (remapUVs)
        {
            ApplyMeshUVs();
        }


        blackout = Texture2D.blackTexture;// = (Texture2D) GetComponent<Renderer>().material.GetTexture("_MainTex");

        GetComponent<Renderer>().material.SetTexture("_MaskTex",maskTexture);

        renderTex = new RenderTexture((int) texSize.x, (int) texSize.y, 0);
        videoPlayerChild = new GameObject("VideoPlayer").transform;
        videoPlayerChild.parent = transform;
        renderTex2 = new RenderTexture((int)texSize.x, (int)texSize.y, 0);
        videoPlayerChild2 = new GameObject("VideoPlayer2").transform;
        videoPlayerChild2.parent = transform;

        if (vp == null)
        {
            vp = videoPlayerChild.gameObject.AddComponent<VideoPlayer>();
        }
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.isLooping = false;
        vp.targetTexture = renderTex;
        vp.enabled = false;
        vp.aspectRatio = videoAspectRatio;

        if (vp2 == null)
        {
            vp2 = videoPlayerChild2.gameObject.AddComponent<VideoPlayer>();
        }
        vp2.renderMode = VideoRenderMode.RenderTexture;
        vp2.isLooping = false;
        vp2.targetTexture = renderTex2;
        vp2.enabled = false;
        vp2.aspectRatio = videoAspectRatio;

        TextureScale.Bilinear(blackout, (int) texSize.x, (int)texSize.y);
        GetComponent<Renderer>().material.SetTexture("_MainTex", blackout);


    }

    void AdjustTextureAspect()
    {
        if (vp != null && !vp.enabled)
        {
            if(vp.targetTexture==null && vp.width>0 && vp.height>0)
            {
                if(renderTex!=null) renderTex.Release();
                renderTex=new RenderTexture((int)vp.width,(int)vp.height,0,RenderTextureFormat.ARGB32);
                vp.targetTexture=renderTex;
                GetComponent<Renderer>().material.mainTexture=vp.targetTexture;
            }
        }
        if(GetComponent<Renderer>().material.mainTexture==null)
        {
            return;
        }
        float tw=GetComponent<Renderer>().material.GetTexture("_MainTex").width;
        float th=GetComponent<Renderer>().material.GetTexture("_MainTex").height;

        if (vp != null && !vp2.enabled)
        {
            if (vp2.targetTexture == null && vp2.width > 0 && vp2.height > 0)
            {
                if (renderTex2 != null) renderTex2.Release();
                renderTex2 = new RenderTexture((int)vp2.width, (int)vp2.height, 0, RenderTextureFormat.ARGB32);
                vp2.targetTexture = renderTex2;
                GetComponent<Renderer>().material.mainTexture = vp2.targetTexture;
            }
        }
        if (GetComponent<Renderer>().material.mainTexture == null)
        {
            return;
        }
        float tw2 = GetComponent<Renderer>().material.GetTexture("_SecondaryTex").width;
        float th2 = GetComponent<Renderer>().material.GetTexture("_SecondaryTex").height;

        if (matchAspect==false)
        {
            GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(1f,1.0f));
            GetComponent<Renderer>().material.SetTextureScale("_SecondaryTex", new Vector2(1f, 1.0f));
            GetComponent<Renderer>().material.SetTextureOffset("_MainTex",new Vector2(0,0));
            GetComponent<Renderer>().material.SetTextureOffset("_SecondaryTex", new Vector2(0, 0));
        }
        else
        {

            if(expand)
            {
                Debug.Log("doing expand");
                if(tw/th > targetW/targetH)
                {
                    float sx=(targetW/targetH)/(tw/th);
                    // height too big - clip left and right
                    GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(sx,1.0f));
                    GetComponent<Renderer>().material.SetTextureOffset("_MainTex",new Vector2(0.5f*(1f-sx),0f));
                    
                }else
                {
                    // width too big - clip top and bottom and scale vertically
                    float sy=(tw/th)/(targetW/targetH);
                    GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(1.0f,sy));
                    GetComponent<Renderer>().material.SetTextureOffset("_MainTex",new Vector2(0,0.5f*(1f-sy)));
                }

                if (tw2 / th2 > targetW / targetH)
                {
                    float sx2 = (targetW / targetH) / (tw2 / th2);
                    // height too big - clip left and right
                    GetComponent<Renderer>().material.SetTextureScale("_SecondaryTex", new Vector2(sx2, 1.0f));
                    GetComponent<Renderer>().material.SetTextureOffset("_SecondaryTex", new Vector2(0.5f * (1f - sx2), 0f));

                }
                else
                {
                    // width too big - clip top and bottom and scale vertically
                    float sy2 = (tw2 / th2) / (targetW / targetH);
                    GetComponent<Renderer>().material.SetTextureScale("_SecondaryTex", new Vector2(1.0f, sy2));
                    GetComponent<Renderer>().material.SetTextureOffset("_SecondaryTex", new Vector2(0, 0.5f * (1f - sy2)));
                }


            }
            else
            {
                Debug.Log("doing stretch");
                if(tw/th > targetW/targetH)
                {
                    // width too big - clip top and bottom and scale vertically
                    float sy=(tw/th)/(targetW/targetH);
                    GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(1.0f,sy));
                    GetComponent<Renderer>().material.SetTextureOffset("_MainTex",new Vector2(0,0.5f*(1f-sy)));
                    
                }else
                {
                    float sx=(targetW/targetH)/(tw/th);
                    // height too big - clip left and right
                    GetComponent<Renderer>().material.SetTextureScale("_MainTex",new Vector2(sx,1.0f));
                    GetComponent<Renderer>().material.SetTextureOffset("_MainTex",new Vector2(0.5f*(1f-sx),0f));
                }

                if (tw2 / th2 > targetW / targetH)
                {
                    // width too big - clip top and bottom and scale vertically
                    float sy2 = (tw2 / th2) / (targetW / targetH);
                    GetComponent<Renderer>().material.SetTextureScale("_SecondaryTex", new Vector2(1.0f, sy2));
                    GetComponent<Renderer>().material.SetTextureOffset("_SecondaryTex", new Vector2(0, 0.5f * (1f - sy2)));

                }
                else
                {
                    float sx2 = (targetW / targetH) / (tw2 / th2);
                    // height too big - clip left and right
                    GetComponent<Renderer>().material.SetTextureScale("_SecondaryTex", new Vector2(sx2, 1.0f));
                    GetComponent<Renderer>().material.SetTextureOffset("_SecondaryTex", new Vector2(0.5f * (1f - sx2), 0f));
                }
            }
        }
    }

    void UpdateEdgeBlur()
    {
        if (ignoreEdgeBlur)
        {
            return;
        }
        //if (maskTexture.format != TextureFormat.RGBA32)
        //{
        //    maskTexture = maskTexture.ChangeFormat(TextureFormat.RGBA32);
        //}
        Color32[] outPixels = new Color32[maskTexture.width*maskTexture.height];
        float boundaryWidth = edgeBlur * 32.0f;
        for (int y = 0; y < maskTexture.height; y++)
        {
            for (int x = 0; x < maskTexture.width; x++)
            {
                outPixels[x + y * maskTexture.height] = new Color32(255,255,255,255);
                if (edgeDistanceMap[x, y] < boundaryWidth)
                {
                    float alpha = 255;
                    alpha *= (edgeDistanceMap[x, y] / boundaryWidth);
                    outPixels[x + y * maskTexture.height].a = (byte)alpha;
                }
            }
        }
        print(gameObject.name);
        Debug.Log(maskTexture.graphicsFormat);
        maskTexture.SetPixels32(outPixels); // removed to get rid of errors
        maskTexture.Apply();
//        GetComponent<Renderer>().material.SetTexture("_MaskTex", maskTexture);

    }

    public void moveTexToSecondary()
    {
        GetComponent<Renderer>().material.SetTexture("_SecondaryTex", GetComponent<Renderer>().material.GetTexture("_MainTex"));
        GetComponent<Renderer>().material.SetFloat("_Blend", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (lastEdgeBlur != edgeBlur)
        {
            UpdateEdgeBlur();
        }
        //AdjustTextureAspect();
        if (oldMediaName != mediaName)
        {
            oldMediaName = mediaName;
            if (mediaName.EndsWith(".mp4"))
            {
                moveTexToSecondary();
                if (!vp.enabled)
                {
                    vp.enabled = true;
                    vp.isLooping = loop;
                    vp.url = Path.GetFullPath(mediaName);
                    vp.Stop();
                    vp.Play();
                    GetComponent<Renderer>().material.SetTexture("_MainTex", vp.targetTexture);
                }
                else
                {
                    vp2.enabled = true;
                    vp2.isLooping = loop;
                    vp2.url = Path.GetFullPath(mediaName);
                    vp2.Stop();
                    vp2.Play();
                    GetComponent<Renderer>().material.SetTexture("_MainTex", vp2.targetTexture);
                }
                StartCoroutine(doFade(GetComponent<Renderer>(), userFadeTime));
            }
            else if (mediaName.EndsWith(".png") || mediaName.EndsWith(".jpg"))
            {
               
                byte[] bytes = File.ReadAllBytes(mediaName);
                Texture2D texture = new Texture2D(2, 2);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.LoadImage(bytes);

                TextureScale.Bilinear(texture, (int) texSize.x, (int) texSize.y);

                // our aspect ratio                

                Renderer renderer = GetComponent<Renderer>();

                moveTexToSecondary();
                renderer.material.SetTexture("_MainTex", texture);
                //AdjustTextureAspect();
                StartCoroutine(doFade(renderer, userFadeTime));
                
                //renderer.material.mainTexture = texture;

            }
        }

        if (mediaName.Length != 0 && !vp.enabled || vp.isPlaying && vp.isPrepared || mediaName.Length != 0 && !vp2.enabled || vp2.isPlaying && vp2.isPrepared)
        {
            GetComponent<Renderer>().enabled = true;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    private void AdjustBlackoutTex()
    {
        Texture t = GetComponent<Renderer>().material.GetTexture("_MainTex");
        Texture2D b = new Texture2D(t.width, t.height);
        for (int y = 0; y < b.height; y++)
        {
            for (int x = 0; x < b.width; x++)
            {
                Color color = Color.black;
                b.SetPixel(x, y, color);
            }
        }
        b.Apply();
        blackout = b;
    }

    public void fadeOut(float fadeTime)
    {
        //AdjustBlackoutTex();
        moveTexToSecondary();
        GetComponent<Renderer>().material.SetTexture("_MainTex", blackout);
        StartCoroutine(doFade(GetComponent<Renderer>(), fadeTime));
    }
    
    public IEnumerator doFade(Renderer renderer, float fadeTime)
    {
        
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float newBlend = Mathf.Lerp(1f, 0f, t / fadeTime);
            renderer.material.SetFloat("_Blend", newBlend);
            yield return null;
        }
        
        if (renderer.material.GetTexture("_SecondaryTex") == renderTex)
        {
            vp.Stop();
            vp.enabled = false;
        }
        if (renderer.material.GetTexture("_SecondaryTex") == renderTex2)
        {
            vp2.Stop();
            vp2.enabled = false;
        }
        renderer.material.SetTexture("_SecondaryTex", blackout);

    }
    
    public void ApplyMeshUVs()
    {
        GetComponent<MeshFilter>().mesh=Instantiate(GetComponent<MeshFilter>().mesh);
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];

        int otherAxis=0;
        if(axisV==0 || axisU==0)
        {            
            otherAxis=1;
            if(axisV==1 || axisU==1)
            {
                otherAxis=2;
            }
        }


        float minU=vertices[0][axisU];
        float maxU=vertices[0][axisU];
        float minV=vertices[0][axisV];
        float maxV=vertices[0][axisV];
        for(int i=0;i<vertices.Length;i++)
        {
            minU=Mathf.Min(vertices[i][axisU],minU);
            maxU=Mathf.Max(vertices[i][axisU],maxU);
            minV=Mathf.Min(vertices[i][axisV],minV);
            maxV=Mathf.Max(vertices[i][axisV],maxV);
        }
        //print(minU + ":" + maxU + ":" + minV + ":" + maxV);
        if(minV==maxV )
        {
            //print("V axis has no differences, probably wrong one"+":"+gameObject.name);
        }
        if(minU==maxU)
        {
            //print("U axis has no differences, probably wrong one"+":"+gameObject.name);
        }
        float vMult=1/(maxV-minV);
        float uMult=1/(maxU-minU);
        for (int i = 0; i < uvs.Length; i++)
        {
            float u=vertices[i][axisU];
            float v=vertices[i][axisV];
            u=(u-minU)*uMult;
            v=(v-minV)*vMult;
            if(flipU)
            {
                u=1.0f-u;                
            }
            if(flipV)
            {
                v=1.0f-v;
            }
            uvs[i] = new Vector2(u,v);
        }
        mesh.uv = uvs;

        float totalScale=1f;
        Transform scaleGetter=GetComponent<Transform>();
        while(scaleGetter!=null)
        {
            totalScale*=scaleGetter.localScale.x;
            scaleGetter=scaleGetter.parent;
        }

        if(autoMask)
        {
            
            generatedMask = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            GameObject obj = new GameObject();
            obj.transform.parent = transform;
            renderCamera=obj.AddComponent<Camera>();
            Transform cameraPos=renderCamera.GetComponent<Transform>();
            Vector3 lookDir=new Vector3(0,0,0);
            Vector3 lookUp = new Vector3(0, 0, 0);
            lookDir[otherAxis] = (flipU==flipV) ? -1 : 1;
            lookUp[axisV]=flipV?-1:1;
            cameraPos.localRotation = Quaternion.LookRotation(lookDir, lookUp);
            Vector3 pos = new Vector3();
            pos[axisU]= (minU+maxU)*0.5f;
            pos[axisV]= (minV+maxV)*0.5f;
            pos -= .1f * lookDir;
            cameraPos.localPosition = pos;
            //            GetComponent<Renderer>().material.SetTexture("_MainTex",null);

            Material oldMat = GetComponent<Renderer>().material;
            GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/CutoutShader"));
            renderCamera.orthographic=true;
            renderCamera.orthographicSize=totalScale*(maxV-minV)*0.5f;
            renderCamera.aspect=(maxU-minU)/(maxV-minV);
            renderCamera.targetTexture=generatedMask;
            renderCamera.cullingMask=1<<11;
            renderCamera.nearClipPlane = 0.001f;
            renderCamera.clearFlags=CameraClearFlags.SolidColor;
            renderCamera.backgroundColor=Color.clear;
            gameObject.layer=11;
            renderCamera.enabled=false;
            renderCamera.Render();
            gameObject.layer=0;
            GetComponent<Renderer>().material= oldMat;

            Rect rectReadPicture = new Rect(0,0,512,512);
 
			RenderTexture.active = generatedMask;
            maskTexture = new Texture2D(512,512, TextureFormat.RGBA32, false);
            // Read pixels
            maskTexture.ReadPixels(rectReadPicture, 0, 0);
            maskTexture.Apply();
 
			RenderTexture.active = null; // added to avoid errors 			
			Color32[] inPixels=maskTexture.GetPixels32();
			Color32[] outPixels=new Color32[inPixels.Length];
			Color32[] outPixels2=new Color32[inPixels.Length];
			for(int y=0;y<512;y++)
			{
				for(int x=0;x<512;x++)
				{
                    int xOut = x ;
                    int yOut = y ;
					outPixels[xOut+yOut*512]=inPixels[x+y*512];
				}
			}
            // this code blurs the edges based on how far each pixel is from an edge
            // n.b. this code is SLOW if edgeBlur is a big number
            // make a distance map up to 32 pixels
            for (int y = 0; y < 512; y++)
            {
                for (int x = 0; x < 512; x++)
                {
                    edgeDistanceMap[x, y] = findPixelDistanceToEdge(outPixels, x, y, 31.5f);
                }
            }
            lastEdgeBlur = edgeBlur;
            if (edgeBlur>0)
			{
                float boundaryWidth = edgeBlur * 32.0f;
				for(int y=0;y<512;y++)
				{
					for(int x=0;x<512;x++)
					{
                        outPixels2[x + y * 512] = outPixels[x + y * 512];
						if(edgeDistanceMap[x, y] < boundaryWidth)
						{
							float alpha=outPixels2[x+y*512].a;
							alpha*=(edgeDistanceMap[x, y] / boundaryWidth);
							outPixels2[x+y*512].a=(byte)alpha;
						}
					}
				}
				maskTexture.SetPixels32(outPixels2);
			}else
			{
                maskTexture.SetPixels32(outPixels);
			}
            maskTexture.Apply();
			
			
            GetComponent<Renderer>().material.SetTexture("_MaskTex",maskTexture);
            renderCamera.gameObject.active = false;
            Destroy(renderCamera.gameObject);
        }

    }
    // find closest edge pixel to this pixel
	float findPixelDistanceToEdge(Color32 []outPixels,int x,int  y,float boundaryWidth)
	{
		int offsetMax=(int)(boundaryWidth+0.5f);
		if(outPixels[x+y*512].r==0)return 0f;
		float minDistanceSq=boundaryWidth*boundaryWidth+1f;
		for(int oX=1;oX<offsetMax;oX+=1)
		{
			for(int oY=1;oY<offsetMax;oY+=1)
			{
				float distanceSq=oX*oX+oY*oY;
				if(distanceSq<minDistanceSq)
				{
                    if (x + oX >= 512 || y + oY >= 512 || x - oX < 0 || y - oY < 0)
                    {
                        minDistanceSq = distanceSq;
                    }
                    else
                    {
                        if (outPixels[x + oX + (y + oY) * 512].r == 0) minDistanceSq = distanceSq;
                        if (outPixels[x - oX + (y + oY) * 512].r == 0) minDistanceSq = distanceSq;
                        if (outPixels[x + oX + (y - oY) * 512].r == 0) minDistanceSq = distanceSq;
                        if (outPixels[x - oX + (y - oY) * 512].r == 0) minDistanceSq = distanceSq;
                    }
				}
			}
		}
		return Mathf.Sqrt(minDistanceSq);
	}
	
    
    public void Restart()
    {
        oldMediaName="";
    }
}

public static class TextureHelperClass
{
    public static Texture2D ChangeFormat(this Texture2D oldTexture, TextureFormat newFormat)
    {
        //Create new empty Texture
        Texture2D newTex = new Texture2D(2, 2, newFormat, false);
        //Copy old texture pixels into new one
        newTex.SetPixels(oldTexture.GetPixels());
        //Apply
        newTex.Apply();

        return newTex;
    }
}
