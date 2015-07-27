// ReSharper disable CheckNamespace
// ReSharper disable FieldCanBeMadeReadOnly.Global

public class WinLine
{
	public Vector2Int StartPt;
	public Vector2Int EndPt;
	public Vector2Int[] Direction;

	public WinLine(Vector2Int startPt, Vector2Int endPt, Vector2Int[] direction)
	{
		StartPt = startPt;
		EndPt = endPt;
		Direction = direction;
	}
}