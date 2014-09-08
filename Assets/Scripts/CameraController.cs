using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float minZoom = 2.5f;
	public float maxZoom = 25f;
	private float minZ;
	private float maxZ;

	public LayerMask squareMask;

	public static event Func<RectInt, BoardSize> IncreaseSize;

	private Vector2 prevTPos, prevT0Pos, prevT1Pos;

	private bool init;
	private byte initSizeCheck;

	private BoardSize boardSize = new BoardSize();

	void Start()
	{
		Init();
		GUIController_ScreenChange(1f);
	}

	void OnEnable()
	{
		GUIController.ScreenChange += GUIController_ScreenChange;
	}

	void OnDisable()
	{
		GUIController.ScreenChange -= GUIController_ScreenChange;
	}

	private void Init()
	{
		init = true;
		initSizeCheck = 0;
	}

	private void IsInit()
	{
		init = initSizeCheck++ < 4;
	}

	private void GUIController_ScreenChange(float screenRatio)
	{
		if (screenRatio >= 1f)
			minZ = minZoom;
		else
			minZ = minZoom / screenRatio;

		if (!init)
		{
			camera.orthographicSize = camera.orthographicSize / screenRatio;

			if (!boardSize.max)
				OnIncreaseSize(Vector2.zero, true, true);

			SetMaxZoom(screenRatio);

			Vector2 cameraDelta;
			if (OutOfBounds(out cameraDelta))
				SetSize(-cameraDelta.magnitude);

		}
	}

	private void SetMaxZoom(float screenRatio)
	{
		maxZ = Mathf.Min(new[] { maxZoom, boardSize.rect.width / 2 / screenRatio, boardSize.rect.height / 2 });
		SetSize();
	}

	private void SetSize(float sizeDelta = 0)
	{
		if (!init)
			camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + sizeDelta, minZ, maxZ);
	}

	void Update()
	{
		bool posChanged = false, sizeChanged = false;
		Vector2 cameraDelta = Vector2.zero;

		if (init)
		{
			OnIncreaseSize(cameraDelta, true);
			IsInit();
			return;
		}

		switch (Input.touchCount)
		{
			case 1:
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Moved)
				{
					Vector3 delta = camera.ScreenToWorldPoint(t.position - prevTPos);
					Vector3 bl = camera.ScreenToWorldPoint(Vector2.zero);
					cameraDelta = bl - delta;
					posChanged = true;
				}
				prevTPos = t.position;
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

					var sizeDelta = prevTouchDeltaMag - touchDeltaMag;
					SetSize(sizeDelta);
					sizeChanged = true;

					Vector2 midpoint = camera.ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
					Vector2 midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

					cameraDelta = midpointPrev - midpoint;
					posChanged = true;
				}
				if (t0.phase == TouchPhase.Canceled || t0.phase == TouchPhase.Ended)
					prevTPos = t1.position;
				if (t1.phase == TouchPhase.Canceled || t1.phase == TouchPhase.Ended)
					prevTPos = t0.position;

				prevT0Pos = t0.position;
				prevT1Pos = t1.position;
				break;

		}

		if (posChanged)
		{
			ChangePosition(cameraDelta);
			if (!boardSize.max)
				OnIncreaseSize(cameraDelta, sizeChanged);

			if (!init && OutOfBounds(out cameraDelta))
				ChangePosition(cameraDelta);
			
		}
	}

	private void OnIncreaseSize(Vector2 cameraDelta, bool checkAllSides, bool skipMaxZoom = false )
	{
		const float dist = 0.55f;
		Vector3 ll = camera.ScreenToWorldPoint(Vector2.zero);
		Vector3 ur = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
		var changed = false;
		RectInt rect = RectInt.zero;

		Collider2D check;
		var pos = camera.transform.position;
		if (cameraDelta.y > 0 || checkAllSides) // up
		{
			check = Physics2D.OverlapPoint(new Vector2(pos.x, ur.y + dist), squareMask);
			if (check == null)
			{
				changed = true;
				rect.height = 1;
			}
		}

		if (cameraDelta.x > 0 || checkAllSides) // right
		{
			check = Physics2D.OverlapPoint(new Vector2(ur.x + dist, pos.y), squareMask);
			if (check == null)
			{
				changed = true;
				rect.width = 1;
			}
		}

		if (cameraDelta.y < 0 || checkAllSides) // down
		{
			check = Physics2D.OverlapPoint(new Vector2(pos.x, ll.y - dist), squareMask);
			if (check == null)
			{
				changed = true;
				rect.y = 1;
			}
		}

		if (cameraDelta.x < 0 || checkAllSides) // left
		{
			check = Physics2D.OverlapPoint(new Vector2(ll.x - dist, pos.y), squareMask);
			if (check == null)
			{
				changed = true;
				rect.x = 1;
			}
		}
		if (!changed || IncreaseSize == null)
			return;

		boardSize = IncreaseSize(rect);
		if (!skipMaxZoom)
			SetMaxZoom(GUIController.ScreenRatio);
	}

	private bool OutOfBounds(out Vector2 cameraDelta)
	{
		var outOfBounds = false;
		cameraDelta = Vector2.zero;

		var ll = camera.ScreenToWorldPoint(Vector2.zero);
		var ur = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

		if (ll.x < boardSize.rect.xMin)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(boardSize.rect.xMin - ll.x, cameraDelta.y);
		}

		if (ll.y < boardSize.rect.yMin)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(cameraDelta.x, boardSize.rect.yMin - ll.y);
		}

		if (ur.x > boardSize.rect.xMax)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(boardSize.rect.xMax - ur.x, cameraDelta.y);
		}

		if (ur.y > boardSize.rect.yMax)
		{
			outOfBounds = true;
			cameraDelta = new Vector2(cameraDelta.x, boardSize.rect.yMax - ur.y);
		}
		return outOfBounds;
	}

	private void ChangePosition(Vector2 cameraDelta)
	{
		var pos = camera.transform.position;
		camera.transform.position = new Vector3(pos.x + cameraDelta.x, pos.y + cameraDelta.y, pos.z);
	}
}