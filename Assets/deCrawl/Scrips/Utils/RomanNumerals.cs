namespace DeCrawl.Utils
{
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

        /// <summary>
        /// Converts a non-negative number below 3999 into roman numerals
        /// </summary>
        /// <param name="value">The value to be converted</param>
        /// <returns>Roman numeral representation of the number</returns>
        public static string ToRomanNumerals(this int value)
        {
            if (value == 0) return "";

            if (value < 0)
            {
                throw new System.ArgumentException("Value must be positive");
            }

            var val = value;
            var prefix = "";
            for (int i=0; i<3; i++)
            {
                if (val > 1000)
                {
                    prefix += "M";
                    val -= 1000;
                } else
                {
                    break;
                }
            }

            if (val >= 1000) throw new System.ArgumentException("Max value is 3999 for now");

            return prefix + ConvertToNumeral(value / 100, "C", "D", "M") + ConvertToNumeral(value / 10, "X", "L", "C") + ConvertToNumeral(value, "I", "V", "X");
        }
    }
}