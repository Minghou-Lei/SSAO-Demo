# 开始上手

## What is SSAO?

[https://camo.githubusercontent.com/bfa1bc10138cda3344fd62110a08b26666e4ca5d2f9f481e34d5233fed19dda9/68747470733a2f2f692e696d6775722e636f6d2f6f376c43756b442e676966](https://camo.githubusercontent.com/bfa1bc10138cda3344fd62110a08b26666e4ca5d2f9f481e34d5233fed19dda9/68747470733a2f2f692e696d6775722e636f6d2f6f376c43756b442e676966)

开启和关闭SSAO效果的对比

屏幕空间环境光屏蔽（Screen Space Ambient Occlusion，SSAO）一种用于在计算机图形中实时实现近似环境光屏蔽效果的渲染技术。由Vladimir Kajalin于在Crytek工作的时候开发，该算法作为像素着色器，通过分析场景中纹理的深度值缓冲来实现，可以近似地表现出物体在环境光下产生的阴影。首次使用该特效的是2007年由Crytek开发的电脑游戏Crysis。

## Why using SSAO?

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled.png)

基于顶点的AO技术在模型表面顶点足够密的情况下，能够得到很好的效果。但是基于物理精确的 AO 计算需要进行光线与场景的求交运算 , 十分耗时 ，所以这种方法只能用于离线渲染。

为了达到实时计算，但又能骗过人们的眼睛，我们可以利用SSAO技术来增强现实感的同时达到实时计算的目的：

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%201.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%201.png)

屏幕空间的环境遮挡技术，对每个像素的邻域进行随机采样快速计算AO的近似值。 屏幕空间环境光遮蔽 (SSAO)是实际应用较多的一种AO算法。该算法可以在实时运行的条件下较为逼真的模拟全局光照的渲染效果。 由于所有的计算都发生在屏幕空间,所以也叫做屏幕空间环境光遮罩。 即在屏幕空间上进行 AO 计算 , 并用深度缓存上的深度比较来代替光线求交 。

## How SSAO work?

### 要了解SSAO如何工作，首先要知道环境光遮蔽AO(Ambient occlusion)是什么

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%202.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%202.png)

环境光遮蔽由灰度值来呈现，该位置的灰度越高，说明光线越不可能到达该位置

环境光遮蔽（英语：ambient occlusion）是计算机图形学中的一种着色和渲染技术，用来计算场景中每一点是如何接受环境光的。例如，一个管道的内部显然比外表面更隐蔽（因此也更暗），越深入管道光线就越暗。环境光遮蔽可以被看作是光线能到达表面上每一点的能力的数值。[1]在拥有开放天空的场景中，这是通过估算每个点的可看见天空的大小来完成的；而在室内环境中，只考虑一定范围内的物体，并假设墙壁是环境光源。处理结果是一个漫反射、非定向的着色效果，并不会形成明确的阴影，只是能让靠近物体及被遮蔽的区域更暗，并影响渲染图像的整体色调。环境光遮蔽常被用作后期处理。

与局部方法如Phong着色法不同，环境光遮蔽是一种全局方法，意味着每个点的照明是场景中其他几何体的共同作用。然而，这只是一个非常粗略的近似全局光照。仅通过环境光遮蔽得到的物体外观与阴天下的物体相似。

第一个可以实时模拟环境光遮蔽的算法是由Crytek（CryEngine 2）的研发部门开发的。[2] 随着英伟达2018年发布的实时光线追踪硬件，光线追踪环境光遮蔽（英语：ray traced ambient occlusion, RTAO）在游戏和其他实时应用中成为可能。[3]虚幻引擎在4.22版本中加入了这个特性。

### SSAO如何工作？

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%203.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%203.png)

" SSAO works by sampling the view space around a fragment. The more samples that are below a surface, the darker the fragment color. These samples are positioned at the fragment and pointed in the general direction of the vertex normal. Each sample is used to look up a position in the position frame buffer texture. The position returned is compared to the sample. If the sample is farther away from the camera than the position, the sample counts towards the fragment being occluded. "

" SSAO通过采样片段周围的视图空间来工作。 表面以下的样品越多，片段颜色越深。 这些样本位于片段处，并指向顶点法线的大致方向。 每个样本都用于在位置帧缓冲区纹理中查找位置。 将返回的位置与样本进行比较。 如果样品距离相机的位置远于该位置，则该样品计入被遮挡的片段。 "

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%204.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%204.png)

