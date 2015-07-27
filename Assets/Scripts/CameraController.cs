using System;
using JetBrains.Annotations;
using UnityEngine;
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable UseNullPropagation

[UsedImplicitly]
public class CameraController : MonoBehaviour
{
	public float MoveDeadZone = 0.05f;
	public float MinZoom = 2.5f;
	public float MaxZoom = 15f;
	public LayerMask SquareMask;

	public float Drag = 0.015f;
	public float MaxVelocity = 0.6f;

	private float _minZ;
	private float _maxZ;

	public static event Func<RectInt, BoardSize> IncreaseSize;
	public static event Action MoveBegan;

	private float _zeroCount;
	private bool _isVelocity;
	private Vector2 _velocityDir;
	private float _velocityMag;
	private Vector2 _velocity;

	private bool _moveBegan;
	private Vector2 _initTPos;
	private Vector2 _prevTPos;
	private Vector2 _prevT0Pos;
	private Vector2 _prevT1Pos;

	private bool _init;
	private byte _initSizeCheck;

	private BoardSize _boardSize = new BoardSize();

	void Start()
	{
		Init();
		GUIController_ScreenChange(1f);
	}

	void OnEnable()
	{
		GuiController.ScreenChange += GUIController_ScreenChange;
	}

	void OnDisable()
	{
		GuiController.ScreenChange -= GUIController_ScreenChange;
	}

	private void Init()
	{
		_init = true;
		_initSizeCheck = 0;
	}

	private void GUIController_ScreenChange(float screenRatio)
	{
		if (screenRatio >= 1f)
			_minZ = MinZoom;
		else
			_minZ = MinZoom / screenRatio;

		if (!_init)
		{
			GetComponent<Camera>().orthographicSize = GetComponent<Camera>().orthographicSize / screenRatio;

			if (!_boardSize.Max)
				OnIncreaseSize(Vector2.zero, true, true);

			SetMaxZoom(screenRatio);

			Vector2 cameraDelta;
			if (OutOfBounds(out cameraDelta))
				SetSize(-cameraDelta.magnitude);

		}
	}

	private void SetMaxZoom(float screenRatio)
	{
		_maxZ = Mathf.Min(MaxZoom, _boardSize.Rect.width / 2 / screenRatio, _boardSize.Rect.height / 2);
		SetSize();
	}

