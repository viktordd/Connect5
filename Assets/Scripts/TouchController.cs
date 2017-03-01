using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMemberHiearchy.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable MergeConditionalExpression
// ReSharper disable MemberCanBeProtected.Global

[UsedImplicitly]
public class TouchController : MonoBehaviour
{
	public float MaxZoomTouch = 10f;
	public LayerMask NotCheckedMask;

	private bool _touching;
	protected bool Disabled;

	protected virtual void Start()
	{
		CameraController.MoveBegan += CameraController_MoveBegan;
	}

	void CameraController_MoveBegan()
	{
		if (_touching)
		{
			_touching = false;
			TouchCanceled();
		}
	}

	protected virtual void Update()
	{
		CheckTouch();
	}

	private void CheckTouch()
	{
		var disableTouch = Camera.main.orthographicSize > MaxZoomTouch;

		if (disableTouch != Disabled)
		{
			Disabled = disableTouch;
			TouchEnabledChanged();
		}

		if (disableTouch || !(Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.touchCount == 1))
		{
			if (!_touching)
				return;
			_touching = false;
			TouchCanceled();
		}
		else if (Input.GetMouseButtonDown(0))
		{
			OnTouchStart(Input.mousePosition);
		}
		else if (_touching && Input.GetMouseButtonUp(0))
		{
			OnTouchEnd(Input.mousePosition);
		}
		else if (Input.touchCount == 1)
		{
			var touch = Input.GetTouch(0);
			switch (touch.phase)
			{
				case TouchPhase.Began:
					OnTouchStart(touch.position);
					break;
				case TouchPhase.Ended:
					if (_touching)
						OnTouchEnd(touch.position);
					break;
				case TouchPhase.Canceled:
					_touching = false;
					break;
			}
		}
	}

	private void OnTouchStart(Vector2 touchPos)
	{
		if (GuiController.InGui(touchPos))
		{
			_touching = false;
			return;
		}
		var position = Camera.main.ScreenToWorldPoint(touchPos);
		var obj = GetSquare(position, NotCheckedMask);
		if (obj == null)
			return;
		_touching = true;
		TouchStart(obj);
	}
	private void OnTouchEnd(Vector2 touchPos)
	{
		_touching = false;
		if (GuiController.InGui(touchPos))
		{
			TouchCanceled();
			return;
		}
		var position = Camera.main.ScreenToWorldPoint(touchPos);
		var obj = GetSquare(position, NotCheckedMask);
		TouchEnded(obj);
	}

	protected static GameObject GetSquare(Vector2 position, LayerMask mask)
	{
		var overlapPointCollider = Physics2D.OverlapPoint(position, mask);
		return overlapPointCollider == null ? null : overlapPointCollider.gameObject;
	}

	protected virtual void TouchStart(GameObject gObj) { }
	protected virtual void TouchCanceled() { }
	protected virtual void TouchEnded(GameObject gObj = null) { }
	protected virtual void TouchEnabledChanged() { }
}
