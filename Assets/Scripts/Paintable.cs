using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Paintable : MonoBehaviour
{
	[SerializeField] Texture _brushTexture;
	[SerializeField] bool _initializeOnAwake = true;

	MeshRenderer _meshRenderer;
	MeshFilter _meshFilter;
	DecalPainter _decalPainter;
	Material _material;

	bool _initialized;


	public void Initialize()
	{
		if (_initialized) { return; }
		_initialized = true;

		_meshRenderer = GetComponent<MeshRenderer>();
		_meshFilter = GetComponent<MeshFilter>();

		// マテリアルをインスタンス化してそれを扱う
		_material = _meshRenderer.material;
		if (_material == null)
		{
			_material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		}

		// このMesh用デカール累積テクスチャを生成・設定
		int textureSize = _material.mainTexture != null
			? _material.mainTexture.width
			: 1024;
		_decalPainter = new DecalPainter(_meshFilter, textureSize);
		_decalPainter.BakeBaseTexture(_material.mainTexture);
		_material.mainTexture = _decalPainter.texture;

		// ペイントテクスチャを設定
		_decalPainter.SetDecalTexture(_brushTexture);
	}

	public void Paint(Vector3 worldPosition, Vector3 normal, Vector3 tangent, float size, Color color)
	{
		if (!_initialized)
		{
			Debug.LogWarning("not initialized");
			return;
		}

		var positionOS = transform.InverseTransformPoint(worldPosition);
		var normalOS = transform.InverseTransformDirection(normal);
		var tangentOS = transform.InverseTransformDirection(tangent);
		_decalPainter.SetPointer(
			paintPositionOnObjectSpace: positionOS,
			normal: normalOS,
			tangent: tangentOS,
			decalSize: size,
			color: color,
			transformScale: transform.lossyScale
		);
		_decalPainter.Paint();
	}



	void Awake()
	{
		if (_initializeOnAwake)
		{
			Initialize();
		}
	}

	void OnDestroy()
	{
		_decalPainter?.Dispose();
		_decalPainter = null;

		if (_material != null)
		{
			Destroy(_material);
			_material = null;
		}
	}
}