	private void SetSize(float sizeDelta = 0)
	{
		if (!_init)
			GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize + sizeDelta, _minZ, _maxZ);
	}

	void FixedUpdate()
	{
		if (_isVelocity)
		{
			if (Input.touchCount > 0)
			{
				_isVelocity = false;
				return;
			}

			_velocityMag -= Drag;
			if (_velocityMag <= 0)
			{
				_isVelocity = false;
				return;
			}
			_velocity = _velocityDir * _velocityMag;

			OnPosChange(false);
		}
	}

	void Update()
	{
		bool posChanged = false, sizeChanged = false;

		while (_init)
		{
			OnIncreaseSize(Vector2.zero, true);
			_init = _initSizeCheck++ < 4;
			return;
		}

		switch (Input.touchCount)
		{
			case 1:
				var t = Input.GetTouch(0);
				switch (t.phase)
				{
					case TouchPhase.Began:
						_initTPos = t.position;
						_moveBegan = false;
						break;

					case TouchPhase.Moved:
						if (t.position == _prevTPos)
						{
							_zeroCount += Time.deltaTime;
							if (_zeroCount > t.deltaTime)
								_velocity = Vector2.zero;
						}
						else
						{
							_zeroCount = 0;

							if (_moveBegan)
							{
								_velocity = GetScreenToWorldDistance(t.position, _prevTPos);
								posChanged = true;
							}
							else
							{
								var dist = GetScreenToWorldDistance(t.position, _initTPos);
								var magnitude = dist.magnitude;
								if (magnitude > MoveDeadZone)
								{
									_velocity = dist.normalized * (magnitude - MoveDeadZone);
									posChanged = true;
									_moveBegan = true;
									if (MoveBegan != null)
										MoveBegan();
								}
							}
						}
						break;

					case TouchPhase.Ended:
						if (_moveBegan)
						{
							_velocityMag = _velocity.magnitude;
							if (_velocityMag > 0)
							{
								_isVelocity = true;
								_velocityDir = _velocity.normalized;
								if (_velocityMag > MaxVelocity)
								{
									_velocityMag = MaxVelocity;
									_velocity = _velocityDir * _velocityMag;
								}
								posChanged = true;
							}
						}
						break;
				}
				_prevTPos = t.position;
				break;

			case 2:
				var t0 = Input.GetTouch(0);
				var t1 = Input.GetTouch(1);

				if (t0.phase != TouchPhase.Began && t1.phase != TouchPhase.Began &&
					(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
				{
					var touchDeltaMag = ((Vector2)(GetComponent<Camera>().ScreenToWorldPoint(t0.position) - GetComponent<Camera>().ScreenToWorldPoint(t1.position))).magnitude;

					Vector2 t0Prev = GetComponent<Camera>().ScreenToWorldPoint(_prevT0Pos);
					Vector2 t1Prev = GetComponent<Camera>().ScreenToWorldPoint(_prevT1Pos);
					var prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;

					var sizeDelta = prevTouchDeltaMag - touchDeltaMag;
					SetSize(sizeDelta);
					sizeChanged = true;

					Vector2 midpoint = GetComponent<Camera>().ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
					var midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

					_velocity = midpointPrev - midpoint;
					posChanged = true;
				}
				if (t0.phase == TouchPhase.Canceled || t0.phase == TouchPhase.Ended)
					_prevTPos = t1.position;
				if (t1.phase == TouchPhase.Canceled || t1.phase == TouchPhase.Ended)
					_prevTPos = t0.position;

				_prevT0Pos = t0.position;
				_prevT1Pos = t1.position;
				break;

		}

		if (posChanged)
		{
			OnPosChange(sizeChanged);
		}
	}

	private void OnPosChange(bool sizeChanged)
	{
		// ReSharper disable once RedundantCast
		GetComponent<Camera>().transform.position += (Vector3) _velocity;
		if (!_boardSize.Max)
			OnIncreaseSize(_velocity, sizeChanged);

		Vector2 cameraDelta;
		if (!_init && OutOfBounds(out cameraDelta))
		{
			// ReSharper disable once RedundantCast
			GetComponent<Camera>().transform.position += (Vector3) cameraDelta;
		}
	}

	private Vector3 GetScreenToWorldDistance(Vector2 pos1, Vector2 pos2)
	{
		var delta = GetComponent<Camera>().ScreenToWorldPoint(pos1 - pos2);
		return GetComponent<Camera>().ScreenToWorldPoint(Vector2.zero) - delta;
	}

	private void OnIncreaseSize(Vector2 cameraDelta, bool checkAllSides, bool skipMaxZoom = false )
	{
		const float dist = 0.55f;
		var ll = GetComponent<Camera>().ScreenToWorldPoint(Vector2.zero);
		var ur = GetComponent<Camera>().ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		var changed = false;
		var rect = RectInt.Zero;

		Collider2D check;
		var pos = GetComponent<Camera>().transform.position;
		if (cameraDelta.y > 0 || checkAllSides) // up
		{
			check = Physics2D.OverlapPoint(new Vector2(pos.x, ur.y + dist), SquareMask);
			if (check == null)
			{
				changed = true;
				rect.Height = 1;
			}
		}

		if (cameraDelta.x > 0 || checkAllSides) // right
		{
			check = Physics2D.OverlapPoint(new Vector2(ur.x + dist, pos.y), SquareMask);
			if (check == null)
			{
				changed = true;
				rect.Width = 1;
			}
		}

		if (cameraDelta.y < 0 || checkAllSides) // down
		{
			check = Physics2D.OverlapPoint(new Vector2(pos.x, ll.y - dist), SquareMask);
			if (check == null)
			{
				changed = true;
				rect.Y = 1;
			}
		}

		if (cameraDelta.x < 0 || checkAllSides) // left
		{
			check = Physics2D.OverlapPoint(new Vector2(ll.x - dist, pos.y), SquareMask);
			if (check == null)
			{
				changed = true;
				rect.X = 1;
			}
		}
		if (!changed || IncreaseSize == null)
			return;

		_boardSize = IncreaseSize(rect);
		if (!skipMaxZoom)
			SetMaxZoom(GuiController.ScreenRatio);
	}

	private bool OutOfBounds(out Vector2 cameraDelta)
	{
		var outOfBounds = false;
		cameraDelta = Vector2.zero;

		var ll = GetComponent<Camera>().ScreenToWorldPoint(Vector2.zero);
		var ur = GetComponent<Camera>().ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

		if (ll.x < _boardSize.Rect.xMin)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(_boardSize.Rect.xMin - ll.x, cameraDelta.y);
		}

		if (ll.y < _boardSize.Rect.yMin)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(cameraDelta.x, _boardSize.Rect.yMin - ll.y);
		}

		if (ur.x > _boardSize.Rect.xMax)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(_boardSize.Rect.xMax - ur.x, cameraDelta.y);
		}

		if (ur.y > _boardSize.Rect.yMax)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(cameraDelta.x, _boardSize.Rect.yMax - ur.y);
		}
		return outOfBounds;
	}
}