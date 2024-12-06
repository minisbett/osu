// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Graphics.Containers;
using osuTK;
using osu.Game.Overlays;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using System;
using osu.Game.Rulesets.Difficulty.Editor;

namespace osu.Game.Rulesets.Osu.Difficulty.Editor
{
    internal partial class OsuDifficultyEvaluatorInspector : EditorToolboxGroup
    {
        [Resolved]
        private DifficultyEditorBeatmap difficultyBeatmap { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private OsuTextFlowContainer text = null!;

        public OsuDifficultyEvaluatorInspector() : base("Evaluators", true) { }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children =
                [
                    text = new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                ]
            };

            Scheduler.AddDelayed(update, 20, true);
        }

        private void update()
        {
            text.Clear();

            if (difficultyBeatmap.CurrentObject is null)
                return;

            addResult("Aim (withSliderTravelDistance = false)", AimEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject, false));
            addResult("Aim (withSliderTravelDistance = true)", AimEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject, true));
            addResult("Speed", SpeedEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject));
            addResult("Rhythm", RhythmEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject));
            addResult("Flashlight (hidden = false)", FlashlightEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject, false));
            addResult("Flashlight (hidden = true)", FlashlightEvaluator.EvaluateDifficultyOf(difficultyBeatmap.CurrentObject, true));
        }

        private void addResult(string name, double value, int decimals = 5)
        {
            value = Math.Round(value, decimals);

            text.AddParagraph($"{name}:", s =>
            {
                s.Padding = new MarginPadding { Top = 2 };
                s.Font = s.Font.With(size: 12);
                s.Colour = colourProvider.Content2;
            });

            text.AddParagraph(value.ToString(), s =>
            {
                s.Font = s.Font.With(weight: FontWeight.SemiBold);
                s.Colour = colourProvider.Content1;
            });
        }
    }
}
