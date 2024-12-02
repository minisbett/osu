// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Difficulty.Evaluators;
using osu.Game.Graphics.Containers;
using osuTK;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osu.Game.Graphics;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Difficulty
{
    internal partial class DifficultyEvaluatorInspector : EditorToolboxGroup
    {
        [Resolved]
        private Bindable<RulesetInfo> rulesetInfo { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; } = null!;

        private OsuTextFlowContainer evaluatorText = null!;

        private OsuSpriteText selectNoteHint = null!;
        private SliderWithTextBoxInput<double> clockRateSlider = null!;
        private RoundedButton openModsOverlay = null!;
        private UserModSelectOverlay modSelectOverlay = null!;
        private Bindable<IReadOnlyList<Mod>> appliedMods = new Bindable<IReadOnlyList<Mod>>([]);
        private DifficultyCalculator diffCalc = null!;

        public DifficultyEvaluatorInspector() : base("Evaluators") { }

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
                    selectNoteHint = new OsuSpriteText
                    {
                        Text = "Please select a note.",
                        RelativeSizeAxes = Axes.X
                    },
                    evaluatorText = new OsuTextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    },
                    clockRateSlider = new SliderWithTextBoxInput<double>("Clock Rate")
                    {
                        Current = new BindableNumber<double>(1)
                        {
                            MinValue = 0.01,
                            MaxValue = 2.00,
                            Precision = 0.01
                        }
                    },
                    openModsOverlay = new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Margin = new MarginPadding { Top = 4.0f, Right = 5.0f },
                        BackgroundColour = colourProvider.Background1,
                        Text = "Mods",
                        Action = () => modSelectOverlay.Show()
                    },
                    modSelectOverlay = new UserModSelectOverlay
                    {
                        SelectedMods = { BindTarget = appliedMods }
                    }
                ]
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            diffCalc = rulesetInfo.Value.CreateInstance().CreateDifficultyCalculator(new FlatWorkingBeatmap(EditorBeatmap.PlayableBeatmap));

            EditorBeatmap.SelectedHitObjects.CollectionChanged += (_, _) => update();
            EditorBeatmap.PlacementObject.BindValueChanged(_ => update());
            EditorBeatmap.TransactionBegan += update;
            EditorBeatmap.TransactionEnded += update;
            clockRateSlider.Current.BindValueChanged(_ => update());
            appliedMods.BindValueChanged(_ => update());
            update();
        }

        private void update()
        {
            evaluatorText.Clear();
            clockRateSlider.Hide();
            openModsOverlay.Hide();
            selectNoteHint.Show();

            if (EditorBeatmap.SelectedHitObjects.Count != 1)
                return;

            clockRateSlider.Show();
            //openModsOverlay.Show();
            selectNoteHint.Hide();

            DifficultyHitObject? diffObject = diffCalc.CreateDifficultyHitObjects(EditorBeatmap.PlayableBeatmap, clockRateSlider.Current.Value)
                                                      .FirstOrDefault(x => x.BaseObject == EditorBeatmap.SelectedHitObjects[0]);

            if (diffObject == null)
                return;

            addResult("Aim (withSliderTravelDistance = false)", AimEvaluator.EvaluateDifficultyOf(diffObject, false));
            addResult("Aim (withSliderTravelDistance = true)", AimEvaluator.EvaluateDifficultyOf(diffObject, true));
            addResult("Speed", SpeedEvaluator.EvaluateDifficultyOf(diffObject));
            addResult("Rhythm", RhythmEvaluator.EvaluateDifficultyOf(diffObject));
            addResult("Flashlight (hidden = false)", FlashlightEvaluator.EvaluateDifficultyOf(diffObject, false));
            addResult("Flashlight (hidden = true)", FlashlightEvaluator.EvaluateDifficultyOf(diffObject, true));
        }

        private void addResult(string name, double value)
        {
            evaluatorText.AddParagraph($"{name}:", s =>
            {
                s.Padding = new MarginPadding { Top = 2 };
                s.Font = s.Font.With(size: 12);
                s.Colour = colourProvider.Content2;
            });

            evaluatorText.AddParagraph(value.ToString(), s =>
            {
                s.Font = s.Font.With(weight: FontWeight.SemiBold);
                s.Colour = colourProvider.Content1;
            });
        }
    }
}
