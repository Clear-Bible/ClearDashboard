using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices
{
    // https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809
    // Defining this dummy class is apparently the solution to this error message:
    //      Error CS0518  Predefined type 'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported
    // According to article at the above link:
    //
    // "Unfortunately this is not a bug. The IsExternalInit type is only included in the net5.0 (and future) target frameworks.
    //  When compiling against older target frameworks you will need to manually define this type."
    internal class IsExternalInit
    {
    }
}
