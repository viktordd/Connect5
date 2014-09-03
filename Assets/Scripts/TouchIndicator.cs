using System.Collections.Generic;
using UnityEngine;

public class TouchIndicator : MonoBehaviour
{
	public GameObject touchIndicator;
	private List<GameObject> touchIndicators = new List<GameObject>();
	private GameObject midpoint;

	private void Update()
	{
		if (touchIndicator == null)
			return;

		int i;
		for (i = 0; i < Input.touchCount; i++)
		{
			var touch = Input.GetTouch(i);
			Vector3 pos = Camera.main.ScreenToWorldPoint(touch.position);
			pos.z = 0;

			if (touchIndicators.Count <= i)
			{
				var obj = (GameObject)Instantiate(touchIndicator, pos, new Quaternion());
				obj.transform.parent = transform;
				touchIndicators.Add(obj);
			}
			else
				touchIndicators[i].transform.position = pos;
		}
		while (i < touchIndicators.Count)
		{
			Destroy(touchIndicators[i]);
			touchIndicators.RemoveAt(i);
		}

		if (Input.touchCount == 2)
		{
			Vector2 pos = Camera.main.ScreenToWorldPoint(Vector3.Lerp(Input.GetTouch(0).position, Input.GetTouch(1).position, 0.5f));
			if (midpoint == null)
			{
				midpoint = (GameObject)Instantiate(touchIndicator, pos, new Quaternion());
				midpoint.transform.parent = transform;
			}
			else
				midpoint.transform.position = pos;
		}
		else
		{
			Destroy(midpoint);
			midpoint = null;
		}
	}
}