红色的采样点被阻挡，蓝色的采样点没有被阻挡
p点接收到光的概率就可以大概估算为 蓝点数量 / 蓝点数量 + 红点数量 =  5/11

[https://camo.githubusercontent.com/7402bad209edd0bd77df6e3c5fd8dc9fe99777905606eae2e808e6aaad87eb5a/68747470733a2f2f692e696d6775722e636f6d2f4e6d34434a444e2e676966](https://camo.githubusercontent.com/7402bad209edd0bd77df6e3c5fd8dc9fe99777905606eae2e808e6aaad87eb5a/68747470733a2f2f692e696d6775722e636f6d2f4e6d34434a444e2e676966)

对每个片段进行采样的过程可视化

球体空间内( 若有法向缓存则为半球体空间内 ) 随机地产生若干三维采样点，然后估算每个采样点产生的 AO ，计算每个采样点在深度缓存上的投影点 , 用投影点产生的遮蔽近似代替采样点的遮蔽。对于投影点的遮蔽 , 不同的 SSAO 算法使用了不同的计算方法 . 最简单的方法是直接利用投影点跟 p 点深度值差异计算遮蔽大小, 但这会带来自身遮蔽等走样问题 . 一种改进方法是引入法向缓存 , 使得所有采样点都在 p 点上方 , 并利用采样点跟投影点的深度差异计算遮蔽大小，接着计算所有采样点的平均遮蔽 。（当采样点较少时进行必要的降噪处理）

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%205.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%205.png)

p点AO = 8/8 = 1； SSAO Texture为透明

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%206.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%206.png)

p点AO = 6/8 = 0.75； SSAO Texture为灰色

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%207.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%207.png)

p点AO = 3/8 = 0.375； SSAO Texture深灰色

![https://www.legendsmb.com/images/pasted-60.png](https://www.legendsmb.com/images/pasted-60.png)

值得注意的是，显然我们要生成的半球应该尽可能的贴着物体的表面，否则不管怎样半球都会插到物体表面中，这样就会导致分数升高(相当于都有了个基数)，这样整体看上去就会有点糊。那么在切线空间下生成采样点解决这个问题就简单多了，在切线空间下物体表面的法向量是一直垂直于物体表面的，然后切线和副切相是沿着物体表面的(uv状态下),这样只用保证沿法向方向的分量>0,那么生成的采样点一定是在贴着物体的半球内的。

### SSAO Steps:

1. Geometry Pass: Render scene's geometry / color data int Gbuffer
2. Generate SSAO Texture
3. Blur SSAO Texture to remove noise

### SSAO需要以下的信息：

- Vertex position vectors in view space

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%208.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%208.png)

view space中的顶点位置向量

- Vertex normal vectors in view space

