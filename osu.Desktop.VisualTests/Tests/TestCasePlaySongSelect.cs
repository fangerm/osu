// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System; 
using System.Collections.Generic;
using osu.Desktop.VisualTests.Platform;
using osu.Framework.Screens.Testing;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Select;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCasePlaySongSelect : TestCase
    {
        private BeatmapDatabase db, oldDb;
        private TestStorage storage;
        private Random rnd = new Random();
        private PlaySongSelect SongSelect;

        public override string Name => @"Song Select";
        public override string Description => @"with fake data";

        public Action OnArtist;
        public Action OnTitle;
        public Action OnAuthor;
        public Action OnDifficulty;

        public override void Reset()
        {
            base.Reset();
            oldDb = Dependencies.Get<BeatmapDatabase>();
            if (db == null)
            {
                storage = new TestStorage(@"TestCasePlaySongSelect");
                db = new BeatmapDatabase(storage);
                Dependencies.Cache(db, true);

                var sets = new List<BeatmapSetInfo>();

                for (int i = 0; i < 100; i += 10)
                    sets.Add(createTestBeatmapSet(i));

                db.Import(sets);
            }

            Add(SongSelect = new PlaySongSelect());
            OnArtist = () => { SongSelect.Filter.Sort = FilterControl.SortMode.Artist; };
            OnTitle = () => { SongSelect.Filter.Sort = FilterControl.SortMode.Title; };
            OnAuthor = () => { SongSelect.Filter.Sort = FilterControl.SortMode.Author; };
            OnDifficulty = () => { SongSelect.Filter.Sort = FilterControl.SortMode.Difficulty; };

            AddButton(@"Sort by Artist", OnArtist);
            AddButton(@"Sort by Title", OnTitle);
            AddButton(@"Sort by Author", OnAuthor);
            AddButton(@"Sort by Difficulty", OnDifficulty);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (oldDb != null)
            {
                Dependencies.Cache(oldDb, true);
                db = null;
            }

            base.Dispose(isDisposing);
        }

        private BeatmapSetInfo createTestBeatmapSet(int i)
        {
            return new BeatmapSetInfo
            {
                OnlineBeatmapSetID = 1234 + i,
                Hash = "d8e8fca2dc0f896fd7cb4cb0031ba249",
                Path = string.Empty,
                Metadata = new BeatmapMetadata
                {
                    OnlineBeatmapSetID = 1234 + i,
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "MONACA " + rnd.Next(0, 9),
                    Title = "Black Song " + rnd.Next(0, 9),
                    Author = "Some Guy " + rnd.Next(0, 9),
                },
                Beatmaps = new List<BeatmapInfo>(new[]
                {
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1234 + i,
                        Mode = PlayMode.Osu,
                        Path = "normal.osu",
                        Version = "Normal",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 3.5f,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1235 + i,
                        Mode = PlayMode.Osu,
                        Path = "hard.osu",
                        Version = "Hard",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 5,
                        }
                    },
                    new BeatmapInfo
                    {
                        OnlineBeatmapID = 1236 + i,
                        Mode = PlayMode.Osu,
                        Path = "insane.osu",
                        Version = "Insane",
                        BaseDifficulty = new BaseDifficulty
                        {
                            OverallDifficulty = 7,
                        }
                    },
                }),
            };
        }
    }
}
