using MusicMetaData.MetaData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MusicMetaData
{
    public class MTag
    {
        /// <summary>
        /// Creates a <see cref="FileSignatureMatcher"/> to check if the passed file header is supported.
        /// Initializes a <see cref="ITags"/> object, that holds the metadata of the passed media file.
        /// </summary>
        /// <param name="filePath">Path to the media file.</param>
        /// <returns>The <see cref="ITags"/> object.</returns>
        public static ITags Create(string filePath)
        {
            Stream s = new FileStream(filePath, FileMode.Open);
            FileSignatureMatcher matcher = new FileSignatureMatcher();
            var tag = matcher.FindSignature(s).Invoke(s);

            tag.ExtractTags(new BinaryReader(s));

            s.Close();

            return tag;
        }

        /// <summary>
        /// Creates a <see cref="FileSignatureMatcher"/> to check if the passed file header is supported.
        /// Initializes a <see cref="ITags"/> object, that holds the metadata of the passed media file stream.
        /// </summary>
        /// <param name="stream">The stream object.</param>
        /// <returns>The <see cref="ITags"/> object.</returns>
        public static ITags Create(Stream stream)
        {
            FileSignatureMatcher matcher = new FileSignatureMatcher();
            var tag = matcher.FindSignature(stream).Invoke(stream);

            tag.ExtractTags(stream);
            return tag;
        }
    }
}
