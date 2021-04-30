using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

using NaughtyAttributes;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class cellular : NoiseProfile
{
	public enum Signature
	{
		[InspectorName("(float2 P):float2")]
		_0,
		[InspectorName("(float3 P):float2")]
		_1
	}

	public enum OutputType : int
	{
		[InspectorName("x")]
		X = 0,
		[InspectorName("y")]
		Y = 1
	}
	
	public Signature signature;
	public OutputType outputType;
	[ShowIf("signature", Signature._1)]
	public float z;
	
	public override JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency)
	{
		switch (signature)
		{
			default:
			case Signature._0:
				return new cellular0Job {res = resolution, frequency = frequency, channel = (int) outputType, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
			case Signature._1:
				return new cellular1Job {z = z, res = resolution, frequency = frequency, channel = (int) outputType, colors = colors}.Schedule(resolution.AsArrayLength(), 64);
		}
	}
	
	[BurstCompile]
	private struct cellular0Job : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public int channel;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = cellular(uv)[channel];
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
	[BurstCompile]
	private struct cellular1Job : IJobParallelFor
	{
		public int2 res;
		public float frequency;
		public int channel;
		public float z;
		public NativeArray<Color32> colors;

		public void Execute(int index)
		{
			float2 uv = index.ToUV(res) * frequency;
			float n = cellular(float3(uv, z))[channel];
			float4 color = float4(n, n, n, 1f);
			colors[index] = color.As32();
		}
	}
}