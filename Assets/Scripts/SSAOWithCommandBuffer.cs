using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class SSAOWithCommandBuffer : MonoBehaviour
{
    private Camera cam;
    private Shader shader;
    private RenderTexture blurRT;
    private RenderTexture AORenderTexture;
    private Renderer AORenderer;
    private CommandBuffer commandBuffer;
    private RenderTexture rt;

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
        GetComponent<Camera>().forceIntoRenderTexture = true;
        AORenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        commandBuffer = new CommandBuffer();

        commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, rt, new Vector2(Screen.width, Screen.height),
            new Vector2(0, 0));

        SSAOMaterial.SetTexture("_NoiseTex", noiseTexture);
        SSAOMaterial.SetFloat("_Height", (float) Screen.height);
        SSAOMaterial.SetFloat("_Width", (float) Screen.width);
        SSAOMaterial.SetMatrix("_InverseProjectionMatrix", cam.projectionMatrix.inverse);
        SSAOMaterial.SetVectorArray("_SamplePointArray", samplePoint.ToArray());
        SSAOMaterial.SetFloat("_SamplePointCount", samplePointCount);
        SSAOMaterial.SetFloat("_SampleKernelRadius", sampleKernelRadius);


        commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, AORenderTexture, SSAOMaterial,
            (int) ShaderPipline.AOGenerater);

        blurRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        SSAOMaterial.SetFloat("_BilaterFilterFactor", 1f - bilaterFilterStrength);
        SSAOMaterial.SetVector("_BlurRadius", new Vector4(blurRadius, 0, 0, 0));
        SSAOMaterial.SetFloat("_Bias", bias);
        SSAOMaterial.SetFloat("_Strength", strength);

        commandBuffer.Blit(AORenderTexture, blurRT, SSAOMaterial, (int) ShaderPipline.BilateralFilter);

        SSAOMaterial.SetVector("_BlurRadius", new Vector4(0, blurRadius, 0, 0));

        if (AOTexture)
        {
            commandBuffer.Blit(blurRT, BuiltinRenderTextureType.CameraTarget, SSAOMaterial,
                (int) ShaderPipline.BilateralFilter);
        }
        else
        {
            commandBuffer.Blit(blurRT, AORenderTexture, SSAOMaterial, (int) ShaderPipline.BilateralFilter);

            //设置AO和原始画面
            SSAOMaterial.SetTexture("_MainTex", rt);
            SSAOMaterial.SetTexture("_AOTex", AORenderTexture);

            //混合
            commandBuffer.Blit(rt, BuiltinRenderTextureType.CameraTarget, SSAOMaterial, (int) ShaderPipline.Composite);
        }

        cam.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    private void OnDisable()
    {
        cam.depthTextureMode &= ~DepthTextureMode.DepthNormals;
        cam.RemoveCommandBuffer(CameraEvent.AfterImageEffects, commandBuffer);
        commandBuffer.Clear();
        RenderTexture.ReleaseTemporary(AORenderTexture);
        RenderTexture.ReleaseTemporary(blurRT);
        RenderTexture.ReleaseTemporary(rt);
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