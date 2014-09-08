using UnityEngine;

public class BoardSize
{
	public Rect rect;
	public bool max;

	public BoardSize() : this(new Rect(), false) { }

	public BoardSize(Rect rect, bool max)
	{
		this.rect = rect;
		this.max = max;
	}
	public BoardSize(Vector2 ll, Vector2 ur, bool max)
	{
		this.rect = new Rect(ll.x, ll.y, ur.x - ll.x, ur.y - ll.y);
		this.max = max;
	}
}