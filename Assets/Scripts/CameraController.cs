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

	private BoardSize _boardSize = new BoardSize();

	void Start()
	{
		Init();
		GuiController_ScreenChange(1f);
	}

	void OnEnable()
	{
		GuiController.ScreenChange += GuiController_ScreenChange;
	}

	void OnDisable()
	{
		GuiController.ScreenChange -= GuiController_ScreenChange;
	}

	private void Init()
	{
		_init = true;
	    Input.simulateMouseWithTouches = false;
	}

	private void GuiController_ScreenChange(float screenRatio)
	{
		if (screenRatio >= 1f)
			_minZ = MinZoom;
		else
			_minZ = MinZoom / screenRatio;

		if (!_init)
		{
		    var cameraComponent = GetComponent<Camera>();
		    cameraComponent.orthographicSize = cameraComponent.orthographicSize / screenRatio;

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
		{
		    var cameraComponent = GetComponent<Camera>();
		    cameraComponent.orthographicSize = Mathf.Clamp(cameraComponent.orthographicSize + sizeDelta, _minZ, _maxZ);
		}
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
	    CheckTouch();
	}

    private void CheckTouch()
    {
        var posChanged = false;
        var sizeChanged = false;

        while (_init)
        {
            _init = OnIncreaseSize(Vector2.zero, true);
            return;
        }

        var mouseInput = false;
        if (Input.mousePresent)
        {
            Vector2 mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                mouseInput = true;
                OnTouchStart(mousePosition);
            }

            else if (Input.GetMouseButton(0))
            {
                mouseInput = true;
                if (mousePosition == _prevTPos)
                    OnTouchStationary();
                else
                    OnTouchMove(mousePosition, 0, out posChanged);
            }

            else if (Input.GetMouseButtonUp(0))
            {
                mouseInput = true;
                OnTouchEnded(out posChanged);
            }

            var mouseScrollDelta = Input.mouseScrollDelta;
            if (mouseScrollDelta != Vector2.zero)
            {
                mouseInput = true;
                Zoom(mouseScrollDelta, mousePosition, out posChanged, out sizeChanged);
            }

            _prevTPos = mousePosition;
        }

        if (!mouseInput)
            switch (Input.touchCount)
            {
                case 1:
                    var t = Input.GetTouch(0);
                    switch (t.phase)
                    {
                        case TouchPhase.Began:
                            OnTouchStart(t.position);
                            break;

                        case TouchPhase.Stationary:
                            OnTouchStationary();
                            break;

                        case TouchPhase.Moved:
                            OnTouchMove(t.position, t.deltaTime, out posChanged);
                            break;

                        case TouchPhase.Ended:
                            OnTouchEnded(out posChanged);
                            break;

                        default:
                            //case TouchPhase.Canceled:
                            _velocity = Vector2.zero;
                            break;
                    }
                    _prevTPos = t.position;
                    break;

                case 2:
                    TouchZoom(Input.GetTouch(0), Input.GetTouch(1), out posChanged, out sizeChanged);
                    break;
            }

        if (posChanged)
        {
            OnPosChange(sizeChanged);
        }
    }

    private void OnTouchStart(Vector2 position)
    {
        _initTPos = position;
        _moveBegan = false;
    }

    private void OnTouchStationary()
    {
        _velocity = Vector2.zero;
    }

    private void OnTouchMove(Vector2 position, float deltaTime, out bool posChanged)
    {
        posChanged = false;

        if (position == _prevTPos)
        {
            _zeroCount += Time.deltaTime;
            if (_zeroCount > deltaTime)
                _velocity = Vector2.zero;
        }
        else
        {
            _zeroCount = 0;

            if (_moveBegan)
            {
                _velocity = GetScreenToWorldDistance(position, _prevTPos);
                posChanged = true;
            }
            else
            {
                var dist = GetScreenToWorldDistance(position, _initTPos);
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
    }

    private void OnTouchEnded(out bool posChanged)
    {
        posChanged = false;
        if (!_moveBegan)
            return;

        _velocityMag = _velocity.magnitude;
        if (Math.Abs(_velocityMag) < Drag)
            return;

        _isVelocity = true;
        _velocityDir = _velocity.normalized;
        if (_velocityMag > MaxVelocity)
        {
            _velocityMag = MaxVelocity;
            _velocity = _velocityDir * _velocityMag;
        }
        posChanged = true;
    }

    private void TouchZoom(Touch t0, Touch t1, out bool posChanged, out bool sizeChanged)
    {
        posChanged = false;
        sizeChanged = false;

        if (t0.phase != TouchPhase.Began && t1.phase != TouchPhase.Began &&
            (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
        {
            var cameraComponent = GetComponent<Camera>();

            var touchDelta = cameraComponent.ScreenToWorldPoint(t0.position) -
                             cameraComponent.ScreenToWorldPoint(t1.position);
            var touchDeltaMag = ((Vector2) touchDelta).magnitude;

            Vector2 t0Prev = cameraComponent.ScreenToWorldPoint(_prevT0Pos);
            Vector2 t1Prev = cameraComponent.ScreenToWorldPoint(_prevT1Pos);
            var prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;

            var sizeDelta = prevTouchDeltaMag - touchDeltaMag;
            SetSize(sizeDelta);
            sizeChanged = true;

            Vector2 midpoint = cameraComponent.ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
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
    }

    private void Zoom(Vector2 mouseScrollDelta, Vector2 position, out bool posChanged, out bool sizeChanged)
    {
        var sizeDelta = -mouseScrollDelta.y / 2;
        SetSize(sizeDelta);
        sizeChanged = true;

        var cameraComponent = GetComponent<Camera>();
        Vector2 midpoint = cameraComponent.ScreenToWorldPoint(new Vector2(Screen.width / 2f, Screen.height / 2f));

        _velocity = (midpoint - (Vector2) cameraComponent.ScreenToWorldPoint(position)).normalized * sizeDelta;
        posChanged = true;
    }

    private void OnPosChange(bool sizeChanged)
	{
		// ReSharper disable once RedundantCast
	    var cameraComponent = GetComponent<Camera>();
	    cameraComponent.transform.position += (Vector3) _velocity;
		if (!_boardSize.Max)
			OnIncreaseSize(_velocity, sizeChanged);

		Vector2 cameraDelta;
		if (!_init && OutOfBounds(out cameraDelta))
		{
			// ReSharper disable once RedundantCast
			cameraComponent.transform.position += (Vector3) cameraDelta;
		}
	}

	private Vector3 GetScreenToWorldDistance(Vector2 pos1, Vector2 pos2)
	{
	    var cameraComponent = GetComponent<Camera>();
	    var delta = cameraComponent.ScreenToWorldPoint(pos1 - pos2);
		return cameraComponent.ScreenToWorldPoint(Vector2.zero) - delta;
	}

	private bool OnIncreaseSize(Vector2 cameraDelta, bool checkAllSides, bool skipMaxZoom = false )
	{
		const float dist = 0.55f;
	    var cameraComponent = GetComponent<Camera>();
	    var ll = cameraComponent.ScreenToWorldPoint(Vector2.zero);
		var ur = cameraComponent.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		var changed = false;
		var rect = RectInt.Zero;

		Collider2D check;
		var pos = cameraComponent.transform.position;
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
			return changed;

		_boardSize = IncreaseSize(rect);
		if (!skipMaxZoom)
			SetMaxZoom(GuiController.ScreenRatio);

	    return changed;
	}

	private bool OutOfBounds(out Vector2 cameraDelta)
	{
		var outOfBounds = false;
		cameraDelta = Vector2.zero;

	    var cameraComponent = GetComponent<Camera>();
	    var ll = cameraComponent.ScreenToWorldPoint(Vector2.zero);
		var ur = cameraComponent.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

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