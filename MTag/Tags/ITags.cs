using System.IO;

namespace MusicMetaData.MetaData
{
    public abstract class ITags
    {
        public string Title { get; protected set; }
        public string AlbumName { get; protected set; }
        public int SetNumber { get; protected set; }
        public int AmountOfSets { get; protected set; }
        public string LeadComposer { get; protected set; }
        public string[] Composers { get; protected set; }
        public string LeadArtist { get; protected set; }
        public string Band { get; protected set; }
        public string[] Artists { get; protected set; }
        public int Year { get; protected set; }
        public int BPM { get; protected set; }
        public int Length { get; protected set; }
        public string Publisher { get; protected set; }
        public string[] Genres { get; protected set; }
        public int TrackNumber { get; protected set; }
        public int AmountOfTracksInSet { get; protected set; }

        public byte[] FrontCover { get; protected set; }
        public byte[] BackCover { get; protected set; }
        public byte[] Icon { get; protected set; }

        public byte[] Images { get; protected set; }
        
        public abstract void ExtractTags(BinaryReader reader);
        public abstract void ExtractTags(Stream s);
    }
}
