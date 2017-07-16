using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local

[UsedImplicitly]
public class BoardController : MonoBehaviour
{
    public float MaxZoomTouch = 10f;
    public LayerMask NotCheckedMask;
    public LayerMask CheckedMask;

	public GameObject SquarePrefab;
	public Sprite Empty;
	public Sprite CheckedX;
	public Sprite CheckedO;
	public Sprite OnTouch;
	public Sprite LineH;
	public Sprite LineD;

	public float Bezel = 0.5f;

	private LayerMask anySquare;
	private int notCheckedLayer;
	private int checkedLayer;

	private List<Vector2Int> undoActions;
	private List<Vector2Int> redoActions;
	private Board board;
	private Player currPlayer;

	private GameObject currObj;
	private Vector2Int currPos;

	private GameObject winnerLine;

    private TouchController touchController;
    private CameraController cameraController;
    private Button undo;
    private Button redo;
    private Image playerPanel;
    private Text playerName;
    private GameObject gameEnd;

    private bool disabled;

    public void Start()
	{
		anySquare = NotCheckedMask | CheckedMask;
		notCheckedLayer = GetLayerFromMask(NotCheckedMask);
		checkedLayer = GetLayerFromMask(CheckedMask);

		undoActions = new List<Vector2Int>();
		redoActions = new List<Vector2Int>();
	    board = new Board(100, 100, new Vector2Int(50, 50));

        undo = GameObject.Find("Canvas/ButtonsPanel/Undo").GetComponent<Button>();
	    redo = GameObject.Find("Canvas/ButtonsPanel/Redo").GetComponent<Button>();
	    playerPanel = GameObject.Find("Canvas/PlayerPanel").GetComponent<Image>();
	    playerName = GameObject.Find("Canvas/PlayerPanel/Name").GetComponent<Text>();
	    gameEnd = GameObject.Find("Canvas/GameEnd");
        gameEnd.SetActive(false);
        undo.interactable = false;
	    redo.interactable = false;
        
	    SetPlayerText(Player.X);

	    touchController = GameObject.Find("TouchController").GetComponent<TouchController>();
	    touchController.TouchStart += OnTouchStart;
	    touchController.TouchEnd += OnTouchEnd;
	    touchController.TouchCancel += TouchCanceled;

	    cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
	    cameraController.ScreenPositionChanged += OnScreenPositionChanged;
	    cameraController.ScreenRatioChanged += ClearAndReInit;

	    InitBoard();
    }

    public void OnDestroy()
	{
	    touchController.TouchStart -= OnTouchStart;
	    touchController.TouchEnd -= OnTouchEnd;
	    touchController.TouchCancel -= TouchCanceled;

	    cameraController.ScreenPositionChanged -= OnScreenPositionChanged;
	    cameraController.ScreenRatioChanged -= ClearAndReInit;
    }

    private void InitBoard()
    {
        var sizeChange = new ViewBoundsChange();
        sizeChange.Init();

        AddSquares(sizeChange.Left, sizeChange.Right, sizeChange.Bottom, sizeChange.Top);
    }

    public void Update()
    {
        var disableTouch = Camera.main.orthographicSize > MaxZoomTouch;
        if (disableTouch == disabled)
            return;

        disabled = disableTouch;
        SetPlayerText(currPlayer);
    }

    private void OnTouchStart(Vector2 touchPos)
    {
        if (disabled || winnerLine != null)
            return;

        var position = Camera.main.ScreenToWorldPoint(touchPos);
        var obj = GetSquare(position, NotCheckedMask);
        if (obj == null)
            return;

        currObj = obj;
        var position1 = currObj.transform.position;
        currPos = position1;
        var sRenderer = (SpriteRenderer)currObj.GetComponent<Renderer>();
        sRenderer.sprite = OnTouch;
    }
    private void OnTouchEnd(Vector2 touchPos)
    {
        if (currObj == null)
            return;
        var position = Camera.main.ScreenToWorldPoint(touchPos);
        var obj = GetSquare(position, NotCheckedMask);
        if (obj == null)
            return;

        if (currObj != obj)
        {
            TouchCanceled();
            return;
        }
        AddUndo(currPos);
        SetMark(currPos, currPlayer, currObj);
        currObj = null;
    }

