using System;

namespace s2industries.ZUGFeRD
{
    public class IllegalStreamException : Exception
    {
        public IllegalStreamException(string message = "")
            : base(message)
        { 
        }
    }
}