[https://camo.githubusercontent.com/3019750a51831c78db54e0b500ffc61a05ce28b9351f647716749b3127c68412/68747470733a2f2f692e696d6775722e636f6d2f696c6e626b7a712e676966](https://camo.githubusercontent.com/3019750a51831c78db54e0b500ffc61a05ce28b9351f647716749b3127c68412/68747470733a2f2f692e696d6775722e636f6d2f696c6e626b7a712e676966)

view space中的顶点法线向量

- Sample vectors in tangent space

[https://camo.githubusercontent.com/c0f1e4ce77e484f7f6f7dbdfe4c176fc09c08764511e231554dadd556d7e2f55/68747470733a2f2f692e696d6775722e636f6d2f75636478394b702e676966](https://camo.githubusercontent.com/c0f1e4ce77e484f7f6f7dbdfe4c176fc09c08764511e231554dadd556d7e2f55/68747470733a2f2f692e696d6775722e636f6d2f75636478394b702e676966)

tangent space中的样本向量

- Noise vectors in tangent space

[https://camo.githubusercontent.com/c2d2eeb2c1b20aa0aef7359e8e91d3b24b99d565f59fe4aefcec5a174938e2e7/68747470733a2f2f692e696d6775722e636f6d2f4b4b74373456452e676966](https://camo.githubusercontent.com/c2d2eeb2c1b20aa0aef7359e8e91d3b24b99d565f59fe4aefcec5a174938e2e7/68747470733a2f2f692e696d6775722e636f6d2f4b4b74373456452e676966)

tangent space中的噪声向量

- The camera lens' projection matrix.[相机镜头的投影矩阵]

### SSAO的实现

### 生成采样核心

首先我们要生成上面所说的采样核心。每个核心中的样本将会被用来偏移观察空间片段位置从而采样周围的几何体，正如上面提到的如果没有变化采样核心，我们将需要大量的样本来获得真实的结果。通过引入一个随机的转动到采样核心中，我们可以很大程度上减少这一数量。

```csharp
void GenerateAOSampleKernel()
    {
        if (SampleKernelCount == sampleKernelList.Count)
            return;
        sampleKernelList.Clear();
        for (int i = 0; i < SampleKernelCount; i++)
        {
            var vec = new Vector4(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(0, 1.0f), 1.0f);
            vec.Normalize();
            var scale = (float)i / SampleKernelCount;
            scale = Mathf.Lerp(0.1f, 1.0f, scale * scale);
            //给向量乘一个scale是为了让生成的随机采样点更靠近片段点，这样得到的采样点更有意义
            vec *= scale;
            sampleKernelList.Add(vec);
        }
    }
```

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%209.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%209.png)

法向半球体(Normal-oriented Hemisphere)

[%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/YCHKNhSGLk.mp4](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/YCHKNhSGLk.mp4)

3D View of 法向半球体(Normal-oriented Hemisphe

作者在此处把点跟二次方程函数进行拟合：

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Unity_STPaEYUMFm.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Unity_STPaEYUMFm.png)

未拟合前，采样点的分布较为均匀

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2010.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2010.png)

拟合后，采样点较为集中在P点附近，采样点更具有代表性

### 采样核心引入随机性

如果对于所有的片段点周围都用同一个采样核心，那显然是不合适的，但是给每个片段点都去创建一个采样核心那又不太现实，因此我们的方法是创建一个小的随机旋转向量纹理平铺在屏幕上,然后用它来扰动采样核心。通过使用一个逐片段观察空间位置，我们可以将一个采样半球核心对准片段的观察空间表面法线。对于每一个核心样本我们会采样线性深度纹理来比较结果。采样核心会根据旋转矢量稍微偏转一点；我们所获得的遮蔽因子将会之后用来限制最终的环境光照分量。

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2011.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2011.png)

随机数贴图

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2012.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2012.png)

shader中读取噪声贴图，利用得到的噪声与原来的正交基进行旋转，消耗较少的性能给采样点带来随机化

[%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/hHbWtrvGJV.mp4](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/hHbWtrvGJV.mp4)

使用Gizmos实现的屏幕上一点的AO模拟

[%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/One_Sample_Point.mp4](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/One_Sample_Point.mp4)

### Shader阶段

Shader中需要View Space中的深度信息和法线信息，刚好Unity的Camera组件中提供了Camera.depthTextureMode可以很方便的获取。
Camera的位置信息可以这样获取：**Camera.projectionMatrix.inverse**

**DepthTextureMode.DepthNormals:**
将生成一个从这个相机可以看到的基于屏幕坐标的深度纹理和基于视图坐标的法线纹理。纹理将是在RenderTextureFormat.ARGB32格式，并设置为_CameraDepthNormalsTexture全局着色器属性。深度和法线将特别编码

