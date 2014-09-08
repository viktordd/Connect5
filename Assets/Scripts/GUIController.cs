using System;
using UnityEngine;

public class GUIController : MonoBehaviour
{
	public float textWidth = 0.3f;
	public float buttonWidth = 0.15f;
	public float xOffset = 0f;
	public float yOffset = -0.03f;

	public Texture2D player;
	public Texture2D blue;
	public Texture2D red;
	public GUIStyle undo;
	public GUIStyle undoDisabled;
	public GUIStyle redo;
	public GUIStyle redoDisabled;

	private float textRatio;
	private float buttonRatio;

	public static event Action Undo;
	public static event Action Redo;

	public static bool undoEnabled = false;
	public static bool redoEnabled = false;

	private static Rect boxPos;
	private static Rect undoPos;
	private static Rect redoPos;
	private static Rect textPos;

	public static Player currPlayer = Player.X;
	public static bool playerDisabled;

	public static event Action<float> ScreenChange;

	private int currWidth;
	private int currHeight;

	void Start()
	{
		textRatio = (player.height / (float)player.width);
		buttonRatio = undo.normal.background.height / (float)undo.normal.background.width;
	}

	void OnGUI()
	{
		TestForScreenChange();

		if (!playerDisabled)
		{
			GUI.DrawTexture(textPos, player);
			GUI.DrawTexture(textPos, currPlayer == Player.X ? blue : red);
		}

		GUI.Box(boxPos, "");

		if (undoEnabled)
		{
			if (GUI.Button(undoPos, String.Empty, undo) && Undo != null)
				Undo();
		}
		else
		{
			GUI.Button(undoPos, String.Empty, undoDisabled);
		}

		if (redoEnabled)
		{
			if (GUI.Button(redoPos, String.Empty, redo) && Redo != null)
				Redo();
		}
		else
		{
			GUI.Button(redoPos, String.Empty, redoDisabled);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
	}

	private void TestForScreenChange()
	{
		if (currWidth != Screen.width || currHeight != Screen.height)
		{
			currWidth = Screen.width;
			currHeight = Screen.height;

			float screenRatio = ScreenRatio;
			CalcPositions(screenRatio);

			if (ScreenChange != null)
				ScreenChange(screenRatio);
		}
	}

	public static float ScreenRatio
	{
		get { return Screen.width / (float) Screen.height; }
	}

	void CalcPositions(float screenRatio)
	{
		float txtW, txtH, btnW, btnH, xd, yd;
		if (screenRatio >= 1f)
		{
			txtW = textWidth;
			txtH = txtW * textRatio * screenRatio;
			btnW = buttonWidth;
			btnH = btnW * buttonRatio * screenRatio;
			xd = xOffset;
			yd = yOffset;
		}
		else
		{
			txtW = textWidth / screenRatio;
			txtH = txtW * textRatio * screenRatio;
			btnW = buttonWidth / screenRatio;
			btnH = btnW * buttonRatio * screenRatio;
			xd = xOffset / screenRatio;
			yd = yOffset * screenRatio;
		}

		textPos = new Rect(0f, 0f, Screen.width * txtW, Screen.height * txtH);

		float left1 = Screen.width - Screen.width * (2 * btnW + xd);
		float left2 = Screen.width - Screen.width * (btnW + xd);
		float top = Screen.height - Screen.height * (btnH + yd);
		float totalWidth = Screen.width * 2 * btnW;
		float width = Screen.width * btnW;
		float height = Screen.height * btnH;

		boxPos = new Rect(left1, top, totalWidth, height);
		undoPos = new Rect(left1, top, width, height);
		redoPos = new Rect(left2, top, width, height);
	}

	public static bool InGui(Vector2 pos)
	{
		pos = new Vector2(pos.x, Screen.height - pos.y);
		bool inGui = textPos.Contains(pos) || boxPos.Contains(pos);
		return inGui;
	}
}
