using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float minZoom = 2.5f;
	public float maxZoom = 10f;
	private float minZ;
	private float maxZ;

	public LayerMask squareMask;

	public static event Func<RectInt, Vector2[]> IncreaseSize;

	private Vector2 prevT0Pos, prevT1Pos;

	private byte initSizeCheck = 0;

	private Vector2[] maxBoardSize;

	void Start()
	{
		GUIController_ScreenChange(1f);
		onEnable();
	}

	void onEnable()
	{
		GUIController.ScreenChange += GUIController_ScreenChange;
	}

	void onDisable()
	{
		GUIController.ScreenChange -= GUIController_ScreenChange;
	}

	private void GUIController_ScreenChange(float screenRatio)
	{
		if (screenRatio >= 1f)
		{
			minZ = minZoom;
			maxZ = maxZoom;
			if (initSizeCheck >= 4)
				camera.orthographicSize = Mathf.Clamp(camera.orthographicSize * screenRatio, minZ, maxZ);
		}
		else
		{
			minZ = minZoom / screenRatio;
			maxZ = maxZoom / screenRatio;
			if (initSizeCheck >= 4)
				camera.orthographicSize = Mathf.Clamp(camera.orthographicSize * screenRatio, minZ, maxZ);
		}
		initSizeCheck = 0;
	}

	void Update()
	{
		bool sizeChanged = false;
		bool posChanged = false;
		Vector2 cameraDelta = Vector2.zero;

		bool init = initSizeCheck < 4;
		if (init)
		{
			posChanged = true;
			sizeChanged = true;
			initSizeCheck++;
		}

		switch (Input.touchCount)
		{
			case 1:
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Moved)
				{
					Vector3 delta = camera.ScreenToWorldPoint(t.position - prevT0Pos);
					Vector3 bl = camera.ScreenToWorldPoint(Vector2.zero);
					cameraDelta = bl - delta;
					posChanged = true;
				}
				prevT0Pos = t.position;
				break;

			case 2:
				Touch t0 = Input.GetTouch(0);
				Touch t1 = Input.GetTouch(1);

				if (t0.phase != TouchPhase.Began && t1.phase != TouchPhase.Began &&
					(t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
				{
					float touchDeltaMag = ((Vector2)(camera.ScreenToWorldPoint(t0.position) - camera.ScreenToWorldPoint(t1.position))).magnitude;

					Vector2 t0Prev = camera.ScreenToWorldPoint(prevT0Pos);
					Vector2 t1Prev = camera.ScreenToWorldPoint(prevT1Pos);
					float prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;

					camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - touchDeltaMag + prevTouchDeltaMag, minZ, maxZ);
					sizeChanged = true;

					Vector2 midpoint = camera.ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
					Vector2 midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

					cameraDelta = midpointPrev - midpoint;
					posChanged = true;
				}
				prevT0Pos = t0.position;
				prevT1Pos = t1.position;
				break;
		}

		if (posChanged)
		{
			ChangePosition(cameraDelta);

			var ll = camera.ScreenToWorldPoint(Vector2.zero);
			var ur = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

			if (maxBoardSize == null)
			{
				var rect = CheckSize(cameraDelta, sizeChanged, ur, ll);

				if (rect != RectInt.zero)
				{
					if (IncreaseSize != null)
					{
						var boardSize = IncreaseSize(rect);

						if (boardSize != null && !init)
						{
							AdjustPosition(boardSize, ur, ll);

							if (boardSize[2] == Vector2.one)
								maxBoardSize = boardSize;
						}
					}
				}
			}
			else
			{
				AdjustPosition(maxBoardSize, ur, ll);
			}
		}
	}

	private RectInt CheckSize(Vector2 cameraDelta, bool sizeChanged, Vector3 ur, Vector3 ll)
	{
		var pos = camera.transform.position;
		Collider2D check;
		var rect = RectInt.zero;
		if (cameraDelta.y > 0 || sizeChanged) // up
		{
			check = Physics2D.OverlapCircle(new Vector2(pos.x, ur.y + 1), 0.1f, squareMask);
			if (check == null)
				rect.height = 1;
		}

		if (cameraDelta.x > 0 || sizeChanged) // right
		{
			check = Physics2D.OverlapCircle(new Vector2(ur.x + 1, pos.y), 0.1f, squareMask);
			if (check == null)
				rect.width = 1;
		}

		if (cameraDelta.y < 0 || sizeChanged) // down
		{
			check = Physics2D.OverlapCircle(new Vector2(pos.x, ll.y - 1), 0.1f, squareMask);
			if (check == null)
				rect.y = 1;
		}

		if (cameraDelta.x < 0 || sizeChanged) // left
		{
			check = Physics2D.OverlapCircle(new Vector2(ll.x - 1, pos.y), 0.1f, squareMask);
			if (check == null)
				rect.x = 1;
		}
		return rect;
	}

	private void AdjustPosition(Vector2[] boardSize, Vector3 ur, Vector3 ll)
	{
		var posChanged = false;
		var cameraDelta = Vector2.zero;

		if (ll.x < boardSize[0].x)
		{
			posChanged = true;
			cameraDelta = new Vector2(boardSize[0].x - ll.x, cameraDelta.y);
		}

		if (ll.y < boardSize[0].y)
		{
			posChanged = true;
			cameraDelta = new Vector2(cameraDelta.x, boardSize[0].y - ll.y);
		}

		if (ur.x > boardSize[1].x)
		{
			posChanged = true;
			cameraDelta = new Vector2(boardSize[1].x - ur.x, cameraDelta.y);
		}

		if (ur.y > boardSize[1].y)
		{
			posChanged = true;
			cameraDelta = new Vector2(cameraDelta.x, boardSize[1].y - ur.y);
		}

		if (posChanged)
			ChangePosition(cameraDelta);
	}

	private void ChangePosition(Vector2 cameraDelta)
	{
		var pos = camera.transform.position;
		camera.transform.position = new Vector3(pos.x + cameraDelta.x, pos.y + cameraDelta.y, pos.z);
	}
}