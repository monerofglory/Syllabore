﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Syllabore
{
    /// <summary>
    /// This transformer creates variations of names by replacing one syllable
    /// with another syllable. Syllables are derived from <see cref="DefaultSyllableGenerator"/>.
    /// </summary>
    [Obsolete("No longer used", false)]
    public class DefaultNameTransformer : NameTransformer
    {
        private ISyllableGenerator Provider { get; set; }

        private Random Random { get; set; }

        [Obsolete("No longer used", false)]
        public DefaultNameTransformer()
        {
            this.Provider = new DefaultSyllableGenerator();
            this.Random = new Random();

            this.WithTransform(x => x
                .ExecuteUnserializableAction(name => {
                    int index = this.Random.Next(name.Syllables.Count);

                    if (index == 0)
                    {
                        name.Syllables[index] = this.Provider.NextStartingSyllable();
                    }
                    else if (index == name.Syllables.Count - 1)
                    {
                        name.Syllables[index] = this.Provider.NextEndingSyllable();
                    }
                    else
                    {
                        name.Syllables[index] = this.Provider.NextSyllable();
                    }
            }));

        }

    }
}
