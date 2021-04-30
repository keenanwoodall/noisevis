using NaughtyAttributes;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[CreateAssetMenu]
public class cnoise : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 P):float")]
		_0,
		[InspectorName("(float3 P):float")]
		_1,
		[InspectorName("(float4 P):float")]
		_2,
	}

	public Signature signature;
	[ShowIf("ShowZ")]
	public float z;
	[ShowIf("ShowW")]
	public float w;

	private bool ShowZ() => signature == Signature._1 || signature == Signature._2;
	private bool ShowW() => signature == Signature._2;

	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new cnoise0 { res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
			case Signature._1:
				return new cnoise1 { z = z, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
			case Signature._2:
				return new cnoise2 { z = z, w = w, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
		}
	}
	
	[BurstCompile]
	private struct cnoise0 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = cnoise(uv);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct cnoise1 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public float z;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = cnoise(float3(uv, z));
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct cnoise2 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public float z;
		public float w;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = cnoise(float4(uv, z, w));
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
}