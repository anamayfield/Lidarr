using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedAlbumInfo _parsedAlbumInfo;
        private List<Album> _albums;
        private List<TrackFile> _trackFiles;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _parsedAlbumInfo = Builder<ParsedAlbumInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.FLAC,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Lidarr")
                                                           .Build();

            _albums = Builder<Album>.CreateListOfSize(1)
                                        .All()
                                        .BuildList();

            _trackFiles = Builder<TrackFile>.CreateListOfSize(3)
                                            .All()
                                            .With(t => t.AlbumId = _albums.First().Id)
                                            .BuildList();

            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(_trackFiles);
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_there_are_is_no_track_files()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(c => c.GetFilesByAlbum(It.IsAny<int>()))
                  .Returns(new List<TrackFile>());

            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_a_different_quality()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.MP3_256);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_all_existing_files()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_some_but_not_all_trackfiles()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            _trackFiles.First().ReleaseGroup = "NotLidarr";

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_different_group()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "NotLidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_existing_file_is_unknown()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_release_group_for_release_is_unknown()
        {
            _parsedAlbumInfo.Quality.Revision.IsRepack = true;
            _parsedAlbumInfo.ReleaseGroup = null;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();

            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_true_when_repacks_are_not_preferred()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotPrefer);

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "";
                return c;
            }).ToList();

            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_repack_but_auto_download_repacks_is_true()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_repack_but_auto_download_repacks_is_false()
        {
            Mocker.GetMock<IConfigService>()
            .Setup(s => s.DownloadPropersAndRepacks)
            .Returns(ProperDownloadTypes.DoNotUpgrade);

            _parsedAlbumInfo.Quality.Revision.IsRepack = true;

            _trackFiles.Select(c =>
            {
                c.ReleaseGroup = "Lidarr";
                return c;
            }).ToList();
            _trackFiles.Select(c =>
            {
                c.Quality = new QualityModel(Quality.FLAC);
                return c;
            }).ToList();

            var remoteAlbum = Builder<RemoteAlbum>.CreateNew()
                                                      .With(e => e.ParsedAlbumInfo = _parsedAlbumInfo)
                                                      .With(e => e.Albums = _albums)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteAlbum, null).Accepted.Should().BeFalse();
        }
    }
}
