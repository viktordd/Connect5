using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float minZoom = 2.5f;
	public float maxZoom = 10f;

	private Vector2 prevT0Pos, prevT1Pos;

	private void Update()
	{
		bool changeCameraPos = false;
		Vector2 cameraDelta = Vector2.zero;

		switch (Input.touchCount)
		{
			case 1:
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Moved)
				{
					changeCameraPos = true;
					Vector3 delta = camera.ScreenToWorldPoint(t.position - prevT0Pos);
					Vector3 bl = camera.ScreenToWorldPoint(Vector2.zero);
					cameraDelta = delta - bl;
				}
				prevT0Pos = t.position;
				break;

			case 2:
				Touch t0 = Input.GetTouch(0);
				Touch t1 = Input.GetTouch(1);

				if (t0.phase != TouchPhase.Began && t1.phase != TouchPhase.Began &&
				    (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
				{
					float touchDeltaMag = ((Vector2) (camera.ScreenToWorldPoint(t0.position) - camera.ScreenToWorldPoint(t1.position))).magnitude;

					Vector2 t0Prev = camera.ScreenToWorldPoint(prevT0Pos);
					Vector2 t1Prev = camera.ScreenToWorldPoint(prevT1Pos);
					float prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;

					camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - touchDeltaMag + prevTouchDeltaMag, minZoom, maxZoom);

					Vector2 midpoint = camera.ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
					Vector2 midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

					changeCameraPos = true;
					cameraDelta = midpoint - midpointPrev;
				}
				prevT0Pos = t0.position;
				prevT1Pos = t1.position;
				break;
		}

		if (changeCameraPos)
		{
			Vector3 pos = camera.transform.position;
			camera.transform.position = new Vector3(pos.x - cameraDelta.x, pos.y - cameraDelta.y, pos.z);
		}
	}
}