using UnityEngine;

public class MeshPaintTester : MonoBehaviour
{
	[Header("Decal")]
	[SerializeField] MeshRenderer _decalPlane;

	[Header("Target")]
	[SerializeField] MeshFilter _targetMesh;
	[SerializeField] MeshRenderer _targetMeshRenderer;

	DecalPainter _decalPainter;
	Material _targetMeshMaterial;


	void Awake()
	{
		// TargetMeshのMaterialを複製して使う (参照先マテリアルを変更したくないのでInstantiateしたMaterialをSharedに入れて使う)
		_targetMeshMaterial = _targetMeshRenderer.material;
		_targetMeshRenderer.sharedMaterial = _targetMeshMaterial;

		// TargetMesh専用のデカール累積テクスチャを生成し、セットする
		_decalPainter = new DecalPainter(_targetMesh);
		_decalPainter.BakeBaseTexture(_targetMeshMaterial.mainTexture);
		_targetMeshMaterial.mainTexture = _decalPainter.texture;

		// デカール画像を設定する
		_decalPainter.SetDecalTexture(_decalPlane.sharedMaterial.mainTexture);
	}

	void OnDestroy()
	{
		_decalPainter?.Dispose();
		_decalPainter = null;

		if (_targetMeshMaterial != null)
		{
			Destroy(_targetMeshMaterial);
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			var decalPlaneTransform = _decalPlane.transform;

			// ペイント情報をセットアップ
			var targetMeshTransform = _targetMesh.transform;
			var size = 1;
			_decalPainter.SetPointer(
				paintPositionOnObjectSpace: targetMeshTransform.InverseTransformPoint(decalPlaneTransform.position),
				normal: targetMeshTransform.InverseTransformDirection(decalPlaneTransform.up),
				tangent: targetMeshTransform.InverseTransformDirection(decalPlaneTransform.right),
				decalSize: size,
				color: Color.white,
				transformScale: targetMeshTransform.lossyScale
			);

			// 累積描画
			_decalPainter.Paint();
		}
	}
	
	
#if UNITY_EDITOR
	// Inspectorにボタン表示
	[UnityEditor.CustomEditor(typeof(MeshPaintTester))]
	public class MeshPaintTesterEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			UnityEditor.EditorGUILayout.Space(30);
			UnityEditor.EditorGUILayout.HelpBox("Spaceキーでペイント", UnityEditor.MessageType.Info);
		}
	}
#endif
	
}
