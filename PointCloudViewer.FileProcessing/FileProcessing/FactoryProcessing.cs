using PointCloudViewer.FileProcessing.Abstract;

namespace PointCloudViewer.FileProcessing.FileProcessing
{
    public static class FactoryProcessing
    {
        public static IFileProcessing GetFileProcessor(SupportedFile fileExtension)
        {
            switch (fileExtension)
            {
                case SupportedFile.XYZ:
                    return new XyzProcessing();
            }
            return null;
        }
    }
}
