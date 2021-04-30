#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using static Unity.Mathematics.math;

[ExecuteAlways]
public class NoiseImage : MonoBehaviour
{
	private const int RES = 480;
	public float frequency = 15f;
	public NoiseProfile profile;

	[SerializeField, HideInInspector]
	private Texture2D texture;

	private void OnEnable()
	{
		texture = new Texture2D(RES, RES);
		if (TryGetComponent(out RawImage image))
			image.texture = texture;
	}

	private void OnDisable()
	{
		DestroyImmediate(texture);
		texture = null;
	}

	private void Update()
	{
		if (profile == null)
			return;
		
		var colors = texture.GetRawTextureData<Color32>();

		profile.Render(colors, int2(RES, RES), frequency).Complete();
		
		texture.Apply();
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(NoiseImage))]
public class NoiseImageEditor : Editor
{
	private SerializedProperty profileProperty;
	private Editor embeddedProfileEditor;

	private void OnEnable()
	{
		profileProperty = serializedObject.FindProperty(nameof(NoiseImage.profile));
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if (profileProperty.objectReferenceValue is NoiseProfile generator)
		{
			CreateCachedEditor(generator, null, ref embeddedProfileEditor);
			using (new EditorGUILayout.VerticalScope("box"))
			using (var check = new EditorGUI.ChangeCheckScope())
			{
				embeddedProfileEditor.OnInspectorGUI();
				if (check.changed)
				{
					foreach (Component t in targets)
						t.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
				}
			}

			if (GUILayout.Button("Save"))
			{
				var textureProperty = serializedObject.FindProperty("texture");
				if (textureProperty.objectReferenceValue is Texture2D texture)
				{
					string path = EditorUtility.SaveFilePanelInProject("Save png", "Noise", "png",
						"Please enter a file name to save the texture to");
					if (path.Length != 0)
					{
						byte[] pngData = texture.EncodeToPNG();
						if (pngData != null)
						{
							File.WriteAllBytes(path, pngData);

							// As we are saving to the asset folder, tell Unity to scan for modified or new assets
							AssetDatabase.Refresh();
						}
					}
				}
			}
		}
	}
}
#endif