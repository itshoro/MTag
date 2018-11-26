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
            exception.Message.Should().Be("Corrupted File Signature Header");
            stream.Close();
        }

        [TestMethod]
        public void WhenSizeBytesAreSetTo0_ReturnsAHeaderSizeOf0 ()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0 };
            FileSignatureMatcher matcher = new FileSignatureMatcher();

            var stream = new MemoryStream(data);

            var tags = matcher.FindSignature(data).Invoke(stream);
            tags.TagSize.Should().Be(0);
            stream.Close();
        }

        [TestMethod]
        public void WhenSizeBytesAreSet_ReturnsCorrectSize()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0x2, 0x1 };
            FileSignatureMatcher matcher = new FileSignatureMatcher();

            var stream = new MemoryStream(data);

            var tags = matcher.FindSignature(data).Invoke(stream);
            tags.TagSize.Should().Be(257);
            stream.Close();
        }

        [TestMethod]
        public void WhenSizeBytesAreSetSetsTheMostSignificantBitOfEachByteTo0_ReturnsCorrectSize()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF};
            FileSignatureMatcher matcher = new FileSignatureMatcher();

            var stream = new MemoryStream(data);

            var tags = matcher.FindSignature(data).Invoke(stream);

            int size = (0x7F << 21) | (0x7F << 14) | (0x7F << 7) | (0x7F << 0);

            tags.TagSize.Should().Be(size);
            stream.Close();
        }

        [TestMethod]
        public void WhenSizeOfHeaderIsGreaterThanThePassedData_ThrowInvalidHeaderException()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0x12, 0x5C };
            var stream = new MemoryStream(data);

            var exception = Assert.ThrowsException<InvalidHeaderException>(() => MTag.Create(stream));

            exception.Message.Should().Be("Tags are too short");
            stream.Close();
        }

        [TestMethod]
        public void WhenTestIsSetAsTitleTag_ReturnsTest()
        {

            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0x0F, 0x54, 0x49, 0x54, 0x32, 0, 0, 0, 0x6, 0x0, 0x0, 0, 0x54, 0x65, 0x73, 0x74 };
            var stream = new MemoryStream(data);

            var tags = MTag.Create(stream);
            tags.Title.Should().Be("Test");
        }


        [TestMethod]
        public void WhenMultipleTagsAreSet_ReturnsTheSetTags ()
        {
            byte[] data = new byte[] { 0x49, 0x44, 0x33, 0, 0, 0, 0, 0, 0, 0x1E, 0x54, 0x49, 0x54, 0x32, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x54, 0x65, 0x73, 0x74, 0x41, 0x54, 0x41, 0x4C, 0x42, 0, 0, 0, 0x6, 0x0, 0x0, 0, 0x54, 0x65, 0x73, 0x74, 0x42 };
            var stream = new MemoryStream(data);

            var tags = MTag.Create(stream);
            tags.Title.Should().Be("TestA");
            tags.Album.Should().Be("TestB");
        }

    }
}
