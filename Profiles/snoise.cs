using NaughtyAttributes;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[CreateAssetMenu]
public class snoise : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 v):float")]
		_0,
		[InspectorName("(float3 v):float")]
		_1,
		[InspectorName("(float3 v, out float3 gradient):float")]
		_2,
		[InspectorName("(float4 v):float")]
		_3,
	}
	public enum OutputType { Float, Gradient }

	public Signature signature;
	[ShowIf("signature", Signature._2)]
	public OutputType outputType;
	[ShowIf("ShowZ")]
	public float z;
	[ShowIf("ShowW")]
	public float w;

	private bool ShowZ() => signature != Signature._0;
	private bool ShowW() => signature == Signature._3;

	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new psrnoise0 { res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
			case Signature._1:
				return new psrnoise1 { z = z, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
			case Signature._2:
				return new psrnoise2 { outputGradient = outputType == OutputType.Gradient, z = z, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
			case Signature._3:
				return new psrnoise3 { z = z, w = w, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
		}
	}
	
	[BurstCompile]
	private struct psrnoise0 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = snoise(uv);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct psrnoise1 : IJobParallelFor
	{
		public int2 res;
		public float z;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = snoise(float3(uv, z));
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct psrnoise2 : IJobParallelFor
	{
		public int2 res;
		public float z;
		public bool outputGradient;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = snoise(float3(uv, z), out var gradient);
			float4 color = float4(1f);
			if (outputGradient)
				color.xyz = gradient;
			else
				color.xyz = n;
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct psrnoise3 : IJobParallelFor
	{
		public int2 res;
		public float z;
		public float w;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = snoise(float4(uv, z, w));
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
}