using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Local

[UsedImplicitly]
public class TouchController : MonoBehaviour
{
    public float MoveDeadZone = 0.05f;

    public event Action<Vector2> TouchStart;
    public event Action TouchStationary;
    public event Action<Vector2, float, Vector2> TouchMove;
    public event Action<Vector2> TouchEnd;
    public event Action<Vector2, Vector2, Vector2, Vector2> MultiTouch;
    public event Action<Vector2, Vector2> MouseScroll;
    public event Action TouchCancel;
    
    private bool initTPosOverGui;
    private Vector2 prevTPos;
    private Vector2 prevT0Pos;
    private Vector2 prevT1Pos;

    public void Start()
    {
        Input.simulateMouseWithTouches = false;
    }

    public void Update()
    {
        CheckTouch();
    }

    public void RestartGame()
    {
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void CheckTouch()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { Application.Quit(); }

        var mouseInput = false;
        if (Input.mousePresent)
        {
            Vector2 mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                mouseInput = true;
                OnTouchStart(mousePosition);
            }

            else if (Input.GetMouseButton(0))
            {
                mouseInput = true;
                if (mousePosition == prevTPos)
                    OnTouchStationary();
                else
                    OnTouchMove(mousePosition, 0);
            }

            else if (Input.GetMouseButtonUp(0))
            {
                mouseInput = true;
                OnTouchEnd(mousePosition);
            }

            var mouseScrollDelta = Input.mouseScrollDelta;
            if (mouseScrollDelta != Vector2.zero)
            {
                mouseInput = true;
                OnMouseScroll(mousePosition, mouseScrollDelta);
            }

            prevTPos = mousePosition;
        }

        if (mouseInput)
            return;

        switch (Input.touchCount)
        {
            case 1:
                var t = Input.GetTouch(0);
                switch (t.phase)
                {
                    case TouchPhase.Began:
                        OnTouchStart(t.position, t.fingerId);
                        break;

                    case TouchPhase.Stationary:
                        OnTouchStationary();
                        break;

                    case TouchPhase.Moved:
                        OnTouchMove(t.position, t.deltaTime);
                        break;

                    case TouchPhase.Ended:
                        OnTouchEnd(t.position);
                        break;

                    //case TouchPhase.Canceled:
                    default:
                        OnTouchCancell();
                        break;
                }
                prevTPos = t.position;
                break;

            case 2:
                OnMultiTouch(Input.GetTouch(0), Input.GetTouch(1));
                break;
        }
    }

    protected bool IsOnGui(int? pointerId = null)
    {
        return pointerId.HasValue
            ? EventSystem.current.IsPointerOverGameObject(pointerId.Value)
            : EventSystem.current.IsPointerOverGameObject();
    }

    private void OnTouchStart(Vector2 pos, int? pointerId = null)
    {
        initTPosOverGui = IsOnGui(pointerId);
        if (initTPosOverGui)
            return;

        if (TouchStart != null)
            TouchStart(pos);
    }

    private void OnTouchStationary()
    {
        if (!initTPosOverGui && TouchStationary != null)
            TouchStationary();
    }

    private void OnTouchMove(Vector2 pos, float deltaTime)
    {
        if (!initTPosOverGui && TouchMove != null)
            TouchMove(pos, deltaTime, prevTPos);
    }

    private void OnTouchEnd(Vector2 pos)
    {
        if (!initTPosOverGui && TouchEnd != null)
            TouchEnd(pos);
    }

    private void OnMultiTouch(Touch t0, Touch t1)
    {
        if (initTPosOverGui)
            return;

        if (t0.phase != TouchPhase.Began && t1.phase != TouchPhase.Began &&
            (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved))
        {
            if (MultiTouch != null)
                MultiTouch(t0.position, t1.position, prevT0Pos, prevT1Pos);
        }
        if (t0.phase == TouchPhase.Canceled || t0.phase == TouchPhase.Ended)
            prevTPos = t1.position;
        if (t1.phase == TouchPhase.Canceled || t1.phase == TouchPhase.Ended)
            prevTPos = t0.position;

        prevT0Pos = t0.position;
        prevT1Pos = t1.position;
    }

    private void OnTouchCancell()
    {
        if (TouchCancel != null)
            TouchCancel();
    }

    private void OnMouseScroll(Vector2 pos, Vector2 delta)
    {
        if (MouseScroll == null)
            return;
        MouseScroll(pos, delta);
    }
}
