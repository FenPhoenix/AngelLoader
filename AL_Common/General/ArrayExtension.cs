namespace System;

public static class ArrayExtension
{
    extension(Array)
    {
        // The Max Length of an Array
        // Copy of https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Array.cs
        public static int MaxLength => 0X7FFFFFC7;
    }
}
