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
	TextAnchor defaultAlignment;

    override protected void  Start()
    {
        base.Start();
        defaultFont=font;
        defaultFontSize=fontSize;
        defaultColor=color;
		defaultAlignment=alignment;
    }

    public IEnumerator fadeOut(float fadeTime)
    {
        float t = 0;
        float oldColAlpha = color.a;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(oldColAlpha, 0f, t / (fadeTime / 2f));
            color = new Color(color.r, color.g, color.b, newAlpha);
            UpdateMaterial();
            yield return null;
        }
        text = "";
        color = new Color(color.r, color.g, color.b, oldColAlpha);
        UpdateMaterial();
    }

    public IEnumerator SetText(string txt,Font newFont=null,int newSize=-1,Color? newColor=null,TextAnchor? textAlign=null,bool setDefault=false, float fadeTime=0f)    
    {
        //fade out first:
        float fadeOutTime = fadeTime / 2f;

        if (text == "")
        {
            fadeOutTime = 0f;
        }
        float t = 0;
        float oldColAlpha = color.a;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(oldColAlpha, 0f, t / (fadeOutTime));
            color = new Color(color.r, color.g, color.b, newAlpha);
            UpdateMaterial();
            yield return null;
        }

        //now we change the text

        prevText = "";
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
		if(textAlign!=null)
		{
			alignment=textAlign.Value;
			if(setDefault)
			{	
				defaultAlignment=alignment;
			}
		}else
		{
			alignment=defaultAlignment;
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


        //now we'll fade back in

        if (text == "")
        {
            fadeTime *= 2f;
        }
        t = 0;
        oldColAlpha = color.a;
        while (t < fadeTime / 2f)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, oldColAlpha, t / (fadeTime / 2f));
            color = new Color(color.r, color.g, color.b, newAlpha);
            UpdateMaterial();
            yield return null;
        }
    }

}