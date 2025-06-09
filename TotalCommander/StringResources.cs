using System;
using System.Collections.Generic;
using System.Globalization;

namespace TotalCommander
{
    /// <summary>
    /// Class for handling string resources for multilingual support
    /// </summary>
    public static class StringResources
    {
        private static Dictionary<string, Dictionary<string, string>> _resources = new Dictionary<string, Dictionary<string, string>>();
        private static string _currentLanguage = "en"; // Default language is English

        /// <summary>
        /// Static constructor to initialize string resources
        /// </summary>
        static StringResources()
        {
            InitializeResources();
        }

        /// <summary>
        /// Initialize all string resources for all supported languages
        /// </summary>
        private static void InitializeResources()
        {
            // Initialize English resources
            Dictionary<string, string> enResources = new Dictionary<string, string>();
            
            // Common
            enResources["OK"] = "OK";
            enResources["Cancel"] = "Cancel";
            enResources["Yes"] = "Yes";
            enResources["No"] = "No";
            enResources["Error"] = "Error";
            enResources["Warning"] = "Warning";
            enResources["Information"] = "Information";
            enResources["Confirmation"] = "Confirmation";
            
            // File operations
            enResources["DeleteConfirmation"] = "Are you sure you want to delete this item?";
            enResources["DeleteMultipleConfirmation"] = "Are you sure you want to delete {0} items?";
            enResources["CopyConfirmation"] = "Copy Confirmation";
            enResources["DeleteError"] = "Error deleting file: {0}";
            enResources["CopyError"] = "Error copying file: {0}";
            enResources["CopySuccess"] = "{0} item(s) copied successfully.";
            enResources["SourcePath"] = "Source: ";
            enResources["DestinationPath"] = "Destination: ";
            enResources["FileCount"] = "File count: {0}";
            enResources["CopySize"] = "Copy size: {0}";
            enResources["Copy"] = "Copy";
            enResources["CopyFailure"] = "Copy operation failed.";
            enResources["CopyInProgress"] = "Copying files...";
            enResources["MoveInProgress"] = "Moving files...";
            enResources["ItemsSelected"] = "Selected {0} file(s), {1} dir(s) | Total size: {2}";
            enResources["FilesSelected"] = "Selected {0} file(s) | Total size: {1}";
            enResources["DirectoriesSelected"] = "Selected {0} dir(s)";
            enResources["FilesAndDirectories"] = "{0} File(s), {1} Dir(s)";
            enResources["NoItemsSelected"] = "No items selected.";
            
            // Navigation
            enResources["NavigationError"] = "Error navigating to path: {0}";
            enResources["InvalidPath"] = "Invalid path";
            enResources["DirectoryNotFound"] = "Directory not found";
            
            // Error messages
            enResources["ErrorReadingFolder"] = "Error reading folder contents";
            enResources["ErrorProcessingFile"] = "Error processing file: {0}";
            
            // Delete operation
            enResources["DeleteConfirmTitle"] = "Delete Confirmation";
            enResources["DeleteFile"] = "Are you sure you want to delete '{0}'?";
            enResources["DeleteMultiple"] = "Are you sure you want to delete {0} items?";
            enResources["DeletionError"] = "Error during deletion: {0}";
            
            // New folder
            enResources["NewFolder"] = "New Folder";
            enResources["CreateNewFolder"] = "Create New Folder";
            enResources["FolderName"] = "Folder name:";
            enResources["FolderExists"] = "A folder with this name already exists.";
            enResources["InvalidFolderName"] = "Invalid folder name.";
            enResources["FolderCreated"] = "Folder created successfully.";
            enResources["FolderCreationError"] = "Error creating folder: {0}";
            
            // File search
            enResources["Search"] = "Search";
            enResources["SearchResults"] = "Search Results";
            enResources["SearchText"] = "Search text:";
            enResources["SearchLocation"] = "Search in:";
            enResources["IncludeSubfolders"] = "Include subfolders";
            enResources["SearchingIn"] = "Searching in {0}...";
            enResources["NoResultsFound"] = "No results found.";
            enResources["SearchingCanceled"] = "Search canceled.";
            enResources["FoundItems"] = "Found {0} item(s)";
            
            // Add new string resources for file rename
            enResources["RenameTitle"] = "Rename";
            enResources["RenameError"] = "Rename Error";
            enResources["RenameMultipleNotAllowed"] = "Please select only one item to rename.";
            enResources["FolderAlreadyExists"] = "A folder with name '{0}' already exists.";
            enResources["FileAlreadyExists"] = "A file with name '{0}' already exists.";
            enResources["RenameErrorMessage"] = "Error occurred while renaming: {0}";
            enResources["FileOpenError"] = "Cannot open file: {0}";
            enResources["PathNotFound"] = "Path not found: {0}";
            
            // Add new string resources for file operations
            enResources["InvalidPathReason"] = "The path is not valid for one of the following reasons: it is a zero-length string; it contains only white space; it contains invalid characters; or it is a device path (starts with \\\\.\\).";
            enResources["FileNotFound"] = "This file or directory does not exist.";
            enResources["FileNameTooLong"] = "The file name exceeds system-defined maximum length.";
            enResources["InvalidName"] = "Invalid name";
            enResources["PermissionDenied"] = "You don't have enough permission on this file or folder.";
            enResources["InvalidFileName"] = "Invalid file name";
            enResources["ReadOnlyParentDirectory"] = "The parent directory of the file to be created is read-only";
            enResources["CannotCreateFileInThisFolder"] = "Cannot create file in this folder";
            enResources["InvalidDirectoryName"] = "Invalid directory name";
            enResources["DirectoryNameTooLong"] = "The directory name is too long";
            enResources["CannotCreateDirectoryInThisFolder"] = "Cannot create directory in this folder";
            enResources["AccessDeniedOnFile"] = "Access denied on file{0}{1}";
            enResources["AccessDeniedOnFolder"] = "Access denied on folder{0}{1}";
            enResources["TotalCommander"] = "Total Commander";
            enResources["SelectedFilesDirs"] = "Selected {0} file(s), {1} dir(s) | Total size: {2}";
            enResources["SelectedFiles"] = "Selected {0} file(s) | Total size: {1}";
            enResources["SelectedDirs"] = "Selected {0} dir(s)";
            enResources["FolderSummary"] = "{0} File(s), {1} Dir(s)";
            enResources["ErrorReadingFolderContents"] = "Error reading folder contents";
            
            // Debug and error messages
            enResources["ParameterDebugInfo"] = "Parameter Debug Information:";
            enResources["LeftExplorerPath"] = "Left Explorer Path: {0}";
            enResources["RightExplorerPath"] = "Right Explorer Path: {0}";
            enResources["FocusingExplorerPath"] = "Focusing Explorer Path: {0}";
            enResources["OriginalParameters"] = "Original Parameters: {0}";
            enResources["LeftDirectoryPath"] = "Left Directory Path: {0}";
            enResources["RightDirectoryPath"] = "Right Directory Path: {0}";
            enResources["FocusingDirectoryPath"] = "Focusing Directory Path: {0}";
            enResources["ParametersAfterSubstitution"] = "Parameters After Substitution: {0}";
            enResources["ExecutionError"] = "Execution Error";
            enResources["ExecutionErrorMessage"] = "Error occurred during execution: {0}";
            enResources["F5KeySettingBeforeSave"] = "F5 Key Setting Before Save: Action={0}";
            enResources["F5KeySettingVerification"] = "F5 Key Setting Verification: Action={0}";
            enResources["SettingsSaveConfirmation"] = "Settings Save Confirmation";
            enResources["SettingsVerificationError"] = "Settings Verification Error";
            enResources["SavedSettingsVerificationError"] = "Error verifying saved settings file: {0}";
            enResources["SettingsSaveError"] = "Settings Save Error";
            enResources["ErrorSavingSettings"] = "Error occurred while saving settings: {0}";
            enResources["SettingsFilePath"] = "Settings File Path: {0}";
            enResources["FileExistence"] = "File Exists: {0}";
            enResources["LoadedF5KeySetting"] = "Loaded F5 Key Setting: Action={0}";
            enResources["NoF5KeySettingInLoadedSettings"] = "No F5 key setting in loaded settings!";
            enResources["SettingsLoadError"] = "Settings Load Error";
            enResources["NullSettingsReturned"] = "Loaded settings returned null value.";
            enResources["ErrorLoadingSettings"] = "Error occurred while loading settings: {0}";
            
            // FormUserExecuteOption
            enResources["UserExecuteOptionTitle"] = "User Execute Option";
            enResources["ExecuteOptionName"] = "Execute Option Name:";
            enResources["ExecutableOption"] = "Executable Option:";
            enResources["Parameters"] = "Parameters:";
            enResources["ParametersHint"] = "Hint: Click on parameter variables below to add them to the parameters textbox.";
            enResources["Save"] = "Save";
            enResources["SelectExecutableTitle"] = "Select Executable";
            enResources["ExecutableFilter"] = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            enResources["ExecuteOptionNameRequired"] = "Please enter an execution option name.";
            enResources["ExecutablePathRequired"] = "Please enter an executable path.";
            enResources["ExecuteOptionNameExists"] = "An execute option with this name already exists.";
            enResources["LeftSelectedItemPath"] = "Full path of the selected item in the left file list";
            enResources["LeftSelectedItemDirPath"] = "Directory path of the selected item in the left file list";
            enResources["RightSelectedItemPath"] = "Full path of the selected item in the right file list";
            enResources["RightSelectedItemDirPath"] = "Directory path of the selected item in the right file list";
            enResources["FocusingSelectedItemPath"] = "Full path of the selected item in the currently focused file list";
            enResources["FocusingSelectedItemDirPath"] = "Directory path of the selected item in the currently focused file list";
            
            // FormManageUserOptions
            enResources["ManageUserOptionsTitle"] = "Manage User Execute Options";
            enResources["AddUserOption"] = "Add...";
            enResources["EditUserOption"] = "Edit...";
            enResources["DeleteUserOption"] = "Delete";
            enResources["UserOptionDeleteConfirmation"] = "Are you sure you want to delete '{0}' option?\n\nAll key settings using this option will be reset.";
            enResources["UserOptionDeleteConfirmationTitle"] = "Confirm Delete Option";
            enResources["UserOptionDeleteError"] = "Error deleting option: {0}";
            enResources["UserOptionError"] = "User Option Error";
            enResources["NoOptionSelected"] = "No user option selected.";
            
            // FormProgressCopy
            enResources["FileCopyTitle"] = "File Copy";
            enResources["Preparing"] = "Preparing...";
            enResources["Cancelling"] = "Cancelling...";
            enResources["FileMoving"] = "Moving files...";
            enResources["FileCopying"] = "Copying files...";
            enResources["CopyingMovingFile"] = "{0} file: {1}";
            enResources["OperationError"] = "Operation Error";
            enResources["ErrorMessage"] = "Error: {0}";
            enResources["OperationCancelled"] = "Operation was cancelled.";
            enResources["OperationCompleted"] = "Operation completed.";
            enResources["CompletedFiles"] = "Completed: {0}/{1} files";
            enResources["CompletedFilesPercent"] = "Completed: {0}/{1} files, {2}%";
            
            // FormPacking
            enResources["PackingTitle"] = "File Compression";
            enResources["DestinationZipFile"] = "Destination zip file:";
            enResources["ArchiveDestination"] = "Archive destination:";
            enResources["CompressionLevel"] = "Compression level:";
            enResources["FastCompression"] = "Fast compression (lower ratio)";
            enResources["NormalCompression"] = "Normal compression";
            enResources["MaximumCompression"] = "Maximum compression (slower)";
            enResources["AddToExistingArchive"] = "Add to existing archive";
            enResources["UpdateExistingFiles"] = "Update existing files";
            enResources["CreateNewArchive"] = "Create new archive";
            enResources["StartCompression"] = "Start compression";
            enResources["ZipFileRequired"] = "Please specify a zip file path.";
            enResources["NoFilesToCompress"] = "There are no files to compress.";
            enResources["Compressing"] = "Compressing...";
            enResources["CompressionProgress"] = "Compressing... {0}%";
            enResources["CompressionError"] = "Compression Error";
            enResources["CompressionErrorMessage"] = "Error occurred during compression: {0}";
            enResources["CompressionComplete"] = "Compression complete!";
            enResources["CompressionCompleteMessage"] = "File compression has completed.";
            enResources["CompressionStartError"] = "Compression Start Error";
            enResources["CompressionStartErrorMessage"] = "Error occurred while starting compression operation: {0}";
            
            // Form Common
            enResources["Archive"] = "Archive:";
            enResources["Path"] = "Path";
            enResources["Browse"] = "Browse";
            enResources["Warning"] = "Warning";
            enResources["Information"] = "Information";
            enResources["Close"] = "Close";
            
            // FormFontSettings
            enResources["FontSettingsTitle"] = "Font Settings";
            enResources["FontFamily"] = "Font Family:";
            enResources["FontSize"] = "Font Size:";
            enResources["Bold"] = "Bold";
            enResources["Italic"] = "Italic";
            enResources["Preview"] = "Preview";
            enResources["StatusBarPreview"] = "Status bar preview";
            enResources["ApplyToStatusBar"] = "Apply to status bar";
            enResources["SampleText"] = "AaBbYyZz 123";
            enResources["FontError"] = "Font Error";
            enResources["InvalidFontError"] = "Cannot apply the selected font: {0}";
            enResources["InputError"] = "Input Error";
            enResources["SelectFontAndSize"] = "Please select a font and size.";
            
            // FormKeySettings
            enResources["KeySettingsTitle"] = "Function Key Settings";
            enResources["F2Key"] = "F2 key:";
            enResources["F3Key"] = "F3 key:";
            enResources["F4Key"] = "F4 key:";
            enResources["F5Key"] = "F5 key:";
            enResources["F6Key"] = "F6 key:";
            enResources["F7Key"] = "F7 key:";
            enResources["F8Key"] = "F8 key:";
            enResources["KeyAction"] = "Action:";
            enResources["UserExecuteOption"] = "User Execute Option:";
            enResources["ManageUserOptions"] = "Manage User Options...";
            enResources["KeyAssignmentError"] = "Key Assignment Error";
            enResources["DuplicateKeyAssignment"] = "The key '{0}' is already assigned to '{1}'. Assign it to '{2}' instead?";
            
            // Form_TotalCommander
            enResources["BuildDateTime"] = "Build Date: {0}";
            enResources["F5KeySettingAtStartup"] = "F5 key setting at startup: action={0}";
            enResources["F5KeyWithOption"] = "F5 key setting at startup: action={0}, option={1}";
            enResources["F5KeyExecutable"] = "Executable: {0}";
            enResources["LoadedKeySettings"] = "Loaded key settings:";
            enResources["KeySettingFormat"] = "{0}: {1}";
            enResources["KeySettingWithOption"] = "{0}: {1} ({2})";
            enResources["DebugKeyEvent"] = "Form_TotalCommander_KeyDown: KeyCode={0}, KeyData={1}, Handled={2}";
            
            // Add to resources dictionary
            _resources["en"] = enResources;
            
            // Initialize Korean resources
            Dictionary<string, string> koResources = new Dictionary<string, string>();
            
            // Common
            koResources["OK"] = "Ȯ��";
            koResources["Cancel"] = "���";
            koResources["Yes"] = "��";
            koResources["No"] = "�ƴϿ�";
            koResources["Error"] = "����";
            koResources["Warning"] = "���";
            koResources["Information"] = "����";
            koResources["Confirmation"] = "Ȯ��";
            koResources["Close"] = "�ݱ�";
            
            // File operations
            koResources["DeleteConfirmation"] = "�� �׸��� �����Ͻðڽ��ϱ�?";
            koResources["DeleteMultipleConfirmation"] = "{0}�� �׸��� �����Ͻðڽ��ϱ�?";
            koResources["CopyConfirmation"] = "���� Ȯ��";
            koResources["DeleteError"] = "���� ���� ����: {0}";
            koResources["CopyError"] = "���� ���� ����: {0}";
            koResources["CopySuccess"] = "{0}�� �׸� ���� �Ϸ�";
            koResources["SourcePath"] = "����: ";
            koResources["DestinationPath"] = "���: ";
            koResources["FileCount"] = "���� ��: {0}";
            koResources["CopySize"] = "���� ũ��: {0}";
            koResources["Copy"] = "����";
            koResources["CopyFailure"] = "���� �۾� ����";
            koResources["CopyInProgress"] = "���� ���� ��...";
            koResources["MoveInProgress"] = "���� �̵� ��...";
            koResources["ItemsSelected"] = "���õ�: {0}�� ����, {1}�� ���� | �� ũ��: {2}";
            koResources["FilesSelected"] = "���õ�: {0}�� ���� | �� ũ��: {1}";
            koResources["DirectoriesSelected"] = "���õ�: {0}�� ����";
            koResources["FilesAndDirectories"] = "{0}�� ����, {1}�� ����";
            koResources["NoItemsSelected"] = "���õ� �׸��� �����ϴ�.";
            
            // Navigation
            koResources["NavigationError"] = "��� �̵� ����: {0}";
            koResources["InvalidPath"] = "�߸��� ���";
            koResources["DirectoryNotFound"] = "���丮�� ã�� �� �����ϴ�";
            
            // Error messages
            koResources["ErrorReadingFolder"] = "���� ���� �д� �� ���� �߻�";
            koResources["ErrorProcessingFile"] = "���� ó�� �� ���� �߻�: {0}";
            
            // Delete operation
            koResources["DeleteConfirmTitle"] = "���� Ȯ��";
            koResources["DeleteFile"] = "'{0}' ������ �����Ͻðڽ��ϱ�?";
            koResources["DeleteMultiple"] = "{0}���� �׸��� �����Ͻðڽ��ϱ�?";
            koResources["DeletionError"] = "���� �� ���� �߻�: {0}";
            
            // New folder
            koResources["NewFolder"] = "�� ����";
            koResources["CreateNewFolder"] = "�� ���� �����";
            koResources["FolderName"] = "���� �̸�:";
            koResources["FolderExists"] = "���� �̸��� ������ �����մϴ�";
            koResources["InvalidFolderName"] = "�߸��� ���� �̸��Դϴ�";
            koResources["FolderCreated"] = "������ ���������� �����Ǿ����ϴ�";
            koResources["FolderCreationError"] = "���� ���� �� ���� �߻�: {0}";
            
            // File search
            koResources["Search"] = "�˻�";
            koResources["SearchResults"] = "�˻����";
            koResources["SearchText"] = "�˻���:";
            koResources["SearchLocation"] = "�˻���ġ:";
            koResources["IncludeSubfolders"] = "���� ���� ����";
            koResources["SearchingIn"] = "{0} �˻���...";
            koResources["NoResultsFound"] = "����� �����ϴ�.";
            koResources["SearchingCanceled"] = "�˻��� ��ҵǾ����ϴ�";
            koResources["FoundItems"] = "{0}���� �׸� �߰�";
            
            // Add new string resources for file rename
            koResources["RenameTitle"] = "�̸� ����";
            koResources["RenameError"] = "�̸� ���� ����";
            koResources["RenameMultipleNotAllowed"] = "�̸��� �����Ϸ��� �ϳ��� �׸� �����ؾ� �մϴ�";
            koResources["FolderAlreadyExists"] = "'{0}' ������ �����մϴ�";
            koResources["FileAlreadyExists"] = "'{0}' ������ �����մϴ�";
            koResources["RenameErrorMessage"] = "�̸��� �����ϴ� �� ������ �߻��߽��ϴ�: {0}";
            koResources["FileOpenError"] = "������ �� �� �����ϴ�: {0}";
            koResources["PathNotFound"] = "��θ� ã�� �� �����ϴ�: {0}";
            
            // Add new string resources for file operations
            koResources["InvalidPathReason"] = "���� ���� �� �ϳ��� ��ΰ� ��ȿ���� �ʽ��ϴ�: ���̰� 0�� ���ڿ��̰ų�, ���鸸 �ְų�, ��ȿ���� ���� ���ڰ� ���Եǰų�, ��ġ ���(\\\\.\\�� ����)�Դϴ�";
            koResources["FileNotFound"] = "�� ���� �Ǵ� ���丮�� �������� �ʽ��ϴ�.";
            koResources["FileNameTooLong"] = "���� �̸��� �ý��ۿ��� ������ �ִ� ���̸� �ʰ��մϴ�";
            koResources["InvalidName"] = "��ȿ���� ���� �̸�";
            koResources["PermissionDenied"] = "�� �����̳� ������ ���� ������ ������� �ʽ��ϴ�.";
            koResources["InvalidFileName"] = "��ȿ���� ���� ���� �̸�";
            koResources["ReadOnlyParentDirectory"] = "������ ������ ���� ���丮�� �б� �����Դϴ�";
            koResources["CannotCreateFileInThisFolder"] = "�� ������ ������ ������ �� �����ϴ�";
            koResources["InvalidDirectoryName"] = "��ȿ���� ���� ���丮 �̸�";
            koResources["DirectoryNameTooLong"] = "���丮 �̸��� �ʹ� ��ϴ�";
            koResources["CannotCreateDirectoryInThisFolder"] = "�� ������ ���丮�� ������ �� �����ϴ�";
            koResources["AccessDeniedOnFile"] = "���Ͽ� ���� ������ �źεǾ����ϴ�{0}{1}";
            koResources["AccessDeniedOnFolder"] = "������ ���� ������ �źεǾ����ϴ�{0}{1}";
            koResources["TotalCommander"] = "��Ż Ŀ�Ǵ�";
            koResources["SelectedFilesDirs"] = "���õ� ���� {0}��, ���� {1}�� | �� ũ��: {2}";
            koResources["SelectedFiles"] = "���õ� ���� {0}�� | �� ũ��: {1}";
            koResources["SelectedDirs"] = "���õ� ���� {0}��";
            koResources["FolderSummary"] = "���� {0}��, ���� {1}��";
            koResources["ErrorReadingFolderContents"] = "���� ������ �д� �� ������ �߻��߽��ϴ�";
            
            // Debug and error messages
            koResources["ParameterDebugInfo"] = "�Ķ���� ����� ����:";
            koResources["LeftExplorerPath"] = "���� Ž���� ���: {0}";
            koResources["RightExplorerPath"] = "������ Ž���� ���: {0}";
            koResources["FocusingExplorerPath"] = "���� ��Ŀ�� Ž���� ���: {0}";
            koResources["OriginalParameters"] = "���� �Ķ����: {0}";
            koResources["LeftDirectoryPath"] = "���� ���丮 ���: {0}";
            koResources["RightDirectoryPath"] = "������ ���丮 ���: {0}";
            koResources["FocusingDirectoryPath"] = "��Ŀ�� ���丮 ���: {0}";
            koResources["ParametersAfterSubstitution"] = "ġȯ �� �Ķ����: {0}";
            koResources["ExecutionError"] = "���� ����";
            koResources["ExecutionErrorMessage"] = "���� �� ������ �߻��߽��ϴ�: {0}";
            koResources["F5KeySettingBeforeSave"] = "������ F5 Ű ����: �׼�={0}";
            koResources["F5KeySettingVerification"] = "����� F5 Ű ���� Ȯ��: �׼�={0}";
            koResources["SettingsSaveConfirmation"] = "���� ���� Ȯ��";
            koResources["SettingsVerificationError"] = "���� ���� ����";
            koResources["SavedSettingsVerificationError"] = "����� ���� ���� ���� ����: {0}";
            koResources["SettingsSaveError"] = "���� ���� ����";
            koResources["ErrorSavingSettings"] = "������ �����ϴ� �� ������ �߻��߽��ϴ�: {0}";
            koResources["SettingsFilePath"] = "���� ���� ���: {0}";
            koResources["FileExistence"] = "���� ���� ����: {0}";
            koResources["LoadedF5KeySetting"] = "�ε�� F5 Ű ����: �׼�={0}";
            koResources["NoF5KeySettingInLoadedSettings"] = "�ε�� ������ F5 Ű ������ �����ϴ�!";
            koResources["SettingsLoadError"] = "���� �ε� ����";
            koResources["NullSettingsReturned"] = "������ �ε�Ǿ����� null ���� ��ȯ�Ǿ����ϴ�";
            koResources["ErrorLoadingSettings"] = "������ �ҷ����� �� ������ �߻��߽��ϴ�: {0}";
            
            // FormUserExecuteOption
            koResources["UserExecuteOptionTitle"] = "����� ���� �ɼ�";
            koResources["ExecuteOptionName"] = "���� �ɼ� �̸�:";
            koResources["ExecutableOption"] = "���� ���� �ɼ�:";
            koResources["Parameters"] = "�Ű�����";
            koResources["ParametersHint"] = "��Ʈ: �Ʒ� �Ű����� ������ Ŭ���ϸ� �Ű����� �ؽ�Ʈ�� �ڵ� �߰��˴ϴ�";
            koResources["Save"] = "����";
            koResources["SelectExecutableTitle"] = "���� ���� ����";
            koResources["ExecutableFilter"] = "���� ���� (*.exe)|*.exe|��� ���� (*.*)|*.*";
            koResources["ExecuteOptionNameRequired"] = "���� �ɼ� �̸��� �Է��ϼ���";
            koResources["ExecutablePathRequired"] = "���� ���� ��θ� �Է��ϼ���";
            koResources["ExecuteOptionNameExists"] = "�� �̸��� ���� �ɼ��� �̹� �����մϴ�";
            koResources["LeftSelectedItemPath"] = "���� ���� ��Ͽ��� ���õ� �׸��� ��ü ���";
            koResources["LeftSelectedItemDirPath"] = "���� ���� ��Ͽ��� ���õ� �׸��� ���丮 ���";
            koResources["RightSelectedItemPath"] = "������ ���� ��Ͽ��� ���õ� �׸��� ��ü ���";
            koResources["RightSelectedItemDirPath"] = "������ ���� ��Ͽ��� ���õ� �׸��� ���丮 ���";
            koResources["FocusingSelectedItemPath"] = "���� ��Ŀ���� ���� ��Ͽ��� ���õ� �׸��� ��ü ���";
            koResources["FocusingSelectedItemDirPath"] = "���� ��Ŀ���� ���� ��Ͽ��� ���õ� �׸��� ���丮 ���";
            
            // FormManageUserOptions
            koResources["ManageUserOptionsTitle"] = "����� ���� �ɼ� ����";
            koResources["AddUserOption"] = "�߰�...";
            koResources["EditUserOption"] = "����...";
            koResources["DeleteUserOption"] = "����";
            koResources["UserOptionDeleteConfirmation"] = "'{0}' �ɼ��� �����Ͻðڽ��ϱ�?\n\n�� �ɼ��� ����ϴ� ��� Ű ������ �ʱ�ȭ�˴ϴ�.";
            koResources["UserOptionDeleteConfirmationTitle"] = "�ɼ� ���� Ȯ��";
            koResources["UserOptionDeleteError"] = "�ɼ� ���� �� ����: {0}";
            koResources["UserOptionError"] = "����� �ɼ� ����";
            koResources["NoOptionSelected"] = "���õ� ����� �ɼ��� �����ϴ�.";
            
            // Form_TotalCommander
            koResources["BuildDateTime"] = "���� �Ͻ�: {0}";
            koResources["F5KeySettingAtStartup"] = "���� �� F5 Ű ����: �׼�={0}";
            koResources["F5KeyWithOption"] = "���� �� F5 Ű ����: �׼�={0}, �ɼ�={1}";
            koResources["F5KeyExecutable"] = "���� ����: {0}";
            koResources["LoadedKeySettings"] = "�ε�� Ű ����:";
            koResources["KeySettingFormat"] = "{0}: {1}";
            koResources["KeySettingWithOption"] = "{0}: {1} ({2})";
            koResources["DebugKeyEvent"] = "Form_TotalCommander_KeyDown: KeyCode={0}, KeyData={1}, Handled={2}";
            
            // FormKeySettings
            koResources["KeySettingsTitle"] = "��� Ű ����";
            koResources["F2Key"] = "F2 Ű:";
            koResources["F3Key"] = "F3 Ű:";
            koResources["F4Key"] = "F4 Ű:";
            koResources["F5Key"] = "F5 Ű:";
            koResources["F6Key"] = "F6 Ű:";
            koResources["F7Key"] = "F7 Ű:";
            koResources["F8Key"] = "F8 Ű:";
            koResources["KeyAction"] = "����:";
            koResources["UserExecuteOption"] = "����� ���� �ɼ�:";
            koResources["ManageUserOptions"] = "����� �ɼ� ����...";
            koResources["KeyAssignmentError"] = "Ű �Ҵ� ����";
            koResources["DuplicateKeyAssignment"] = "'{0}' Ű�� �̹� '{1}'�� �Ҵ�Ǿ� �ֽ��ϴ�. ��� '{2}'�� �Ҵ��Ͻðڽ��ϱ�?";
            
            // Add to resources dictionary
            _resources["ko"] = koResources;
            
            // Set the current language based on system culture
            string systemLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            if (_resources.ContainsKey(systemLanguage))
            {
                _currentLanguage = systemLanguage;
            }
        }

        /// <summary>
        /// Set the current language
        /// </summary>
        /// <param name="languageCode">Two-letter ISO language code (e.g., "en", "ko")</param>
        public static void SetLanguage(string languageCode)
        {
            if (_resources.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
            }
        }

        /// <summary>
        /// Get string resource by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        public static string GetString(string key)
        {
            if (_resources.ContainsKey(_currentLanguage) && _resources[_currentLanguage].ContainsKey(key))
            {
                return _resources[_currentLanguage][key];
            }
            
            // Fallback to English
            if (_resources["en"].ContainsKey(key))
            {
                return _resources["en"][key];
            }
            
            // Return key if not found
            return key;
        }

        /// <summary>
        /// Get formatted string resource by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        public static string GetString(string key, params object[] args)
        {
            string format = GetString(key);
            return string.Format(format, args);
        }
    }
} 