[https://camo.githubusercontent.com/3019750a51831c78db54e0b500ffc61a05ce28b9351f647716749b3127c68412/68747470733a2f2f692e696d6775722e636f6d2f696c6e626b7a712e676966](https://camo.githubusercontent.com/3019750a51831c78db54e0b500ffc61a05ce28b9351f647716749b3127c68412/68747470733a2f2f692e696d6775722e636f6d2f696c6e626b7a712e676966)

法线纹理

SSAO着色器核心部分如下所示: 

[SSAO](https://learnopengl-cn.github.io/05%20Advanced%20Lighting/09%20SSAO/)

```c
//计算AO贴图
	fixed4 frag_ao (v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);
		
		float linear01Depth;
		float3 viewNormal;
		
		float4 cdn = tex2D(_CameraDepthNormalsTexture, i.uv);
		//采样获得深度值和法线值
		DecodeDepthNormal(cdn, linear01Depth, viewNormal);

		float3 viewPos = linear01Depth * i.viewRay;
		viewNormal = normalize(viewNormal) * float3(1, 1, -1);
		
		//铺平纹理
		float2 noiseScale = float2(Height / 4.0,Width / 4.0);
		//float2 noiseUV = i.uv * noiseScale;
		float2 noiseUV = float2(i.uv.x * noiseScale.x,i.uv.y * noiseScale.y);
		//采样噪声图
		float3 randvec = tex2D(_NoiseTex,noiseUV).xyz;
		//Gramm-Schimidt处理创建正交基
		float3 tangent = normalize(randvec - viewNormal * dot(randvec,viewNormal));
		float3 bitangent = cross(viewNormal,tangent);
		float3x3 TBN = float3x3(tangent,bitangent,viewNormal);
		int sampleCount = _SampleKernelCount;
		
		float oc = 0.0;
		for(int i = 0; i < sampleCount; i++)
		{
			//1.注意不要把矩阵乘反了，否则得到的结果很黑;CG语言构造矩阵是"行优先"，OpenGL是"列优先"，两者之间是转置的关系,所以请把learnOpenGL中的顺序反过来
			//float3 randomVec = mul(TBN, _SampleKernelArray[i].xyz);
			float3 randomVec = mul(_SampleKernelArray[i].xyz,TBN);

			float3 randomPos = viewPos + randomVec * _SampleKeneralRadius;
			float3 rclipPos = mul((float3x3)unity_CameraProjection, randomPos);
			float2 rscreenPos = (rclipPos.xy / rclipPos.z) * 0.5 + 0.5;
			
			float randomDepth;
			float3 randomNormal;
			float4 rcdn = tex2D(_CameraDepthNormalsTexture, rscreenPos);
			DecodeDepthNormal(rcdn, randomDepth, randomNormal);

			//1.range check & accumulate
			float rangeCheck = smoothstep(0.0,1.0,_SampleKeneralRadius / abs(randomDepth - linear01Depth));
			oc += (randomDepth >= linear01Depth ? 1.0 : 0.0) * rangeCheck;
		}
		//1.求分数平均值
		oc = oc / sampleCount;
		col.rgb = oc;
		return col;
	}
```

### Shader模糊处理

搜资料时发现有三种模糊算法：

1. Gaussian Blur (高斯模糊)
    1. 公式表示

    ![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2013.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2013.png)

    ![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2014.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2014.png)

2. Bilateral Color Filter (基于颜色差值的双边滤波)
    1. 公式表示

    ![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2015.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2015.png)

    ![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2016.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2016.png)

3. Bilateral Normal Filter (基于法线的双边滤波)
    1. 公式和图像同上

---

### 实际对比

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2017.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2017.png)

高斯模糊，整个画面都被糊掉

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2018.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2018.png)

基于颜色插值的双边滤波：**善于保留不同颜色之间的边缘信息**；
可以看到右边人物手臂下部颜色与背景颜色相近，因此被模糊掉，而人物的眼睛和背景中的白色线条则与他们附近的颜色差别很大，他们的颜色边缘信息被很好的保存了下来，没有被模糊掉。

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2019.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2019.png)

基于法线的双边滤波：**善于保留不同法线值之间的边缘信息**；
对比两张图片可以发现右边人物手臂下部边缘变得更加清晰了，他们的法线值边缘信息被很好的保存了下来，没有被模糊掉，而人物的眼睛和背景中的白色线条则与他们附近的法线值差别不大，被模糊掉了。

那么对于SSAO技术来说，最合适的应该是基于法线的双边滤波算法，因为SSAO的最终目的是为了纠正Phong光照模型中环境光变量处处相等的缺点，减少相邻较近的物体边缘的环境光强度，而基于法线的方法正好可以保留物体边缘信息，故选用基于法线的双边滤波算法（Bilateral Normal Filter）

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2020.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2020.png)

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2021.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2021.png)

使用 Bilateral Normal Filter 前，噪声较大，物体边缘很不平顺

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2022.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2022.png)

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2023.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2023.png)

使用 Bilateral Normal Filter 后，物体边缘更加平顺，效果更真实

## Gramm-Schimidt方法处理创建正交基

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2024.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2024.png)

