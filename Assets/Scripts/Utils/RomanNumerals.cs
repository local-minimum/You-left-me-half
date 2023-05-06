using UnityEngine;

public static class RomanNumerals
{
    static string ConvertToNumeral(int value, string baseUnit, string midUnit, string nextUnit)
    {
        switch (value)
        {
            case 1:
                return baseUnit;
            case 2:
                return baseUnit + baseUnit;
            case 3:
                return baseUnit + baseUnit + baseUnit;
            case 4:
                return baseUnit + midUnit;
            case 5:
                return midUnit;
            case 6:
                return midUnit + baseUnit;
            case 7:
                return midUnit + baseUnit + baseUnit;
            case 8:
                return midUnit + baseUnit + baseUnit + baseUnit;
            case 9:
                return baseUnit + nextUnit;
        }
        return "";
    }

    public static string ToRomanNumerals(this int value)
    {
        if (value >= 1000) throw new System.NotImplementedException("Max value is 1000 for now");
        return ConvertToNumeral(value / 100, "C", "D", "M") + ConvertToNumeral(value / 10, "X", "L", "C") + ConvertToNumeral(value, "I", "V", "X");
    }
}
