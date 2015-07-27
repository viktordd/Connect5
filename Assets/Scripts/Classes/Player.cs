// ReSharper disable CheckNamespace

public enum Player : byte
{
	None = 0,
	X = 1,
	O = 2
}

public static class PlayerExtensions
{
	public static Player Next(this Player player)
	{
		switch (player)
		{
			case Player.X: return Player.O;
			default:
			case Player.O: return Player.X;
		}
	}
}