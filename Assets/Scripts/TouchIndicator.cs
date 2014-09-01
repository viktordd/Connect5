using System.Collections.Generic;
using UnityEngine;

public class TouchIndicator : MonoBehaviour
{

	public GameObject touchIndicator;
	private List<GameObject> touchIndicators = new List<GameObject>();
	private List<GameObject> otherIndicators = new List<GameObject>();

	// Use this for initialization
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
		if (touchIndicator != null)
		{
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
		}
	}

	public int SetIndicator(int index, Vector2 pos)
	{
		int i = index - 1;
		if (i >= 0 && i < otherIndicators.Count)
			otherIndicators[i].transform.position = pos;
		else
		{
			var obj = (GameObject) Instantiate(touchIndicator, pos, new Quaternion());
			obj.transform.parent = transform;
			otherIndicators.Add(obj);
			index = otherIndicators.Count;
		}
		return index;
	}

	public int RemoveIndicator(int i)
	{
		i--;
		if (i >= 0 && i < otherIndicators.Count)
		{
			Destroy(otherIndicators[i]);
			otherIndicators.RemoveAt(i);
		}
		return 0;
	}
}