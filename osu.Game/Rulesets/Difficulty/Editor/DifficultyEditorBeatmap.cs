// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;



// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Difficulty.Editor
{
    public partial class DifficultyEditorBeatmap : Component
    {
        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public DifficultyCalculator DifficultyCalculator { get; private set; } = null!;

        public Bindable<DifficultyHitObject[]> DifficultyHitObjects { get; } = new Bindable<DifficultyHitObject[]>([]);

        public DifficultyHitObject[] SelectedDifficultyHitObjects
            => DifficultyHitObjects.Value.Where(x => editorBeatmap.SelectedHitObjects.Contains(x.BaseObject)).ToArray();

        /// <summary>
        /// Returns the "current" object, which is either the only currently selected object or, if none or multiple are selected,
        /// the object that lasted started from the current cursor position in the editor timeline.
        /// </summary>
        public DifficultyHitObject? CurrentObject => SelectedDifficultyHitObjects.Length == 1
                                                   ? SelectedDifficultyHitObjects[0]
                                                   : DifficultyHitObjects.Value.LastOrDefault(x => x.StartTime < editorClock.CurrentTime);

        [BackgroundDependencyLoader]
        private void load(Bindable<RulesetInfo> rulesetInfo)
        {
            DifficultyCalculator = rulesetInfo.Value.CreateInstance().CreateDifficultyCalculator(new FlatWorkingBeatmap(editorBeatmap));

            Scheduler.AddDelayed(() =>
            {
                DifficultyHitObjects.Value = DifficultyCalculator.CreateDifficultyHitObjects(editorBeatmap.PlayableBeatmap, 1).ToArray();
            }, 20, true);
        }

        public DifficultyHitObject? FromBaseObject(HitObject obj) => DifficultyHitObjects.Value.FirstOrDefault(x => x.BaseObject == obj);
    }
}
