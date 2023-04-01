﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Syllabore
{
    /// <summary>
    /// <para>
    /// Generates names by constructing syllables and joining them together.
    /// It can also filter its output through an <see cref="INameFilter"/> if one is specified.
    /// </para>
    /// <para>
    /// Call <see cref="Next()"/> to return names as strings and <see cref="NextName()"/>
    /// to return names as <see cref="Name"/> structs. The former method gives access to the 
    /// individual syllables of the name.
    /// </para>
    /// </summary>
    public class NameGenerator : INameGenerator
    {
        private const int DefaultMaximumRetries = 1000;
        private const double DefaultTransformChance = 1.0;

        private Random Random { get; set; }

        public ISyllableProvider Provider { get; set; }
        public INameTransformer Transformer { get; set; }
        public INameFilter Filter { get; set; }
        public int MinimumSyllables { get; set; }
        public int MaximumSyllables { get; set; }

        /// <summary>
        /// Maximum attempts this generator will attempt to satisfy the
        /// NameFilter before it throws an Exception. This is used to protect
        /// against scenarios where a <see cref="NameGenerator"/> has been configured in such
        /// a way that it can't generate any name that would satisfy its own filter.
        /// </summary>
        public int MaximumRetries { get; set; }

        /// <summary>
        /// When there are no constructor arguments, the name generator is configured to
        /// use a <see cref="DefaultSyllableProvider"/>, no <see cref="INameTransformer"/>, and no <see cref="INameFilter"/>.
        /// </summary>
        public NameGenerator() : this(new DefaultSyllableProvider(), null, null) { }

        /// <summary>
        /// Instantiates a new <see cref="NameGenerator"/> with the specified <see cref="ISyllableProvider"/>.
        /// No <see cref="INameTransformer"/> or <see cref="INameFilter"/> will be used.
        /// </summary>
        public NameGenerator(ISyllableProvider provider) : this(provider, null, null) { }

        /// <summary>
        /// Instantiates a new <see cref="NameGenerator"/> with the specified <see cref="ISyllableProvider"/> and
        /// <see cref="INameTransformer"/>. No <see cref="INameFilter"/> will be used.
        /// </summary>
        public NameGenerator(ISyllableProvider provider, INameTransformer transformer) : this(provider, transformer, null) { }

        /// <summary>
        /// Instantiates a new <see cref="NameGenerator"/> with the specified <see cref="ISyllableProvider"/> and
        /// <see cref="INameFilter"/>. No <see cref="INameTransformer"/> will be used.
        /// </summary>
        public NameGenerator(ISyllableProvider provider, INameFilter filter) : this(provider, null, filter) { }

        /// <summary>
        /// Instantiates a new <see cref="NameGenerator"/> with the specified <see cref="ISyllableProvider"/>, 
        /// <see cref="INameFilter"/>, and <see cref="INameTransformer"/>.
        /// </summary>
        public NameGenerator(ISyllableProvider provider, INameTransformer transformer, INameFilter filter)
        {
            this.UsingProvider(provider)
                .UsingSyllableCount(2, 2)
                .LimitRetries(DefaultMaximumRetries);

            if (transformer != null)
            {
                this.UsingTransformer(transformer);
            }

            if (filter != null)
            {
                this.UsingFilter(filter);
            }

            this.Random = new Random();
        }

        /// <summary>
        /// Sets the syllable provider of this <see cref="NameGenerator"/> to the specified <see cref="SyllableProvider"/>.
        /// </summary>
        public NameGenerator UsingProvider(Func<SyllableProvider,SyllableProvider> configure)
        {
            this.Provider = configure(new SyllableProvider());
            return this;
        }

        /// <summary>
        /// Sets the syllable provider of this <see cref="NameGenerator"/> to the specified <see cref="ISyllableProvider"/>.
        /// </summary>
        public NameGenerator UsingProvider(ISyllableProvider provider)
        {
            this.Provider = provider ?? throw new ArgumentNullException("The specified ISyllableProvider is null.");
            return this;
        }

        /// <summary>
        /// Sets the name filter of this <see cref="NameGenerator"/> to the specified <see cref="NameFilter"/>.
        /// A vanilla <see cref="NameGenerator"/> does not use filters by default.
        /// </summary>
        public NameGenerator UsingFilter(Func<NameFilter, NameFilter> configure)
        {
            this.Filter = configure(new NameFilter());
            return this;
        }

        /// <summary>
        /// Sets the name filter of this <see cref="NameGenerator"/> to the specified <see cref="INameFilter"/>.
        /// A vanilla <see cref="NameGenerator"/> does not use filters by default.
        /// </summary>
        public NameGenerator UsingFilter(INameFilter filter)
        {
            this.Filter = filter ?? throw new ArgumentNullException("The specified INameFilter is null.");
            return this;
        }

        /// <summary>
        /// Sets the name transformer of this <see cref="NameGenerator"/> to the specified <see cref="NameTransformer"/>.
        /// A vanilla <see cref="NameGenerator"/> does not use transformers by default.
        /// </summary>
        public NameGenerator UsingTransformer(Func<NameTransformer, NameTransformer> configure)
        {
            this.Transformer = configure(new NameTransformer());

            
            if (!this.Transformer.TransformChance.HasValue)
            {
                this.Transformer.TransformChance = DefaultTransformChance;
            }
            

            return this;
        }

        /// <summary>
        /// Sets the name transformer of this <see cref="NameGenerator"/> to the specified <see cref="INameTransformer"/>.
        /// A vanilla <see cref="NameGenerator"/> does not use transformers by default.
        /// </summary>
        public NameGenerator UsingTransformer(INameTransformer transformer)
        {
            this.Transformer = transformer ?? throw new ArgumentNullException("The specified INameTransformer is null.");
            return this;
        }

        /// <summary>
        /// Sets the minimum and maximum syllable count of generated names to the specified value.
        /// (Both minimum and maximum will be set to the same value.)
        /// </summary>
        public NameGenerator UsingSyllableCount(int length)
        {
            return this.UsingSyllableCount(length, length);
        }

        /// <summary>
        /// Sets the minimum and maximum syllable length of generated names.
        /// </summary>
        public NameGenerator UsingSyllableCount(int min, int max)
        {

            if(min < 1)
            {
                throw new ArgumentException("The minimum syllable length must be a positive number.");
            }
            else if (max < min)
            {
                throw new ArgumentException("The maximum syllable length must be equal or greater to the minimum syllable length.");
            }

            this.MinimumSyllables = min;
            this.MaximumSyllables = max;
            return this;
        }

        /// <summary>
        /// <para>
        /// Sets the maximum number of generation retries before an exception is thrown.
        /// Retry limits are useful in detecting <see cref="NameGenerator"/>s that
        /// cannot satisfy their own <see cref="NameFilter"/>.
        /// </para>
        /// <para>
        /// The default retry limit is 1000.
        /// </para>
        /// </summary>
        public NameGenerator LimitRetries(int limit)
        {

            if(limit < 1)
            {
                throw new ArgumentException("The number of maximum attempts to make must be one or greater.");
            }

            this.MaximumRetries = limit;

            return this;
        }



        /// <summary>
        /// <para>
        /// Generates and returns a random name. The name will be consistent with this <see cref="NameGenerator"/>'s syllable provider, name transformer (if it is used), and name filter (if it is used).
        /// </para>
        /// <para>
        /// If you need to access to individual syllables of a name, use <see cref="NextName"/> instead.
        /// </para>
        /// </summary>
        public string Next()
        {
            // We validate the minimum and maximum syllable lengths in this method instead of the property setters because per Microsoft:
            // "...exceptions resulting from invalid state should be postponed until the interraleted properties are actually used together..."
            // (https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/property?redirectedfrom=MSDN)
            if (this.MinimumSyllables < 1 || this.MaximumSyllables < 1 || this.MinimumSyllables > this.MaximumSyllables)
            {
                throw new InvalidOperationException("The value of property MinimumSyllables must be less than or equal to property MaximumSyllables. Both values must be postive integers.");
            }

            var syllableLength = this.Random.Next(this.MinimumSyllables, this.MaximumSyllables + 1);
            return this.Next(syllableLength);
        }

        /// <summary>
        /// <para>
        /// Generates and returns a random name with the specified syllable length. The specified syllable length 
        /// will override the <see cref="NameGenerator"/>'s <see cref="MinimumSyllables"/> and <see cref="MaximumSyllables"/> 
        /// set by <see cref="UsingSyllableCount(int, int)"/>. The name will be consistent with this <see cref="NameGenerator"/>'s 
        /// syllable provider, name transformer (if it is used), and name filter (if it is used).
        /// </para>
        /// <para>
        /// If you need to access to individual syllables of a name, use <see cref="NextName"/> instead.
        /// </para>
        /// </summary>
        public string Next(int syllableLength)
        {
            return this.NextName(syllableLength).ToString();
        }

        /// <summary>
        /// Identical to <see cref="Next()"/> except a <see cref="Name"/> struct is returned instead of a string. 
        /// The struct is useful in obtaining the syllables that make up the name.
        /// </summary>
        public Name NextName()
        {
            if (this.MinimumSyllables < 1 || this.MaximumSyllables < 1 || this.MinimumSyllables > this.MaximumSyllables)
            {
                throw new InvalidOperationException("The value of property MinimumSyllables must be less than or equal to property MaximumSyllables. Both values must be postive integers.");
            }

            var syllableLength = this.Random.Next(this.MinimumSyllables, this.MaximumSyllables + 1);
            return this.NextName(syllableLength);
        }

        /// <summary>
        /// Identical to <see cref="Next(int)"/> except a <see cref="Name"/> struct is returned instead of a string. 
        /// The struct is useful in obtaining the syllables that make up the name.
        /// </summary>
        public Name NextName(int syllableLength)
        {

            if(syllableLength < 1)
            {
                throw new ArgumentException("The desired syllable length must be a positive value.");
            }

            var validNameGenerated = false;
            var totalAttempts = 0;
            var result = new Name();

            while (!validNameGenerated)
            {
                result.Syllables.Clear();

                for (int i = 0; i < syllableLength; i++)
                {
                    if (i == 0 && syllableLength > 1)
                    {
                        //result.Syllables[i] = this.Provider.NextStartingSyllable();
                        result.Syllables.Add(this.Provider.NextStartingSyllable());
                    }
                    else if (i == syllableLength - 1 && syllableLength > 1)
                    {
                        result.Syllables.Add(this.Provider.NextEndingSyllable());
                    }
                    else
                    {
                        result.Syllables.Add(this.Provider.NextSyllable());
                    }
                }

                if (this.Transformer != null 
                    && this.Transformer.TransformChance.HasValue 
                    && this.Random.NextDouble() < this.Transformer.TransformChance)
                {
                    result = this.Transformer.Transform(result);
                }

                validNameGenerated = this.Filter != null ? this.Filter.IsValidName(result) : true;

                if (totalAttempts++ >= this.MaximumRetries && !validNameGenerated)
                {
                    throw new InvalidOperationException("This NameGenerator has run out of attempts generating a valid name. It may be configured in such a way that it cannot generate names that satisfy the specified NameFilter.");
                }
            }

            return result;
        }

    }
}
