
public class WinnLine
{
	public Vector2Int startPt;
	public Vector2Int endPt;
	public Vector2Int[] direction;

	public WinnLine(Vector2Int startPt, Vector2Int endPt, Vector2Int[] direction)
	{
		this.startPt = startPt;
		this.endPt = endPt;
		this.direction = direction;
	}
}