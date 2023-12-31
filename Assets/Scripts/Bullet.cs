using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] Renderer _renderer;
	[SerializeField] Rigidbody _rigidbody;

	[Header("Prefabs")]
	[SerializeField] Fx _fxPrefab;

	bool _initialized;
	bool _isFired;
	float _elapsedTime;
	float _size = 1f;



	public Bullet Initialize()
	{
		if (_initialized) { return this; }
		_initialized = true;
		return this;
	}

	public Bullet SetMaterial(Material material)
	{
		_renderer.sharedMaterial = material;
		return this;
	}

	public Material GetMaterial()
	{
		return _renderer.sharedMaterial;
	}

	public Bullet SetPosition(Vector3 position)
	{
		transform.position = position;
		return this;
	}

	public Bullet SetPaintSize(float size)
	{
		_size = size;
		return this;
	}

	public void Fire(Vector3 direction, float speed)
	{
		if (!_initialized || _isFired) { return; }
		_isFired = true;
		_rigidbody.velocity = direction * speed;
		_elapsedTime = 0f;
	}

	void Hit(Paintable paintable, ContactPoint contactPoint)
	{
		var normal = contactPoint.normal;
		var tangent = Vector3.Cross(normal, Vector3.right).normalized;
		var hitPosition = contactPoint.point;

		if (tangent.sqrMagnitude < 0.01f)
		{
			tangent = Vector3.Cross(normal, Vector3.forward).normalized;
		}

		paintable.Paint(
			worldPosition: hitPosition,
			normal: normal,
			tangent: tangent,
			size: _size,
			color: _renderer.sharedMaterial.color
		);

		if (_fxPrefab != null)
		{
			var fx = Instantiate(_fxPrefab);
			fx.Initialize()
				.SetColor(_renderer.sharedMaterial.color)
				.Play(transform.position, normal);
		}

		Destroy(gameObject);
	}


	void Update()
	{
		if (!_initialized) { return; }
		_elapsedTime += Time.deltaTime;
	}

	void OnCollisionEnter(Collision other)
	{
		if (!_initialized) { return; }
		if (_elapsedTime < 0.05f) { return; }

		if (other.contactCount > 0 && other.gameObject.TryGetComponent(out Paintable paintable))
		{
			Hit(paintable, other.GetContact(0));

			// Debug確認用Plane作成
			/*var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			plane.transform.position = hitPosition + normal;
			plane.transform.rotation = Quaternion.LookRotation(Vector3.Cross(normal, tangent).normalized, normal);
			plane.transform.localScale = Vector3.one * (_size * 0.25f);
			Destroy(plane.GetComponent<Collider>());*/
		}
	}
}