首先获得我们已知的信息，该点的法向量和噪声贴图里采样的随机向量

```c
//Gramm-Schimidt处理创建正交基 步骤一
float3 tangent = normalize(randvec - viewNormal * dot(randvec,viewNormal));
```

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2025.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2025.png)

normalize(randvec - viewNormal * dot(randvec,viewNormal))的含义如图所示，对随机向量作其在法向量上的投影后与随机向量相减，得到橙色的tangent（正交）向量，该向量与法向量垂直，并且法向量、随机向量、tangent向量在同一平面内。

```cpp
//Gramm-Schimidt处理创建正交基 步骤二
float3 bitangent = cross(viewNormal,tangent);
```

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2026.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2026.png)

将法向量和正交向量进行叉乘操作，得到的向量bittangent垂直于法向量与正交向量所构成的平面，由于法向量与正交向量是垂直关系，故可以建立起新的坐标系。

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2027.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2027.png)

建立起新的坐标系，跟原来的坐标系带来了一定的偏转，给采样点带来了随机性。

### Unity屏幕特效函数

### void OnRenderImage(RenderTexture src, RenderTexture dest)

unity shader中的pass是会顺序执行的，但是由于在图像处理中我们常常需要使用一个pass的处理结果作为另一个pass的输入，这个时候就需要用到OnRenderImage()函数了。

> 官方描述Description:
" OnRenderImage is called after all rendering is complete to render image.，该函数在所有的渲染完成后由monobehavior自动调用。
官方解释：该函数允许我们使用着色器滤波操作来修改最终的图像，输入原图像source，输出的图像放在desitination里。
该脚本必须挂载在有相机组件的游戏对象上，在该相机渲染完成后调用OnRenderImage()函数 "

### Graphics.Blit();

该函数的作用就是通过一个shader将原图的像素放置到destionation中。

> 官方描述Description:
" 使用着色器将源纹理复制到目标渲染纹理。
主要用于实现 post-processing effects。
Blit 将 dest 设置为渲染目标，在材质上设置 source _MainTex 属性， 并绘制全屏四边形 "

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2028.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2028.png)

### 深度和法线纹理

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2029.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2029.png)

Unity 中的普通场景

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2030.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2030.png)

深度贴图

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2031.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2031.png)

深度法线贴图

**为什么要用深度和法线纹理？**

之前的屏幕后处理效果都只是在屏幕颜色图像上进行各种操作来实现的。然而，很多时候我们不仅需要当前屏幕的颜色信息，还希望得到深度和法线信息。例如，在进行**边缘检测**时，直接利用颜色信息会使检测到的边缘信息受物体纹理和光照等外部因素的影响，得到很多我们不需要的边缘点。一种更好的方法是，我们可以在深度纹理和法线纹理上进行边缘检测，这些图像不会受纹理和光照的影响，而仅仅保存了当前渲染物体的模型信息，通过这样的方式检测出来的边缘更加可靠。

**如何获取深度和法线纹理？**

一、深度纹理实际就是一张存储高精度的深度值的渲染纹理
由于被存储在一张纹理中，深度纹理里的深度值范围是[0, 1]，而且通常是非线性分布的。这些深度值来自于顶点变换后得到的归一化的设备坐标（Normalized Device Coordinates , NDC）。一个模型要想最终被绘制在屏幕上，需要把它的顶点从模型空间变换到齐次裁剪坐标系下，这是通过在顶点着色器中乘以MVP变换矩阵得到的。在变换的最后一步，我们需要使用一个投影矩阵来变换顶点，当我们使用的是透视投影类型的摄像机时，这个投影矩阵就是非线性的。

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2032.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2032.png)

Normalized Device Coordinates（NDC）的转化过程

![%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2033.png](%E5%BC%80%E5%A7%8B%E4%B8%8A%E6%89%8B%20fce1c7e9b2824406b97d5ae03ecdbe83/Untitled%2033.png)

Perspective Frustum (透视视锥) → NDC（归一化的设备坐标）的对比

在得到NDC后，深度纹理中的像素值就可以很方便地计算得到了，这些深度值就对应了NDC中顶点坐标的z分量的值。由于NDC中z分量的范围在[-1, 1]，为了让这些值能够存储在一张图像中，需要将其映射到[0,1]。