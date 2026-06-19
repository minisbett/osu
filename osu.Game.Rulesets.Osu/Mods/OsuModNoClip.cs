// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

 using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoClip : Mod, IApplicableToDrawableHitObject
    {
        public override string Name => "No Clip";

        public override LocalisableString Description => "Replaces the tapping accuracy system with an aim accuracy system.";

        public override double ScoreMultiplier => 1;

        public override string Acronym => "NO";

        [SettingSource("Tolerance", "The size extension for the hitboxes of hit objects.")]
        public BindableInt Tolerance { get; } = new BindableInt { MinValue = 0, Value = 100, MaxValue = 500 };

        public override ModType Type => ModType.Automation;

        public static int StaticTolerance => 0;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            switch (drawable)
            {
                case DrawableHitCircle circle:
                    circle.HitArea.Size += new Vector2(Tolerance.Value);
                    break;

                case DrawableSlider slider:
                    slider.SliderInputManager.AdditionalFollowRadiusTolerance = Tolerance.Value;
                    break;
            }
        }
    }
}
