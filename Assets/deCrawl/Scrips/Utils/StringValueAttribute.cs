using System;

namespace DeCrawl.Utils
{
    public class StringValueAttribute : Attribute
    {
        #region Properties
        public string StringValue { get; protected set; }
        #endregion

        #region Constructors
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }
        #endregion
    }
}