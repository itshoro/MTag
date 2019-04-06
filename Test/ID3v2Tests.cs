using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicMetaData;
using MusicMetaData.Tags;
using MusicMetaData.Tags.Exceptions;
using System.IO;
using FluentAssertions;

namespace Test
{
    [TestClass]
    public class ID3v2Tests
    {
        [TestMethod]
        public void WhenPassedAnID3v2FileHeader_ReturnsAnObjectOfTypeID3v2Tags()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0 };
            FileSignatureMatcher matcher = new FileSignatureMatcher();

            var stream = new MemoryStream(data);

            var tags = matcher.FindSignature(data).Invoke(stream);
            tags.Should().BeOfType<ID3v2Tags>();
            stream.Close();
        }

        [TestMethod]
        public void WhenPassedAnInvalidID3v2FileHeader_ThrowsAnInvalidHeaderException()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33 };
            FileSignatureMatcher matcher = new FileSignatureMatcher();

            var stream = new MemoryStream(data);

            var exception = Assert.ThrowsException<InvalidHeaderException>(() => matcher.FindSignature(data).Invoke(stream));
            exception.Message.Should().Be("Header is incomplete.");
            stream.Close();
        }

        [TestMethod]
        public void WhenTestIsSetAsTitleTag_ReturnsTest()
        {

            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0x19, 0x54, 0x49, 0x54, 0x32, 0, 0, 0, 0x5, 0x0, 0x0, 0, 0x54, 0x65, 0x73, 0x74 };
            var stream = new MemoryStream(data);


            var tags = MTag.Create(stream);
            tags.Title.Should().Be("Test");
        }

        [TestMethod]
        public void WhenLatin1EncodingIsUsed_TerminateStringOnNull()
        {

            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0x1B, 0x54, 0x49, 0x54, 0x32, 0, 0, 0, 0x7, 0x0, 0x0, 0, 0x54, 0x65, 0x73, 0x74, 0x0, 0x41 };
            var stream = new MemoryStream(data);

            var tags = MTag.Create(stream);
            tags.Title.Should().Be("Test");
        }

        [TestMethod]
        public void WhenMultipleTagsAreSet_ReturnsTheExpectedTags ()
        {
            string[] input = File.ReadAllText("test.txt").Split(' ');
            var data = new byte[input.Length];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)int.Parse(input[i], System.Globalization.NumberStyles.HexNumber);
            }

            var stream = new MemoryStream(data);

            var tags = MTag.Create(stream);

            tags.Title.Should().Be("The Crutch");
            tags.LeadArtist.Should().Be("Billy Talent");
            tags.AlbumName.Should().Be("Afraid of Heights");
            tags.Year.Should().Be(2016);
        }
        [TestMethod]
        public void WhenYearIsSet_ReturnYear()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0x21, 0x54, 0x59, 0x45, 0x52, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x0, 0x32, 0x30, 0x31, 0x33 };
            var stream = new MemoryStream(data);

            var tags = MTag.Create(stream);
            tags.Year.Should().Be(2013);
        }
    }
}
