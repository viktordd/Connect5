
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
	private Vector2Int offset = Vector2Int.zero;

	public Board(int sizeX, int sizeY)
	{
		squares = new Player[sizeY, sizeX];
	}

	public Player this[Vector2Int index]
	{
		get { return squares[index.y + offset.y, index.x + offset.x]; }
		set { squares[index.y + offset.y, index.x + offset.x] = value; }
	}

	public WinLine CheckIfWon(Vector2Int point)
	{
		Vector2Int[] line = { point, point };

		int sizeY = squares.GetLength(0);
		int sizeX = squares.GetLength(1);

		Player currPlayer = this[point];

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
				return new WinLine(line[0], line[1], dir);
		}
		return null;
	}

	public void IncreaseSize(Vector2Int squaresToAdd)
	{
		
	}
}


