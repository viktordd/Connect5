using UnityEngine;

public class TouchController : MonoBehaviour
{
	public float maxZoomTouch = 7f;
	public float maxMoveTouchCancel = 7f;
	public LayerMask notCheckedMask;

	private bool touching = false;
	private float totalMovement;
	protected bool disabled;

	protected virtual void Update()
	{
		CheckTouch();
	}

	private void CheckTouch()
	{
		bool disableTouch = Camera.main.orthographicSize > maxZoomTouch;

		if (disableTouch != disabled)
		{
			disabled = disableTouch;
			TouchEnabledChanged();
		}

		if (Input.touchCount != 1 || disableTouch)
		{
			if (touching)
			{
				touching = false;
				TouchCanceled();
			}
		}
		else
		{
			Touch touch = Input.GetTouch(0);
			switch (touch.phase)
			{
				case TouchPhase.Began:
					totalMovement = 0f;
					if (GUIController.InGui(touch.position))
					{
						touching = false;
						return;
					}
					var position = Camera.main.ScreenToWorldPoint(touch.position);
					var obj = GetSquare(position, notCheckedMask);
					if (obj != null)
					{
						touching = true;
						TouchStart(obj);
					}
					break;
				case TouchPhase.Ended:
					if (touching)
					{
						touching = false;
						TouchEnded();
					}
					break;
				case TouchPhase.Moved:
					if (touching)
					{
						totalMovement += ((Vector2)Camera.main.ScreenToWorldPoint(touch.deltaPosition)).magnitude / Camera.main.orthographicSize;
						if (totalMovement > maxMoveTouchCancel)
						{
							touching = false;
							TouchCanceled();
						}
					}
					break;
				case TouchPhase.Canceled:
					touching = false;
					break;
			}
		}
	}

	protected GameObject GetSquare(Vector2 position, LayerMask mask)
	{
		Collider2D collider = Physics2D.OverlapPoint(position, mask);
		return collider == null ? null : collider.gameObject;
	}

	protected virtual void TouchStart(GameObject gObj) { }
	protected virtual void TouchCanceled() { }
	protected virtual void TouchEnded() { }
	protected virtual void TouchEnabledChanged() { }
}
