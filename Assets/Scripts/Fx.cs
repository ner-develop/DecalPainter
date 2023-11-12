using UnityEngine;

public class Fx : MonoBehaviour
{
	bool _initialized;
	bool _isPlaying;
	float _elapsedTime;
	ParticleSystem[] _particleSystems;
	float _particleSystemDuration;


	public Fx Initialize()
	{
		if (_initialized) { return this; }
		_initialized = true;
		_particleSystems = GetComponentsInChildren<ParticleSystem>();
		foreach (var particle in _particleSystems)
		{
			_particleSystemDuration = Mathf.Max(_particleSystemDuration, particle.main.duration);
		}
		return this;
	}

	public Fx SetColor(Color color)
	{
		if (!_initialized) { Initialize(); }
		foreach (var particle in _particleSystems)
		{
			var main = particle.main;
			main.startColor = color;
		}
		return this;
	}

	public void Play(Vector3 position, Vector3 upward)
	{
		if (!_initialized) { Initialize(); }
		if (_isPlaying) { return; }

		transform.position = position;

		var forward = Vector3.Cross(upward, Vector3.right).normalized;
		transform.rotation = Quaternion.LookRotation(forward, upward);

		_isPlaying = true;
		_elapsedTime = 0f;

		foreach (var particle in _particleSystems)
		{
			particle.Play();
		}
	}



	void Update()
	{
		if (!_initialized || !_isPlaying) { return; }
		_elapsedTime += Time.deltaTime;
		if (_elapsedTime > _particleSystemDuration)
		{
			Destroy(gameObject);
		}
	}
}
