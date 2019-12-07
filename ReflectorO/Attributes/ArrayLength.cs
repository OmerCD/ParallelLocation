﻿using System;

namespace ReflectorO.Attributes
{
    public class ArrayLengthAttribute: Attribute
    {
        public ArrayLengthAttribute(string arrayPropertyName)
        {
            ArrayPropertyName = arrayPropertyName;
        }

        public string ArrayPropertyName { get; set; }
    }
}