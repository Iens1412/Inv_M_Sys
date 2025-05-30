﻿using System;
using System.ComponentModel;
using System.Reflection;

namespace Inv_M_Sys.Models
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.Description ?? value.ToString();
        }
    }
}
