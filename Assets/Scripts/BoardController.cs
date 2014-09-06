using System.Collections.Generic;
using UnityEngine;

public class BoardController : TouchController
{
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

	public int initSizeX = 20;
	public int initSizeY = 20;

	public int maxSizeX = 50;
	public int maxSizeY = 50;

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
		anySquare = LayerMask.GetMask(new[] { @notChecked, @checked });
		@notCheckedLayerIndx = LayerMask.NameToLayer(@notChecked);
		@checkedLayerIndx = LayerMask.NameToLayer(@checked);

		undoActions = new List<Vector2Int>();
		redoActions = new List<Vector2Int>();

		board = new Board(initSizeX, initSizeY);

		for (int y = 0; y < initSizeY; y++)
			for (int x = 0; x < initSizeX; x++)
			{
				var obj = (GameObject) Instantiate(squarePrefab, new Vector3(x, y, 0), new Quaternion());
				obj.transform.parent = transform;
			}

		Camera.main.transform.position = new Vector3(initSizeX / 2f, initSizeY / 2f, Camera.main.transform.position.z);

		SetPlayerText(Player.X);

		GUIController.Redo += GUIController_Redo;
		GUIController.Undo += GUIController_Undo;
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
		GUIController.Redo += GUIController_Redo;
		GUIController.Undo += GUIController_Undo;
	}

	void onDisable()
	{
		GUIController.Redo -= GUIController_Redo;
		GUIController.Undo -= GUIController_Undo;
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
