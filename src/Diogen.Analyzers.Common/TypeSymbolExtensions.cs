using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Diogen.Analyzers.Common;

internal static class TypeSymbolExtensions
{
    private static readonly HashSet<SpecialType> BuiltInTypes =
    [
        SpecialType.System_Boolean,
        SpecialType.System_Char,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_Decimal,
        SpecialType.System_SByte,
        SpecialType.System_Byte,
        SpecialType.System_Int16, 
        SpecialType.System_UInt16,
        SpecialType.System_Int32, 
        SpecialType.System_UInt32,
        SpecialType.System_Int64, 
        SpecialType.System_UInt64,
        SpecialType.System_String,
        SpecialType.System_Object
    ];

    public static bool IsBuiltInType(this ITypeSymbol typeSymbol) =>
        BuiltInTypes.Contains(typeSymbol.SpecialType);
}
