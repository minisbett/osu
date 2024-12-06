// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

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

        private DifficultyCalculator diffCalc = null!;
        private DifficultyHitObject[] difficultyHitObjects = [];
        private TimedDifficultyAttributes[] timedDifficultyAttributes = [];

        /// <summary>
        /// The difficulty hit objects corresponding to the selected hit objects. Hit objects without an equivalent are ignored.
        /// </summary>
        private DifficultyHitObject[] selectedDifficultyHitObjects
            => difficultyHitObjects.Where(x => editorBeatmap.SelectedHitObjects.Contains(x.BaseObject)).ToArray();

        /// <summary>
        /// Returns the "current" object, which is either the only currently selected object or, if none or multiple are selected,
        /// the object that lasted started from the current cursor position in the editor timeline.
        /// If there is no applicable difficulty hit object for the current time, null is returned instead.
        /// </summary>
        public DifficultyHitObject? CurrentObject
        {
            get
            {
                if (selectedDifficultyHitObjects.Length == 1)
                    return selectedDifficultyHitObjects[0];

                return difficultyHitObjects.LastOrDefault(x => x.StartTime < editorClock.CurrentTime);
            }
        }

        /// <summary>
        /// Returns the timed difficulty attributes at the time of the selected hit object, or the current cursor position in the editor timeline.
        /// This differs from retrieving the difficulty attributes at the time of <see cref="CurrentObject"/>, as the timed difficulty attributes
        /// only apply at the endtime of an object. Thus, if the current object is for example a slider, it would give the next ones too early.
        /// </summary>
        public TimedDifficultyAttributes? CurrentDifficultyAttributes
        {
            get
            {
                if (selectedDifficultyHitObjects.Length == 1)
                    return timedDifficultyAttributes.FirstOrDefault(x => x.Time == selectedDifficultyHitObjects[0].EndTime);

                return timedDifficultyAttributes.LastOrDefault(x => x.Time < editorClock.CurrentTime);
            }
        }

        [BackgroundDependencyLoader]
        private void load(Bindable<RulesetInfo> rulesetInfo)
        {
            Ruleset ruleset = rulesetInfo.Value.CreateInstance();
            diffCalc = ruleset.CreateDifficultyCalculator(new FlatWorkingBeatmap(editorBeatmap.PlayableBeatmap));

            Scheduler.AddDelayed(updateDifficultyHitObjects, 20, true);
            Scheduler.AddDelayed(updateDifficultyAttributes, 1000, true);
        }

        private void updateDifficultyHitObjects()
        {
            difficultyHitObjects = diffCalc.CreateDifficultyHitObjects(editorBeatmap.PlayableBeatmap, 1).ToArray();
        }

        private void updateDifficultyAttributes()
        {
            timedDifficultyAttributes = diffCalc.CalculateTimed().ToArray();
        }

        /// <summary>
        /// Returns the corresponding difficulty hit object for the specified hit object. If the hit object has no corresponding object, null is returned.
        /// </summary>
        /// <param name="obj">The hit object.</param>
        /// <returns>The corresponding difficulty hit object.</returns>
        public DifficultyHitObject? FromBaseObject(HitObject obj) => difficultyHitObjects.FirstOrDefault(x => x.BaseObject == obj);
    }
}
