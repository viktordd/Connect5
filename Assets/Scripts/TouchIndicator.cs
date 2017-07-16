using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local

[UsedImplicitly]
public class TouchIndicator : MonoBehaviour
{
	public GameObject Touch;
	private readonly List<GameObject> touchIndicators = new List<GameObject>();
	private GameObject midpoint;

    public void Update()
	{
		if (Touch == null)
			return;

		int i;
		for (i = 0; i < Input.touchCount; i++)
		{
			var touch = Input.GetTouch(i);
			var pos = Camera.main.ScreenToWorldPoint(touch.position);
			pos.z = 0;

			if (touchIndicators.Count <= i)
			{
				var obj = (GameObject)Instantiate(Touch, pos, new Quaternion());
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
				midpoint = (GameObject)Instantiate(Touch, pos, new Quaternion());
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