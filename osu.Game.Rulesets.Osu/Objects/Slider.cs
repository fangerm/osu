﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Slider : OsuHitObject, IHasCurve
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        public readonly SliderCurve Curve = new SliderCurve();

        public double EndTime => StartTime + RepeatCount * Curve.Distance / Velocity;
        public double Duration => EndTime - StartTime;

        public override Vector2 EndPosition => PositionAt(1);

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        /// <summary>
        /// The position of the cursor at the point of completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal Vector2? LazyEndPosition;

        /// <summary>
        /// The distance travelled by the cursor upon completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal float LazyTravelDistance;

        public List<List<SampleInfo>> RepeatSamples { get; set; } = new List<List<SampleInfo>>();
        public int RepeatCount { get; set; } = 1;

        private int stackHeight;
        public override int StackHeight
        {
            get { return stackHeight; }
            set
            {
                stackHeight = value;
                Curve.Offset = StackOffset;
            }
        }

        public double Velocity;
        public double TickDistance;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate;
        }

        public Vector2 PositionAt(double progress) => Curve.PositionAt(ProgressAt(progress));

        public double ProgressAt(double progress)
        {
            double p = progress * RepeatCount % 1;
            if (RepeatAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        public int RepeatAt(double progress) => (int)(progress * RepeatCount);

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();

            createTicks();
            createRepeatPoints();
        }

        private void createTicks()
        {
            if (TickDistance == 0) return;

            var length = Curve.Distance;
            var tickDistance = Math.Min(TickDistance, length);
            var repeatDuration = length / Velocity;

            var minDistanceFromEnd = Velocity * 0.01;

            for (var repeat = 0; repeat < RepeatCount; repeat++)
            {
                var repeatStartTime = StartTime + repeat * repeatDuration;
                var reversed = repeat % 2 == 1;

                for (var d = tickDistance; d <= length; d += tickDistance)
                {
                    if (d > length - minDistanceFromEnd)
                        break;

                    var distanceProgress = d / length;
                    var timeProgress = reversed ? 1 - distanceProgress : distanceProgress;

                    AddNested(new SliderTick
                    {
                        RepeatIndex = repeat,
                        StartTime = repeatStartTime + timeProgress * repeatDuration,
                        Position = Curve.PositionAt(distanceProgress),
                        StackHeight = StackHeight,
                        Scale = Scale,
                        ComboColour = ComboColour,
                        Samples = new List<SampleInfo>(Samples.Select(s => new SampleInfo
                        {
                            Bank = s.Bank,
                            Name = @"slidertick",
                            Volume = s.Volume
                        }))
                    });
                }
            }
        }

        private void createRepeatPoints()
        {
            var repeatDuration = Distance / Velocity;

            for (var repeat = 1; repeat < RepeatCount; repeat++)
            {
                var repeatStartTime = StartTime + repeat * repeatDuration;

                AddNested(new RepeatPoint
                {
                    RepeatIndex = repeat,
                    StartTime = repeatStartTime,
                    Position = Curve.PositionAt(repeat % 2),
                    StackHeight = StackHeight,
                    Scale = Scale,
                    ComboColour = ComboColour,
                    Samples = new List<SampleInfo>(RepeatSamples[repeat])
                });
            }
        }
    }
}
