﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

public class MessageHandler : MonoBehaviour
{
    public string mediaFolder="./media/";

    
    public SaveLoad presetHandler;

    [Serializable]
    public class StringTrigger : UnityEvent <string,OscMessage> {}

    [Serializable]
    public class EffectMapPair 
    {
        public string addressBase;
        public StringTrigger trigger;
    }
    
    public List<EffectMapPair> effects;
    public List<Font> fonts;
    
    Dictionary<string,StringTrigger > effectMap=new Dictionary<string,StringTrigger>();    
    
    // Start is called before the first frame update
    void Start()
    {
        OSC osc=GetComponent<OSC>();
        osc.SetAllMessageHandler(OnOscMessage);
        foreach( EffectMapPair pair in effects)
        {
            effectMap[pair.addressBase]=pair.trigger;
        }
        //initialKillAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnOscMessage( OscMessage oscM )
    {        
        print(oscM);
        string[] addressSplits=oscM.address.Split('/');
        if(addressSplits.Length>=2 && effectMap.ContainsKey(addressSplits[1]))
        {
            effectMap[addressSplits[1]].Invoke(addressSplits[1],oscM);
        }
        if(addressSplits.Length>=3 && addressSplits[1]=="text")
        {
            OnTextUpdate(addressSplits[2],oscM);
        }
        if(addressSplits.Length>=3 && addressSplits[1]=="kill")
        {
            OnKill(addressSplits[2],oscM);
        }
        if(addressSplits.Length>=3 && addressSplits[1]=="media")
        {
            OnMediaUpdate(addressSplits[2],oscM);
        }
        if(addressSplits.Length>=3 && addressSplits[1]=="scene")
        {
            presetHandler.LoadPreset(addressSplits[2]);
        }
    }

    public void OnTextUpdate( string targetName,OscMessage oscM )
    {
        string txt="";
        if(oscM.values.Count>0)
        {
            txt=(string)oscM.values[0];
        }
        txt=txt.Replace("\\n", "\n");
        
        Font font=null;
        Color? color=null;
        TextAnchor? textAlign = null;
        int fontSize=-1;
        bool setDefault=false;
        float fade = 0f;
        
        for(int c=1;c<oscM.values.Count;c++)
        {
            string valueCommand=(string)oscM.values[c];
            if(valueCommand=="default")
            {
                setDefault=true;
            }else
            {
                string[] splits=valueCommand.Split(':');
                if(splits.Length==2)
                {
                    string cmd=splits[0];
                    string cmdVal=splits[1];
                    if(cmd=="font")
                    {
                        foreach(Font ft in fonts)
                        {
                            if(ft.name.ToLower().StartsWith(cmdVal.ToLower()))
                            {
                                print(ft.name+":"+cmdVal);
                                font=ft;
                            }
                        }                            
                    }
                    if(cmd=="size")
                    {
                        int.TryParse(cmdVal,out fontSize);
                        print(cmdVal+":"+fontSize+"!!!");
                    }
                    if(cmd=="color")
                    {
                        string[] colorVals=cmdVal.Split(',');
                        int r=255,g=255,b=255,a=255;
                        if(colorVals.Length>=3)
                        {
                            int.TryParse(colorVals[0],out r);
                            int.TryParse(colorVals[1],out g);
                            int.TryParse(colorVals[2],out b);
                        }
                        if(colorVals.Length>=4)
                        {
                            int.TryParse(colorVals[3],out a);
                        }
                        color=new Color(((float)r)/255.0f,((float)g)/255.0f,((float)b)/255.0f,((float)a)/255.0f);
                    }if(cmd=="align")
                    {
                        if (cmdVal.ToLower() == "tl") textAlign = TextAnchor.UpperLeft;
                        if (cmdVal.ToLower() == "tr") textAlign = TextAnchor.UpperRight;
                        if (cmdVal.ToLower() == "tc") textAlign = TextAnchor.UpperCenter;
                        if (cmdVal.ToLower() == "ml") textAlign = TextAnchor.MiddleLeft;
                        if (cmdVal.ToLower() == "mr") textAlign = TextAnchor.MiddleRight;
                        if (cmdVal.ToLower() == "mc") textAlign = TextAnchor.MiddleCenter;
                        if (cmdVal.ToLower() == "bl") textAlign = TextAnchor.LowerLeft;
                        if (cmdVal.ToLower() == "br") textAlign = TextAnchor.LowerCenter;
                        if (cmdVal.ToLower() == "bc") textAlign = TextAnchor.LowerRight;
                    }
                    if (cmd == "fade")
                    {
                        float.TryParse(cmdVal, out fade);
                    }
                }
            }
        }
        GameObject txtOb=GameObject.Find(targetName+"_Text");
        GameObject mediaOb=GameObject.Find(targetName+"_Media");
        if(txtOb!=null)
        {
            StartCoroutine(txtOb.GetComponent<TextDisplayArea>().SetText(txt,font,fontSize,color, textAlign,setDefault, fade));
        }
        if(mediaOb!=null)
        {
            mediaOb.GetComponent<MediaArea>().mediaName="";
        }
        // update text
        // clear any related media objects
    }
    
    public void OnMediaUpdate( string targetName,OscMessage oscM)
    {
        bool stretch=true;
        bool expand=true;
        bool loop=false;
        float? edgeBlur = null;
        string media="";
        float fade = 0f;
        if(oscM.values.Count>0)
        {
            media=(string)oscM.values[0];
        }
        for(int c=1;c<oscM.values.Count;c++)
        {
            if(oscM.values[c].ToString()=="stretch")
            {
                stretch=true;
            }
            if(oscM.values[c].ToString()=="fit")
            {
                expand=false;
            }
            if(oscM.values[c].ToString()=="loop")
            {
                loop=true;
            }
            if (oscM.values[c].ToString().StartsWith("edgeblur:"))
            {
                float eb = 0;
                if(float.TryParse(oscM.values[c].ToString().Split(':')[1], out eb))
                {
                    edgeBlur = eb;
                }
            }
            if (oscM.values[c].ToString().StartsWith("fade:"))
            {
                float eb = 0;
                if (float.TryParse(oscM.values[c].ToString().Split(':')[1], out eb))
                {
                    fade = eb;
                }
            }

        }
        GameObject txtOb=GameObject.Find(targetName+"_Text");
        GameObject mediaOb=GameObject.Find(targetName+"_Media");
        if(mediaOb!=null)
        {
            mediaOb.GetComponent<MediaArea>().mediaName=mediaFolder+media;
            mediaOb.GetComponent<MediaArea>().matchAspect=!stretch;
            mediaOb.GetComponent<MediaArea>().expand=expand;
            mediaOb.GetComponent<MediaArea>().loop=loop;
            mediaOb.GetComponent<MediaArea>().userFadeTime = fade;
            if(edgeBlur.HasValue)
            {
                mediaOb.GetComponent<MediaArea>().edgeBlur = edgeBlur.Value;
            }
            mediaOb.GetComponent<MediaArea>().Restart();
        }        
        if(txtOb!=null)
        {
            StartCoroutine(txtOb.GetComponent<TextDisplayArea>().fadeOut(fade));
        }
    }

    public void OnKill( string targetName,OscMessage oscM)
    {
        float fade = 0f; 
        for (int c = 0; c < oscM.values.Count; c++)
        {
            if (oscM.values[c].ToString().StartsWith("fade:"))
            {
                float eb = 0;
                if (float.TryParse(oscM.values[c].ToString().Split(':')[1], out eb))
                {
                    fade = eb;
                }
            }
        }

        if (targetName=="All")
        {
            TextDisplayArea[] areas=FindObjectsOfType<TextDisplayArea>();
            foreach(TextDisplayArea area in areas)
            {
                StartCoroutine(area.fadeOut(fade));
            }
            MediaArea[] areas2=FindObjectsOfType<MediaArea>();
            foreach(MediaArea area in areas2)
            {
                area.fadeOut(fade);
            }
            
        }
        GameObject txtOb=GameObject.Find(targetName+"_Text");
        GameObject mediaOb=GameObject.Find(targetName+"_Media");
        if(txtOb!=null)
        {
            StartCoroutine(txtOb.GetComponent<TextDisplayArea>().fadeOut(fade));
        }
        if(mediaOb!=null)
        {
            mediaOb.GetComponent<MediaArea>().fadeOut(fade);
        }
    }


    public void OnSceneChange( OscMessage oscM)
    {
    }

    private void initialKillAll()
    {
        TextDisplayArea[] areas = FindObjectsOfType<TextDisplayArea>();
        foreach (TextDisplayArea area in areas)
        {
            StartCoroutine(area.fadeOut(0f));
        }
        MediaArea[] areas2 = FindObjectsOfType<MediaArea>();
        foreach (MediaArea area in areas2)
        {
            area.fadeOut(0f);
        }
    }
    
}
