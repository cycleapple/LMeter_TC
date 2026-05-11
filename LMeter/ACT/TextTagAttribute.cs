using System;

namespace LMeter.Act
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class TextTagAttribute : Attribute
    {
        public TextTagAttribute(string tagName = "")
        {
            TagName = tagName;
        }
        string TagName { get; }
    }
}