    private void TouchCanceled()
    {
        if (currObj == null)
            return;
        var sRenderer = (SpriteRenderer)currObj.GetComponent<Renderer>();
		sRenderer.sprite = Empty;
	    currObj = null;
    }

    private GameObject GetSquare(Vector2 position, LayerMask mask)
    {
        var overlapPointCollider = Physics2D.OverlapPoint(position, mask);
        return overlapPointCollider == null ? null : overlapPointCollider.gameObject;
    }

    private void OnScreenPositionChanged(ViewBoundsChange viewBoundsChange)
    {
        TouchCanceled();

        if (!viewBoundsChange.AnyChanges)
            return;

        board.IncreaseSize(viewBoundsChange);
        
		AddRemoveSquares(viewBoundsChange);
	}

    private void ClearAndReInit()
    {
        var count = transform.childCount;
        for (var i = 0; i < count; i++)
            RemoveSquare(transform.GetChild(i).gameObject);

        InitBoard();
    }

    private void AddRemoveSquares(ViewBoundsChange viewBoundsChange)
    {
        if (viewBoundsChange.AddTop > 0)
            AddSquares(viewBoundsChange.NewLeft, viewBoundsChange.NewRight, viewBoundsChange.Top + 1, viewBoundsChange.NewTop);
        else if (viewBoundsChange.AddTop < 0)
            RemoveSquares(viewBoundsChange.Left, viewBoundsChange.Right, viewBoundsChange.NewTop + 1, viewBoundsChange.Top);

        if (viewBoundsChange.AddRight > 0)
            AddSquares(viewBoundsChange.Right + 1, viewBoundsChange.NewRight, viewBoundsChange.NewBottom, viewBoundsChange.NewTop);
        else if (viewBoundsChange.AddRight < 0)
            RemoveSquares(viewBoundsChange.NewRight + 1, viewBoundsChange.Right, viewBoundsChange.Bottom, viewBoundsChange.Top);

        if (viewBoundsChange.AddBottom > 0)
            AddSquares(viewBoundsChange.NewLeft, viewBoundsChange.NewRight, viewBoundsChange.NewBottom, viewBoundsChange.Bottom - 1);
        else if (viewBoundsChange.AddBottom < 0)
            RemoveSquares(viewBoundsChange.Left, viewBoundsChange.Right, viewBoundsChange.Bottom, viewBoundsChange.NewBottom - 1);

        if (viewBoundsChange.AddLeft > 0)
            AddSquares(viewBoundsChange.NewLeft, viewBoundsChange.Left - 1, viewBoundsChange.NewBottom, viewBoundsChange.NewTop);
        else if (viewBoundsChange.AddLeft < 0)
            RemoveSquares(viewBoundsChange.Left, viewBoundsChange.NewLeft - 1, viewBoundsChange.Bottom, viewBoundsChange.Top);
    }

    private void AddSquares(int xFrom, int xTo, int yFrom, int yTo)
    {
        for (var y = yFrom; y <= yTo; y++)
        for (var x = xFrom; x <= xTo; x++)
            AddSquare(x, y);
    }

    private void RemoveSquares(int xFrom, int xTo, int yFrom, int yTo)
    {
        for (var y = yFrom; y <= yTo; y++)
        for (var x = xFrom; x <= xTo; x++)
            RemoveSquare(x, y);
    }

    private void AddSquare(int x, int y)
	{
	    var location = board[x, y];
	    if (location.HasSquare)
	        return;
        
        var square = Instantiate(SquarePrefab, new Vector3(x, y, 0), new Quaternion());
		square.transform.parent = transform;
	    SetPlayerSprite(location.Player, square);
        
	    location.HasSquare = true;
        board[x, y] = location;
    }

