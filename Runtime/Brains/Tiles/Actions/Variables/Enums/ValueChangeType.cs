namespace Mona.SDK.Brains.Tiles.Actions.Variables.Enums
{
    [System.Serializable]
    public enum ValueChangeType
    {
        Set = 0,
        Add = 1,
        Subtract = 2,
        Multiply = 3,
        Divide = 4,
        Exponent = 10,
        SquareRoot = 20,
        Modulo = 30,
        RoundClosest = 40,
        RoundUp = 50,
        RoundDown = 60,
        SetPositive = 70,
        SetNegative = 80,
        SetToMax = 90,
        SetToMin = 100,
        SetToDefault = 110
    }
}