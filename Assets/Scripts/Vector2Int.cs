using UnityEngine;

public struct Vector2Int
{
	public int x;
	public int y;

	public static Vector2Int zero
	{
		get
		{
			return new Vector2Int(0, 0);
		}
	}

	public static Vector2Int one
	{
		get
		{
			return new Vector2Int(1, 1);
		}
	}

	public static Vector2Int up
	{
		get
		{
			return new Vector2Int(0, 1);
		}
	}

	public static Vector2Int right
	{
		get
		{
			return new Vector2Int(1, 0);
		}
	}

	public Vector2Int(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public static implicit operator Vector2Int(Vector2 v)
	{
		return new Vector2Int((int)v.x, (int)v.y);
	}

	public static implicit operator Vector2Int(Vector3 v)
	{
		return new Vector2Int((int)v.x, (int)v.y);
	}

	public static implicit operator Vector2(Vector2Int v)
	{
		return new Vector2(v.x, v.y);
	}

	public static implicit operator Vector3(Vector2Int v)
	{
		return new Vector3(v.x, v.y, 0f);
	}

	public static Vector2Int operator +(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x + b.x, a.y + b.y);
	}

	public static Vector2Int operator -(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x - b.x, a.y - b.y);
	}

	public static Vector2Int operator -(Vector2Int a)
	{
		return new Vector2Int(-a.x, -a.y);
	}

	public static Vector2Int operator *(Vector2Int a, int d)
	{
		return new Vector2Int(a.x * d, a.y * d);
	}

	public static Vector2Int operator *(int d, Vector2Int a)
	{
		return new Vector2Int(a.x * d, a.y * d);
	}

	public static Vector2Int operator /(Vector2Int a, int d)
	{
		return new Vector2Int(a.x / d, a.y / d);
	}

	public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}

	public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
	{
		return lhs.x != rhs.x && lhs.y != rhs.y;
	}

	public void Set(int new_x, int new_y)
	{
		this.x = new_x;
		this.y = new_y;
	}

	public static Vector2Int Scale(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x * b.x, a.y * b.y);
	}

	public void Scale(Vector2Int scale)
	{
		this.x *= scale.x;
		this.y *= scale.y;
	}

	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
	}

	public override bool Equals(object other)
	{
		if (!(other is Vector2Int))
			return false;
		Vector2Int Vector2Int = (Vector2Int)other;
		if (this.x.Equals(Vector2Int.x))
			return this.y.Equals(Vector2Int.y);
		else
			return false;
	}

	public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs)
	{
		return new Vector2Int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));
	}

	public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs)
	{
		return new Vector2Int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));
	}
}