    private void RemoveSquare(int x, int y)
    {
        var location = board[x, y];
        if (!location.HasSquare)
            return;

        var square = GetSquare(new Vector2(x, y), anySquare);
        if (square == null)
            return;

        Destroy(square);

        location.HasSquare = false;
        board[x, y] = location;
    }

    private void RemoveSquare(GameObject square)
    {
        var x = (int)square.transform.position.x;
        var y = (int)square.transform.position.y;

        Destroy(square);

        var location = board[x, y];
        location.HasSquare = false;
        board[x, y] = location;
    }

	private void SetMark(Vector2Int pos, Player player, GameObject square)
	{
	    board.SetPlayer(pos.X, pos.Y, player);

	    if (SetPlayerSprite(player, square))
		{
			var winLine = board.CheckIfWon(pos);
			if (winLine == null)
			{
				SetPlayerText(player.Next());
			}
			else
			{
				DrawLine(winLine);
                
                gameEnd.SetActive(true);

			    var wins = gameEnd.transform.Find("Wins").gameObject.GetComponent<Text>();
			    wins.text = player.GetText() + " Player Wins!";

			    undo.interactable = false;
			    redo.interactable = false;
            }
		}
		else
		{
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

    private bool SetPlayerSprite(Player player, GameObject square)
    {
        var sRenderer = (SpriteRenderer) square.GetComponent<Renderer>();
        switch (player)
        {
            case Player.X:
                sRenderer.sprite = CheckedX;
                square.layer = checkedLayer;
                return true;

            case Player.O:
                sRenderer.sprite = CheckedO;
                square.layer = checkedLayer;
                return true;

            default:
                sRenderer.sprite = Empty;
                square.layer = notCheckedLayer;
                return false;
        }
    }

    private void AddUndo(Vector2Int pos)
	{
		redoActions.Clear();
		undoActions.Add(pos);
	    undo.interactable = true;
	    redo.interactable = false;
	}

	public void OnUndo()
	{
		var last = undoActions.Count - 1;
		var pos = undoActions[last];

		var square = GetSquare(pos, anySquare);
		if (square == null)
			return;

		SetMark(pos, Player.None, square);

		undoActions.RemoveAt(last);
		redoActions.Add(pos);
	    undo.interactable = undoActions.Count > 0;
	    redo.interactable = true;
    }

	public void OnRedo()
	{
		var last = redoActions.Count - 1;
		var pos = redoActions[last];

		var square = GetSquare(pos, anySquare);
		if (square == null)
			return;

		SetMark(pos, currPlayer, square);

		redoActions.RemoveAt(last);
		undoActions.Add(pos);
	    undo.interactable = true;
	    redo.interactable = redoActions.Count > 0;
    }

	private void SetPlayerText(Player player)
	{
		currPlayer = player;
	    playerName.text = player.GetText();
	    playerName.color = player.GetColor();

	    Color color;
	    ColorUtility.TryParseHtmlString("#A8FF94FF", out color);
	    playerPanel.color = disabled ? Color.gray : color;
	}

	private void DrawLine(WinLine line)
	{
		winnerLine = new GameObject();
		var sRenderer = winnerLine.AddComponent<SpriteRenderer>();
		sRenderer.color = new Color(1f, 1f, 1f, .7f);

		winnerLine.transform.parent = transform;
		winnerLine.transform.position = Vector3.Lerp(line.StartPt, line.EndPt, 0.5f) + Vector3.back;

		if (line.Direction[0] == Board.Direction[0][0])
			sRenderer.sprite = LineH;
		else if (line.Direction[0] == Board.Direction[1][0])
			sRenderer.sprite = LineD;
		else if (line.Direction[0] == Board.Direction[2][0])
		{
			sRenderer.sprite = LineH;
			winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
		else if (line.Direction[0] == Board.Direction[3][0])
		{
			sRenderer.sprite = LineD;
			winnerLine.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
	}

	private int GetLayerFromMask(LayerMask layer)
	{
		if (layer.value == 0)
			return 0;

		for (var i = 0; i < 32; i++)
			if ((layer & (1 << i)) != 0)
				return i;

		return 0;
	}
}
