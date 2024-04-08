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
        Logarithm = 15,
        SquareRoot = 20,
        Modulo = 30,
        Sine = 32,
        Cosine = 33,
        Tangent = 34,
        Arcsine = 35,
        Arccosine = 36,
        Arctangent = 37,
        Arctangent2 = 38,
        RoundClosest = 40,
        RoundUp = 50,
        RoundDown = 60,
        SetPositive = 70,
        SetNegative = 80,
        SetToSign = 65,
        SetToMax = 90,
        SetToMin = 100,
        SetToDefault = 110
    }
}