using MusicMetaData.MetaData;
using MusicMetaData.Tags.Exceptions;
using MusicMetaData.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicMetaData.Tags
{
    /// <summary>
    /// The type of a frame
    /// </summary>
    public enum FrameType
    {
        Invalid = -1,

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

    /// <summary>
    /// A struct containing all the information that can be stored in the header of a frame.
    /// </summary>
    public struct FrameHeader
    {
        public FrameType type;
        public int size;
        public bool tagAlterPreservation;
        public bool fileAlterPreservation;
        public bool isReadOnly;
        public bool isCompressed;
        public bool isEncrypted;
        public bool isGrouped;
        public byte? groupIdentifier;
        public long position;
    }

    public class ID3v2Tags : ITags
    {
        private const byte IGNORE_MOST_SIGNIFICANT_BYTE = 0b01111111;

        private readonly byte[][] frames;
        private readonly Dictionary<FrameType, Action<string>> frameTypeFields;

        private readonly byte version;
        private readonly byte revision;
        private readonly bool isUnsynchronized;
        private readonly bool hasExtendedHeader;
        private readonly bool isExperimental;

        public int DataSize { get; private set; }
        private const int HEADER_SIZE = 10;
        private int ExtendedHeaderSize = 0;

        private const int ISO_8859_1 = 0x0;
        private const int UNICODE = 0x1;

        /// <summary>
        /// Initializes a new <see cref="ID3v2Tags"/> object.
        /// Extracts information from the header of the file, the header consists of the following building blocks:
        ///
        /// - 3 Bytes ("ID3")
        /// - 2 Bytes (Major Version, Revision)
        /// - 1 Byte  (Flags)
        /// - 4 Bytes (Size of the area for tags, excluding the 10 bytes used by this header)
        /// </summary>
        /// <param name="stream"></param>
        public ID3v2Tags(Stream stream)
        {
            frames = new byte[][]
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
                new byte[]{ 0x54, 0x52, 0x43, 0x4B }, // TRCK, Track number (/ Position in set)
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

            frameTypeFields = new Dictionary<FrameType, Action<string>>();
            InitializeFrameDictionary();

            stream.Position = 0;

            var reader = new BinaryReader(stream);
            byte[] header = reader.ReadBytes(HEADER_SIZE);

            if (header.Length < HEADER_SIZE)
            {
                Logger.Log(LogLevel.Error, " Header might be corrupted or couldn't be read completely.");
                throw new InvalidHeaderException("Header is incomplete.");
            }

            version = header[3];
            revision = header[4];

            isUnsynchronized = header[5].IsSet(7);
            hasExtendedHeader = header[5].IsSet(6);
            isExperimental = header[5].IsSet(5);

            var size = header.SubArray(6, 4);
            DataSize = CalculateFrameLength(size, IGNORE_MOST_SIGNIFICANT_BYTE);
        }

        private void InitializeFrameDictionary()
        {
            frameTypeFields.Add(FrameType.AENC, null);
            frameTypeFields.Add(FrameType.APIC, null);
            frameTypeFields.Add(FrameType.COMM, null);
            frameTypeFields.Add(FrameType.COMR, null);
            frameTypeFields.Add(FrameType.ENCR, null);
            frameTypeFields.Add(FrameType.EQUA, null);
            frameTypeFields.Add(FrameType.ETCO, null);
            frameTypeFields.Add(FrameType.GEOB, null);
            frameTypeFields.Add(FrameType.GRID, null);
            frameTypeFields.Add(FrameType.IPLS, null);
            frameTypeFields.Add(FrameType.LINK, null);
            frameTypeFields.Add(FrameType.MCDI, null);
            frameTypeFields.Add(FrameType.MLLT, null);
            frameTypeFields.Add(FrameType.OWNE, null);
            frameTypeFields.Add(FrameType.PRIV, null);
            frameTypeFields.Add(FrameType.PCNT, null);
            frameTypeFields.Add(FrameType.POPM, null);
            frameTypeFields.Add(FrameType.POSS, null);
            frameTypeFields.Add(FrameType.RBUF, null);
            frameTypeFields.Add(FrameType.RVAD, null);
            frameTypeFields.Add(FrameType.RVRB, null);
            frameTypeFields.Add(FrameType.SYLT, null);
            frameTypeFields.Add(FrameType.SYTC, null);
            frameTypeFields.Add(FrameType.TALB, (value) => AlbumName = value);
            frameTypeFields.Add(FrameType.TBPM, (value) => BPM = int.Parse(value));
            frameTypeFields.Add(FrameType.TCOM, (value) => LeadComposer = value);
            frameTypeFields.Add(FrameType.TCON, (value) => Genres = value.Split('\0'));
            frameTypeFields.Add(FrameType.TCOP, null);
            frameTypeFields.Add(FrameType.TDAT, null); 
            frameTypeFields.Add(FrameType.TDLY, null); 
            frameTypeFields.Add(FrameType.TENC, null); 
            frameTypeFields.Add(FrameType.TEXT, null); 
            frameTypeFields.Add(FrameType.TFLT, null); 
            frameTypeFields.Add(FrameType.TIME, null); 
            frameTypeFields.Add(FrameType.TIT1, null); 
            frameTypeFields.Add(FrameType.TIT2, (value) => Title = value);
            frameTypeFields.Add(FrameType.TIT3, null); 
            frameTypeFields.Add(FrameType.TKEY, null); 
            frameTypeFields.Add(FrameType.TLAN, null); 
            frameTypeFields.Add(FrameType.TLEN, null); 
            frameTypeFields.Add(FrameType.TMED, null); 
            frameTypeFields.Add(FrameType.TOAL, null); 
            frameTypeFields.Add(FrameType.TOFN, null); 
            frameTypeFields.Add(FrameType.TOLY, null); 
            frameTypeFields.Add(FrameType.TOPE, null); 
            frameTypeFields.Add(FrameType.TORY, null); 
            frameTypeFields.Add(FrameType.TOWN, null); 
            frameTypeFields.Add(FrameType.TPE1, (value) => LeadArtist = value);
            frameTypeFields.Add(FrameType.TPE2, (value) => Band = value);
            frameTypeFields.Add(FrameType.TPE3, null);
            frameTypeFields.Add(FrameType.TPE4, null);
            frameTypeFields.Add(FrameType.TPOS, (value) => { var numbers = value.Split('/'); SetNumber = int.Parse(numbers[0]); AmountOfSets = int.Parse(numbers[1]); });
            frameTypeFields.Add(FrameType.TPUB, (value) => Publisher = value);
            frameTypeFields.Add(FrameType.TRCK, (value) => { var numbers = value.Split('/'); TrackNumber = int.Parse(numbers[0]); AmountOfTracksInSet = int.Parse(numbers[1]); });
            frameTypeFields.Add(FrameType.TRDA, null);
            frameTypeFields.Add(FrameType.TRSN, null);
            frameTypeFields.Add(FrameType.TRSO, null);
            frameTypeFields.Add(FrameType.TSIZ, null);
            frameTypeFields.Add(FrameType.TSRC, null);
            frameTypeFields.Add(FrameType.TSSE, null);
            frameTypeFields.Add(FrameType.TYER, (value) => Year = int.Parse(value));
            frameTypeFields.Add(FrameType.TXXX, null);
            frameTypeFields.Add(FrameType.UFID, null);
            frameTypeFields.Add(FrameType.USER, null);
            frameTypeFields.Add(FrameType.USLT, null);
            frameTypeFields.Add(FrameType.WCOM, null);
            frameTypeFields.Add(FrameType.WCOP, null);
            frameTypeFields.Add(FrameType.WOAF, null);
            frameTypeFields.Add(FrameType.WOAR, null);
            frameTypeFields.Add(FrameType.WOAS, null);
            frameTypeFields.Add(FrameType.WORS, null);
            frameTypeFields.Add(FrameType.WPAY, null);
            frameTypeFields.Add(FrameType.WPUB, null);
            frameTypeFields.Add(FrameType.WXXX, null);
        }

        /// <summary>
        /// Initializes a <see cref="FrameHeader"/> that holds information on a tag
        /// </summary>
        /// <param name="frameHeader">An array holding the byte data for the <see cref="FrameHeader"/></param>
        /// <returns>The <see cref="FrameHeader"/></returns>
        private FrameHeader InitializeFrameHeader(byte[] frameHeader)
        {
            //  The Header of a Frame looks like this:
            //   -4 Character(Already found, see tag)
            //   -4 Bytes Size
            //   -2 Bytes Flags
            //   -(1 Byte Group Identifier, only set if flag for isGrouped is set)

            FrameType type = FindFrameType(frameHeader.SubArray(0, 4));
            int length = CalculateFrameLength(frameHeader.SubArray(4, 4));

            if (length < 1)
                throw new ArgumentOutOfRangeException("[Frame] Length has to be atleast 1 (excluding header)");

            byte[] flags = frameHeader.SubArray(8, 2);
            FrameHeader header = new FrameHeader()
            {
                type = type,
                size = length,
                tagAlterPreservation = flags[0].IsSet(7),
                fileAlterPreservation = flags[0].IsSet(6),
                isReadOnly = flags[0].IsSet(5),
                isCompressed = flags[1].IsSet(7),
                isEncrypted = flags[1].IsSet(6),
                isGrouped = flags[1].IsSet(5)
            };

            return header;
        }

        /// <summary>
        /// Finds the <see cref="FrameType"/> of the tag, which is stored in the header of a frame
        /// </summary>
        /// <param name="type">An array filled with the 4 bytes that make up a type</param>
        /// <returns>The <see cref="FrameType"/></returns>
        private FrameType FindFrameType(byte[] type)
        {
            int pos = 0;
            while (pos < frameTypeFields.Count)
            {
                if (frames[pos].SequenceEqual(type))
                    return (FrameType)(pos);
                pos++;
            }
            return FrameType.Invalid;
        }

        /// <summary>
        /// Checks if the provided <see cref="FrameType"/> contains data in a numeric or url format
        /// </summary>
        /// <param name="id">The frame id</param>
        /// <returns>True when the FrameId is in a numeric or url format</returns>
        private bool IsNumericOrUrl(FrameType id)
        {
            var numericOrUrl = new FrameType[] {
                FrameType.TBPM,
                FrameType.TCON,
                FrameType.TDAT,
                FrameType.TDLY,
                FrameType.TIME,
                FrameType.TLEN,
                FrameType.TPOS,
                FrameType.TRCK,
                FrameType.TSIZ,
                FrameType.TYER,
                FrameType.OWNE,
                FrameType.COMR
            };
            return numericOrUrl.Contains(id);
        }

        /// <summary>
        /// Extracts tags from a given media file that has been fed into a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public override void ExtractTags(BinaryReader reader)
        {
            var headers = new List<FrameHeader>();

            if (hasExtendedHeader)
            {
                ReadExtendedHeader(reader);
            }

            reader.BaseStream.Position = HEADER_SIZE + ExtendedHeaderSize;
            byte[] data = reader.ReadBytes(DataSize - HEADER_SIZE - ExtendedHeaderSize);

            int position = 0;
            while (position < data.Length)
            {
                FrameHeader header = InitializeFrameHeader(data.SubArray(position, 10));
                header.position = position;
                if (header.isGrouped)
                {
                    header.groupIdentifier = data[position + HEADER_SIZE];
                    position++;
                }
                position += header.size + HEADER_SIZE;
                headers.Add(header);
            }

            var textHeaders = headers.Where(h => h.type >= FrameType.TALB && h.type <= FrameType.TYER);
            foreach (var header in textHeaders)
            {
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                var frameData = data.SubArray((int)header.position + HEADER_SIZE, header.size);
                if (!IsNumericOrUrl(header.type))
                {
                    encoding = GetEncoding(frameData.SubArray(0, 3));
                }
                SetTag(header.type, ExtractTextData(frameData, encoding));
            }
        }

        /// <summary>
        /// Sets the corresponding fields for the extracted tags.
        /// </summary>
        /// <param name="type">The type of the tag that is to be set</param>
        /// <param name="value">The value that should be inserted</param>
        private void SetTag(FrameType type, string value)
        {
            if (frameTypeFields.TryGetValue(type, out Action<string> setAction) && setAction != null)
            {
                setAction(value);
            }
        }

        /// <summary>
        /// Extracts data from the extended ID3v2 header.
        /// TODO!
        /// </summary>
        /// <param name="reader">A binary reader at the first position of the extended header</param>
        private void ReadExtendedHeader(BinaryReader reader)
        {
            byte[] extendedHeader = reader.ReadBytes(HEADER_SIZE);
            ExtendedHeaderSize = HEADER_SIZE;
            // TODO: We ignore the extended Header for now.

            if (extendedHeader[5].IsSet(7))
            {
                extendedHeader = extendedHeader.Append(reader.ReadBytes(4));
                ExtendedHeaderSize += 4;
            }
        }

        /// <summary>
        /// Extracts the Tags from the given Stream object
        /// </summary>
        /// <param name="stream">The stream object</param>
        public override void ExtractTags(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            ExtractTags(reader);
            reader.Close();
        }

        /// <summary>
        /// Extracts text information out of the given data array, by using the provided Encoding
        /// </summary>
        /// <param name="data">The array containing information</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>A string containing the information</returns>
        private string ExtractTextData(byte[] data, Encoding encoding)
        {
            int terminator = data.Length;
            // We assume Unicode Encoding
            int index = 3;
            if (encoding == Encoding.GetEncoding("iso-8859-1"))
            {
                index = 1;

                for (int i = index; i < data.Length; i++)
                {
                    if (data[i] == 0x0)
                    {
                        terminator = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = index; i < data.Length - 1; i++)
                {
                    if (data[i] == 0x0 && data[i + 1] == 0x0)
                    {
                        terminator = i + 1;
                        break;
                    }
                }
            }

            return encoding.GetString(data, index, terminator - index);
        }

        /// <summary>
        /// Returns the corresponding Encoding defined by the provided encoding information in front of the data block.
        /// </summary>
        /// <param name="encodingInformation">The encoding information</param>
        /// <returns>The encoding</returns>
        private Encoding GetEncoding(byte[] encodingInformation)
        {
            // 0x0 = [ISO-8859-1] ISO/IEC DIS 8859-1. 8-bit single-byte coded graphic character sets, Part 1: Latin alphabet No. 1. Technical committee / subcommittee: JTC 1 / SC 2
            // 0x1 = Unicode
            if (encodingInformation[0] == ISO_8859_1)
            {
                return Encoding.GetEncoding("iso-8859-1");
            }
            else if (encodingInformation[0] == UNICODE)
            {
                // UNICODE HAS to be followed by two bytes either "0xFF 0xFE" or "0xFE 0xFF" to determine the byte order
                byte b1 = encodingInformation[1];
                byte b2 = encodingInformation[2];

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
        private int CalculateFrameLength(byte[] data, byte mask = byte.MaxValue)
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