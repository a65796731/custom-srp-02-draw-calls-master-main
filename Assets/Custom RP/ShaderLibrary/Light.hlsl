#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
int _DircetionalLightCount;
float4 _DirectionalLightColor[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DircetionalLightDirection[MAX_DIRECTIONAL_LIGHT_COUNT];

CBUFFER_END
struct Light
{
	float3 color;
	float3 direction;
};
int GetDirectionalLightCount()
{
	return  _DircetionalLightCount;
}
Light GetDirectionalLight(int index)
{
	Light light;
	light.color = _DirectionalLightColor[index].rgb;
	light.direction = _DircetionalLightDirection[index].xyz;
	return light;
}

#endif