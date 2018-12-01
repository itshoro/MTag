using MusicMetaData.MetaData;
using MusicMetaData.Tags.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicMetaData.Tags
{
    public enum FrameId
    {
        AENC = 0,
        APIC = 1,
        COMM = 2,
        COMR = 3,
        ENCR = 4,
        EQUA = 5,
        ETCO = 6,
        GEOB = 7,
        GRID = 8,
        IPLS = 9,
        LINK = 10,
        MCDI = 11,
        MLLT = 12,
        OWNE = 13,
        PRIV = 14,
        PCNT = 15,
        POPM = 16,
        POSS = 17,
        RBUF = 18,
        RVAD = 19,
        RVRB = 20,
        SYLT = 21,
        SYTC = 22,
        TALB = 23,
        TBPM = 24,
        TCOM = 25,
        TCON = 26,
        TCOP = 27,
        TDAT = 28,
        TDLY = 29,
        TENC = 30,
        TEXT = 31,
        TFLT = 32,
        TIME = 33,
        TIT1 = 34,
        TIT2 = 35,
        TIT3 = 36,
        TKEY = 37,
        TLAN = 38,
        TLEN = 39,
        TMED = 40,
        TOAL = 41,
        TOFN = 42,
        TOLY = 43,
        TOPE = 44,
        TORY = 45,
        TOWN = 46,
        TPE1 = 47,
        TPE2 = 48,
        TPE3 = 49,
        TPE4 = 50,
        TPOS = 51,
        TPUB = 52,
        TRCK = 53,
        TRDA = 54,
        TRSN = 55,
        TRSO = 56,
        TSIZ = 57,
        TSRC = 58,
        TSSE = 59,
        TYER = 60,
        TXXX = 61,
        UFID = 62,
        USER = 63,
        USLT = 64,
        WCOM = 65,
        WCOP = 66,
        WOAF = 67,
        WOAR = 68,
        WOAS = 69,
        WORS = 70,
        WPAY = 71,
        WPUB = 72,
        WXXX = 73,
    }

    public struct Frame
    {
        public int size;
        public bool tagAlterPreservation;
        public bool fileAlterPreservation;
        public bool isReadOnly;
        public bool isCompressed;
        public bool isEncrypted;
        public bool isGrouped; // Important: Adds a group identifier byte to the header!!

        public byte groupIdentifier;

        public long position;

        public Encoding enc;
        public byte[] data;
    }

    public class ID3v2Tags : ITags
    {
        private const byte IGNORE_MOST_SIGNIFICANT_BYTE = 0b01111111;
        private static readonly byte[][] frameIds = new byte[][]
        {
            new byte[]{ 0x41, 0x45, 0x4E, 0x43 }, // AENC, Audio encryption
            new byte[]{ 0x41, 0x50, 0x49, 0x43 }, // APIC, Attached Picture
            new byte[]{ 0x43, 0x4F, 0x4D, 0x4D }, // COMM, Comment
            new byte[]{ 0x43, 0x4F, 0x4D, 0x52 }, // COMR, Commercial frame
            new byte[]{ 0x45, 0x4e, 0x43, 0x52 }, // ENCR, Encryption method registration
            new byte[]{ 0x45, 0x51, 0x55, 0x41 }, // EQUA, Equalization
            new byte[]{ 0x45, 0x54, 0x43, 0x4F }, // ETCO, Event Timing Codes
            new byte[]{ 0x47, 0x45, 0x4F, 0x42 }, // GEOB, General Encapsulated Object
            new byte[]{ 0x47, 0x52, 0x49, 0x44 }, // GRID, Group identification registration
            new byte[]{ 0x49, 0x50, 0x4C, 0x53 }, // IPLS, Involved People List
            new byte[]{ 0x4C, 0x49, 0x4E, 0x4B }, // LINK, Linked Information
            new byte[]{ 0x4D, 0x43, 0x44, 0x49 }, // MCDI, Music CD identifier
            new byte[]{ 0x4D, 0x4C, 0x4C, 0x54 }, // MLLT, MPEG location lookup table
            new byte[]{ 0x4F, 0x57, 0x4E, 0x45 }, // OWNE, Ownership frame
            new byte[]{ 0x50, 0x52, 0x49, 0x56 }, // PRIV, Private frame
            new byte[]{ 0x50, 0x43, 0x4E, 0x54 }, // PCNT, Play Counter
            new byte[]{ 0x50, 0x4F, 0x50, 0x4D }, // POPM, Popularimeter
            new byte[]{ 0x50, 0x4F, 0x53, 0x53 }, // POSS, Position Synchronisation frame
            new byte[]{ 0x52, 0x42, 0x55, 0x46 }, // RBUF, Recommended Buffer Size
            new byte[]{ 0x52, 0x56, 0x41, 0x44 }, // RVAD, Relative volume adjustment
            new byte[]{ 0x52, 0x56, 0x52, 0x42 }, // RVRB, Reverb
            new byte[]{ 0x53, 0x59, 0x4C, 0x54 }, // SYLT, Synchronized lyric / text
            new byte[]{ 0x53, 0x59, 0x54, 0x43 }, // SYTC, Synchronized tempo codes
            new byte[]{ 0x54, 0x41, 0x4C, 0x42 }, // TALB, Album / Movie / Show title
            new byte[]{ 0x54, 0x42, 0x50, 0x4D }, // TBPM, BPM
            new byte[]{ 0x54, 0x43, 0x4F, 0x4D }, // TCOM, Composer
            new byte[]{ 0x54, 0x43, 0x4F, 0x4E }, // TCON, Content Type
            new byte[]{ 0x54, 0x43, 0x4F, 0x50 }, // TCOP, Copyright message
            new byte[]{ 0x54, 0x44, 0x41, 0x54 }, // TDAT, Date
            new byte[]{ 0x54, 0x44, 0x4C, 0x59 }, // TDLY, Playlist delay
            new byte[]{ 0x54, 0x45, 0x4E, 0x43 }, // TENC, Encoded by
            new byte[]{ 0x54, 0x45, 0x58, 0x54 }, // TEXT, Lyricist / Text Writer
            new byte[]{ 0x54, 0x46, 0x4C, 0x54 }, // TFLT, File type
            new byte[]{ 0x54, 0x49, 0x4D, 0x45 }, // TIME, Time
            new byte[]{ 0x54, 0x49, 0x54, 0x31 }, // TIT1, Content group description
            new byte[]{ 0x54, 0x49, 0x54, 0x32 }, // TIT2, Title / Songname / Content description
            new byte[]{ 0x54, 0x49, 0x54, 0x33 }, // TIT3, Subtitle / Description Refinement
            new byte[]{ 0x54, 0x4B, 0x45, 0x59 }, // TKEY, Initial Key
            new byte[]{ 0x54, 0x4C, 0x41, 0x4E }, // TLAN, Language(s)
            new byte[]{ 0x54, 0x4C, 0x45, 0x4E }, // TLEN, Length
            new byte[]{ 0x54, 0x4D, 0x45, 0x44 }, // TMED, Media Type
            new byte[]{ 0x54, 0x4F, 0x41, 0x4C }, // TOAL, Original Album / Movie / Show Title
            new byte[]{ 0x54, 0x4F, 0x46, 0x4E }, // TOFN, Original Filename
            new byte[]{ 0x54, 0x4F, 0x4C, 0x59 }, // TOLY, Original lyricist(s) / text writer(s)
            new byte[]{ 0x54, 0x4F, 0x50, 0x45 }, // TOPE, Original artist(s) / performer(s)
            new byte[]{ 0x54, 0x4F, 0x52, 0x59 }, // TORY, Original release year
            new byte[]{ 0x54, 0x4F, 0x57, 0x4E }, // TOWN, File owner / licensee
            new byte[]{ 0x54, 0x50, 0x45, 0x31 }, // TPE1, Lead Performer(s) / Soloist(s)
            new byte[]{ 0x54, 0x50, 0x45, 0x32 }, // TPE2, Band / Orchesta / accompaniment
            new byte[]{ 0x54, 0x50, 0x45, 0x33 }, // TPE3, Conductor / Performer refinement
            new byte[]{ 0x54, 0x50, 0x45, 0x34 }, // TPE4, Interpreted, Remixed, or otherwise modified by
            new byte[]{ 0x54, 0x50, 0x4F, 0x53 }, // TPOS, Part of a set
            new byte[]{ 0x54, 0x50, 0x50, 0x42 }, // TPUB, Publisher
            new byte[]{ 0x54, 0x52, 0x53, 0x53 }, // TRCK, Track number / Position in set
            new byte[]{ 0x54, 0x52, 0x44, 0x41 }, // TRDA, Recording dates
            new byte[]{ 0x54, 0x52, 0x53, 0x4E }, // TRSN, Internet radio station name
            new byte[]{ 0x54, 0x52, 0x53, 0x4F }, // TRSO, Internet radio station owner
            new byte[]{ 0x54, 0x53, 0x49, 0x5A }, // TSIZ, Size
            new byte[]{ 0x54, 0x53, 0x52, 0x43 }, // TSRC, ISRC (international standard recording code)
            new byte[]{ 0x54, 0x53, 0x53, 0x45 }, // TSSE, Software / Hardware and settings used for encoding
            new byte[]{ 0x54, 0x59, 0x45, 0x52 }, // TYER, Year
            new byte[]{ 0x54, 0x58, 0x58, 0x58 }, // TXXX, User defined text information frame
            new byte[]{ 0x55, 0x46, 0x49, 0x44 }, // UFID, unique file identifier
            new byte[]{ 0x55, 0x53, 0x45, 0x52 }, // USER, Terms of use
            new byte[]{ 0x55, 0x53, 0x4C, 0x54 }, // USLT, Unsychronized lyric / text transcription
            new byte[]{ 0x57, 0x43, 0x4F, 0x4D }, // WCOM, Commercial information
            new byte[]{ 0x57, 0x43, 0x4F, 0x50 }, // WCOP, Copyright / Legal information
            new byte[]{ 0x57, 0x4F, 0x41, 0x46 }, // WOAF, audio file webpage
            new byte[]{ 0x57, 0x4F, 0x41, 0x52 }, // WOAR, artist / performer webpage
            new byte[]{ 0x57, 0x4F, 0x41, 0x53 }, // WOAS, audio source webpage
            new byte[]{ 0x57, 0x4F, 0x52, 0x53 }, // WORS, internet radio station homepage
            new byte[]{ 0x57, 0x50, 0x41, 0x59 }, // WPAY, Payment
            new byte[]{ 0x57, 0x50, 0x55, 0x42 }, // WPUB, Publishers official webpage
            new byte[]{ 0x57, 0x58, 0x58, 0x58 }, // WXXX, User defined URL link frame
        };

        private readonly byte version;
        private readonly byte revision;
        private readonly bool isUnsyncronised;
        private readonly bool hasExtendedHeader;
        private readonly bool isExperimental;

        private const int HEADER_SIZE = 10;
        private int ExtendedHeaderSize = 0;

        public ID3v2Tags(Stream s)
        {
            /* The constructor extracts information of of the header of the file. It contains the following information:
             * - 3 Bytes ("ID3")
             * - 2 Bytes (Major Version, Revision)
             * - 1 Byte  (Flags)
             * - 4 Bytes (Size of the area for tags, excluding the 10 bytes used by this header)
             */

            s.Position = 0;

            var reader = new BinaryReader(s);
            byte[] header = reader.ReadBytes(HEADER_SIZE);

            if (header.Length != HEADER_SIZE)
                throw new InvalidHeaderException("Corrupted File Signature Header");

            version = header[3];
            revision = header[4];

            isUnsyncronised = header[5].IsSet(7);
            hasExtendedHeader = header[5].IsSet(6);
            isExperimental = header[5].IsSet(5);

            var size = header.SubArray(6, 4);
            TagSize = CalculateSize(size, IGNORE_MOST_SIGNIFICANT_BYTE);
        }

        private Frame ReadTag(BinaryReader reader)
        {
            /* At this point we have a fully functional ID3v2 Header, each "Frame" (that's what the name of the tags is),
             * however has it's own header, so we have to determine the frame's flags and size.
             * The Header of a Frame looks like this:
             * -  4 Character (Already found, see tag)
             * -  4 Bytes Size
             * -  2 Bytes Flags
             * - (1 Byte Group Identifier, only set if flag for isGrouped is set)
             */

            byte[] sizeData = reader.ReadBytes(4);
            int frameSize = CalculateSize(sizeData);

            if (frameSize < 1)
                throw new InvalidHeaderException("Frames Have To Be atleast of 1 byte size");

            byte[] flags = reader.ReadBytes(2);
            Frame frame = new Frame()
            {
                size = frameSize,
                tagAlterPreservation = flags[0].IsSet(7),
                fileAlterPreservation = flags[0].IsSet(6),
                isReadOnly = flags[0].IsSet(5),
                isCompressed = flags[1].IsSet(7),
                isEncrypted = flags[1].IsSet(6),
                isGrouped = flags[1].IsSet(5),

                position = reader.BaseStream.Position - HEADER_SIZE,

                groupIdentifier = byte.MinValue
            };

            // IsGrouped adds one byte to the header that provides an identifier. Frames with the same identifier should be seen as a group
            if (frame.isGrouped)
            {
                frame.groupIdentifier = reader.ReadByte();
            }
            frame.data = reader.ReadBytes(frameSize);

            // Should we already remove the unnecessary bits here, as opposed to in ExtractTag? Could save a few bytes of storage.
            frame.enc = GetEncoding(frame.data);

            return frame;
        }

        public override void ReadTags(BinaryReader reader)
        {
            Frame[] frames = new Frame[frameIds.Length];

            if (hasExtendedHeader)
            {
                ReadExtendedHeader(reader);
            }

            reader.BaseStream.Position = HEADER_SIZE + ExtendedHeaderSize;
            byte[] header = reader.ReadBytes(TagSize - HEADER_SIZE - ExtendedHeaderSize);

            int position;
            for (int i = 0; i < frameIds.Length; i++)
            {
                if ((position = header.SearchPattern(frameIds[i])) == -1)
                    continue;

                position += frameIds[i].Length + HEADER_SIZE;
                reader.BaseStream.Position = position;
                frames[i] = ReadTag(reader);
            }
            RemoveIntersectingFrame(frames);
            ExtractFrameInformation(frames);
        }

        private void RemoveIntersectingFrame(Frame[] frames)
        {
            Frame intersecting;
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].data != null && IntersectsFrame(frames[i], frames, out intersecting))
                {
                    if (intersecting.position > frames[i].position)
                    {
                        intersecting = new Frame();
                    }
                    else
                    {
                        frames[i] = new Frame();
                    }
                }
            }
        }

        private bool IntersectsFrame(Frame f, Frame[] frames, out Frame intersecting)
        {
            foreach (Frame frame in frames)
            {
                if (frame.data != null && frame.position != f.position && (f.position > frame.position && f.position <= frame.position + frame.size))
                {
                    intersecting = frame;
                    return true;
                }
            }
            intersecting = new Frame();
            return false;
        }

        private void ReadExtendedHeader(BinaryReader reader)
        {
            byte[] extendedHeader = reader.ReadBytes(HEADER_SIZE);
            ExtendedHeaderSize = HEADER_SIZE;
            // We ignore the extended Header for now.

            if (extendedHeader[5].IsSet(7))
            {
                extendedHeader = extendedHeader.Append(reader.ReadBytes(4));
                ExtendedHeaderSize += 4;
            }
        }

        public override void ReadTags(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);

            ReadTags(reader);
            reader.Close();
        }

        // TODO: Currently only sets Text Based Frames, need to implement number based ones and extend this method
        private void ExtractFrameInformation(Frame[] frames)
        {
            if (frames[(int)FrameId.TIT2].data != null)
            {
                Title = ExtractTag(frames[(int)FrameId.TIT2].data, frames[(int)FrameId.TIT2].enc);
            }
            if (frames[(int)FrameId.TALB].data != null)
            {
                Album = ExtractTag(frames[(int)FrameId.TALB].data, frames[(int)FrameId.TALB].enc);
            }
            if (frames[(int)FrameId.TCOM].data != null)
            {
                Composers = ExtractTag(frames[(int)FrameId.TCOM].data, frames[(int)FrameId.TCOM].enc).Split('/');
                LeadComposer = Composers[0];
            }
            if (frames[(int)FrameId.TPE1].data != null)
            {
                Artists = ExtractTag(frames[(int)FrameId.TPE1].data, frames[(int)FrameId.TPE1].enc).Split('/');
                LeadArtist = Artists[0];
            }
            if (frames[(int)FrameId.TPUB].data != null)
            {
                Publisher = ExtractTag(frames[(int)FrameId.TPUB].data, frames[(int)FrameId.TPUB].enc);
            }
            if (frames[(int)FrameId.TBPM].data != null)
            {
                BPM = int.Parse(ExtractTag(frames[(int)FrameId.TBPM].data, frames[(int)FrameId.TBPM].enc));
            }
            if (frames[(int)FrameId.TYER].data != null)
            {
                var year = ExtractTag(frames[(int)FrameId.TYER].data, frames[(int)FrameId.TYER].enc);
                Year = int.Parse(year);
            }
        }

        private string ExtractTag(byte[] tag, Encoding enc)
        {
            int terminator = tag.Length;
            // We assume Unicode Encoding
            int index = 3;
            if (enc == Encoding.GetEncoding("iso-8859-1"))
            {
                index = 1;

                for (int i = index; i < tag.Length; i++)
                {
                    if (tag[i] == 0x0)
                    {
                        terminator = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = index; i < tag.Length - 1; i++)
                {
                    if (tag[i] == 0x0 && tag[i+1] == 0x0)
                    {
                        terminator = i + 1;
                        break;
                    }
                }
            }
            
            return enc.GetString(tag, index, terminator - index);
        }

        private Encoding GetEncoding(byte[] v)
        {
            // 0x0 = [ISO-8859-1] ISO/IEC DIS 8859-1. 8-bit single-byte coded graphic character sets, Part 1: Latin alphabet No. 1. Technical committee / subcommittee: JTC 1 / SC 2
            // 0x1 = Unicode
            if (v[0] == 0x0)
            {
                return Encoding.GetEncoding("iso-8859-1");
            }
            else if (v[0] == 0x1)
            {
                // 0x1 HAS to be followed by two bytes either "0xFF 0xFE" or "0xFE 0xFF" to determine the byte order
                byte b1 = v[1];
                byte b2 = v[2];


                if (b1 > b2)
                    return Encoding.Unicode;
                return Encoding.BigEndianUnicode;
            }
            throw new NotSupportedException("Encoding not supported");
        }

        /// <summary>
        /// Helper Method that determines the amount of byte a header takes up.
        /// 
        /// The masking allows for certain bytes to be ignored.
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Header Size calculated from the array, or -1 if the passed array's length is not 4.</returns>
        private int CalculateSize(byte[] data, byte mask = byte.MaxValue)
        {
            if (data.Length != 4)
                return -1;

            int a = data[0] & mask;
            int b = data[1] & mask;
            int c = data[2] & mask;
            int d = data[3] & mask;

            return (a << 21) |
                   (b << 14) |
                   (c << 7) |
                   (d << 0);
        }
    }
}
