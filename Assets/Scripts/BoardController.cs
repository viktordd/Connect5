using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global
// ReSharper disable UseStringInterpolation

[UsedImplicitly]
public class BoardController : TouchController
{
	public LayerMask CheckedMask;

	public GameObject SquarePrefab;
	public Sprite Empty;
	public Sprite CheckedX;
	public Sprite CheckedO;
	public Sprite OnTouch;
	public Sprite LineH;
	public Sprite LineD;

	public float Bezel = 0.5f;
	public int SizeIncrement = 5;

	public int MaxSizeX = 100;
	public int MaxSizeY = 100;

	private Vector2 _squareSize;

	private LayerMask _anySquare;
	private int _notCheckedLayer;
	private int _checkedLayer;

	private List<Vector2Int> _undoActions;
	private List<Vector2Int> _redoActions;
	private Board _board;
	private Player _currPlayer;

	private GameObject _currObj;
	private Vector2Int _currPos;

	private GameObject _winnerLine;

	protected override void Start()
	{
		base.Start();
		_squareSize = new Vector2(SquarePrefab.transform.localScale.x, SquarePrefab.transform.localScale.y);

		_anySquare = NotCheckedMask | CheckedMask;
		_notCheckedLayer = GetLayerFromMask(NotCheckedMask);
		_checkedLayer = GetLayerFromMask(CheckedMask);

		_undoActions = new List<Vector2Int>();
		_redoActions = new List<Vector2Int>();
		_board = new Board();
		SetPlayerText(Player.X);
	}

	protected override void Update()
	{
		if (_winnerLine != null)
			return;

		base.Update();
	}

	protected override void TouchStart(GameObject gObj)
	{
		_currObj = gObj;
		var position = _currObj.transform.position;
		_currPos = position;
		var sRenderer = (SpriteRenderer)_currObj.GetComponent<Renderer>();
		sRenderer.sprite = OnTouch;
	}

	protected override void TouchCanceled()
	{
		var sRenderer = (SpriteRenderer)_currObj.GetComponent<Renderer>();
		sRenderer.sprite = Empty;
	}

	protected override void TouchEnded(GameObject gObj = null)
	{
		if (_currObj != gObj)
		{
			TouchCanceled();
			return;
		}
		SetMark(_currPos, _currPlayer, _currObj);
		AddUndo(_currPos);
	}

	protected override void TouchEnabledChanged()
	{
		SetPlayerText(_currPlayer);
	}

	void OnEnable()
	{
		CameraController.IncreaseSize += CameraController_IncreaseSize;
		GuiController.RedoAction += GuiControllerRedoAction;
		GuiController.UndoAction += GuiControllerUndoAction;
	}

	void OnDisable()
	{
		CameraController.IncreaseSize -= CameraController_IncreaseSize;
		GuiController.RedoAction -= GuiControllerRedoAction;
		GuiController.UndoAction -= GuiControllerUndoAction;
	}

	private BoardSize CameraController_IncreaseSize(RectInt sides)
	{
		var squaresToAdd = sides * SizeIncrement;
		var currSize = _board.GetSize();

		var isMaxX = currSize.X == MaxSizeX || currSize.X + squaresToAdd.XMax > MaxSizeX;
		var isMaxY = currSize.Y == MaxSizeY || currSize.Y + squaresToAdd.YMax > MaxSizeY;

		if (isMaxX && isMaxY)
			return GetBoardSize(true);

		if (isMaxX)
		{
			var width = MaxSizeX - currSize.X;
			if (width <= 0)
			{
				squaresToAdd.X = 0;
				squaresToAdd.Width = 0;
			}
			else if (squaresToAdd.X > 0 && squaresToAdd.Width > 0)
			{
				var half = width / 2f;
				squaresToAdd.X = Mathf.FloorToInt(half);
				squaresToAdd.Width = Mathf.CeilToInt(half);
			}
			else if (squaresToAdd.X > 0)
				squaresToAdd.X = width;

			else if (squaresToAdd.Width > 0)
				squaresToAdd.Width = width;
		}
		if (isMaxY)
		{
			var height = MaxSizeY - currSize.Y;
			if (height <= 0)
			{
				squaresToAdd.Y = 0;
				squaresToAdd.Height = 0;
			}
			else if (squaresToAdd.X > 0 && squaresToAdd.Width > 0)
			{
				var half = height / 2f;
				squaresToAdd.Y = Mathf.FloorToInt(half);
				squaresToAdd.Height = Mathf.CeilToInt(half);
			}
			if (squaresToAdd.Y > 0)
				squaresToAdd.Y = height;

			else if (squaresToAdd.Height > 0)
				squaresToAdd.Height = height;
		}

		if (squaresToAdd == RectInt.Zero)
			return GetBoardSize(false);

		AddSquares(squaresToAdd);

		_board.IncreaseSize(squaresToAdd);

		Debug.Log(string.Format("Expanding board by {0} to {1}", squaresToAdd, _board.GetSize()));

		return GetBoardSize(false);
	}

