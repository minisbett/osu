// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Osu.Difficulty.Evaluators
{
    public static class RhythmEvaluator
    {
        private const int maxobjectcount = 20;
        private const int piterations = 8;

        public static double EvaluateDifficultyOf(DifficultyHitObject current)
        {
            double[] deltaTimes = Enumerable.Range(0, maxobjectcount)
                .TakeWhile(x => current.Previous(x - 1) is not null)
                .Select(x => current.Previous(x - 1).DeltaTime).ToArray();

            double entropy = 0;

            foreach (double x in deltaTimes)
            {
                double probability = p(x, deltaTimes);

                entropy += -probability * Math.Log(probability);
            }

            return entropy;
        }

        private static double p(double x, double[] deltaTimes)
        {
            double coef(double x, double i)
                => Enumerable.Range(1, piterations)
                     .Sum(n => Math.Pow(Math.Cos(x / i * n * Math.PI), 2) / biggestPrimeFactor[n])
                     / Enumerable.Range(1, piterations).Sum(x => 1d / biggestPrimeFactor[x]);

            double probability = 0;

            foreach (double i in deltaTimes)
                probability += coef(x, i);

            return probability / deltaTimes.Length;
        }


        private static int[] biggestPrimeFactor = new int[]
        {
            0,
            1,
            2,
            3,
            2,
            5,
            3,
            7,
            2
        };
    }
}
