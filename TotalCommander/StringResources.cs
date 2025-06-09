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
            enResources["RenameFailed"] = "Failed to rename item: ";
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
            koResources["OK"] = "확인";
            koResources["Cancel"] = "취소";
            koResources["Yes"] = "예";
            koResources["No"] = "아니오";
            koResources["Error"] = "오류";
            koResources["Warning"] = "경고";
            koResources["Information"] = "정보";
            koResources["Confirmation"] = "확인";
            koResources["Close"] = "닫기";
            
            // File operations
            koResources["DeleteConfirmation"] = "이 항목을 삭제하시겠습니까?";
            koResources["DeleteMultipleConfirmation"] = "{0}개 항목을 삭제하시겠습니까?";
            koResources["CopyConfirmation"] = "복사 확인";
            koResources["DeleteError"] = "파일 삭제 오류: {0}";
            koResources["CopyError"] = "파일 복사 오류: {0}";
            koResources["CopySuccess"] = "{0}개 항목 복사 완료";
            koResources["SourcePath"] = "원본: ";
            koResources["DestinationPath"] = "대상: ";
            koResources["FileCount"] = "파일 수: {0}";
            koResources["CopySize"] = "복사 크기: {0}";
            koResources["Copy"] = "복사";
            koResources["CopyFailure"] = "복사 작업 실패";
            koResources["CopyInProgress"] = "파일 복사 중...";
            koResources["MoveInProgress"] = "파일 이동 중...";
            koResources["ItemsSelected"] = "선택됨: {0}개 파일, {1}개 폴더 | 총 크기: {2}";
            koResources["FilesSelected"] = "선택됨: {0}개 파일 | 총 크기: {1}";
            koResources["DirectoriesSelected"] = "선택됨: {0}개 폴더";
            koResources["FilesAndDirectories"] = "{0}개 파일, {1}개 폴더";
            koResources["NoItemsSelected"] = "선택된 항목이 없습니다.";
            
            // Navigation
            koResources["NavigationError"] = "경로 이동 오류: {0}";
            koResources["InvalidPath"] = "잘못된 경로";
            koResources["DirectoryNotFound"] = "디렉토리를 찾을 수 없습니다";
            
            // Error messages
            koResources["ErrorReadingFolder"] = "폴더 내용 읽는 중 오류 발생";
            koResources["ErrorProcessingFile"] = "파일 처리 중 오류 발생: {0}";
            
            // Delete operation
            koResources["DeleteConfirmTitle"] = "삭제 확인";
            koResources["DeleteFile"] = "'{0}' 파일을 삭제하시겠습니까?";
            koResources["DeleteMultiple"] = "{0}개의 항목을 삭제하시겠습니까?";
            koResources["DeletionError"] = "삭제 중 오류 발생: {0}";
            
            // New folder
            koResources["NewFolder"] = "새 폴더";
            koResources["CreateNewFolder"] = "새 폴더 생성";
            koResources["FolderName"] = "폴더 이름:";
            koResources["FolderExists"] = "같은 이름의 폴더가 이미 있습니다";
            koResources["InvalidFolderName"] = "잘못된 폴더 이름입니다";
            koResources["FolderCreated"] = "폴더가 성공적으로 생성되었습니다";
            koResources["FolderCreationError"] = "폴더 생성 중 오류 발생: {0}";
            
            // File search
            koResources["Search"] = "검색";
            koResources["SearchResults"] = "검색결과";
            koResources["SearchText"] = "검색어:";
            koResources["SearchLocation"] = "검색위치:";
            koResources["IncludeSubfolders"] = "하위 폴더 포함";
            koResources["SearchingIn"] = "{0} 검색중...";
            koResources["NoResultsFound"] = "결과가 없습니다.";
            koResources["SearchingCanceled"] = "검색이 취소되었습니다";
            koResources["FoundItems"] = "{0}개의 항목 발견";
            
            // Add new string resources for file rename
            koResources["RenameTitle"] = "이름 변경";
            koResources["RenameError"] = "이름 변경 오류";
            koResources["RenameMultipleNotAllowed"] = "이름을 변경하려면 하나의 항목만 선택해야 합니다";
            koResources["FolderAlreadyExists"] = "'{0}' 폴더가 이미 있습니다";
            koResources["FileAlreadyExists"] = "'{0}' 파일이 이미 있습니다";
            koResources["RenameErrorMessage"] = "이름을 변경하는 중 오류가 발생했습니다: {0}";
            koResources["RenameFailed"] = "이름 변경 실패: ";
            koResources["FileOpenError"] = "파일을 열 수 없습니다: {0}";
            koResources["PathNotFound"] = "경로를 찾을 수 없습니다: {0}";
            
            // Add new string resources for file operations
            koResources["InvalidPathReason"] = "다음 이유 중 하나로 경로가 유효하지 않습니다: 길이가 0인 문자열이거나, 공백만 있거나, 유효하지 않은 문자가 포함되거나, 장치 경로(\\\\.\\로 시작)입니다";
            koResources["FileNotFound"] = "이 파일 또는 디렉토리가 존재하지 않습니다.";
            koResources["FileNameTooLong"] = "파일 이름이 시스템에서 정의된 최대 길이를 초과합니다";
            koResources["InvalidName"] = "유효하지 않은 이름";
            koResources["PermissionDenied"] = "이 파일이나 폴더에 대한 권한이 없습니다.";
            koResources["InvalidFileName"] = "유효하지 않은 파일 이름";
            koResources["ReadOnlyParentDirectory"] = "파일을 생성할 상위 디렉토리가 읽기 전용입니다";
            koResources["CannotCreateFileInThisFolder"] = "이 폴더에 파일을 생성할 수 없습니다";
            koResources["InvalidDirectoryName"] = "유효하지 않은 디렉토리 이름";
            koResources["DirectoryNameTooLong"] = "디렉토리 이름이 너무 깁니다";
            koResources["CannotCreateDirectoryInThisFolder"] = "이 폴더에 디렉토리를 생성할 수 없습니다";
            koResources["AccessDeniedOnFile"] = "파일에 대한 접근이 거부되었습니다{0}{1}";
            koResources["AccessDeniedOnFolder"] = "폴더에 대한 접근이 거부되었습니다{0}{1}";
            koResources["TotalCommander"] = "토탈 커맨더";
            koResources["SelectedFilesDirs"] = "선택된 파일 {0}개, 폴더 {1}개 | 총 크기: {2}";
            koResources["SelectedFiles"] = "선택된 파일 {0}개 | 총 크기: {1}";
            koResources["SelectedDirs"] = "선택된 폴더 {0}개";
            koResources["FolderSummary"] = "파일 {0}개, 폴더 {1}개";
            koResources["ErrorReadingFolderContents"] = "폴더 내용을 읽는 중 오류가 발생했습니다";
            
            // Debug and error messages
            koResources["ParameterDebugInfo"] = "파라미터 디버그 정보:";
            koResources["LeftExplorerPath"] = "왼쪽 탐색기 경로: {0}";
            koResources["RightExplorerPath"] = "오른쪽 탐색기 경로: {0}";
            koResources["FocusingExplorerPath"] = "현재 포커스 탐색기 경로: {0}";
            koResources["OriginalParameters"] = "원본 파라미터: {0}";
            koResources["LeftDirectoryPath"] = "왼쪽 디렉토리 경로: {0}";
            koResources["RightDirectoryPath"] = "오른쪽 디렉토리 경로: {0}";
            koResources["FocusingDirectoryPath"] = "포커스 디렉토리 경로: {0}";
            koResources["ParametersAfterSubstitution"] = "치환 후 파라미터: {0}";
            koResources["ExecutionError"] = "실행 오류";
            koResources["ExecutionErrorMessage"] = "실행 중 오류가 발생했습니다: {0}";
            koResources["F5KeySettingBeforeSave"] = "저장할 F5 키 설정: 액션={0}";
            koResources["F5KeySettingVerification"] = "저장된 F5 키 설정 확인: 액션={0}";
            koResources["SettingsSaveConfirmation"] = "설정 저장 확인";
            koResources["SettingsVerificationError"] = "설정 확인 오류";
            koResources["SavedSettingsVerificationError"] = "저장된 설정 파일 확인 오류: {0}";
            koResources["SettingsSaveError"] = "설정 저장 오류";
            koResources["ErrorSavingSettings"] = "설정을 저장하는 중 오류가 발생했습니다: {0}";
            koResources["SettingsFilePath"] = "설정 파일 경로: {0}";
            koResources["FileExistence"] = "파일 존재 여부: {0}";
            koResources["LoadedF5KeySetting"] = "로드된 F5 키 설정: 액션={0}";
            koResources["NoF5KeySettingInLoadedSettings"] = "로드된 설정에 F5 키 설정이 없습니다!";
            koResources["SettingsLoadError"] = "설정 로드 오류";
            koResources["NullSettingsReturned"] = "설정이 로드되었으나 null 값이 반환되었습니다";
            koResources["ErrorLoadingSettings"] = "설정을 불러오는 중 오류가 발생했습니다: {0}";
            
            // FormUserExecuteOption
            koResources["UserExecuteOptionTitle"] = "사용자 실행 옵션";
            koResources["ExecuteOptionName"] = "실행 옵션 이름:";
            koResources["ExecutableOption"] = "실행 파일 옵션:";
            koResources["Parameters"] = "매개변수";
            koResources["ParametersHint"] = "힌트: 아래 매개변수 변수를 클릭하면 매개변수 텍스트에 자동 추가됩니다";
            koResources["Save"] = "저장";
            koResources["SelectExecutableTitle"] = "실행 파일 선택";
            koResources["ExecutableFilter"] = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*";
            koResources["ExecuteOptionNameRequired"] = "실행 옵션 이름을 입력하세요";
            koResources["ExecutablePathRequired"] = "실행 파일 경로를 입력하세요";
            koResources["ExecuteOptionNameExists"] = "이 이름의 실행 옵션이 이미 존재합니다";
            koResources["LeftSelectedItemPath"] = "왼쪽 파일 목록에서 선택된 항목의 전체 경로";
            koResources["LeftSelectedItemDirPath"] = "왼쪽 파일 목록에서 선택된 항목의 디렉토리 경로";
            koResources["RightSelectedItemPath"] = "오른쪽 파일 목록에서 선택된 항목의 전체 경로";
            koResources["RightSelectedItemDirPath"] = "오른쪽 파일 목록에서 선택된 항목의 디렉토리 경로";
            koResources["FocusingSelectedItemPath"] = "현재 포커스된 파일 목록에서 선택된 항목의 전체 경로";
            koResources["FocusingSelectedItemDirPath"] = "현재 포커스된 파일 목록에서 선택된 항목의 디렉토리 경로";
            
            // FormManageUserOptions
            koResources["ManageUserOptionsTitle"] = "사용자 실행 옵션 관리";
            koResources["AddUserOption"] = "추가...";
            koResources["EditUserOption"] = "편집...";
            koResources["DeleteUserOption"] = "삭제";
            koResources["UserOptionDeleteConfirmation"] = "'{0}' 옵션을 삭제하시겠습니까?\n\n이 옵션을 사용하는 모든 키 설정이 초기화됩니다.";
            koResources["UserOptionDeleteConfirmationTitle"] = "옵션 삭제 확인";
            koResources["UserOptionDeleteError"] = "옵션 삭제 중 오류: {0}";
            koResources["UserOptionError"] = "사용자 옵션 오류";
            koResources["NoOptionSelected"] = "선택된 사용자 옵션이 없습니다.";
            
            // Form_TotalCommander
            koResources["BuildDateTime"] = "빌드 시간: {0}";
            koResources["F5KeySettingAtStartup"] = "시작 시 F5 키 설정: 액션={0}";
            koResources["F5KeyWithOption"] = "시작 시 F5 키 설정: 액션={0}, 옵션={1}";
            koResources["F5KeyExecutable"] = "실행 파일: {0}";
            koResources["LoadedKeySettings"] = "로드된 키 설정:";
            koResources["KeySettingFormat"] = "{0}: {1}";
            koResources["KeySettingWithOption"] = "{0}: {1} ({2})";
            koResources["DebugKeyEvent"] = "Form_TotalCommander_KeyDown: KeyCode={0}, KeyData={1}, Handled={2}";
            
            // FormKeySettings
            koResources["KeySettingsTitle"] = "기능 키 설정";
            koResources["F2Key"] = "F2 키:";
            koResources["F3Key"] = "F3 키:";
            koResources["F4Key"] = "F4 키:";
            koResources["F5Key"] = "F5 키:";
            koResources["F6Key"] = "F6 키:";
            koResources["F7Key"] = "F7 키:";
            koResources["F8Key"] = "F8 키:";
            koResources["KeyAction"] = "동작:";
            koResources["UserExecuteOption"] = "사용자 실행 옵션:";
            koResources["ManageUserOptions"] = "사용자 옵션 관리...";
            koResources["KeyAssignmentError"] = "키 할당 오류";
            koResources["DuplicateKeyAssignment"] = "'{0}' 키는 이미 '{1}'에 할당되어 있습니다. 대신 '{2}'에 할당하시겠습니까?";
            
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
