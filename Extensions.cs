using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class Extensions
{
	public static Color32 As32(this float4 color)
	{
		color = saturate(color);
		return new Color32((byte)(color.x * 255), (byte)(color.y * 255), (byte)(color.z * 255), (byte)(color.w * 255));
	}

	public static int2 ToTextureCoordinate(this int index, int width)
	{
		return int2(index % width, index / width);
	}

	public static float2 ToUV(this int index, int2 resolution)
	{
		return index.ToTextureCoordinate(resolution.x) / (float2)resolution;
	}

	public static int AsArrayLength(this int2 res) => res.x * res.y;
}