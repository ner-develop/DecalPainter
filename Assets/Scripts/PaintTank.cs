using UnityEngine;

public class PaintTank : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField] float _acceleration = 1f;
	[SerializeField] float _maxLinearVelocity = 1f;
	[SerializeField] float _rotateSpeed = 1f;
	[SerializeField] float _maxAngularVelocity = 1f;

	[Header("Components")]
	[SerializeField] Rigidbody _rigidbody;


	bool _initialized;
	Transform _transform;
	Vector3 _moveDirection;

	public void Initialize()
	{
		if (_initialized) { return; }

		_initialized = true;
		_transform = transform;
	}

	public void Move(Vector3 direction)
	{
		_moveDirection = (_moveDirection + direction.normalized).normalized;
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

		_moveDirection = Vector3.zero;
	}


	void Awake()
	{
		Initialize();
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
		UpdateManually(Time.deltaTime);
	}
}
