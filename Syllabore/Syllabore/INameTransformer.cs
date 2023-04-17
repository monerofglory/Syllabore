﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Syllabore
{
    /// <summary>
    /// Transforms names generated by an <see cref="INameGenerator"/>.
    /// </summary>
    public interface INameTransformer
    {
        /*
        /// <summary>
        /// <para>
        /// A number from 0 to 1 inclusive that represents the probablity
        /// that a <see cref="NameGenerator"/> will apply a transform from this
        /// transformer during name generation (during a call to <see cref="NameGenerator.Next"/> or
        /// <see cref="NameGenerator.NextName"/>).
        /// </para>
        /// <para>
        /// A value of 0 means a transform can never occur and a value of 1
        /// means a mutation will always occur.
        /// </para>
        /// </summary>
        double TransformChance { get; set; }
        */

        Name Transform(Name sourceName);
    }
}
