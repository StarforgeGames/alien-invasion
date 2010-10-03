﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Entities
{

    class Attribute<T>
    {
        public T Value { get; set; }

        public Attribute(T value)
        {
            Value = value;
        }

        public static implicit operator T(Attribute<T> attribute)
        {
            return attribute.Value;
        }
    }

}
