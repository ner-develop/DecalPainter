using UnityEngine;
using UnityEngine.Serialization;

public class PaintTank : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField] float _acceleration = 1f;
	[SerializeField] float _maxLinearVelocity = 1f;
	[SerializeField] float _rotateSpeed = 1f;
	[SerializeField] float _maxAngularVelocity = 1f;
	[SerializeField] float _turretRotateSpeed = 30f;
	[SerializeField] Color _initializePaintColor = Color.red;
	[SerializeField] float _bulletSpeed = 10f;

	[Header("Components")]
	[SerializeField] Rigidbody _rigidbody;
	[SerializeField] Transform _turret;
	[SerializeField] Transform _muzzle;

	[Header("Prefabs")]
	[SerializeField] Bullet _bulletPrefab;


	bool _initialized;
	Transform _transform;
	Vector3 _moveDirection;
	Quaternion _initialTurretRotation;
	float _turretAngle;
	int _turretRotateDirection;
	Material _bulletMaterial;

	public void Initialize()
	{
		if (_initialized) { return; }

		_initialized = true;
		_transform = transform;
		_initialTurretRotation = _turret.localRotation;

		if (_bulletPrefab != null)
		{
			_bulletMaterial = new Material(_bulletPrefab.GetMaterial()) { color = _initializePaintColor };
		}
	}

	public void Move(Vector3 direction)
	{
		_moveDirection = (_moveDirection + direction.normalized).normalized;
	}

	public void RotateTurretUpward()
	{
		_turretRotateDirection = 1;
	}

	public void RotateTurretDownward()
	{
		_turretRotateDirection = -1;
	}

	public void Fire()
	{
		var bullet = Instantiate(_bulletPrefab);
		bullet.Initialize()
			.SetMaterial(_bulletMaterial)
			.SetPosition(_muzzle.position)
			.Fire(_muzzle.forward, _bulletSpeed);
	}

	void UpdateManually(float dt)
	{
		if (!_initialized) { return; }

		_rigidbody.maxLinearVelocity = _maxLinearVelocity;
		_rigidbody.maxAngularVelocity = _maxAngularVelocity;

		var forwardPower = _moveDirection.z;
		var currentVelocity = Vector3.Dot(_transform.forward, _rigidbody.velocity);
		var velocity = currentVelocity + forwardPower * _acceleration * dt;
		var dv = velocity - currentVelocity;
		var acceleration = dv / dt;
		var force = _rigidbody.mass * acceleration;
		_rigidbody.AddForce(force * _transform.forward, ForceMode.Force);
		
		var rotatePower = _moveDirection.x;
		var angularVelocity = rotatePower * _rotateSpeed * _transform.up;
		var id = _rigidbody.inertiaTensor;
		var ir = _rigidbody.inertiaTensorRotation;
		var iri = Quaternion.Inverse(ir);
		var torque = ir * Vector3.Scale(id, iri * angularVelocity);
		_rigidbody.AddTorque(torque, ForceMode.Force);

		_turretAngle = Mathf.Clamp(_turretAngle + _turretRotateDirection * _turretRotateSpeed * dt, 0, 70f);
		_turret.localRotation = _initialTurretRotation * Quaternion.AngleAxis(_turretAngle, Vector3.right);

		_moveDirection = Vector3.zero;
		_turretRotateDirection = 0;
	}


	void Awake()
	{
		Initialize();
	}

	void OnDestroy()
	{
		if (_bulletMaterial != null)
		{
			Destroy(_bulletMaterial);
		}
	}

	void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.W))
		{
			Move(Vector3.forward);
		}
		if (Input.GetKey(KeyCode.S))
		{
			Move(Vector3.back);
		}
		if (Input.GetKey(KeyCode.A))
		{
			Move(Vector3.left);
		}
		if (Input.GetKey(KeyCode.D))
		{
			Move(Vector3.right);
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			RotateTurretUpward();
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			RotateTurretDownward();
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Fire();
		}
		UpdateManually(Time.deltaTime);
	}
}
