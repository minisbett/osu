﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuGridToolboxGroup : EditorToolboxGroup, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IExpandingContainer? expandingContainer { get; set; }

        /// <summary>
        /// X position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionX { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.X / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.X,
            Precision = 1f
        };

        /// <summary>
        /// Y position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionY { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.Y / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.Y,
            Precision = 1f
        };

        /// <summary>
        /// The spacing between grid lines.
        /// </summary>
        public BindableFloat Spacing { get; } = new BindableFloat(4f)
        {
            MinValue = 4f,
            MaxValue = 128f,
            Precision = 1f
        };

        /// <summary>
        /// Rotation of the grid lines in degrees.
        /// </summary>
        public BindableFloat GridLinesRotation { get; } = new BindableFloat(0f)
        {
            MinValue = -180f,
            MaxValue = 180f,
            Precision = 1f
        };

        /// <summary>
        /// Read-only bindable representing the grid's origin.
        /// Equivalent to <code>new Vector2(StartPositionX, StartPositionY)</code>
        /// </summary>
        public Bindable<Vector2> StartPosition { get; } = new Bindable<Vector2>();

        /// <summary>
        /// Read-only bindable representing the grid's spacing in both the X and Y dimension.
        /// Equivalent to <code>new Vector2(Spacing)</code>
        /// </summary>
        public Bindable<Vector2> SpacingVector { get; } = new Bindable<Vector2>();

        public Bindable<PositionSnapGridType> GridType { get; } = new Bindable<PositionSnapGridType>();

        private ExpandableSlider<float> startPositionXSlider = null!;
        private ExpandableSlider<float> startPositionYSlider = null!;
        private ExpandableSlider<float> spacingSlider = null!;
        private ExpandableSlider<float> gridLinesRotationSlider = null!;
        private EditorRadioButtonCollection gridTypeButtons = null!;

        private ExpandableButton useSelectedObjectPositionButton = null!;

        public OsuGridToolboxGroup()
            : base("grid")
        {
        }

        private const float max_automatic_spacing = 64;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                startPositionXSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionX,
                    KeyboardStep = 1,
                },
                startPositionYSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionY,
                    KeyboardStep = 1,
                },
                useSelectedObjectPositionButton = new ExpandableButton
                {
                    ExpandedLabelText = "Centre on selected object",
                    Action = () =>
                    {
                        if (editorBeatmap.SelectedHitObjects.Count != 1)
                            return;

                        var position = ((IHasPosition)editorBeatmap.SelectedHitObjects.Single()).Position;
                        StartPosition.Value = new Vector2(MathF.Round(position.X), MathF.Round(position.Y));
                        updateEnabledStates();
                    },
                    RelativeSizeAxes = Axes.X,
                },
                spacingSlider = new ExpandableSlider<float>
                {
                    Current = Spacing,
                    KeyboardStep = 1,
                },
                gridLinesRotationSlider = new ExpandableSlider<float>
                {
                    Current = GridLinesRotation,
                    KeyboardStep = 1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        gridTypeButtons = new EditorRadioButtonCollection
                        {
                            RelativeSizeAxes = Axes.X,
                            Items = new[]
                            {
                                new RadioButton("Square",
                                    () => GridType.Value = PositionSnapGridType.Square,
                                    () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                                new RadioButton("Triangle",
                                    () => GridType.Value = PositionSnapGridType.Triangle,
                                    () => new OutlineTriangle(true, 20)),
                                new RadioButton("Circle",
                                    () => GridType.Value = PositionSnapGridType.Circle,
                                    () => new SpriteIcon { Icon = FontAwesome.Regular.Circle }),
                            }
                        },
                    }
                },
            };

            Spacing.Value = editorBeatmap.BeatmapInfo.GridSize;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gridTypeButtons.Items.First().Select();

            StartPositionX.BindValueChanged(x =>
            {
                startPositionXSlider.ContractedLabelText = $"X: {x.NewValue:N0}";
                startPositionXSlider.ExpandedLabelText = $"X Offset: {x.NewValue:N0}";
                StartPosition.Value = new Vector2(x.NewValue, StartPosition.Value.Y);
            }, true);

            StartPositionY.BindValueChanged(y =>
            {
                startPositionYSlider.ContractedLabelText = $"Y: {y.NewValue:N0}";
                startPositionYSlider.ExpandedLabelText = $"Y Offset: {y.NewValue:N0}";
                StartPosition.Value = new Vector2(StartPosition.Value.X, y.NewValue);
            }, true);

            StartPosition.BindValueChanged(pos =>
            {
                StartPositionX.Value = pos.NewValue.X;
                StartPositionY.Value = pos.NewValue.Y;
                updateEnabledStates();
            });

            Spacing.BindValueChanged(spacing =>
            {
                spacingSlider.ContractedLabelText = $"S: {spacing.NewValue:N0}";
                spacingSlider.ExpandedLabelText = $"Spacing: {spacing.NewValue:N0}";
                SpacingVector.Value = new Vector2(spacing.NewValue);
                editorBeatmap.BeatmapInfo.GridSize = (int)spacing.NewValue;
            }, true);

            GridLinesRotation.BindValueChanged(rotation =>
            {
                gridLinesRotationSlider.ContractedLabelText = $"R: {rotation.NewValue:#,0.##}";
                gridLinesRotationSlider.ExpandedLabelText = $"Rotation: {rotation.NewValue:#,0.##}";
            }, true);

            GridType.BindValueChanged(v =>
            {
                GridLinesRotation.Disabled = v.NewValue == PositionSnapGridType.Circle;

                switch (v.NewValue)
                {
                    case PositionSnapGridType.Square:
                        GridLinesRotation.Value = ((GridLinesRotation.Value + 405) % 90) - 45;
                        GridLinesRotation.MinValue = -45;
                        GridLinesRotation.MaxValue = 45;
                        break;

                    case PositionSnapGridType.Triangle:
                        GridLinesRotation.Value = ((GridLinesRotation.Value + 390) % 60) - 30;
                        GridLinesRotation.MinValue = -30;
                        GridLinesRotation.MaxValue = 30;
                        break;
                }
            }, true);

            editorBeatmap.BeatmapReprocessed += updateEnabledStates;
            editorBeatmap.SelectedHitObjects.BindCollectionChanged((_, _) => updateEnabledStates());
            expandingContainer?.Expanded.BindValueChanged(v =>
            {
                gridTypeButtons.FadeTo(v.NewValue ? 1f : 0f, 500, Easing.OutQuint);
                gridTypeButtons.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
                updateEnabledStates();
            }, true);
        }

        private void updateEnabledStates()
        {
            useSelectedObjectPositionButton.Enabled.Value = expandingContainer?.Expanded.Value == true
                                                            && editorBeatmap.SelectedHitObjects.Count == 1
                                                            && !Precision.AlmostEquals(StartPosition.Value, ((IHasPosition)editorBeatmap.SelectedHitObjects.Single()).Position, 0.5f);
        }

        private void nextGridSize()
        {
            Spacing.Value = Spacing.Value * 2 >= max_automatic_spacing ? Spacing.Value / 8 : Spacing.Value * 2;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorCycleGridDisplayMode:
                    nextGridSize();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public partial class OutlineTriangle : BufferedContainer
        {
            public OutlineTriangle(bool outlineOnly, float size)
                : base(cachedFrameBuffer: true)
            {
                Size = new Vector2(size);

                InternalChildren = new Drawable[]
                {
                    new EquilateralTriangle { RelativeSizeAxes = Axes.Both },
                };

                if (outlineOnly)
                {
                    AddInternal(new EquilateralTriangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Y,
                        Y = 0.48f,
                        Colour = Color4.Black,
                        Size = new Vector2(size - 7),
                        Blending = BlendingParameters.None,
                    });
                }

                Blending = BlendingParameters.Additive;
            }
        }
    }

    public enum PositionSnapGridType
    {
        Square,
        Triangle,
        Circle,
    }
}
