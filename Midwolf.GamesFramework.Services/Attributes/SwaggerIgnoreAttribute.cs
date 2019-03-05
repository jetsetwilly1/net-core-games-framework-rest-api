using System;
using System.Collections.Generic;
using System.Text;

namespace Midwolf.GamesFramework.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}
