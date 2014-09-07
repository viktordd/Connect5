using System.Collections.Generic;
using UnityEngine;

public class BoardController : TouchController
{
	public float bezel = 0.2f;
	public string @notChecked;
	public string @checked;
	public GameObject squarePrefab;

	public GameObject playerText;

	public Sprite empty;
	public Sprite checkedX;
	public Sprite checkedO;
	public Sprite onTouch;
	public Sprite lineH;
	public Sprite lineD;

	public int sizeIncrement = 5;

	public int maxSizeX = 100;
	public int maxSizeY = 100;

	private Vector2 squareSize;

	private LayerMask anySquare;
	private int notCheckedLayerIndx;
	private int checkedLayerIndx;

	private List<Vector2Int> undoActions;
	private List<Vector2Int> redoActions;
	private Board board;
	private Player currPlayer;

	private GameObject currObj;
	private Vector2Int currPos;

	private GameObject winnerLine;

	void Start()
	{
		squareSize = new Vector2(squarePrefab.transform.localScale.x, squarePrefab.transform.localScale.y); 

		anySquare = LayerMask.GetMask(new[] { @notChecked, @checked });
		@notCheckedLayerIndx = LayerMask.NameToLayer(@notChecked);
		@checkedLayerIndx = LayerMask.NameToLayer(@checked);

		undoActions = new List<Vector2Int>();
		redoActions = new List<Vector2Int>();
		board = new Board();
		SetPlayerText(Player.X);

		onEnable();
	}

	protected override void Update()
	{
		if (winnerLine != null)
		{
			return;
		}
		base.Update();
	}

	protected override void TouchStart(GameObject gObj)
	{
		currObj = gObj;
		Vector3 position = currObj.transform.position;
		currPos = position;
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.sprite = onTouch;
	}

	protected override void TouchCanceled()
	{
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.sprite = empty;
	}

	protected override void TouchEnded()
	{
		SetMark(currPos, currPlayer, currObj);
		AddUndo(currPos);
	}

	void onEnable()
	{
		CameraController.IncreaseSize += CameraController_IncreaseSize;
		GUIController.Redo += GUIController_Redo;
		GUIController.Undo += GUIController_Undo;
	}

	void onDisable()
	{
		CameraController.IncreaseSize -= CameraController_IncreaseSize;
		GUIController.Redo -= GUIController_Redo;
		GUIController.Undo -= GUIController_Undo;
	}

	private Vector2[] CameraController_IncreaseSize(RectInt sides)
	{
		var squaresToAdd = sides * sizeIncrement;
		Vector2Int currSize = board.getSize();

		var ll = board.getOffset() * -1;
		var ur = currSize + ll;

		bool isMaxX = currSize.x + squaresToAdd.xMax > maxSizeX;
		bool isMaxY = currSize.y + squaresToAdd.yMax > maxSizeY;

		if (isMaxX && isMaxY)
			return GetBoardSize(ll, ur, Vector2.one);

		if (isMaxX)
		{
			squaresToAdd.x = 0;
			squaresToAdd.width = 0;
		}
		if (isMaxY)
		{
			squaresToAdd.y = 0;
			squaresToAdd.height = 0;
		}

		board.IncreaseSize(squaresToAdd);

		AddSquares(ll, ur, squaresToAdd);

		return GetBoardSize(ll, ur, Vector2.zero);
	}

	private void AddSquares(Vector2Int ll, Vector2Int ur, RectInt squaresToAdd)
	{
		var newLl = new Vector2Int(ll.x - squaresToAdd.x, ll.y - squaresToAdd.y);
		var newUr = new Vector2Int(ur.x + squaresToAdd.width, ur.y + squaresToAdd.height);

		if (ur.y < newUr.y || newLl.y < ll.y)
			for (int x = newLl.x; x < newUr.x; x++) //horizontal
			{
				for (int y = ur.y; y < newUr.y; y++) //up + diagonals
					AddSquare(x, y);

				for (int y = newLl.y; y < ll.y; y++) //down + diagonals
					AddSquare(x, y);
			}

		if (ur.x < newUr.x || newLl.x < ll.x)
			for (int y = ll.y; y < ur.y; y++) //horizontal
			{
				for (int x = ur.x; x < newUr.x; x++) //right
					AddSquare(x, y);

				for (int x = newLl.x; x < ll.x; x++) //left
					AddSquare(x, y);
			}
	}

