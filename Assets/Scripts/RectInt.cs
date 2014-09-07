using UnityEngine;

public class RectInt
{
	private int m_XMin;
	private int m_YMin;
	private int m_Width;
	private int m_Height;

	public int x
	{
		get
		{
			return this.m_XMin;
		}
		set
		{
			this.m_XMin = value;
		}
	}

	public int y
	{
		get
		{
			return this.m_YMin;
		}
		set
		{
			this.m_YMin = value;
		}
	}

	public Vector2Int position
	{
		get
		{
			return new Vector2Int(this.m_XMin, this.m_YMin);
		}
		set
		{
			this.m_XMin = value.x;
			this.m_YMin = value.y;
		}
	}

	public Vector2 center
	{
		get
		{
			return new Vector2(this.x + this.m_Width / 2f, this.y + this.m_Height / 2f);
		}
		set
		{
			this.m_XMin = Mathf.RoundToInt(value.x) - this.m_Width / 2;
			this.m_YMin = Mathf.RoundToInt(value.y) - this.m_Height / 2;
		}
	}

	public Vector2Int min
	{
		get
		{
			return new Vector2Int(this.xMin, this.yMin);
		}
		set
		{
			this.xMin = value.x;
			this.yMin = value.y;
		}
	}

	public Vector2Int max
	{
		get
		{
			return new Vector2Int(this.xMax, this.yMax);
		}
		set
		{
			this.xMax = value.x;
			this.yMax = value.y;
		}
	}

	public int width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public int height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public Vector2Int size
	{
		get
		{
			return new Vector2Int(this.m_Width, this.m_Height);
		}
		set
		{
			this.m_Width = value.x;
			this.m_Height = value.y;
		}
	}

	public int xMin
	{
		get
		{
			return this.m_XMin;
		}
		set
		{
			int xMax = this.xMax;
			this.m_XMin = value;
			this.m_Width = xMax - this.m_XMin;
		}
	}

	public int yMin
	{
		get
		{
			return this.m_YMin;
		}
		set
		{
			int yMax = this.yMax;
			this.m_YMin = value;
			this.m_Height = yMax - this.m_YMin;
		}
	}

	public int xMax
	{
		get
		{
			return this.m_Width + this.m_XMin;
		}
		set
		{
			this.m_Width = value - this.m_XMin;
		}
	}

	public int yMax
	{
		get
		{
			return this.m_Height + this.m_YMin;
		}
		set
		{
			this.m_Height = value - this.m_YMin;
		}
	}

	public RectInt(int left, int top, int width, int height)
	{
		this.m_XMin = left;
		this.m_YMin = top;
		this.m_Width = width;
		this.m_Height = height;
	}

	public RectInt(RectInt source)
	{
		this.m_XMin = source.m_XMin;
		this.m_YMin = source.m_YMin;
		this.m_Width = source.m_Width;
		this.m_Height = source.m_Height;
	}

	public static RectInt zero
	{
		get
		{
			return new RectInt(0, 0, 0, 0);
		}
	}

	public static bool operator !=(RectInt lhs, RectInt rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width)
			return lhs.height != rhs.height;
		else
			return true;
	}

	public static bool operator ==(RectInt lhs, RectInt rhs)
	{
		if (lhs.x == rhs.x && lhs.y == rhs.y && lhs.width == rhs.width)
			return lhs.height == rhs.height;
		else
			return false;
	}

	public static RectInt operator *(RectInt a, int d)
	{
		return new RectInt(a.x * d, a.y * d, a.width * d, a.height * d);
	}

	public static RectInt operator *(int d, RectInt a)
	{
		return new RectInt(a.x * d, a.y * d, a.width * d, a.height * d);
	}

	public static RectInt MinMaxRect(int left, int top, int right, int bottom)
	{
		return new RectInt(left, top, right - left, bottom - top);
	}

	public void Set(int left, int top, int width, int height)
	{
		this.m_XMin = left;
		this.m_YMin = top;
		this.m_Width = width;
		this.m_Height = height;
	}

	public bool Contains(Vector2 point)
	{
		if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
			return (double)point.y < (double)this.yMax;
		else
			return false;
	}

	public bool Contains(Vector3 point)
	{
		if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
			return (double)point.y < (double)this.yMax;
		else
			return false;
	}

	public bool Contains(Vector3 point, bool allowInverse)
	{
		if (!allowInverse)
			return this.Contains(point);
		bool flag = false;
		if ((double)this.width < 0.0 && (double)point.x <= (double)this.xMin && (double)point.x > (double)this.xMax || (double)this.width >= 0.0 && (double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax)
			flag = true;
		return flag && ((double)this.height < 0.0 && (double)point.y <= (double)this.yMin && (double)point.y > (double)this.yMax || (double)this.height >= 0.0 && (double)point.y >= (double)this.yMin && (double)point.y < (double)this.yMax);
	}

	private static RectInt OrderMinMax(RectInt rect)
	{
		if (rect.xMin > rect.xMax)
		{
			int xMin = rect.xMin;
			rect.xMin = rect.xMax;
			rect.xMax = xMin;
		}
		if (rect.yMin > rect.yMax)
		{
			int yMin = rect.yMin;
			rect.yMin = rect.yMax;
			rect.yMax = yMin;
		}
		return rect;
	}

	public bool Overlaps(RectInt other)
	{
		if (other.xMax > this.xMin && other.xMin < this.xMax && other.yMax > this.yMin)
			return other.yMin < this.yMax;
		else
			return false;
	}

	public bool Overlaps(RectInt other, bool allowInverse)
	{
		RectInt rect = this;
		if (allowInverse)
		{
			rect = RectInt.OrderMinMax(rect);
			other = RectInt.OrderMinMax(other);
		}
		return rect.Overlaps(other);
	}

	public static Vector2 NormalizedToPoint(Rect rectangle, Vector2 normalizedRectCoordinates)
	{
		return new Vector2(Mathf.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Mathf.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
	}

	public static Vector2 PointToNormalized(Rect rectangle, Vector2 point)
	{
		return new Vector2(Mathf.InverseLerp(rectangle.x, rectangle.xMax, point.x), Mathf.InverseLerp(rectangle.y, rectangle.yMax, point.y));
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.y.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
	}

	public override bool Equals(object other)
	{
		if (!(other is Rect))
			return false;
		Rect rect = (Rect)other;
		if (this.x.Equals(rect.x) && this.y.Equals(rect.y) && this.width.Equals(rect.width))
			return this.height.Equals(rect.height);
		else
			return false;
	}
}
