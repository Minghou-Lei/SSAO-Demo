using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ShaderPipline{
    AOGenerater = 0,
    BilateralFilter = 1,
    Composite = 2
}

public class SSAO : MonoBehaviour
{
    private Camera cam;
    private Shader shader;
    private RenderTexture blurRT;
    private RenderTexture AORenderTexture;
    
    public Material SSAOMaterial;
    public float sampleKernelRadius;
    public List<Vector4> samplePoint = new List<Vector4>();
    public float samplePointCount;
    public Texture noiseTexture;
    public float blurRadius = 2;
    public float bilaterFilterStrength = 0.2f;
    public bool AOTexture = false;
    public float bias;
    public float strength = 1;
    
    private void Awake()
    {
        shader = Shader.Find("Custom/MySSAO");
        SSAOMaterial = new Material(shader);
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        cam.depthTextureMode |= DepthTextureMode.DepthNormals;
    }
    private void OnDisable()
    {
        cam.depthTextureMode &= ~DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        GenSampleKernal();
        RenderTexture rawImage = RenderTexture.GetTemporary(src.width, src.height, 0);
        Graphics.Blit(src,rawImage);
        AORenderTexture = RenderTexture.GetTemporary(src.width, src.height, 0);
        SSAOMaterial.SetTexture("_NoiseTex",noiseTexture);
        SSAOMaterial.SetFloat("_Height",(float)Screen.height);
        SSAOMaterial.SetFloat("_Width",(float)Screen.width);
        SSAOMaterial.SetMatrix("_InverseProjectionMatrix", cam.projectionMatrix.inverse);
        SSAOMaterial.SetVectorArray("_SamplePointArray",samplePoint.ToArray());
        SSAOMaterial.SetFloat("_SamplePointCount",samplePointCount);
        SSAOMaterial.SetFloat("_SampleKernelRadius",sampleKernelRadius);
        Graphics.Blit(src,AORenderTexture,SSAOMaterial,(int)ShaderPipline.AOGenerater);
        
        blurRT = RenderTexture.GetTemporary(src.width, src.height, 0);
        SSAOMaterial.SetFloat("_BilaterFilterFactor",1f-bilaterFilterStrength);
        SSAOMaterial.SetVector("_BlurRadius",new Vector4(blurRadius,0,0,0));
        SSAOMaterial.SetFloat("_Bias",bias);
        SSAOMaterial.SetFloat("_Strength",strength);
        Graphics.Blit(AORenderTexture,blurRT,SSAOMaterial,(int)ShaderPipline.BilateralFilter);
        
        SSAOMaterial.SetVector("_BlurRadius",new Vector4(0,blurRadius,0,0));
        if (AOTexture)
        {
            Graphics.Blit(blurRT,dest,SSAOMaterial,(int)ShaderPipline.BilateralFilter);
        }
        else
        {
            SSAOMaterial.SetTexture("_MainTex",rawImage);
            Graphics.Blit(blurRT,AORenderTexture,SSAOMaterial,(int)ShaderPipline.BilateralFilter);
            SSAOMaterial.SetTexture("_AOTex",AORenderTexture);
            Graphics.Blit(src,dest,SSAOMaterial,(int)ShaderPipline.Composite);
        }
        
        //RenderTexture.ReleaseTemporary(AORenderTexture);
        //RenderTexture.ReleaseTemporary(blurRT);
        
    }

    void GenSampleKernal()
    {
        if (samplePointCount == samplePoint.Count)
            return;
        samplePoint.Clear();
        for (int i = 0; i < samplePointCount; ++i)
        {
            var vector = new Vector4(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(0f, 1f), 1f).normalized;
            //拟合二次方程
            var scale = (float) i / samplePointCount;
            scale = Mathf.Lerp(0.01f, 1f, scale * scale);
            vector *= scale;
            samplePoint.Add(vector);
        }
    }
}
