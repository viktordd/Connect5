using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

public class ViewBounds
{
    public Vector2 BottomLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    
    public float Top { get { return TopRight.y; } }
    public float Right { get { return TopRight.x; } }
    public float Bottom { get { return BottomLeft.y; } }
    public float Left { get { return BottomLeft.x; } }

    private readonly Camera camera;

    public ViewBounds()
    {
        camera = Camera.main;
    }

    public void Init()
    {
        BottomLeft = camera.ScreenToWorldPoint(Vector2.zero);
        TopRight = camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }
}