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

	private Location[,] location;
	private Vector2Int offset;
    private readonly ViewBoundsChange viewBoundsChange;

    public Board() : this(0, 0) { }

    public Board(int sizeX, int sizeY) : this(sizeX, sizeY, Vector2Int.Zero) { }

    public Board(int sizeX, int sizeY, Vector2Int of)
    {
        location = new Location[sizeX, sizeY];
        offset = of;
        viewBoundsChange = new ViewBoundsChange();
    }

    private int GetX(int x)
    {
        return x + offset.X;
    }

    private int GetY(int y)
    {
        return y + offset.Y;
    }

    public Location this[int x, int y]
    {
        get{ return location[GetX(x), GetY(y)]; }
        set { location[GetX(x), GetY(y)] = value; }
    }

    public Location this[Vector2Int index]
	{
		get { return location[GetX(index.X), GetY(index.Y)]; }
		set { location[GetX(index.X), GetY(index.Y)] = value; }
    }

    public void SetPlayer(int x, int y, Player player)
    {
        location[GetX(x), GetY(y)].Player = player;
    }

    public void SetHasSquare(int x, int y, bool hasSquare)
    {
        location[GetX(x), GetY(y)].HasSquare = hasSquare;
    }

    public Vector2Int GetOffset()
	{
		return offset;
	}

	public Vector2Int GetSize()
	{
		return new Vector2Int(location.GetLength(0), location.GetLength(1));
	}

	public WinLine CheckIfWon(Vector2Int point)
	{
		point += offset;

		var sizeX = location.GetLength(0);
		var sizeY = location.GetLength(1);

	    var currPlayer = location[point.X, point.Y].Player;

		foreach (var dir in Direction)
		{
			var numInRow = new [] { 0, 0 };
			for (var i = 0; i < dir.Length; i++)
			{
				var curr = point + dir[i];

				while (0 <= curr.X && curr.X < sizeX && 0 <= curr.Y && curr.Y < sizeY && location[curr.X, curr.Y].Player == currPlayer)
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

	public void IncreaseSize(ViewBoundsChange viewBoundsCheck)
	{
	    NeedSizeChange(viewBoundsCheck);
	    if (!viewBoundsChange.AddAny)
	        return;
        
        var size = GetSize();
		var newLoc = new Location[size.X + viewBoundsChange.AddHorizontal, size.Y + viewBoundsChange.AddVertical];

		for (var y = 0; y < size.Y; y++)
			for (var x = 0; x < size.X; x++)
				newLoc[x + viewBoundsChange.AddLeft, y + viewBoundsChange.AddBottom] = location[x, y];

		location = newLoc;
		offset = new Vector2Int(offset.X + viewBoundsChange.AddLeft, offset.Y + viewBoundsChange.AddBottom);
	}

    public void NeedSizeChange(ViewBoundsChange viewBoundsCheck)
    {
        var currSize = GetSize();

        var expandTop = GetY(viewBoundsCheck.NewTop) - currSize.Y + 1;
        var expandRight = GetX(viewBoundsCheck.NewRight) - currSize.X + 1;
        var expandBottom = -GetY(viewBoundsCheck.NewBottom);
        var expandLeft = -GetX(viewBoundsCheck.NewLeft);
        
        viewBoundsChange.AddTop = expandTop > 0 ? 100 : 0;
        viewBoundsChange.AddRight = expandRight > 0 ? 100 : 0;
        viewBoundsChange.AddBottom = expandBottom > 0 ? 100 : 0;
        viewBoundsChange.AddLeft = expandLeft > 0 ? 100 : 0;
    }
}
