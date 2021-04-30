using NaughtyAttributes;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[CreateAssetMenu]
public class psrnoise : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 pos, float2 per):float")]
		_0,
		[InspectorName("(float2 pos, float2 per, float rot):float")]
		_1,
	}

	public Signature signature;
	public Vector2 per = new Vector2(5f, 5f);
	[ShowIf("signature", Signature._1)]
	public float rot;

	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new psrnoise0 { per = per, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
			case Signature._1:
				return new psrnoise1 { per = per, rot = rot, res = resolution, frequency = frequency, colors = colors}.Schedule(resolution.AsArrayLength(), BatchCount);
		}
	}
	
	[BurstCompile]
	private struct psrnoise0 : IJobParallelFor
	{
		public int2 res;
		public float2 per;
		public float frequency;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = psrnoise(uv, per);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct psrnoise1 : IJobParallelFor
	{
		public int2 res;
		public float2 per;
		public float frequency;
		public float rot;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = psrnoise(uv, per, rot);
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
}