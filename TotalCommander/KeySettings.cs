using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace TotalCommander
{
    /// <summary>
    /// 사용 가능한 기능 키 액션 목록
    /// </summary>
    public enum KeyAction
    {
        None,
        View,               // 파일 보기
        Edit,               // 파일 편집
        Copy,               // 복사
        Cut,                // 잘라내기
        Paste,              // 붙여넣기
        Delete,             // 삭제
        CreateFolder,       // 새 폴더 생성
        Properties,         // 속성
        Refresh,            // 새로고침
        GoParent,           // 상위 폴더로 이동
        GoBack,             // 뒤로 이동
        GoForward,          // 앞으로 이동
        Exit,               // 종료
        Rename,             // 이름 변경
        UserExecute         // 사용자 정의 실행 옵션
    }

    /// <summary>
    /// 사용자 정의 실행 옵션 클래스
    /// </summary>
    [Serializable]
    public class UserExecuteOption
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string Parameters { get; set; }

        public UserExecuteOption()
        {
            // 기본 생성자 (직렬화용)
            Name = "";
            ExecutablePath = "";
            Parameters = "";
        }

        public UserExecuteOption(string name, string executablePath, string parameters)
        {
            Name = name;
            ExecutablePath = executablePath;
            Parameters = parameters;
        }

        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// 사용자 실행 옵션을 실행합니다.
        /// </summary>
        /// <param name="leftExplorerFullPath">왼쪽 파일 목록에서 선택된 항목의 전체 경로</param>
        /// <param name="rightExplorerFullPath">오른쪽 파일 목록에서 선택된 항목의 전체 경로</param>
        /// <param name="focusingExplorerFullPath">현재 포커싱된 파일표시창에 선택된 항목의 전체 경로</param>
        public void Execute(string leftExplorerFullPath, string rightExplorerFullPath, string focusingExplorerFullPath = null)
        {
            try
            {
                // 디버깅 정보 - 각 경로 확인
                System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
                debugInfo.AppendLine("파라미터 변수 디버그 정보:");
                debugInfo.AppendLine($"왼쪽 탐색기 경로: {leftExplorerFullPath ?? "없음"}");
                debugInfo.AppendLine($"오른쪽 탐색기 경로: {rightExplorerFullPath ?? "없음"}");
                debugInfo.AppendLine($"현재 포커싱 탐색기 경로: {focusingExplorerFullPath ?? "없음"}");
                debugInfo.AppendLine($"원본 파라미터: {Parameters}");

                // 파라미터 변수 치환
                string parameters = Parameters;
                
                // 경로 유효성 검사 - null이면 빈 문자열로 대체
                leftExplorerFullPath = leftExplorerFullPath ?? string.Empty;
                rightExplorerFullPath = rightExplorerFullPath ?? string.Empty;
                focusingExplorerFullPath = focusingExplorerFullPath ?? string.Empty;
                
                // 디렉토리 경로 미리 계산
                string leftExplorerDirPath = string.Empty;
                string rightExplorerDirPath = string.Empty;
                string focusingExplorerDirPath = string.Empty;
                
                // 디렉토리 경로 안전하게 추출
                if (!string.IsNullOrEmpty(leftExplorerFullPath))
                {
                    try {
                        leftExplorerDirPath = Path.GetDirectoryName(leftExplorerFullPath) ?? leftExplorerFullPath;
                    } catch {
                        leftExplorerDirPath = leftExplorerFullPath;
                    }
                    debugInfo.AppendLine($"왼쪽 디렉토리 경로: {leftExplorerDirPath}");
                }
                
                if (!string.IsNullOrEmpty(rightExplorerFullPath))
                {
                    try {
                        rightExplorerDirPath = Path.GetDirectoryName(rightExplorerFullPath) ?? rightExplorerFullPath;
                    } catch {
                        rightExplorerDirPath = rightExplorerFullPath;
                    }
                    debugInfo.AppendLine($"오른쪽 디렉토리 경로: {rightExplorerDirPath}");
                }
                
                if (!string.IsNullOrEmpty(focusingExplorerFullPath))
                {
                    try {
                        focusingExplorerDirPath = Path.GetDirectoryName(focusingExplorerFullPath) ?? focusingExplorerFullPath;
                    } catch {
                        focusingExplorerDirPath = focusingExplorerFullPath;
                    }
                    debugInfo.AppendLine($"포커싱 디렉토리 경로: {focusingExplorerDirPath}");
                }
                
                // 모든 변수 치환 - 가장 긴 변수명부터 치환 (짧은 변수명이 긴 변수명의 일부일 경우를 대비)
                // 왼쪽 탐색기 변수 치환
                parameters = parameters.Replace("{SelectedItemFullPath:LeftExplorer}", leftExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:LeftExplorer}", leftExplorerDirPath);
                
                // 오른쪽 탐색기 변수 치환
                parameters = parameters.Replace("{SelectedItemFullPath:RightExplorer}", rightExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:RightExplorer}", rightExplorerDirPath);
                
                // 포커싱 탐색기 변수 치환
                parameters = parameters.Replace("{SelectedItemFullPath:FocusingExplorer}", focusingExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:FocusingExplorer}", focusingExplorerDirPath);
                
                // 하위 호환성을 위한 이전 변수명 치환
                parameters = parameters.Replace("{Explorer1:SelectedItem}", leftExplorerFullPath);
                parameters = parameters.Replace("{Explorer2:SelectedItem}", rightExplorerFullPath);
                parameters = parameters.Replace("{ActiveExplorer:SelectedItem}", focusingExplorerFullPath);
                
                debugInfo.AppendLine($"치환 후 파라미터: {parameters}");

                // 디버그 정보를 로그 파일에 기록
                Logger.DebugMultiline($"UserExecuteOption({Name})", debugInfo.ToString());
                
                // 프로세스 시작
                Logger.Info($"Executing: {ExecutablePath} {parameters}");
                System.Diagnostics.Process.Start(ExecutablePath, parameters);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"UserExecuteOption({Name}) execution failed");
                System.Windows.Forms.MessageBox.Show($"실행 중 오류가 발생했습니다: {ex.Message}", "실행 오류", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 단일 키 설정 항목
    /// </summary>
    [Serializable]
    public class KeySetting
    {
        public Keys Key { get; set; }
        public KeyAction Action { get; set; }
        public string UserExecuteOptionName { get; set; }

        public KeySetting()
        {
            // 기본 생성자 (직렬화용)
            UserExecuteOptionName = "";
        }

        public KeySetting(Keys key, KeyAction action)
        {
            Key = key;
            Action = action;
            UserExecuteOptionName = "";
        }

        public KeySetting(Keys key, KeyAction action, string userExecuteOptionName)
        {
            Key = key;
            Action = action;
            UserExecuteOptionName = userExecuteOptionName;
        }
    }

    /// <summary>
    /// 모든 키 설정을 관리하는 클래스
    /// </summary>
    [Serializable]
    public class KeySettings
    {
        public List<KeySetting> Settings { get; set; }
        public List<UserExecuteOption> UserExecuteOptions { get; set; }

        private static string SettingsFilePath => GetSettingsFilePath();

        /// <summary>
        /// 설정 파일의 전체 경로를 반환합니다.
        /// </summary>
        public static string GetSettingsFilePath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
            return Path.Combine(tcFolder, "KeySettings.xml");
        }

        public KeySettings()
        {
            Settings = new List<KeySetting>();
            UserExecuteOptions = new List<UserExecuteOption>();
            // 기본값 설정 메서드 호출 제거 (생성자에서 자동 호출하지 않음)
            // SetDefaults();
        }

        /// <summary>
        /// 기본 키 설정으로 초기화
        /// </summary>
        public void SetDefaults()
        {
            Settings.Clear();
            Settings.Add(new KeySetting(Keys.F2, KeyAction.Refresh));
            Settings.Add(new KeySetting(Keys.F3, KeyAction.View));
            Settings.Add(new KeySetting(Keys.F4, KeyAction.Edit));
            Settings.Add(new KeySetting(Keys.F5, KeyAction.Copy));
            Settings.Add(new KeySetting(Keys.F6, KeyAction.Cut));
            Settings.Add(new KeySetting(Keys.F7, KeyAction.CreateFolder));
            Settings.Add(new KeySetting(Keys.F8, KeyAction.Delete));
        }

        /// <summary>
        /// 특정 키에 해당하는 액션 찾기
        /// </summary>
        public KeyAction GetActionForKey(Keys key)
        {
            foreach (var setting in Settings)
            {
                if (setting.Key == key)
                    return setting.Action;
            }
            return KeyAction.None;
        }

        /// <summary>
        /// 특정 키에 해당하는 사용자 실행 옵션 이름 찾기
        /// </summary>
        public string GetUserExecuteOptionNameForKey(Keys key)
        {
            foreach (var setting in Settings)
            {
                if (setting.Key == key && setting.Action == KeyAction.UserExecute)
                    return setting.UserExecuteOptionName;
            }
            return "";
        }

        /// <summary>
        /// 특정 키에 해당하는 사용자 실행 옵션 찾기
        /// </summary>
        public UserExecuteOption GetUserExecuteOptionForKey(Keys key)
        {
            string optionName = GetUserExecuteOptionNameForKey(key);
            if (string.IsNullOrEmpty(optionName))
                return null;

            foreach (var option in UserExecuteOptions)
            {
                if (option.Name == optionName)
                    return option;
            }
            return null;
        }

        /// <summary>
        /// 이름으로 사용자 실행 옵션 찾기
        /// </summary>
        public UserExecuteOption GetUserExecuteOptionByName(string name)
        {
            foreach (var option in UserExecuteOptions)
            {
                if (option.Name == name)
                    return option;
            }
            return null;
        }

        /// <summary>
        /// 사용자 실행 옵션 추가 또는 업데이트
        /// </summary>
        public void AddOrUpdateUserExecuteOption(UserExecuteOption option)
        {
            // 기존 옵션 찾기
            for (int i = 0; i < UserExecuteOptions.Count; i++)
            {
                if (UserExecuteOptions[i].Name == option.Name)
                {
                    // 이름이 같은 옵션이 있으면 업데이트
                    UserExecuteOptions[i] = option;
                    return;
                }
            }

            // 새 옵션 추가
            UserExecuteOptions.Add(option);
        }

        /// <summary>
        /// 사용자 실행 옵션 제거
        /// </summary>
        public bool RemoveUserExecuteOption(string name)
        {
            // 삭제할 옵션 찾기
            for (int i = 0; i < UserExecuteOptions.Count; i++)
            {
                if (UserExecuteOptions[i].Name == name)
                {
                    // 옵션 제거
                    UserExecuteOptions.RemoveAt(i);

                    // 이 옵션을 사용하는 키 설정 업데이트
                    foreach (var setting in Settings)
                    {
                        if (setting.Action == KeyAction.UserExecute && setting.UserExecuteOptionName == name)
                        {
                            setting.Action = KeyAction.None;
                            setting.UserExecuteOptionName = "";
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 설정을 파일에 저장
        /// </summary>
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // 디버깅용 메시지 - 저장 전 F5 키 설정 확인
                string f5Debug = "";
                foreach (var setting in Settings)
                {
                    if (setting.Key == Keys.F5)
                    {
                        f5Debug = $"저장 전 F5 키 설정: 액션={setting.Action}";
                        if (setting.Action == KeyAction.UserExecute)
                            f5Debug += $", 옵션={setting.UserExecuteOptionName}";
                        // 메시지 박스를 로그로 대체
                        Logger.Info(f5Debug);
                        break;
                    }
                }

                XmlSerializer serializer = new XmlSerializer(typeof(KeySettings));
                using (StreamWriter writer = new StreamWriter(SettingsFilePath))
                {
                    serializer.Serialize(writer, this);
                }

                // 저장 후 즉시 파일을 다시 읽어서 확인
                if (File.Exists(SettingsFilePath))
                {
                    KeySettings verifySettings = null;
                    try
                    {
                        XmlSerializer readSerializer = new XmlSerializer(typeof(KeySettings));
                        using (StreamReader reader = new StreamReader(SettingsFilePath))
                        {
                            verifySettings = (KeySettings)readSerializer.Deserialize(reader);
                        }

                        if (verifySettings != null)
                        {
                            foreach (var setting in verifySettings.Settings)
                            {
                                if (setting.Key == Keys.F5)
                                {
                                    string verifyMsg = $"저장 후 F5 키 설정 확인: 액션={setting.Action}";
                                    if (setting.Action == KeyAction.UserExecute)
                                        verifyMsg += $", 옵션={setting.UserExecuteOptionName}";
                                    //MessageBox.Show(verifyMsg, "설정 저장 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 로그에 기록하고 메시지 박스 표시
                        Logger.Error(ex, "저장된 설정 파일 검증 중 오류");
                        MessageBox.Show("저장된 설정 파일 검증 중 오류: " + ex.Message,
                                      "설정 검증 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // 로그에 기록하고 메시지 박스 표시
                Logger.Error(ex, "설정을 저장하는 중 오류가 발생했습니다.");
                MessageBox.Show("설정을 저장하는 중 오류가 발생했습니다: " + ex.Message,
                              "설정 저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 파일에서 설정 불러오기
        /// </summary>
        public static KeySettings Load()
        {
            // 설정 파일 경로
            string settingsPath = SettingsFilePath;
            
            // 디버깅 - 설정 파일 경로 확인 (메시지 박스를 로그로 대체)
            Logger.Info($"설정 파일 경로: {settingsPath}");
            Logger.Info($"파일 존재 여부: {File.Exists(settingsPath)}");
            
            if (!File.Exists(settingsPath))
            {
                KeySettings newSettings = new KeySettings();
                // 중요: 여기서 기본값 설정하지 않음 - SetDefaults() 호출 제거
                // newSettings.SetDefaults(); 
                newSettings.Save(); // 설정 파일이 없으면 빈 설정으로 저장
                return newSettings;
            }

            try
            {
                KeySettings loadedSettings = null;
                XmlSerializer serializer = new XmlSerializer(typeof(KeySettings));
                using (StreamReader reader = new StreamReader(settingsPath))
                {
                    loadedSettings = (KeySettings)serializer.Deserialize(reader);
                }

                // 디버깅용 메시지 - 로드된 F5 키 설정 확인
                if (loadedSettings != null)
                {
                    // 중요: 빈 설정 목록을 기본값으로 초기화하지 않음
                    if (loadedSettings.Settings == null)
                        loadedSettings.Settings = new List<KeySetting>();
                        
                    if (loadedSettings.UserExecuteOptions == null)
                        loadedSettings.UserExecuteOptions = new List<UserExecuteOption>();
                        
                    // F5 키 설정 확인
                    bool hasF5Setting = false;
                    foreach (var setting in loadedSettings.Settings)
                    {
                        if (setting.Key == Keys.F5)
                        {
                            hasF5Setting = true;
                            string loadMsg = $"로드된 F5 키 설정: 액션={setting.Action}";
                            if (setting.Action == KeyAction.UserExecute)
                                loadMsg += $", 옵션={setting.UserExecuteOptionName}";
                            // 메시지 박스를 로그로 대체
                            Logger.Info(loadMsg);
                            break;
                        }
                    }
                    
                    // F5 키 설정이 없으면 경고 표시
                    if (!hasF5Setting)
                    {
                        // 메시지 박스를 로그로 대체
                        Logger.Warning("로드된 설정에 F5 키 설정이 없습니다!");
                    }
                    
                    return loadedSettings;
                }
                else
                {
                    // 로그에 기록하고 메시지 박스 표시
                    Logger.Error("설정을 로드했으나 null 값이 반환되었습니다.");
                    MessageBox.Show("설정을 로드했으나 null 값이 반환되었습니다.",
                                  "설정 로드 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new KeySettings();
                }
            }
            catch (Exception ex)
            {
                // 로그에 기록하고 메시지 박스 표시
                Logger.Error(ex, "설정을 불러오는 중 오류가 발생했습니다.");
                MessageBox.Show("설정을 불러오는 중 오류가 발생했습니다: " + ex.Message,
                              "설정 불러오기 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // 오류 발생 시 설정 파일 백업 및 새 설정 생성
                try
                {
                    string backupPath = settingsPath + ".bak";
                    if (File.Exists(settingsPath))
                    {
                        File.Copy(settingsPath, backupPath, true);
                        File.Delete(settingsPath);
                    }
                }
                catch { /* 백업 실패 무시 */ }

                return new KeySettings(); // 오류 발생 시 기본 설정 반환
            }
        }
    }
} 