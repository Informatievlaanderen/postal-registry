namespace PostalRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Mime;
    using ExtractFiles;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    public static class PostalRegistryExtractArchive
    {
        public static FileStreamResult CreateResponse(this List<ExtractFile> files, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            name = name.EndsWith(".zip")
                ? name
                : $"{name.TrimEnd('.')}.zip";

            // REMARK: Do not put this in a using or FileStreamResult will not be able to access it!
            var archiveStream = new MemoryStream();

            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    using (file)
                    {
                        var fileContent = file.Flush();

                        using (var archiveItem = archive.CreateEntry(fileContent.Name).Open())
                            fileContent.Content.CopyTo(archiveItem);
                    }
                }
            }

            // FileStreamResult does a Stream.CopyTo (which copies 0 bytes if you don't reset the position)
            archiveStream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(archiveStream, new MediaTypeHeaderValue(MediaTypeNames.Application.Zip))
            {
                FileDownloadName = name
            };
        }
    }
}
