// ReSharper disable CheckNamespace

using System;
using UnityEngine;

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
	        default: return Player.X;
	        case Player.X: return Player.O;
	    }
	}
    public static string GetText(this Player player)
    {
        switch (player)
        {
            case Player.X: return "Blue";
            case Player.O: return "Red";
            default: return string.Empty;
        }
    }
    public static Color GetColor(this Player player)
    {
        switch (player)
        {
            case Player.X: return Color.blue;
            case Player.O: return Color.red;
            default: return Color.black;
        }
    }
}