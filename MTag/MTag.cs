using MusicMetaData;
using MusicMetaData.MetaData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MusicMetaData
{
    public static class MTag
    {
        public static ITags Create(string filePath)
        {
            Stream s = new FileStream(filePath, FileMode.Open);
            FileSignatureMatcher matcher = new FileSignatureMatcher();
            var tag = matcher.FindSignature(s).Invoke(s);

            tag.ReadTags(new BinaryReader(s));

            s.Close();

            return tag;
        }

        public static ITags Create(Stream s)
        {
            FileSignatureMatcher matcher = new FileSignatureMatcher();
            var tag = matcher.FindSignature(s).Invoke(s);

            tag.ReadTags(s);
            return tag;
        }
    }
}
