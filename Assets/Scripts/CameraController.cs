using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float minZoom = 2.5f;
	public float maxZoom = 10f;

	private TouchIndicator touchIndicator;
	private int indicator;

	void Start()
	{
		touchIndicator = FindObjectOfType<TouchIndicator>();
	}

	private void Update()
	{
		bool changeCameraPos = false;
		Vector2 cameraDelta = Vector2.zero;
		switch (Input.touchCount)
		{
			case 1:
				changeCameraPos = true;
				cameraDelta = camera.ScreenToWorldPoint(Input.GetTouch(0).deltaPosition) -
							  camera.ScreenToWorldPoint(new Vector2(0, 0));
				break;
			case 2:
				Touch t0 = Input.GetTouch(0);
				Touch t1 = Input.GetTouch(1);

				Vector2 t0Pos = camera.ScreenToWorldPoint(t0.position);
				Vector2 t1Pos = camera.ScreenToWorldPoint(t1.position);

				Vector2 t0Prev = camera.ScreenToWorldPoint(t0.position - t0.deltaPosition);
				Vector2 t1Prev = camera.ScreenToWorldPoint(t1.position - t1.deltaPosition);

				float touchDeltaMag = (t0Pos - t1Pos).magnitude;
				float prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;

				camera.orthographicSize = Mathf.Clamp(camera.orthographicSize - touchDeltaMag + prevTouchDeltaMag, minZoom, maxZoom);

				Vector2 midpointAft = camera.ScreenToWorldPoint(Vector2.Lerp(t0.position, t1.position, 0.5f));
				Vector2 midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

				changeCameraPos = true;
				cameraDelta = midpointAft - midpointPrev;

				//DEBUG
				if (touchIndicator != null)
				{
					indicator = touchIndicator.SetIndicator(indicator, Vector2.Lerp(t0Pos, t1Pos, 0.5f));
				}
			
				break;
			default:
				//DEBUG
				if (touchIndicator != null)
				{
					indicator = touchIndicator.RemoveIndicator(indicator);
				}
				break;
		}
		if (changeCameraPos)
		{
			Vector3 pos = camera.transform.position;
			camera.transform.position = new Vector3(pos.x - cameraDelta.x, pos.y - cameraDelta.y, pos.z);
		}
	}
}