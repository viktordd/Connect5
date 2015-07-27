using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

public class BoardSize
{
	public Rect Rect;
	public bool Max;

	public BoardSize() : this(new Rect(), false) { }

	public BoardSize(Rect rect, bool max)
	{
		Rect = rect;
		Max = max;
	}
	public BoardSize(Vector2 ll, Vector2 ur, bool max)
	{
		Rect = new Rect(ll.x, ll.y, ur.x - ll.x, ur.y - ll.y);
		Max = max;
	}
}