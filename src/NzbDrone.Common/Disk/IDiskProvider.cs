using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace NzbDrone.Common.Disk
{
    public interface IDiskProvider
    {
        long? GetAvailableSpace(string path);
        void InheritFolderPermissions(string filename);
        void SetEveryonePermissions(string filename);
        void SetFilePermissions(string path, string mask, string group);
        void SetPermissions(string path, string mask, string group);
        void CopyPermissions(string sourcePath, string targetPath);
        long? GetTotalSize(string path);
        DateTime FolderGetCreationTime(string path);
        DateTime FolderGetLastWrite(string path);
        DateTime FileGetLastWrite(string path);
        void EnsureFolder(string path);
        bool FolderExists(string path);
        bool FileExists(string path);
        bool FileExists(string path, StringComparison stringComparison);
        bool FolderWritable(string path);
        bool FolderEmpty(string path);
        string[] GetDirectories(string path);
        string[] GetFiles(string path, SearchOption searchOption);
        long GetFolderSize(string path);
        long GetFileSize(string path);
        void CreateFolder(string path);
        void DeleteFile(string path);
        void CloneFile(string source, string destination, bool overwrite = false);
        void CopyFile(string source, string destination, bool overwrite = false);
        void MoveFile(string source, string destination, bool overwrite = false);
        void MoveFolder(string source, string destination);
        bool TryRenameFile(string source, string destination);
        bool TryCreateHardLink(string source, string destination);
        bool TryCreateRefLink(string source, string destination);
        void DeleteFolder(string path, bool recursive);
        string ReadAllText(string filePath);
        void WriteAllText(string filename, string contents);
        void FolderSetLastWriteTime(string path, DateTime dateTime);
        void FileSetLastWriteTime(string path, DateTime dateTime);
        bool IsFileLocked(string path);
        string GetPathRoot(string path);
        string GetParentFolder(string path);
        FileAttributes GetFileAttributes(string path);
        void EmptyFolder(string path);
        string GetVolumeLabel(string path);
        FileStream OpenReadStream(string path);
        FileStream OpenWriteStream(string path);
        List<IMount> GetMounts();
        IMount GetMount(string path);
        IDirectoryInfo GetDirectoryInfo(string path);
        List<IDirectoryInfo> GetDirectoryInfos(string path);
        IFileInfo GetFileInfo(string path);
        List<IFileInfo> GetFileInfos(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly);
        void RemoveEmptySubfolders(string path);
        void SaveStream(Stream stream, string path);
        bool IsValidFolderPermissionMask(string mask);
    }
}
