using UnityEngine;

public class BoardController : TouchController
{
	public string @checked;
	public GameObject squarePrefab;

	public Sprite checkedX;
	public Sprite checkedO;
	public Color onTouch;
	public Color lineColor;

	public int initSizeX = 20;
	public int initSizeY = 20;

	public int maxSizeX = 50;
	public int maxSizeY = 50;

	private int @checkedLayerIndx;

	private int offsetX;
	private int offsetY;

	private byte[,] squares;

	private byte currPlayer = 1;

	private const byte playerX = 1;
	private const byte playerO = 2;

	private GameObject currObj;
	private int currX;
	private int currY;

	private Vector2Int[,] direction =
	{
		{ new Vector2Int { x = -1, y = 0 }, new Vector2Int { x = 1, y = 0 } },
		{ new Vector2Int { x = -1, y = -1 }, new Vector2Int { x = 1, y = 1 } },
		{ new Vector2Int { x = 0, y = -1 }, new Vector2Int { x = 0, y = 1 } },
		{ new Vector2Int { x = 1, y = -1 }, new Vector2Int { x = -1, y = 1 } }
	};

	void Start()
	{
		@checkedLayerIndx = LayerMask.NameToLayer(@checked);

		offsetX = initSizeX / 2;
		offsetY = initSizeY / 2;

		squares = new byte[initSizeY, initSizeX];
		for (int y = 0; y < initSizeY; y++)
			for (int x = 0; x < initSizeX; x++)
			{
				var obj = (GameObject) Instantiate(squarePrefab, new Vector3(x - offsetX, y - offsetY, 0), new Quaternion());
				obj.transform.parent = transform;
			}
	}

	protected override void TouchStart(GameObject gObj)
	{
		currObj = gObj;
		Vector3 position = currObj.transform.position;
		currX = (int)position.x + offsetX;
		currY = (int)position.y + offsetY;
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.color = onTouch;
	}

	protected override void TouchCanceled()
	{
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.color = Color.white;
	}

	protected override void TouchEnded()
	{
		bool isPlayerX = currPlayer == playerX;

		currObj.layer = @checkedLayerIndx;
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.color = Color.white;
		sRenderer.sprite = (isPlayerX ? checkedX : checkedO);

		squares[currY, currX] = currPlayer;

		CheckIfWon();

		currPlayer = (isPlayerX ? playerO : playerX);
	}

	private void CheckIfWon()
	{
		Vector2Int[] vertex = { new Vector2Int { x = currX, y = currY }, new Vector2Int { x = currX, y = currY } };


		int sizeY = squares.GetLength(0);
		int sizeX = squares.GetLength(1);

		int dir;
		var d1Len = direction.GetLength(0);
		var d2Len = direction.GetLength(1);

		for (dir = 0; dir < d1Len; dir++)
		{
			int numInRow = 1;

			for (int i = 0; i < d2Len; i++)
			{
				int x = currX + direction[dir, i].x;
				int y = currY + direction[dir, i].y;

				while (y < sizeY && x < sizeX && squares[y, x] == currPlayer)
				{
					vertex[i].x = x;
					vertex[i].y = y;
					numInRow++;
					x += direction[dir, i].x;
					y += direction[dir, i].y;
				}
			}

			if (numInRow >= 5)
			{
				DrawLine(vertex, dir);
				enabled = false;
				return;
			}
		}
	}

	private void DrawLine(Vector2Int[] vertex, int dir)
	{
		var line = GetComponent<LineRenderer>();
		if (line == null)
		{
			line = gameObject.AddComponent<LineRenderer>();
			line.material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
			line.SetVertexCount(2);
			line.SetWidth(0.15f, 0.15f);
			line.SetColors(lineColor, lineColor);
			line.useWorldSpace = true;
		}

		line.SetPosition(0, new Vector3(vertex[0].x - offsetX + squareCenter * direction[dir, 0].x,
										vertex[0].y - offsetY + squareCenter * direction[dir, 0].y, -5));
		line.SetPosition(1, new Vector3(vertex[1].x - offsetX + squareCenter * direction[dir, 1].x, 
										vertex[1].y - offsetY + squareCenter * direction[dir, 1].y, -5));
	}
}

public struct Vector2Int
{
	public int x;
	public int y;
}
