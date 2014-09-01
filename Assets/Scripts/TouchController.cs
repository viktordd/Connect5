using UnityEngine;

public class TouchController : MonoBehaviour
{
	protected const float squareCenter = 0.5f;

	public float maxZoomTouch = 7f;
	public float maxMoveTouchCancel = 7f;
	public LayerMask notChecked;

	private bool touching = false;
	public float totalMovement;

	protected virtual void Update()
	{
		if (enabled)
		{
			CheckTouch();
		}
	}

	private void CheckTouch()
	{
		if (Input.touchCount != 1 || Camera.main.orthographicSize > maxZoomTouch)
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
					var position = Camera.main.ScreenToWorldPoint(touch.position);
					var obj = Physics2D.OverlapCircle(position, 0.1f, notChecked);
					if (obj != null)
					{
						touching = true;
						TouchStart(obj.gameObject);
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
						totalMovement += ((Vector2) Camera.main.ScreenToWorldPoint(touch.deltaPosition)).magnitude / Camera.main.orthographicSize;
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

	protected virtual void TouchStart(GameObject gObj) { }
	protected virtual void TouchCanceled() { }
	protected virtual void TouchEnded() { }
}
