using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
 
[Serializable]
[PostProcess(typeof(CameraBrightnessRenderer), PostProcessEvent.AfterStack, "Custom/CameraBrightness")]
public sealed class CameraBrightness : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("CameraBrightness effect intensity.")]
    public FloatParameter brightness = new FloatParameter { value = 0.5f };
}
 
public sealed class CameraBrightnessRenderer : PostProcessEffectRenderer<CameraBrightness>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/CameraBrightness"));
        sheet.properties.SetFloat("_Blend", settings.brightness);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
    
}