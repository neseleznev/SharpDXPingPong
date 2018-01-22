struct VS_IN
{
	float4 pos : POSITION;
	float4 color : COLOR;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 color : COLOR;
};

PS_IN VSMain(VS_IN input) 
{
	PS_IN output = (PS_IN)0;

	output.pos = input.pos;
	output.color = input.color;

	return output;
}

float4 PSMain(PS_IN input) : SV_Target
{
	return input.color;
}
