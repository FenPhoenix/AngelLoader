using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelLoader.Common
{
    internal static class Attributes
    {
        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationClassAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Class)]
        internal class FenGenLocalizationReadWriteClass : Attribute { }
    }
}
