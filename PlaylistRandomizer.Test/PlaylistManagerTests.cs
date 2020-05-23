using FluentAssertions;
using Moq;
using NUnit.Framework;
using PlaylistRandomizer.Models;
using PlaylistRandomizer.Spotify;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlaylistRandomizer.Test
{
    public class PlaylistMangerTests
    {
        [Test]
        public async Task ShouldGetAllTracks()
        {
            var mock = new Mock<IWebApi>();
            mock.SetupSequence(w => w.Get<Envelope<TrackShell>>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.FirstPagePlaylistTracks))
                .Returns(Task.FromResult(TestData.LastPagePlaylistTracks));
            
            var manager = new PlaylistManager(mock.Object);
            manager.Playlists.Add(TestData.OriginalPlaylist);
            manager.Session = TestData.Token;

            var expected = await manager.GetPlaylistTracks(TestData.RequestOriginalPlaylist);

            manager.Tracks.Should().HaveCount(2);
            expected.Should().HaveCount(2);
            expected.First().Name.Should().Be(TestData.FirstPageTrack.Name);
            expected.Skip(1).Take(1).First().Name.Should().Be(TestData.LastPageTrack.Name);            
        }

        [Test]
        public async Task ShouldCopyPlaylist()
        {
            var mock = new Mock<IWebApi>();
            mock.Setup(w => w.CreatePlaylist(It.IsAny<Playlist>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(TestData.CopiedPlaylist));

            var manager = new PlaylistManager(mock.Object);
            manager.Playlists.Add(TestData.OriginalPlaylist);
            manager.Me = TestData.User;
            manager.Session = TestData.Token;

            var expected = await manager.CopyPlaylist(TestData.RequestOriginalPlaylist);

            expected.Name.Should().Be(TestData.CopiedPlaylist.Name);
            manager.Playlists.Should().Contain(p => p.Name == TestData.CopiedPlaylist.Name);
        }

        [Test]
        public async Task ShouldAddTrackToPlaylistAtSpotifyAllowedMax() 
        {
            var invocations = new List<AddTracksRequest>();
            var mock = new Mock<IWebApi>();
            mock.Setup(w => w.AddTracks(It.IsAny<AddTracksRequest>(), It.IsAny<string>()))
                .Callback<AddTracksRequest, string>((request, token) => invocations.Add(request))
                .Returns(Task.CompletedTask);

            SpotifyApi._testLimit = 1;

            var manager = new PlaylistManager(mock.Object);
            manager.Session = TestData.Token;

            await manager.AddTracks(TestData.TracksToAdd);
            
            invocations.Should().HaveCount(3);

            ShouldHaveCorrectTrack(invocations[0], "1");
            ShouldHaveCorrectTrack(invocations[1], "2");
            ShouldHaveCorrectTrack(invocations[2], "3");
        }

        private void ShouldHaveCorrectTrack(AddTracksRequest invocation, string track)
        {
            invocation.Tracks.Should().HaveCount(1);
            invocation.Tracks[0].Should().Be(track);
        }
    }

    public static class TestData
    {
        public static MeResponse User => new MeResponse { UserResource = "https://user" };
        public static Playlist OriginalPlaylist => new Playlist { Id = "1", Tracks = new Tracks { Resource = "Playlist track href" } };
        public static Playlist CopiedPlaylist => new Playlist { Id = "2",  Name = "Playlist_Copy" };
        public static TokenResponse Token => new TokenResponse { Token = "A token" };

        public static Models.PlaylistRequest RequestOriginalPlaylist => new Models.PlaylistRequest { PlaylistId = OriginalPlaylist.Id };

        public static Track FirstPageTrack => new Track { Name = "Track 1" };
        public static Track LastPageTrack => new Track { Name = "Track 1" };

        public static Envelope<TrackShell> FirstPagePlaylistTracks => new Envelope<TrackShell> { Next = "TheNextUrl", Items = new List<TrackShell> { new TrackShell { Track = FirstPageTrack } } };

        public static Envelope<TrackShell> LastPagePlaylistTracks => new Envelope<TrackShell> { Next = null, Items = new List<TrackShell> { new TrackShell { Track = LastPageTrack } } };
        public static AddTracksRequest TracksToAdd => new AddTracksRequest { PlaylistId = CopiedPlaylist.Id, Tracks = new[] { "1", "2", "3" } };

    }
}