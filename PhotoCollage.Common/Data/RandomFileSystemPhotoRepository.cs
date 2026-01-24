using System.IO;
using System.Linq;

namespace PhotoCollage.Common.Data;

internal sealed class RandomFileSystemPhotoRepository : FileSystemPhotoRepositoryBase
{
    private readonly List<string> displayedPhotos;
    private readonly object threadLock = new object();
    private readonly string silenceFilename;

    public RandomFileSystemPhotoRepository(string path, string silenceFilename = null)
        : base(path)
    {
        this.displayedPhotos = new List<string>();
        this.silenceFilename = silenceFilename ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Please silence.png");
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
            this.photoCounter++;
        }
        if (this.photoCounter % 3 == 0 && silenceEnabled)
        {
            return this.silenceFilename;
        }
        else
        {
            return Path.Combine(this.RootDirectoryPath, path);
        }
    }

    private int photoCounter = 0;

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
