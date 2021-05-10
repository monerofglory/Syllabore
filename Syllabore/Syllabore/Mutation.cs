﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Syllabore
{
    /// <summary>
    /// Used by NameGenerator through ConfigurableNameMutator to capture
    /// mutations to produce variations on names. This class also has
    /// an optional condition that must be fulfilled for the mutation
    /// to occur.
    /// </summary>
    public class Mutation
    {
        public List<MutationStep> Steps { get; set; }

        public int? ConditionalIndex { get; set; }
        public string ConditionalRegex { get; set; }

        public Mutation()
        {
            this.Steps = new List<MutationStep>();
            this.ConditionalIndex = null;
            this.ConditionalRegex = null;
        }
        public void Apply(Name name)
        {
            foreach (var step in this.Steps)
            {
                step.Apply(name);
            }
        }

        public Mutation When(string regex)
        {
            return this.When(int.MinValue, regex);
        }

        public Mutation When(int index, string regex)
        {
            if (index == int.MaxValue)
            {
                this.ConditionalIndex = null;
            }
            else
            {
                this.ConditionalIndex = index;
            }

            this.ConditionalRegex = regex;

            return this;
        }

        public Mutation ReplaceSyllable(int index, string replacement)
        {
            this.Steps.Add(new MutationStep(MutationStepType.SyllableReplacement, index.ToString(), replacement));
            return this;
        }

        public Mutation InsertSyllable(int index, string syllable)
        {
            this.Steps.Add(new MutationStep(MutationStepType.SyllableInsertion, index.ToString(), syllable));
            return this;
        }

        public Mutation AppendSyllable(string syllable)
        {
            this.Steps.Add(new MutationStep(MutationStepType.SyllableAppend, syllable));
            return this;
        }

        public Mutation RemoveSyllable(int index)
        {
            this.Steps.Add(new MutationStep(MutationStepType.SyllableRemoval, index.ToString()));
            return this;
        }

        
        public Mutation ExecuteUnserializableAction(Action<Name> unserializableAction)
        {
            this.Steps.Add(new MutationStep(unserializableAction));
            return this;
        }
        
    }
}
