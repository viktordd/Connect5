using UnityEngine;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UseStringInterpolation
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable NonReadonlyMemberInGetHashCode

// ReSharper disable CheckNamespace

public class RectInt
{
	public int X { get; set; }

	public int Y { get; set; }

	public Vector2Int Position
	{
		get
		{
			return new Vector2Int(X, Y);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public Vector2 Center
	{
		get
		{
			return new Vector2(X + Width / 2f, Y + Height / 2f);
		}
		set
		{
			X = Mathf.RoundToInt(value.x) - Width / 2;
			Y = Mathf.RoundToInt(value.y) - Height / 2;
		}
	}

	public Vector2Int Min
	{
		get
		{
			return new Vector2Int(XMin, YMin);
		}
		set
		{
			XMin = value.X;
			YMin = value.Y;
		}
	}

	public Vector2Int Max
	{
		get
		{
			return new Vector2Int(XMax, YMax);
		}
		set
		{
			XMax = value.X;
			YMax = value.Y;
		}
	}

	public int Width { get; set; }

	public int Height { get; set; }

	public Vector2Int Size
	{
		get
		{
			return new Vector2Int(Width, Height);
		}
		set
		{
			Width = value.X;
			Height = value.Y;
		}
	}

	public int XMin
	{
		get
		{
			return X;
		}
		set
		{
			var xMax = XMax;
			X = value;
			Width = xMax - X;
		}
	}

	public int YMin
	{
		get
		{
			return Y;
		}
		set
		{
			var yMax = YMax;
			Y = value;
			Height = yMax - Y;
		}
	}

	public int XMax
	{
		get
		{
			return Width + X;
		}
		set
		{
			Width = value - X;
		}
	}

	public int YMax
	{
		get
		{
			return Height + Y;
		}
		set
		{
			Height = value - Y;
		}
	}

	public RectInt(int left, int top, int width, int height)
	{
		X = left;
		Y = top;
		Width = width;
		Height = height;
	}

	public RectInt(RectInt source)
	{
		X = source.X;
		Y = source.Y;
		Width = source.Width;
		Height = source.Height;
    }

    public static RectInt Zero
	{
		get
		{
			return new RectInt(0, 0, 0, 0);
		}
	}

	public static RectInt operator *(RectInt a, int d)
	{
		return new RectInt(a.X * d, a.Y * d, a.Width * d, a.Height * d);
	}

	public static RectInt operator *(int d, RectInt a)
	{
		return new RectInt(a.X * d, a.Y * d, a.Width * d, a.Height * d);
	}

	public static RectInt MinMaxRect(int left, int top, int right, int bottom)
	{
		return new RectInt(left, top, right - left, bottom - top);
	}

	public void Set(int left, int top, int width, int height)
	{
		X = left;
		Y = top;
		Width = width;
		Height = height;
	}

	public override string ToString()
	{
		return string.Format("(x:{0}, y:{1}, width:{2}, height:{3})", X, Y, Width, Height);
	}
	
	public bool Contains(Vector2 point)
	{
		if (point.x >= (double)XMin && point.x < (double)XMax && point.y >= (double)YMin)
			return point.y < (double)YMax;
		return false;
	}

	public bool Contains(Vector3 point)
	{
		if (point.x >= (double)XMin && point.x < (double)XMax && point.y >= (double)YMin)
			return point.y < (double)YMax;
		return false;
	}

	public bool Contains(Vector3 point, bool allowInverse)
	{
		if (!allowInverse)
			return Contains(point);
		var flag = Width < 0.0 && point.x <= (double)XMin && point.x > (double)XMax || Width >= 0.0 && point.x >= (double)XMin && point.x < (double)XMax;
		return flag && (Height < 0.0 && point.y <= (double)YMin && point.y > (double)YMax || Height >= 0.0 && point.y >= (double)YMin && point.y < (double)YMax);
	}

	private static RectInt OrderMinMax(RectInt rect)
	{
		if (rect.XMin > rect.XMax)
		{
			var xMin = rect.XMin;
			rect.XMin = rect.XMax;
			rect.XMax = xMin;
		}
		if (rect.YMin > rect.YMax)
		{
			var yMin = rect.YMin;
			rect.YMin = rect.YMax;
			rect.YMax = yMin;
		}
		return rect;
	}

	public bool Overlaps(RectInt other)
	{
		if (other.XMax > XMin && other.XMin < XMax && other.YMax > YMin)
			return other.YMin < YMax;
		return false;
	}

	public bool Overlaps(RectInt other, bool allowInverse)
	{
		var rect = this;
		if (allowInverse)
		{
			rect = OrderMinMax(rect);
			other = OrderMinMax(other);
		}
		return rect.Overlaps(other);
	}

	public static Vector2 NormalizedToPoint(RectInt rectangle, Vector2 normalizedRectCoordinates)
	{
		return new Vector2(Mathf.Lerp(rectangle.X, rectangle.XMax, normalizedRectCoordinates.x), Mathf.Lerp(rectangle.Y, rectangle.YMax, normalizedRectCoordinates.y));
	}

	public static Vector2 PointToNormalized(RectInt rectangle, Vector2 point)
	{
		return new Vector2(Mathf.InverseLerp(rectangle.X, rectangle.XMax, point.x), Mathf.InverseLerp(rectangle.Y, rectangle.YMax, point.y));
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = X;
			hashCode = (hashCode * 397) ^ Y;
			hashCode = (hashCode * 397) ^ Width;
			hashCode = (hashCode * 397) ^ Height;
			return hashCode;
		}
	}

	public bool Equals(RectInt other)
	{
		return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		return obj.GetType() == GetType() && Equals((RectInt) obj);
	}

	public static bool operator !=(RectInt left, RectInt right)
	{
		return !ReferenceEquals(null, left) && !ReferenceEquals(null, right) && !left.Equals(right);
	}

	public static bool operator ==(RectInt left, RectInt right)
	{
		return !ReferenceEquals(null, left) && !ReferenceEquals(null, right) && left.Equals(right);
	}
}
