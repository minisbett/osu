// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty.Preprocessing
{
    public class DifficultyHitObject;

    /// <summary>
    /// Wraps a <see cref="HitObject"/> and provides additional information to be used for difficulty calculation.
    /// </summary>
    public class DifficultyHitObject<TDifficultyHitObject, THitObject> : DifficultyHitObject where TDifficultyHitObject : DifficultyHitObject<TDifficultyHitObject, THitObject> where THitObject : HitObject
    {
        private readonly IReadOnlyList<TDifficultyHitObject> difficultyHitObjects;

        /// <summary>
        /// The index of this <see cref="DifficultyHitObject"/> in the list of all <see cref="DifficultyHitObject"/>s.
        /// </summary>
        public int Index;

        /// <summary>
        /// The <see cref="HitObject"/> this <see cref="DifficultyHitObject"/> wraps.
        /// </summary>
        public readonly THitObject BaseObject;

        /// <summary>
        /// The last <see cref="HitObject"/> which occurs before <see cref="BaseObject"/>.
        /// </summary>
        public readonly THitObject LastObject;

        /// <summary>
        /// Amount of time elapsed between <see cref="BaseObject"/> and <see cref="LastObject"/>, adjusted by clockrate.
        /// </summary>
        public readonly double DeltaTime;

        /// <summary>
        /// Clockrate adjusted start time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double StartTime;

        /// <summary>
        /// Clockrate adjusted end time of <see cref="BaseObject"/>.
        /// </summary>
        public readonly double EndTime;

        /// <summary>
        /// Creates a new <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which this <see cref="DifficultyHitObject"/> wraps.</param>
        /// <param name="lastObject">The last <see cref="HitObject"/> which occurs before <paramref name="hitObject"/> in the beatmap.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <param name="objects">The list of <see cref="DifficultyHitObject"/>s in the current beatmap.</param>
        /// <param name="index">The index of this <see cref="DifficultyHitObject"/> in <paramref name="objects"/> list.</param>
        public DifficultyHitObject(THitObject hitObject, THitObject lastObject, double clockRate, List<TDifficultyHitObject> objects, int index)
        {
            difficultyHitObjects = objects;
            Index = index;
            BaseObject = hitObject;
            LastObject = lastObject;
            DeltaTime = (hitObject.StartTime - lastObject.StartTime) / clockRate;
            StartTime = hitObject.StartTime / clockRate;
            EndTime = hitObject.GetEndTime() / clockRate;
        }

        public TDifficultyHitObject Previous(int backwardsIndex)
        {
            int index = Index - (backwardsIndex + 1);
            return index >= 0 && index < difficultyHitObjects.Count ? difficultyHitObjects[index] : null;
        }

        public TDifficultyHitObject Next(int forwardsIndex)
        {
            int index = Index + (forwardsIndex + 1);
            return index >= 0 && index < difficultyHitObjects.Count ? difficultyHitObjects[index] : null;
        }
    }
}
