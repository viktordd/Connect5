// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global

public class Board
{
	public static readonly Vector2Int[][] Direction =
	{
		new[] { -Vector2Int.Right , Vector2Int.Right },									// left,		right
		new[] { -Vector2Int.Right + Vector2Int.Up, Vector2Int.Right - Vector2Int.Up },	// left up,		right down
		new[] { Vector2Int.Up, -Vector2Int.Up },										// up,			down
		new[] { Vector2Int.One, -Vector2Int.One }										// right up,	left down
	};

	private Player[,] _squares;
	private Vector2Int _offset;

	public Board() : this(0, 0) { }
	public Board(int sizeX, int sizeY)
	{
		_squares = new Player[sizeX, sizeY];
		_offset = Vector2Int.Zero;
	}

	public Player this[Vector2Int index]
	{
		get { return _squares[index.X + _offset.X, index.Y + _offset.Y]; }
		set { _squares[index.X + _offset.X, index.Y + _offset.Y] = value; }
	}

	public Vector2Int GetOffset()
	{
		return _offset;
	}

	public Vector2Int GetSize()
	{
		return new Vector2Int(_squares.GetLength(0), _squares.GetLength(1));
	}

	public WinLine CheckIfWon(Vector2Int point)
	{
		point += _offset;

		var sizeX = _squares.GetLength(0);
		var sizeY = _squares.GetLength(1);

		var currPlayer = _squares[point.X, point.Y];

		foreach (var dir in Direction)
		{
			var numInRow = new [] { 0, 0 };
			for (var i = 0; i < dir.Length; i++)
			{
				var curr = point + dir[i];

				while (0 <= curr.X && curr.X < sizeX && 0 <= curr.Y && curr.Y < sizeY && _squares[curr.X, curr.Y] == currPlayer)
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
				return new WinLine(point + (dir[0] * numInRow[0]) - _offset, point + (dir[1] * numInRow[1]) - _offset, dir);
		}
		return null;
	}

	public void IncreaseSize(RectInt squaresToAdd)
	{
		var sizeX = _squares.GetLength(0);
		var sizeY = _squares.GetLength(1);

		var newS = new Player[sizeX + squaresToAdd.XMax, sizeY + squaresToAdd.YMax];

		for (var y = 0; y < sizeY; y++)
			for (var x = 0; x < sizeX; x++)
				newS[x + squaresToAdd.X, y + squaresToAdd.Y] = _squares[x, y];

		_squares = newS;
		_offset = new Vector2Int(_offset.X + squaresToAdd.X, _offset.Y + squaresToAdd.Y);
	}
}
