float4x4 xView;
float4x4 xProjection;
float4x4 xWorld;
float3 xCamPos;
float3 xAllowedRotDir;
float scale;


//------- Texture Samplers --------
Texture xBillboardTexture;

sampler textureSampler = sampler_state { texture = <xBillboardTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = CLAMP; AddressV = CLAMP;};
struct BBVertexToPixel
{
    float4 Position : SV_POSITION;
    float2 TexCoord    : TEXCOORD0;
	float3 Position3D   : TEXCOORD1;
};
struct BBPixelToFrame
{
    float4 Color     : COLOR0;
};

//------- Technique: CylBillboard --------
BBVertexToPixel CylBillboardVS(float3 inPos: SV_POSITION, float2 inTexCoord: TEXCOORD0)
{
    BBVertexToPixel Output = (BBVertexToPixel)0;

    float3 center = mul(inPos, xWorld);
    float3 eyeVector = center - xCamPos;

    float3 upVector = xAllowedRotDir;
    //upVector = normalize(upVector);
    float3 sideVector = cross(eyeVector,upVector);
    sideVector = normalize(sideVector);

    float3 finalPosition = center;
    finalPosition += (inTexCoord.x-0.5f)*sideVector*scale;
    finalPosition += (1.5f-inTexCoord.y*1.5f)*upVector*scale;

    float4 finalPosition4 = float4(finalPosition, 1);

    float4x4 preViewProjection = mul (xView, xProjection);
    Output.Position = mul(finalPosition4, preViewProjection);
	Output.Position3D = center;
    Output.TexCoord = inTexCoord;

    return Output;
}

BBPixelToFrame BillboardPS(BBVertexToPixel PSIn) : COLOR0
{
	BBPixelToFrame Output = (BBPixelToFrame)0;
	Output.Color = tex2D(textureSampler, PSIn.TexCoord);
	return Output;
}

technique CylBillboard
{
    pass Pass0
    {        
        VertexShader = compile vs_3_0 CylBillboardVS();
        PixelShader = compile ps_3_0 BillboardPS();        
    }
}