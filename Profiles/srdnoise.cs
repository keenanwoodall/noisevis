using NaughtyAttributes;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[CreateAssetMenu]
public class srdnoise : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 pos):float3")]
		_0,
		[InspectorName("(float2 pos, float rot):float3")]
		_1,
	}

	public Signature signature;
	[ShowIf("signature", Signature._1)]
	public float rot;

	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new psrdnoise0 { res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
			case Signature._1:
				return new psrdnoise1 { rot = rot, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
		}
	}
	
	[BurstCompile]
	private struct psrdnoise0 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float3 n = srdnoise(uv);
			float4 color = float4(n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct psrdnoise1 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public float rot;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float3 n = srdnoise(uv, rot);
			float4 color = float4(n, 1f);
			colors[index] = color.As32();
		}
	}
}