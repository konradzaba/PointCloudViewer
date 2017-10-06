namespace PointCloudViewer.FileProcessing.Abstract
{
    public interface IFileLoading
    {
        /// <summary>
        /// Reads the file to string and returns it.
        /// </summary>
        /// <param name="fileName">The file that is meant to be read.</param>
        /// <returns>String of file contents.</returns>
        string LoadText(string fileName);
    }
}