	private void AddSquare(int x, int y)
	{
		var obj = (GameObject)Instantiate(squarePrefab, new Vector3(x, y, 0), new Quaternion());
		obj.transform.parent = transform;
	}

	private Vector2[] GetBoardSize(Vector2Int ll, Vector2Int ur, Vector2 locked)
	{
		var halfWidth = squareSize / 2f;
		var bezelVector = Vector2.one * bezel;
		var LL = (Vector2) ll - halfWidth - bezelVector;
		var UR = (Vector2) ur - halfWidth + bezelVector;

		return new[] { LL, UR, locked };
	}

	private void SetMark(Vector2Int pos, Player player, GameObject square)
	{
		board[pos] = player;

		bool hasMark = true;
		var sRenderer = (SpriteRenderer)square.renderer;
		switch (player)
		{
			case Player.X:
				sRenderer.sprite = checkedX;
				break;
			case Player.O:
				sRenderer.sprite = checkedO;
				break;
			default:
				hasMark = false;
				sRenderer.sprite = empty;
				break;
		}
		if (hasMark)
		{
			square.layer = @checkedLayerIndx;
			WinLine winLine = board.CheckIfWon(pos);
			if (winLine == null)
			{
				SetPlayerText(player.Next());
			}
			else
			{
				DrawLine(winLine);
			}
		}
		else
		{
			square.layer = @notCheckedLayerIndx;
			if (winnerLine == null)
			{
				SetPlayerText(currPlayer.Next());
			}
			else
			{
				Destroy(winnerLine);
			}
		}
	}

	private void AddUndo(Vector2Int currPos)
	{
		redoActions.Clear();
		undoActions.Add(currPos);
		GUIController.undoEnabled = true;
		GUIController.redoEnabled = false;
	}

	void GUIController_Undo()
	{
		int last = undoActions.Count - 1;
		Vector2Int pos = undoActions[last];

		SetMark(pos, Player.None, GetSquare(pos, anySquare));

		undoActions.RemoveAt(last);
		redoActions.Add(pos);
		GUIController.undoEnabled = undoActions.Count > 0;
		GUIController.redoEnabled = true;
	}

	void GUIController_Redo()
	{
		int last = redoActions.Count - 1;
		Vector2Int pos = redoActions[last];

		SetMark(pos, currPlayer, GetSquare(pos, anySquare));

		redoActions.RemoveAt(last);
		undoActions.Add(pos);
		GUIController.undoEnabled = true;
		GUIController.redoEnabled = redoActions.Count > 0;
	}

	private void SetPlayerText(Player player)
	{
		currPlayer = player;
		GUIController.currPlayer = player;
	}

	private void DrawLine(WinLine line)
	{
		winnerLine = new GameObject();
		var sRenderer = winnerLine.AddComponent<SpriteRenderer>();

		winnerLine.transform.parent = transform;
		winnerLine.transform.position = Vector3.Lerp(line.startPt, line.endPt, 0.5f) + Vector3.back;

		if (line.direction[0] == Board.direction[0][0])
			sRenderer.sprite = lineH;
		else if (line.direction[0] == Board.direction[1][0])
			sRenderer.sprite = lineD;
		else if (line.direction[0] == Board.direction[2][0])
		{
			sRenderer.sprite = lineH;
			winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
		else if (line.direction[0] == Board.direction[3][0])
		{
			sRenderer.sprite = lineD;
			winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
	}
}
