using System;
using JetBrains.Annotations;
using UnityEngine;
// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UseNullPropagation
// ReSharper disable ConvertPropertyToExpressionBody

[UsedImplicitly]
public class GuiController : MonoBehaviour
{
	public float TextWidth = 0.3f;
	public float ButtonWidth = 0.15f;
	public float XOffset = 0f;
	public float YOffset = -0.03f;

	public Texture2D Player;
	public Texture2D Blue;
	public Texture2D Red;
	public GUIStyle Undo;
	public GUIStyle UndoDisabled;
	public GUIStyle Redo;
	public GUIStyle RedoDisabled;

	public static event Action UndoAction;
	public static event Action RedoAction;

	public static bool UndoEnabled = false;
	public static bool RedoEnabled = false;

	public static Player CurrPlayer = global::Player.X;
	public static bool PlayerDisabled;

	public static event Action<float> ScreenChange;

	private float _textRatio;
	private float _buttonRatio;

	private static Rect _boxPos;
	private static Rect _undoPos;
	private static Rect _redoPos;
	private static Rect _textPos;

	private int _currWidth;
	private int _currHeight;

	void Start()
	{
		_textRatio = (Player.height / (float)Player.width);
		_buttonRatio = Undo.normal.background.height / (float)Undo.normal.background.width;
	}

	// ReSharper disable once InconsistentNaming
	void OnGUI()
	{
		TestForScreenChange();

		if (!PlayerDisabled)
		{
			GUI.DrawTexture(_textPos, Player);
			GUI.DrawTexture(_textPos, CurrPlayer == global::Player.X ? Blue : Red);
		}

		GUI.Box(_boxPos, "");

		if (UndoEnabled)
		{
			if (GUI.Button(_undoPos, string.Empty, Undo) && UndoAction != null)
				UndoAction();
		}
		else
		{
			GUI.Button(_undoPos, string.Empty, UndoDisabled);
		}

		if (RedoEnabled)
		{
			if (GUI.Button(_redoPos, string.Empty, Redo) && RedoAction != null)
				RedoAction();
		}
		else
		{
			GUI.Button(_redoPos, string.Empty, RedoDisabled);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }
	}

	private void TestForScreenChange()
	{
		if (_currWidth != Screen.width || _currHeight != Screen.height)
		{
			_currWidth = Screen.width;
			_currHeight = Screen.height;

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
			txtW = TextWidth;
			txtH = txtW * _textRatio * screenRatio;
			btnW = ButtonWidth;
			btnH = btnW * _buttonRatio * screenRatio;
			xd = XOffset;
			yd = YOffset;
		}
		else
		{
			txtW = TextWidth / screenRatio;
			txtH = txtW * _textRatio * screenRatio;
			btnW = ButtonWidth / screenRatio;
			btnH = btnW * _buttonRatio * screenRatio;
			xd = XOffset / screenRatio;
			yd = YOffset * screenRatio;
		}

		_textPos = new Rect(0f, 0f, Screen.width * txtW, Screen.height * txtH);

		float left1 = Screen.width - Screen.width * (2 * btnW + xd);
		float left2 = Screen.width - Screen.width * (btnW + xd);
		float top = Screen.height - Screen.height * (btnH + yd);
		float totalWidth = Screen.width * 2 * btnW;
		float width = Screen.width * btnW;
		float height = Screen.height * btnH;

		_boxPos = new Rect(left1, top, totalWidth, height);
		_undoPos = new Rect(left1, top, width, height);
		_redoPos = new Rect(left2, top, width, height);
	}

	public static bool InGui(Vector2 pos)
	{
		pos = new Vector2(pos.x, Screen.height - pos.y);
		bool inGui = _textPos.Contains(pos) || _boxPos.Contains(pos);
		return inGui;
	}
}
