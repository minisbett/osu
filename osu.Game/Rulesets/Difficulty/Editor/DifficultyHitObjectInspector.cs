// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osu.Game.Overlays;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Edit;
using System.Reflection;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using Humanizer;

namespace osu.Game.Rulesets.Difficulty.Editor
{
    internal partial class DifficultyHitObjectInspector : EditorToolboxGroup
    {
        [Resolved]
        private DifficultyEditorBeatmap difficultyBeatmap { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private OsuTextFlowContainer evaluatorText = null!;
        private OsuSpriteText selectNoteHint = null!;

        public DifficultyHitObjectInspector() : base("Difficulty Hit Object") { }

        [BackgroundDependencyLoader]
        private void load()
        {
            difficultyBeatmap.DifficultyHitObjects.ValueChanged += _ => update();

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
                    }
                ]
            };
        }

        private void update()
        {
            evaluatorText.Clear();
            selectNoteHint.Show();

            if (difficultyBeatmap.SelectedDifficultyHitObjects.Length != 1)
                return;

            selectNoteHint.Hide();

            DifficultyHitObject diffObject = difficultyBeatmap.SelectedDifficultyHitObjects.Single();

            foreach (PropertyInfo property in diffObject.GetType().GetProperties().Where(x => new[] { typeof(double), typeof(double?) }.Contains(x.PropertyType)))
                addResult(property.Name.Titleize(), (double?)property.GetValue(diffObject));

            foreach (FieldInfo field in diffObject.GetType().GetFields().Where(x => new[] { typeof(double), typeof(double?) }.Contains(x.FieldType)))
                addResult(field.Name.Titleize(), (double?)field.GetValue(diffObject));
        }

        private void addResult(string name, double? value)
        {
            evaluatorText.AddParagraph($"{name}:", s =>
            {
                s.Padding = new MarginPadding { Top = 2 };
                s.Font = s.Font.With(size: 12);
                s.Colour = colourProvider.Content2;
            });

            evaluatorText.AddParagraph(value?.ToString() ?? "null", s =>
            {
                s.Font = s.Font.With(weight: FontWeight.SemiBold);
                s.Colour = colourProvider.Content1;
            });
        }
    }
}
