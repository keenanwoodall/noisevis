using NaughtyAttributes;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[CreateAssetMenu]
public class pnoise : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 P, float2 rep):float")]
		_0,
		[InspectorName("(float3 P, float3 rep):float")]
		_1,
		[InspectorName("(float4 P, float4 rep):float")]
		_2,
	}

	public Signature signature;
	[ShowIf("ShowZ")]
	public float z;
	[ShowIf("ShowW")]
	public float w;

	[BoxGroup("rep")]
	public float repX = 5f;
	[BoxGroup("rep")]
	public float repY = 5f;
	[BoxGroup("rep"), ShowIf("ShowZ")]
	public float repZ = 5f;
	[BoxGroup("rep"), ShowIf("ShowW")]
	public float repW = 5f;

	private bool ShowZ() => signature == Signature._1 || signature == Signature._2;
	private bool ShowW() => signature == Signature._2;

	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new pnoise0 { rep = float2(repX, repY), res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
			case Signature._1:
				return new pnoise1 { rep = float3(repX, repY, repZ), z = z, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
			case Signature._2:
				return new pnoise2 { rep = float4(repX, repY, repZ, repW), z = z, w = w, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
		}
	}
	
	[BurstCompile]
	private struct pnoise0 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;
		public float2 rep;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = pnoise(uv, rep);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct pnoise1 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;
		public float3 rep;
		public float z;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = pnoise(float3(uv, z), rep);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct pnoise2 : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public NativeArray<Color32> colors;
		public float4 rep;
		public float z;
		public float w;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = pnoise(float4(uv, z, w), rep);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
}