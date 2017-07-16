using UnityEngine;

// ReSharper disable CheckNamespace
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

public class ViewBoundsChange
{
    public int Top { get; private set; }
    public int Right { get; private set; }
    public int Bottom { get; private set; }
    public int Left { get; private set; }

    public int AddTop { get; set; }
    public int AddRight { get; set; }
    public int AddBottom { get; set; }
    public int AddLeft { get; set; }

    public int NewTop { get { return Top + AddTop; } }
    public int NewRight { get { return Right + AddRight; } }
    public int NewBottom { get { return Bottom - AddBottom; } }
    public int NewLeft { get { return Left - AddLeft; } }

    public int AddHorizontal { get { return AddLeft + AddRight; } }
    public int AddVertical { get { return AddTop + AddBottom; } }

    public bool AddAny { get { return AddTop > 0 || AddRight > 0 || AddBottom > 0 || AddLeft > 0; } }
    public bool AnyChanges { get { return AddTop != 0 || AddRight != 0 || AddBottom != 0 || AddLeft != 0; } }
    
    private readonly ViewBounds viewBounds;

    public ViewBoundsChange()
    {
        viewBounds = new ViewBounds();
    }

    public void Init()
    {
        viewBounds.Init();
        
        Top = Mathf.RoundToInt(viewBounds.Top);
        Right = Mathf.RoundToInt(viewBounds.Right);
        Bottom = Mathf.RoundToInt(viewBounds.Bottom);
        Left = Mathf.RoundToInt(viewBounds.Left);
    }

    public void Check()
    {
        viewBounds.Init();

        var newTop = Mathf.RoundToInt(viewBounds.Top);
        var newRight = Mathf.RoundToInt(viewBounds.Right);
        var newBottom = Mathf.RoundToInt(viewBounds.Bottom);
        var newLeft = Mathf.RoundToInt(viewBounds.Left);

        AddTop = newTop - Top;
        AddRight = newRight - Right;
        AddBottom = -newBottom + Bottom;
        AddLeft = -newLeft + Left;
    }
}