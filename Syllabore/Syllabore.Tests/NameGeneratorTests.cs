using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Syllabore.Tests
{
    [TestClass]
    public class NameGeneratorTests
    {
        [TestMethod]
        public void Constructor_WhenNoParameter_SuccessfulNameGeneration()
        {
            var generator = new NameGenerator();
            for(int i = 0; i < 100; i++)
            {
                Assert.IsTrue(generator.Next() != string.Empty);
            }
        }

        [TestMethod]
        public void Constructor_WhenBasicConstructorUsed_SuccessfulNameGeneration()
        {
            var generator = new NameGenerator("a","strl");
            for (int i = 0; i < 100; i++)
            {
                Assert.IsTrue(generator.Next().Contains("a"));
            }
        }


        [TestMethod]
        public void Constructor_WhenAnyParameterNull_ArgumentNullExceptionThrown()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new NameGenerator(null, null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new NameGenerator(null, new TransformSet(), null));
            Assert.ThrowsException<ArgumentNullException>(() => new NameGenerator(null, null, new NameFilter()));
            Assert.ThrowsException<ArgumentNullException>(() => new NameGenerator(null, new TransformSet(), new NameFilter()));

            Assert.IsNotNull(new NameGenerator(new DefaultSyllableGenerator(), null, null).Next());
            Assert.IsNotNull(new NameGenerator(new DefaultSyllableGenerator(), new TransformSet(), null).Next());
            Assert.IsNotNull(new NameGenerator(new DefaultSyllableGenerator(), new TransformSet(), new NameFilter()).Next());
        }

        [TestMethod]
        public void NameGeneration_WhenSyllableLengthPropertyValuesInvalid_InvalidOperationExceptionThrown()
        {
            var generator = new NameGenerator();

            // Single argument
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(-1).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(0).Next());
            Assert.IsNotNull(generator.UsingSyllableCount(1).Next());

            // Double argument
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(-1, 1).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(0, 1).Next());
            Assert.IsNotNull(generator.UsingSyllableCount(1, 1).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(1, -1).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(1, 0).Next());
            Assert.IsNotNull(generator.UsingSyllableCount(4, 5).Next());
            Assert.IsNotNull(generator.UsingSyllableCount(5, 5).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.UsingSyllableCount(6, 5).Next());

        }

        [TestMethod]
        public void NameGeneration_WhenNonPositiveSyllableLengthProvided_ArgumentExceptionThrown()
        {
            var generator = new NameGenerator();

            Assert.IsNotNull(generator.Next(1));
            Assert.ThrowsException<ArgumentException>(() => generator.Next(0));
            Assert.ThrowsException<ArgumentException>(() => generator.Next(-1));
            Assert.ThrowsException<ArgumentException>(() => generator.Next(int.MinValue));
        }

        [TestMethod]
        public void NameGeneration_WhenInfiniteGeneration_ExceptionThrown()
        {
            var generator = new NameGenerator()
                .UsingFilter(x => x
                    .DoNotAllowPattern(".")) // Set filter to reject names with at least 1 character
                    .UsingSyllableCount(10)  // Ensure the generator only produces names with at least 1 character
                    .LimitRetries(1000);  // All futile attempts

            Assert.ThrowsException<InvalidOperationException>(() => generator.Next());

        }

        [TestMethod]
        public void NameGeneration_WhenMaximumRetriesLessThanOne_ExceptionThrown()
        {
            var generator = new NameGenerator();

            Assert.ThrowsException<ArgumentException>(() => generator.LimitRetries(-1).Next());
            Assert.ThrowsException<ArgumentException>(() => generator.LimitRetries(0).Next());
            Assert.IsNotNull(generator.LimitRetries(1).Next());

        }

        [TestMethod]
        public void NameGeneration_WithSequencesOnly_Allowed()
        {
            // It is valid for a name generator to use a provider that only uses sequences
            var generator = new NameGenerator()
                .UsingSyllables(x => x
                    .WithLeadingConsonantSequences("sr")
                    .WithVowelSequences("ea")
                    .WithTrailingConsonantSequences("bz")
                    .WithProbability(x => x
                        .OfVowels(1.0)
                        .OfVowelIsSequence(1.0)
                        .OfLeadingVowelsInStartingSyllable(0.0)))
                //.WithProbability(vowelBecomesVowelSequence: 1.0)
                //.DisallowStartingSyllableLeadingVowels()
                //.DisallowLeadingVowelsInStartingSyllables())
                .UsingFilter(x => x.DoNotAllowPattern("^.{,2}$"));// Invalidate names with less than 2 characters

            try
            {
                for (int i = 0; i < 10000; i++)
                {
                    var name = generator.Next();
                    Assert.IsTrue(name.Length > 2);
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

    }
}
