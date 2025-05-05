using System;
using System.Collections.Generic;
using System.Text;

namespace Diogen.Generators;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class OptionalAttribute : Attribute;
