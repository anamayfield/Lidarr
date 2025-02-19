using System.Collections.Generic;
using System.Text.Json.Serialization;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class ParsedAlbumInfo
    {
        public string AlbumTitle { get; set; }
        public string ArtistName { get; set; }
        public ArtistTitleInfo ArtistTitleInfo { get; set; }
        public QualityModel Quality { get; set; }
        public string ReleaseDate { get; set; }
        public bool Discography { get; set; }
        public int DiscographyStart { get; set; }
        public int DiscographyEnd { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseHash { get; set; }
        public string ReleaseVersion { get; set; }
        public string ReleaseTitle { get; set; }

        [JsonIgnore]
        public Dictionary<string, object> ExtraInfo { get; set; } = new Dictionary<string, object>();

        public override string ToString()
        {
            var albumString = "[Unknown Album]";

            if (AlbumTitle != null)
            {
                albumString = string.Format("{0}", AlbumTitle);
            }

            return string.Format("{0} - {1} {2}", ArtistName, albumString, Quality);
        }
    }
}
