﻿using System.IO;
using System.Linq;

namespace PhotoCollage.Common.Data;

internal sealed class RandomFileSystemPhotoRepository : FileSystemPhotoRepositoryBase
{
    private readonly List<string> displayedPhotos;
    private readonly object threadLock = new object();

    public RandomFileSystemPhotoRepository(string path)
        : base(path)
    {
        this.displayedPhotos = new List<string>();
    }

    public override string GetNextPhotoFilePath(bool silenceEnabled)
    {
        if (!this.PhotoFilePaths.TryDequeue(out var path))
        {
            this.ReloadPhotoQueue();
            this.PhotoFilePaths.TryDequeue(out path);
        }

        lock (this.threadLock)
        {
            this.displayedPhotos.Add(path);
            this.pleaseSilence = !this.pleaseSilence;
        }
        if (this.pleaseSilence && silenceEnabled)
        {
            return Path.Combine("D:\\OneDrive\\Pictures", "Please silence.png");
        }
        else
        {
            return Path.Combine(this.RootDirectoryPath, path);
        }
    }

    private bool pleaseSilence;

    protected override IEnumerable<string> GetOrderedPaths(IEnumerable<string> paths) => RandomizePaths(paths);

    private static IEnumerable<string> RandomizePaths(IEnumerable<string> paths)
    {
        var random = new Random();
        return paths.OrderBy(item => random.Next());
    }

    private void ReloadPhotoQueue()
    {
        lock (this.threadLock)
        {
            var photosToQueue = RandomizePaths(this.displayedPhotos);
            this.LoadPhotoPathsIntoQueue(photosToQueue);
            this.displayedPhotos.Clear();
        }
    }
}
