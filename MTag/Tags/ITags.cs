using System.IO;

namespace MusicMetaData.MetaData
{
    public abstract class ITags
    {
        public string Title { get; protected set; }
        public string Album { get; protected set; }
        public string LeadComposer { get; protected set; }
        public string[] Composers { get; protected set; }
        public string LeadArtist { get; protected set; }
        public string[] Artists { get; protected set; }
        public int Year { get; protected set; }
        public int BPM { get; protected set; }
        public int Length { get; protected set; }
        public string Publisher { get; protected set; }
        public string[] Genres { get; protected set; }
        public int TrackNumber { get; protected set; }


        public abstract void ReadTags(BinaryReader reader);
        public abstract void ReadTags(Stream s);

        /// <summary>
        /// Returns The size of the area in which Tags are stored.
        /// Excludes the 10 Bytes of ID3v2 Headers
        /// </summary>
        public int TagSize { get; protected set; }
    }
}
