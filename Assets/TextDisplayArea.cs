using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class TextDisplayArea : PolyText
{
    Font defaultFont;
    int defaultFontSize=64;
    Color defaultColor;

    override protected void  Start()
    {
        base.Start();
        defaultFont=font;
        defaultFontSize=fontSize;
        defaultColor=color;
    }
    

    public void SetText(string txt,Font newFont=null,int newSize=-1,Color? newColor=null,bool setDefault=false)    
    {
        prevText="";
        text=txt;
        if(newFont!=null)
        {
            font=newFont;
            if(setDefault)
            {
                defaultFont=font;
            }
        }else
        {
            font=defaultFont;
        }
        if(newColor.HasValue)
        {
            color=newColor.Value;
            if(setDefault)
            {
                defaultColor=color;
            }
        }else
        {
            color=defaultColor;            
        }
        if(newSize!=-1)
        {
            fontSize=newSize;
            if(setDefault)
            {
                defaultFontSize=fontSize;
            }
        }else
        {
            fontSize=defaultFontSize;
        }
        UpdateMaterial();
    }

}