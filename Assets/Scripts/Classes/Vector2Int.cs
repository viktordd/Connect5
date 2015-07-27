using UnityEngine;
// ReSharper disable CheckNamespace
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable UseStringInterpolation
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

public struct Vector2Int
{
	public int X;
	public int Y;

	public static Vector2Int Zero
	{
		get
		{
			return new Vector2Int(0, 0);
		}
	}

	public static Vector2Int One
	{
		get
		{
			return new Vector2Int(1, 1);
		}
	}

	public static Vector2Int Up
	{
		get
		{
			return new Vector2Int(0, 1);
		}
	}

	public static Vector2Int Right
	{
		get
		{
			return new Vector2Int(1, 0);
		}
	}

	public Vector2Int(int x, int y)
	{
		X = x;
		Y = y;
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
		return new Vector2(v.X, v.Y);
	}

	public static implicit operator Vector3(Vector2Int v)
	{
		return new Vector3(v.X, v.Y, 0f);
	}

	public static Vector2Int operator +(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.X + b.X, a.Y + b.Y);
	}

	public static Vector2Int operator -(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.X - b.X, a.Y - b.Y);
	}

	public static Vector2Int operator -(Vector2Int a)
	{
		return new Vector2Int(-a.X, -a.Y);
	}

	public static Vector2Int operator *(Vector2Int a, int d)
	{
		return new Vector2Int(a.X * d, a.Y * d);
	}

	public static Vector2Int operator *(int d, Vector2Int a)
	{
		return new Vector2Int(a.X * d, a.Y * d);
	}

	public static Vector2Int operator /(Vector2Int a, int d)
	{
		return new Vector2Int(a.X / d, a.Y / d);
	}

	public static bool operator ==(Vector2Int x, Vector2Int y)
	{
		return x.X == y.X && x.Y == y.Y;
	}

	public static bool operator !=(Vector2Int x, Vector2Int y)
	{
		return x.X != y.X && x.Y != y.Y;
	}

	public void Set(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static Vector2Int Scale(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.X * b.X, a.Y * b.Y);
	}

	public void Scale(Vector2Int scale)
	{
		X *= scale.X;
		Y *= scale.Y;
	}

	public override string ToString()
	{
		return string.Format("({0}, {1})", X, Y);
	}

	public override int GetHashCode()
	{
		return X.GetHashCode() ^ Y.GetHashCode() << 2;
	}

	public override bool Equals(object other)
	{
		if (!(other is Vector2Int))
			return false;
		var vector2Int = (Vector2Int)other;
		if (X.Equals(vector2Int.X))
			return Y.Equals(vector2Int.Y);
		return false;
	}

	public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs)
	{
		return new Vector2Int(Mathf.Min(lhs.X, rhs.X), Mathf.Min(lhs.Y, rhs.Y));
	}

	public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs)
	{
		return new Vector2Int(Mathf.Max(lhs.X, rhs.X), Mathf.Max(lhs.Y, rhs.Y));
	}
}