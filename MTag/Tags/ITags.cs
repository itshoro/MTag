using System.IO;

namespace MusicMetaData.MetaData
{
    public abstract class ITags
    {
        public string Title { get; protected set; }
        public string Album { get; protected set; }
        public string Composer { get; protected set; }
        public string LeadArtist { get; protected set; }
        public string[] Artists { get; protected set; }
        public int Year { get; protected set; }
        public int Length { get; protected set; }
        public string Publisher { get; protected set; }
        public int TrackNumber { get; protected set; }

        protected int totalSize;

        public abstract void ReadTags(BinaryReader reader);
        public abstract void ReadTags(Stream s);
        protected abstract byte[] ReadTag(BinaryReader reader);

        public int TotalHeaderSize()
        {
            return totalSize;
        }

    }
}
