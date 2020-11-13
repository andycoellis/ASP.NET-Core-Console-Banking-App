using System;
using WDT2020_a1.Model;

namespace WDT2020_a1.Model
{
    public class CustomException : Exception
    {

    public CustomException(string type, string message)
            : base($"\n[{type}] Warning: {message}")
        {}
    

    public CustomException(string message)
            : base($"\nWarning: {message}")
        {}

    }
}
