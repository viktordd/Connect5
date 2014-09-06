using System;
using UnityEngine;

public class GUIController : MonoBehaviour
{
	public float textWidth = .2f;
	public float buttonWidth = 0.13f;
	public float xOffset = 0f;
	public float yOffset = -0.025f;

	public Texture2D player;
	public Texture2D blue;
	public Texture2D red;
	public GUIStyle undo;
	public GUIStyle undoDisabled;
	public GUIStyle redo;
	public GUIStyle redoDisabled;

	public static event Action Undo;
	public static event Action Redo;

	public static bool undoEnabled = false;
	public static bool redoEnabled = false;

	private static Rect boxPos;
	private static Rect undoPos;
	private static Rect redoPos;
	private static Rect textPos;

	public static Player currPlayer = Player.X;

	void Start()
	{
	}

	void CalcPositions()
	{
		float screenRatio = Screen.width / (float)Screen.height;
		float textureRatio = (player.height / (float)player.width);

		float txtW, txtH, btnW, btnH;
		if (screenRatio > 1f)
		{
			txtW = textWidth;
			txtH = txtW * textureRatio * screenRatio;
			btnW = buttonWidth;
			btnH = btnW * screenRatio;
		}
		else
		{
			txtH = textWidth;
			txtW = txtH / (textureRatio * screenRatio);
			btnH = buttonWidth;
			btnW = btnH / screenRatio;
		}

		textPos = new Rect(0f, 0f, Screen.width * txtW, Screen.height * txtH);

		float left1 = Screen.width - Screen.width * 2 * (btnW + xOffset);
		float left2 = Screen.width - Screen.width * (btnW + xOffset);
		float top = Screen.height - Screen.height * (btnH + yOffset);
		float totalWidth = Screen.width * 2 * btnW;
		float width = Screen.width * btnW;
		float height = Screen.height * btnH;

		boxPos = new Rect(left1, top, totalWidth, height);
		undoPos = new Rect(left1, top, width, height);
		redoPos = new Rect(left2, top, width, height);
	}

	void OnGUI()
	{
		CalcPositions();

		GUI.DrawTexture(textPos, player);
		GUI.DrawTexture(textPos, currPlayer == Player.X ? blue : red);

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

	public static bool InGui(Vector2 pos)
	{
		pos = new Vector2(pos.x, Screen.height - pos.y);
		bool inGui = boxPos.Contains(pos);
		return inGui;
	}
}
