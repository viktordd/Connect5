using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local

[UsedImplicitly]
public class TouchIndicator : MonoBehaviour
{
	public GameObject Touch;
	private readonly List<GameObject> _touchIndicators = new List<GameObject>();
	private GameObject _midpoint;

	private void Update()
	{
		if (Touch == null)
			return;

		int i;
		for (i = 0; i < Input.touchCount; i++)
		{
			var touch = Input.GetTouch(i);
			var pos = Camera.main.ScreenToWorldPoint(touch.position);
			pos.z = 0;

			if (_touchIndicators.Count <= i)
			{
				var obj = (GameObject)Instantiate(Touch, pos, new Quaternion());
				obj.transform.parent = transform;
				_touchIndicators.Add(obj);
			}
			else
				_touchIndicators[i].transform.position = pos;
		}
		while (i < _touchIndicators.Count)
		{
			Destroy(_touchIndicators[i]);
			_touchIndicators.RemoveAt(i);
		}

		if (Input.touchCount == 2)
		{
			Vector2 pos = Camera.main.ScreenToWorldPoint(Vector3.Lerp(Input.GetTouch(0).position, Input.GetTouch(1).position, 0.5f));
			if (_midpoint == null)
			{
				_midpoint = (GameObject)Instantiate(Touch, pos, new Quaternion());
				_midpoint.transform.parent = transform;
			}
			else
				_midpoint.transform.position = pos;
		}
		else
		{
			Destroy(_midpoint);
			_midpoint = null;
		}
	}
}