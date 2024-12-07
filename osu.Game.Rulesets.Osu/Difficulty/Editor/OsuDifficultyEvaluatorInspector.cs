﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Editor;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Osu.Difficulty.Editor
{
    internal partial class OsuDifficultyEvaluatorInspector : DifficultyEvaluatorInspector
    {
        protected override Evaluator[] Evaluators => [
            new("Aim (withSliderTravelDistance = false)", obj => AimEvaluator.EvaluateDifficultyOf(obj, false)),
            new("Aim (withSliderTravelDistance = true)", obj => AimEvaluator.EvaluateDifficultyOf(obj, true)),
            new("Speed", SpeedEvaluator.EvaluateDifficultyOf),
            new("Rhythm", RhythmEvaluator.EvaluateDifficultyOf),
            new("Flashlight (hidden = false)", obj => FlashlightEvaluator.EvaluateDifficultyOf(obj, false)),
            new("Flashlight (hidden = true)", obj => FlashlightEvaluator.EvaluateDifficultyOf(obj, true)),
        ];
    }
}
