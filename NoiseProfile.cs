using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

public abstract class NoiseProfile : ScriptableObject
{
	protected const int BatchCount = 32;
	public abstract JobHandle Render(NativeArray<Color32> colors, int2 resolution, float frequency);
}