	private void AddSquares(RectInt squaresToAdd)
	{
		var ll = _board.GetOffset() * -1;
		var ur = _board.GetSize() + ll;
		var newLl = new Vector2Int(ll.X - squaresToAdd.X, ll.Y - squaresToAdd.Y);
		var newUr = new Vector2Int(ur.X + squaresToAdd.Width, ur.Y + squaresToAdd.Height);

		if (ur.Y < newUr.Y || newLl.Y < ll.Y)
			for (var x = newLl.X; x < newUr.X; x++) //horizontal
			{
				for (var y = ur.Y; y < newUr.Y; y++) //up + diagonals
					AddSquare(x, y);

				for (var y = newLl.Y; y < ll.Y; y++) //down + diagonals
					AddSquare(x, y);
			}

		if (ur.X < newUr.X || newLl.X < ll.X)
			for (var y = ll.Y; y < ur.Y; y++) //horizontal
			{
				for (var x = ur.X; x < newUr.X; x++) //right
					AddSquare(x, y);

				for (var x = newLl.X; x < ll.X; x++) //left
					AddSquare(x, y);
			}
	}

	private void AddSquare(int x, int y)
	{
		var obj = (GameObject)Instantiate(SquarePrefab, new Vector3(x, y, 0), new Quaternion());
		obj.transform.parent = transform;
	}

	private BoardSize GetBoardSize(bool max)
	{
		var ll = _board.GetOffset() * -1;
		var ur = _board.GetSize() + ll;
		var halfWidth = _squareSize / 2f;
		var bezelVector = Vector2.one * Bezel;

		var lowerLeft = (Vector2)ll - halfWidth - bezelVector;
		var upperRight = (Vector2)ur - halfWidth + bezelVector;

		return new BoardSize(lowerLeft, upperRight, max);
	}

	private void SetMark(Vector2Int pos, Player player, GameObject square)
	{
		_board[pos] = player;

		var hasMark = true;
		var sRenderer = (SpriteRenderer)square.GetComponent<Renderer>();
		switch (player)
		{
			case Player.X:
				sRenderer.sprite = CheckedX;
				break;
			case Player.O:
				sRenderer.sprite = CheckedO;
				break;
			default:
				hasMark = false;
				sRenderer.sprite = Empty;
				break;
		}
		if (hasMark)
		{
			square.layer = _checkedLayer;
			var winLine = _board.CheckIfWon(pos);
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
			square.layer = _notCheckedLayer;
			if (_winnerLine == null)
			{
				SetPlayerText(_currPlayer.Next());
			}
			else
			{
				Destroy(_winnerLine);
			}
		}
	}

	private void AddUndo(Vector2Int currPos)
	{
		_redoActions.Clear();
		_undoActions.Add(currPos);
		GuiController.UndoEnabled = true;
		GuiController.RedoEnabled = false;
	}

	void GuiControllerUndoAction()
	{
		var last = _undoActions.Count - 1;
		var pos = _undoActions[last];

		var square = GetSquare(pos, _anySquare);
		if (square == null)
			return;

		SetMark(pos, Player.None, square);

		_undoActions.RemoveAt(last);
		_redoActions.Add(pos);
		GuiController.UndoEnabled = _undoActions.Count > 0;
		GuiController.RedoEnabled = true;
	}

	void GuiControllerRedoAction()
	{
		var last = _redoActions.Count - 1;
		var pos = _redoActions[last];

		var square = GetSquare(pos, _anySquare);
		if (square == null)
			return;

		SetMark(pos, _currPlayer, square);

		_redoActions.RemoveAt(last);
		_undoActions.Add(pos);
		GuiController.UndoEnabled = true;
		GuiController.RedoEnabled = _redoActions.Count > 0;
	}

	private void SetPlayerText(Player player)
	{
		_currPlayer = player;
		GuiController.CurrPlayer = player;
		GuiController.PlayerDisabled = Disabled;
	}

	private void DrawLine(WinLine line)
	{
		_winnerLine = new GameObject();
		var sRenderer = _winnerLine.AddComponent<SpriteRenderer>();
		sRenderer.color = new Color(1f, 1f, 1f, .7f);

		_winnerLine.transform.parent = transform;
		_winnerLine.transform.position = Vector3.Lerp(line.StartPt, line.EndPt, 0.5f) + Vector3.back;

		if (line.Direction[0] == Board.Direction[0][0])
			sRenderer.sprite = LineH;
		else if (line.Direction[0] == Board.Direction[1][0])
			sRenderer.sprite = LineD;
		else if (line.Direction[0] == Board.Direction[2][0])
		{
			sRenderer.sprite = LineH;
			_winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
		else if (line.Direction[0] == Board.Direction[3][0])
		{
			sRenderer.sprite = LineD;
			_winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
	}

	private static int GetLayerFromMask(LayerMask layer)
	{
		if (layer.value == 0)
			return 0;

		for (var i = 0; i < 32; i++)
			if ((layer & (1 << i)) != 0)
				return i;

		return 0;
	}
}
