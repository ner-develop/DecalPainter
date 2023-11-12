using System;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Meshに対してモデル空間にてブラシ描画をし、その描画結果をUVに展開されたTextureとして更新する。
/// </summary>
public class DecalPainter : IDisposable
{
	const string SHADER_NAME = "DecalMapping";
	static readonly int _decalTextureNameID = Shader.PropertyToID("_DecalTexture");
	static readonly int _accumulateTextureNameID = Shader.PropertyToID("_AccumulateTexture");
	static readonly int _decalPositionOSNameID = Shader.PropertyToID("_DecalPositionOS");
	static readonly int _decalSizeNameID = Shader.PropertyToID("_DecalSize");
	static readonly int _decalNormalNameID = Shader.PropertyToID("_DecalNormal");
	static readonly int _decalTangentNameID = Shader.PropertyToID("_DecalTangent");
	static readonly int _colorNameID = Shader.PropertyToID("_Color");
	static readonly int _objectScaleNameID = Shader.PropertyToID("_ObjectScale");

	public Texture2D texture { get; private set; }
	public Material mappingMaterial { get; private set; }

	Mesh _targetMesh;


	public DecalPainter(MeshFilter targetMeshFilter, int textureSize = 2048)
	{
		// 転写に使う情報。強制したいのでMeshFilterでもらい、Meshのコピーを複製。
		_targetMesh = targetMeshFilter.mesh;

		// 累積テクスチャ
		texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
		var pixels = new Color[textureSize * textureSize];
		for (int i = 0; i < pixels.Length; ++i) { pixels[i] = Color.white; }
		texture.SetPixels(pixels);
		texture.Apply();

		// 転写用マテリアル
		// NOTE: 動的にシェーダーをFindしているので、ビルド時にはProjectSettings>Graphics>Always Included Shadersに入れておく必要がある。
		var shader = Shader.Find(SHADER_NAME);
		if (shader == null)
		{
			Debug.LogError($"not found {SHADER_NAME} shader");
			return;
		}
		mappingMaterial = new Material(shader);
		mappingMaterial.SetTexture(_accumulateTextureNameID, texture);
	}

	public void Dispose()
	{
		if (texture != null)
		{
			Object.Destroy(texture);
			texture = null;
		}
		if (mappingMaterial != null)
		{
			Object.Destroy(mappingMaterial);
			mappingMaterial = null;
		}
		if (_targetMesh != null)
		{
			Object.Destroy(_targetMesh);
			_targetMesh = null;
		}
	}

	/// <summary>
	/// 描画するデカール画像をセットする
	/// </summary>
	public void SetDecalTexture(Texture decalTexture)
	{
		mappingMaterial.SetTexture(_decalTextureNameID, decalTexture);
	}

	/// <summary>
	/// texture(累積テクスチャ)に上書き描画をする
	/// </summary>
	public void BakeBaseTexture(Texture source)
	{
		if (source == null) { return; }
		var src = source;
		var dst = texture;

		// RenderTargetの設定
		var temporaryActiveRenderTarget = RenderTexture.GetTemporary(dst.width, dst.height, 0);
		var activeRenderTexture = RenderTexture.active;
		RenderTexture.active = temporaryActiveRenderTarget;

		// sourceを描画して、dst(累積テクスチャ)に書き込む
		Graphics.Blit(src, temporaryActiveRenderTarget);
		dst.ReadPixels(new Rect(0f, 0f, dst.width, dst.height), 0, 0);
		dst.Apply();

		// RenderTargetを元に戻す
		RenderTexture.active = activeRenderTexture;
		RenderTexture.ReleaseTemporary(temporaryActiveRenderTarget);
	}

	/// <summary>
	/// マッピング用マテリアルにデカール情報をセットする。
	/// 累積テクスチャにデカールテクスチャを重畳してるだけ。
	/// 累積テクスチャに上書きするまで累積はされていかない。
	/// </summary>
	public void SetPointer(
		Vector3 paintPositionOnObjectSpace,
		Vector3 normal,
		Vector3 tangent,
		float decalSize,
		Color color,
		Vector3 transformScale
	)
	{
		mappingMaterial.SetVector(_decalPositionOSNameID, paintPositionOnObjectSpace);
		mappingMaterial.SetFloat(_decalSizeNameID, decalSize);
		mappingMaterial.SetVector(_decalNormalNameID, normal.normalized);
		mappingMaterial.SetVector(_decalTangentNameID, tangent.normalized);
		mappingMaterial.SetColor(_colorNameID, color);
		mappingMaterial.SetVector(_objectScaleNameID, transformScale);
	}

	/// <summary>
	/// texture(累積テクスチャ)に描画
	/// </summary>
	public void Paint()
	{
		var dst = texture;

		// RenderTargetの設定
		var temporaryRenderTexture = RenderTexture.GetTemporary(dst.width, dst.height, 0);
		var activeRenderTexture = RenderTexture.active;
		RenderTexture.active = temporaryRenderTexture;

		// 対象Meshを用いて、デカール画像を累積テクスチャに重ねてRenderTargetに描画する
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		mappingMaterial.SetPass(0);
		Graphics.DrawMeshNow(_targetMesh, Vector3.zero, Quaternion.identity);

		// RenderTargetを累積テクスチャに書き込む
		dst.ReadPixels(new Rect(0f, 0f, dst.width, dst.height), 0, 0);
		dst.Apply();

		// RenderTargetを元に戻す
		RenderTexture.active = activeRenderTexture;
		RenderTexture.ReleaseTemporary(temporaryRenderTexture);
	}
}
