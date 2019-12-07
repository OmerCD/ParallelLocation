﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ReflectorO.Attributes
{
    public class ArrayAttribute : Attribute
    {
        public ArrayAttribute(string lengthPropertyName)
        {
            LengthPropertyName = lengthPropertyName;
        }
        public ArrayAttribute(params int[] lengths)
        {
            Lengths = lengths;
        }
        public int[] Lengths { get; set; }
        public string LengthPropertyName { get; set; }
        
    }
}
