namespace LDIS.Core.Services
{
    public interface IBackupService
    {
        void BackupDatabase(string destinationFilePath);
    }
}
