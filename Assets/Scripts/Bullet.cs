using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] Renderer _renderer;
	[SerializeField] Rigidbody _rigidbody;

	bool _initialized;
	bool _isFired;
	float _elapsedTime;



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

	public void Fire(Vector3 direction, float speed)
	{
		if (!_initialized || _isFired) { return; }
		_isFired = true;
		_rigidbody.velocity = direction * speed;
		_elapsedTime = 0f;
	}

	void Hit()
	{
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
		if (_elapsedTime < 0.1f) { return; }
		Hit();
	}
}
