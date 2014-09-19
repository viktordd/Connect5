
using UnityEngine;

public class Board
{
	public static readonly Vector2Int[][] direction =
	{
		new[] { -Vector2Int.right , Vector2Int.right },									// left,		right
		new[] { -Vector2Int.right + Vector2Int.up, Vector2Int.right - Vector2Int.up },	// left up,		right down
		new[] { Vector2Int.up, -Vector2Int.up },										// up,			down
		new[] { Vector2Int.one, -Vector2Int.one }										// right up,	left down
	};

	private Player[,] squares;
	private Vector2Int offset;

	public Board() : this(0, 0) { }
	public Board(int sizeX, int sizeY)
	{
		squares = new Player[sizeX, sizeY];
		offset = Vector2Int.zero;
	}

	public Player this[Vector2Int index]
	{
		get { return squares[index.x + offset.x, index.y + offset.y]; }
		set { squares[index.x + offset.x, index.y + offset.y] = value; }
	}

	public Vector2Int getOffset()
	{
		return offset;
	}

	public Vector2Int getSize()
	{
		return new Vector2Int(squares.GetLength(0), squares.GetLength(1));
	}

	public WinLine CheckIfWon(Vector2Int point)
	{
		point += offset;

		int sizeX = squares.GetLength(0);
		int sizeY = squares.GetLength(1);

		var currPlayer = squares[point.x, point.y];

		foreach (Vector2Int[] dir in direction)
		{
			var numInRow = new [] { 0, 0 };
			for (int i = 0; i < dir.Length; i++)
			{
				Vector2Int curr = point + dir[i];

				while (0 <= curr.x && curr.x < sizeX && 0 <= curr.y && curr.y < sizeY && squares[curr.x, curr.y] == currPlayer)
				{
					numInRow[i]++;
					curr += dir[i];
				}
			}

			var total = 1 + numInRow[0] + numInRow[1];

			while (total > 5)
			{
				if (numInRow[0] > numInRow[1])
					numInRow[0]--;
				else
					numInRow[1]--;
				total--;
			}
			if (total == 5)
				return new WinLine(point + (dir[0] * numInRow[0]) - offset, point + (dir[1] * numInRow[1]) - offset, dir);
		}
		return null;
	}

	public void IncreaseSize(RectInt squaresToAdd)
	{
		int sizeX = squares.GetLength(0);
		int sizeY = squares.GetLength(1);

		var newS = new Player[sizeX + squaresToAdd.xMax, sizeY + squaresToAdd.yMax];

		for (int y = 0; y < sizeY; y++)
			for (int x = 0; x < sizeX; x++)
				newS[x + squaresToAdd.x, y + squaresToAdd.y] = squares[x, y];

		squares = newS;
		offset = new Vector2Int(offset.x + squaresToAdd.x, offset.y + squaresToAdd.y);
	}
}
