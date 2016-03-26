cbuffer cbPerObject
{
	float4x4 worldViewProj;
};

struct VertexIn
{
	float3 PosL  : POSITION;
	float4 Color : COLOR;
};

struct VertexOut
{
	float4 PosH  : SV_POSITION;
	float4 Color : COLOR;
};

VertexOut Vs(VertexIn vin)
{
	VertexOut vout;

	vout.PosH = mul(float4(vin.PosL, 1.0f), worldViewProj);
	vout.Color = vin.Color;

	return vout;
}

float4 Ps(VertexOut pin) : SV_Target
{
	return pin.Color;
}