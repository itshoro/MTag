# MTag 🎶
A .NET Core library to extract Metadata out of audio files. Currently only supports ID3v2 mp3 files.
Extraction of Tags is heavily WIP and only supports Tags using the string format. Only sets Lead Artist, Album and Title for now.

## Usage
```csharp
var filePath = "path/to/file.mp3";
var Tags = File.Create(filePath);
```
