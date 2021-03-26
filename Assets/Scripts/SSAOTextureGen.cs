using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SSAOTextureGen : MonoBehaviour
{
    private Camera cam;
    private Shader shader;
    private RenderTexture blurRT;
    
    public RenderTexture AORenderTexture;
    public Material SSAOMaterial;
    public float sampleKernelRadius;
    public List<Vector4> samplePoint = new List<Vector4>();
    public float samplePointCount;
    public Texture noiseTexture;
    public float blurRadius = 2;
    public float bilaterFilterStrength = 0.2f;
    public float bias;
    public float strength = 1;
    public RenderTexture rt;

    private void Awake()
    {
        shader = Shader.Find("Custom/MySSAO");
        SSAOMaterial = new Material(shader);
        cam = GetComponent<Camera>();
        cam.forceIntoRenderTexture = true;
        rt = RenderTexture.GetTemporary(Screen.width,Screen.height,0);
        AORenderTexture = RenderTexture.GetTemporary(Screen.width,Screen.height,0);
        blurRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        Shader.SetGlobalTexture("_ScreenSpaceOcclusionTexture",AORenderTexture);
    }

    private void OnEnable()
    {
        cam.depthTextureMode |= DepthTextureMode.DepthNormals;
        GenSampleKernal();
    }
    private void OnDisable()
    {
        cam.depthTextureMode &= ~DepthTextureMode.DepthNormals;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src,rt);
        SSAOMaterial.SetTexture("_NoiseTex",noiseTexture);
        SSAOMaterial.SetFloat("_Height",(float)Screen.height);
        SSAOMaterial.SetFloat("_Width",(float)Screen.width);
        SSAOMaterial.SetMatrix("_InverseProjectionMatrix", cam.projectionMatrix.inverse);
        SSAOMaterial.SetVectorArray("_SamplePointArray",samplePoint.ToArray());
        SSAOMaterial.SetFloat("_SamplePointCount",samplePointCount);
        SSAOMaterial.SetFloat("_SampleKernelRadius",sampleKernelRadius);
        Graphics.Blit(rt,AORenderTexture,SSAOMaterial,0);
        SSAOMaterial.SetFloat("_BilaterFilterFactor",1f-bilaterFilterStrength);
        SSAOMaterial.SetVector("_BlurRadius",new Vector4(blurRadius,0,0,0));
        SSAOMaterial.SetFloat("_Bias",bias);
        SSAOMaterial.SetFloat("_Strength",strength);
        Graphics.Blit(AORenderTexture,blurRT,SSAOMaterial,1);
        SSAOMaterial.SetVector("_BlurRadius",new Vector4(0,blurRadius,0,0));
        Graphics.Blit(blurRT,dest,SSAOMaterial,1);
        Graphics.Blit(rt,dest);
        
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.ReleaseTemporary(AORenderTexture);
        RenderTexture.ReleaseTemporary(blurRT);

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
