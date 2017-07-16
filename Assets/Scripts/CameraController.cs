using System;
using JetBrains.Annotations;
using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local

[UsedImplicitly]
public class CameraController : MonoBehaviour
{
	public float MoveDeadZone = 0.05f;
	public float MinZoom = 2.5f;
	public float MaxZoom = 15f;

	public float Drag = 0.015f;
	public float MaxVelocity = 0.6f;

    public event Action<ViewBoundsChange> ScreenPositionChanged;
    public event Action ScreenRatioChanged;

    private float zeroCount;
	private Vector2 velocityDir;
	private float velocityMag;
	private Vector2 velocity;
    
    private bool moveBegan;
    private Vector2 initTPos;

    private int currWidth;
    private int currHeight;

    private new Camera camera;
    private TouchController touchController;

    private ViewBoundsChange viewBoundsCheck;

    public void Start()
	{
	    currWidth = Screen.width;
	    currHeight = Screen.height;

	    viewBoundsCheck = new ViewBoundsChange();
        
	    camera = GetComponent<Camera>();

        touchController = GameObject.Find("TouchController").GetComponent<TouchController>();
	    if (touchController == null)
            return;

	    touchController.TouchStart += OnTouchStart;
	    touchController.TouchCancel += OnTouchCancel;
	    touchController.TouchEnd += OnTouchEnd;
	    touchController.TouchStationary += OnTouchStationary;
	    touchController.TouchMove += OnTouchMove;
	    touchController.MultiTouch += TouchZoom;
	    touchController.MouseScroll += Zoom;
	}

    public void OnDestroy()
    {
        if (touchController == null)
            return;

        touchController.TouchStart -= OnTouchStart;
        touchController.TouchCancel -= OnTouchCancel;
        touchController.TouchEnd -= OnTouchEnd;
        touchController.TouchStationary -= OnTouchStationary;
        touchController.TouchMove -= OnTouchMove;
        touchController.MouseScroll -= Zoom;
    }

    public void FixedUpdate()
	{
	    TestForScreenChange();
	    if (velocityMag <= 0)
            return;
        
	    velocity = velocityDir * velocityMag;

        viewBoundsCheck.Init();
	    OnScreenPositionChange();
	    velocityMag -= Drag;
	}

    private void OnTouchStart(Vector2 position)
    {
        initTPos = position;
        moveBegan = false;
        velocityMag = 0;
    }

    private void OnTouchCancel()
    {
        moveBegan = false;
    }

    private void OnTouchStationary()
    {
        velocity = Vector2.zero;
    }

    private void OnTouchMove(Vector2 position, float deltaTime, Vector2 prevTPos)
    {
        if (position == prevTPos)
        {
            zeroCount += Time.deltaTime;
            if (zeroCount > deltaTime)
                velocity = Vector2.zero;
            return;
        }

        zeroCount = 0;

        if (!moveBegan)
        {
            var dist = GetScreenToWorldDistance(position, initTPos);
            var magnitude = dist.magnitude;
            if (magnitude <= MoveDeadZone)
                return;
            velocity = dist.normalized * (magnitude - MoveDeadZone);
            moveBegan = true;
        }
        else
            velocity = GetScreenToWorldDistance(position, prevTPos);

        viewBoundsCheck.Init();
        OnScreenPositionChange();
    }

    private void OnTouchEnd(Vector2 pos)
    {
        if (!moveBegan)
            return;
        
        var mag = velocity.magnitude;
        velocityDir = velocity.normalized;

        if (mag > MaxVelocity)
        {
            mag = MaxVelocity;
            velocity = velocityDir * mag;
        }

        velocityMag = mag;
    }

    private void TouchZoom(Vector2 t0Pos, Vector2 t1Pos, Vector2 prevT0Pos, Vector2 prevT1Pos)
    {
        var touchDelta = camera.ScreenToWorldPoint(t0Pos) -
                         camera.ScreenToWorldPoint(t1Pos);
        var touchDeltaMag = ((Vector2) touchDelta).magnitude;

        Vector2 t0Prev = camera.ScreenToWorldPoint(prevT0Pos);
        Vector2 t1Prev = camera.ScreenToWorldPoint(prevT1Pos);
        var prevTouchDeltaMag = (t0Prev - t1Prev).magnitude;
        
        viewBoundsCheck.Init();

        SetSize(prevTouchDeltaMag - touchDeltaMag);

        Vector2 midpoint = camera.ScreenToWorldPoint(Vector2.Lerp(t0Pos, t1Pos, 0.5f));
        var midpointPrev = Vector2.Lerp(t0Prev, t1Prev, 0.5f);

        velocity = midpointPrev - midpoint;
        OnScreenPositionChange();
    }

    private void Zoom(Vector2 position, Vector2 mouseScrollDelta)
    {
        Vector2 prevPoint = camera.ScreenToWorldPoint(position);
        
        viewBoundsCheck.Init();

        SetSize(-mouseScrollDelta.y / 2);
        
        Vector2 newPoint = camera.ScreenToWorldPoint(position);

        velocity = prevPoint - newPoint;
        OnScreenPositionChange();
    }

    private void SetSize(float sizeDelta = 0)
    {
        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize + sizeDelta, MinZoom, MaxZoom);
    }

    private Vector3 GetScreenToWorldDistance(Vector2 pos1, Vector2 pos2)
	{
	    var delta = camera.ScreenToWorldPoint(pos1 - pos2);
		return camera.ScreenToWorldPoint(Vector2.zero) - delta;
	}

	private void OnScreenPositionChange()
	{
	    camera.transform.position += (Vector3)velocity;
	    //CheckOutOfBounds();

        if (ScreenPositionChanged == null)
	        return;

	    viewBoundsCheck.Check();
	    ScreenPositionChanged(viewBoundsCheck);
	}

    private void TestForScreenChange()
    {
        if (currWidth == Screen.width && currHeight == Screen.height)
        {
            return;
        }

        currWidth = Screen.width;
        currHeight = Screen.height;

        //CheckOutOfBounds();

        if (ScreenRatioChanged != null)
            ScreenRatioChanged();
    }

    //private void CheckOutOfBounds()
    //{
    //    var cameraDelta = Vector2.zero;
        
    //    var viewBounds = new ViewBounds();
    //    viewBounds.Init();

    //    if (viewBounds.Top > BoardSize.Rect.yMax)
    //        cameraDelta = new Vector2(cameraDelta.x, BoardSize.Rect.yMax - viewBounds.Top);

    //    if (viewBounds.Right > BoardSize.Rect.xMax)
    //        cameraDelta = new Vector2(BoardSize.Rect.xMax - viewBounds.Right, cameraDelta.y);

    //    if (viewBounds.Bottom < BoardSize.Rect.yMin)
    //        cameraDelta = new Vector2(cameraDelta.x, BoardSize.Rect.yMin - viewBounds.Bottom);

    //    if (viewBounds.Left < BoardSize.Rect.xMin)
    //        cameraDelta = new Vector2(BoardSize.Rect.xMin - viewBounds.Left, cameraDelta.y);

    //    if (cameraDelta != Vector2.zero)
    //        camera.transform.position += (Vector3)cameraDelta;
    //}
}