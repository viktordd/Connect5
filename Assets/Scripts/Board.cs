
public class Board
{
	public const byte playerX = 1;
	public const byte playerO = 2;

	public static readonly Vector2Int[][] direction =
	{
		new[] { -Vector2Int.right , Vector2Int.right },									// left,		right
		new[] { -Vector2Int.right + Vector2Int.up, Vector2Int.right - Vector2Int.up },	// left up,		right down
		new[] { Vector2Int.up, -Vector2Int.up },										// up,			down
		new[] { Vector2Int.one, -Vector2Int.one }										// right up,	left down
	};

	private byte[,] squares;

	public Board(int sizeX, int sizeY)
	{
		squares = new byte[sizeX, sizeY];
	}

	public byte this[int x, int y]
	{
		get { return squares[y, x]; }
		set { squares[y, x] = value; }
	}

	public byte this[Vector2Int index]
	{
		get { return squares[index.y, index.x]; }
		set { squares[index.y, index.x] = value; }
	}

	public WinnLine CheckIfWon(Vector2Int point)
	{
		Vector2Int[] line = { point, point };

		int sizeY = squares.GetLength(0);
		int sizeX = squares.GetLength(1);

		byte currPlayer = this[point];

		foreach (Vector2Int[] dir in direction)
		{
			int numInRow = 1;
			for (int i = 0; i < dir.Length; i++)
			{
				Vector2Int curr = point + dir[i];

				while (0 <= curr.y && curr.y < sizeY && 0 <= curr.x && curr.x < sizeX && this[curr] == currPlayer)
				{
					line[i] = curr;
					numInRow++;
					curr += dir[i];
				}
			}

			if (numInRow >= 5)
				return new WinnLine(line[0], line[1], dir);
		}
		return null;
	}
}


