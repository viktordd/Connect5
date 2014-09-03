using System.Runtime.InteropServices;
using UnityEngine;

public class BoardController : TouchController
{
	public string @checked;
	public GameObject squarePrefab;

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

	private int @checkedLayerIndx;

	private Board board;
	private byte currPlayer = 1;

	private GameObject currObj;
	private Vector2Int currPos;

	private bool finished;

	void Start()
	{
		@checkedLayerIndx = LayerMask.NameToLayer(@checked);

		board = new Board(initSizeX, initSizeY);

		for (int y = 0; y < initSizeY; y++)
			for (int x = 0; x < initSizeX; x++)
			{
				var obj = (GameObject) Instantiate(squarePrefab, new Vector3(x, y, 0), new Quaternion());
				obj.transform.parent = transform;
			}

		Camera.main.transform.position = new Vector3(initSizeX / 2f, initSizeY / 2f, Camera.main.transform.position.z);
	}

	protected override void Update()
	{
		if (finished)
		{
			//enabled = false;
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
		bool isPlayerX = currPlayer == Board.playerX;

		currObj.layer = @checkedLayerIndx;
		var sRenderer = (SpriteRenderer)currObj.renderer;
		sRenderer.color = Color.white;
		sRenderer.sprite = (isPlayerX ? checkedX : checkedO);

		board[currPos] = currPlayer;

		WinnLine winnLine = board.CheckIfWon(currPos);
		if (winnLine != null)
		{
			DrawLine(winnLine);
			finished = true;
		}

		currPlayer = (isPlayerX ? Board.playerO : Board.playerX);
	}

	private void DrawLine(WinnLine line)
	{
		var obj = new GameObject();
		var sRenderer = obj.AddComponent<SpriteRenderer>();

		obj.transform.parent = transform;
		obj.transform.position = Vector3.Lerp(line.startPt, line.endPt, 0.5f) + Vector3.back;

		if (line.direction[0] == Board.direction[0][0])
			sRenderer.sprite = lineH;
		else if (line.direction[0] == Board.direction[1][0])
			sRenderer.sprite = lineD;
		else if (line.direction[0] == Board.direction[2][0])
		{
			sRenderer.sprite = lineH;
			obj.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
		else if (line.direction[0] == Board.direction[3][0])
		{
			sRenderer.sprite = lineD;
			obj.transform.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
		}
	}
}
