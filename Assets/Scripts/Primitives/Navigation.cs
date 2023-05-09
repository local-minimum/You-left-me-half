﻿using DeCrawl.Utils;

public enum Navigation
{
    [StringValue("forward")]
    Forward,

    [StringValue("backward")]
    Backward,

    [StringValue("left")]
    Left,

    [StringValue("right")]
    Right,

    [StringValue("turnCW")]
    TurnCW,

    [StringValue("turnCCW")]
    TurnCCW,

    [StringValue("turn180")]
    Turn180,

    None
}


public static class NavigationExtensions
{
    public static bool Translates(this Navigation navigation) => navigation == Navigation.Forward || navigation == Navigation.Backward || navigation == Navigation.Left || navigation == Navigation.Right;

    public static bool Rotates(this Navigation navigation) => navigation == Navigation.TurnCW || navigation == Navigation.TurnCCW || navigation == Navigation.Turn180;

    public static FaceDirection asDirection(this Navigation navigation, FaceDirection lookDirection)
    {
        switch (navigation)
        {
            case Navigation.TurnCCW:
                return lookDirection.RotateCCW();
            case Navigation.TurnCW:
                return lookDirection.RotateCW();
            case Navigation.Turn180:
                return lookDirection.Invert();
            default:
                return lookDirection;
        }
    }

    public static Navigation FromToRotation(FaceDirection from, FaceDirection to)
    {
        if (from == FaceDirection.Invalid || to == FaceDirection.Invalid || from == to) return Navigation.None;

        if (from.RotateCW() == to) return Navigation.TurnCW;
        var ccwFrom = from.RotateCCW();
        if (ccwFrom == to) return Navigation.TurnCCW;
        if (ccwFrom.RotateCCW() == to) return Navigation.Turn180;
        
        throw new System.NotImplementedException("Not implemented rotaions involving up and down");
    }
}