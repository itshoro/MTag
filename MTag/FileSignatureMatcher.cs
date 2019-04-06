using MusicMetaData.MetaData;
using MusicMetaData.Tags;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMetaData
{

    public class FileSignatureMatcher
    {

        // https://en.wikipedia.org/wiki/List_of_file_signatures
        private readonly ReadOnlyDictionary<byte[], Func<Stream, ITags>> SupportedSignatures;


        public FileSignatureMatcher()
        {
            Dictionary<byte[], Func<Stream, ITags>> signatures = new Dictionary<byte[], Func<Stream, ITags>>();
            signatures.Add(new byte[]{ 0x49, 0x44, 0x33}, GenerateID3v2Tags); // MP3, Id3v2

            SupportedSignatures = new ReadOnlyDictionary<byte[], Func<Stream, ITags>>(signatures);
        }

        private ID3v2Tags GenerateID3v2Tags(Stream s)
        {
            return new ID3v2Tags(s);
        }

        public Func<Stream, ITags> FindSignature(Stream s)
        {
            if(s.CanRead)
            {
                byte[] b = new byte[20];
                s.Read(b, 0, 20);

                return FindSignature(b);
            }
            throw new Exception("Stream can't be read from");
        }

        public Func<Stream, ITags> FindSignature(byte[] data)
        {
            while (data.Length > 0)
            {
                foreach (var s in SupportedSignatures)
                {
                    if (s.Key.Length == data.Length && s.Key.SequenceEqual(data))
                    {
                        return SupportedSignatures[s.Key];
                    }
                }
                Array.Resize(ref data, data.Length - 1);
            }
            throw new Exception("Signature not supported");
        }
    }
}
