using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TotalCommander
{
    /// <summary>
    /// 사용자 정의 명령 클래스
    /// </summary>
    [Serializable]
    public class UserCommand
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Parameters { get; set; }

        public UserCommand()
        {
            // 기본 생성자 (직렬화를 위해)
            Name = "";
            Path = "";
            Parameters = "";
        }

        public UserCommand(string name, string path, string parameters)
        {
            Name = name;
            Path = path;
            Parameters = parameters;
        }

        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// 사용자 정의 명령 실행
        /// </summary>
        public void Execute(string parameters = null)
        {
            try
            {
                // 사용자가 전달한 매개변수가 있으면 사용, 없으면 저장된 매개변수 사용
                string finalParameters = parameters ?? Parameters;
                
                // 실행
                System.Diagnostics.Process.Start(Path, finalParameters);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"명령 실행 중 오류가 발생했습니다: {ex.Message}", 
                    "실행 오류", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// 사용자 명령 설정 관리 클래스
    /// </summary>
    [Serializable]
    public class UserCommandSettings
    {
        public List<UserCommand> Commands { get; set; }

        private static string SettingsFilePath => GetSettingsFilePath();

        /// <summary>
        /// 설정 파일 경로 가져오기
        /// </summary>
        public static string GetSettingsFilePath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string tcFolder = Path.Combine(appDataFolder, "TotalCommander");
            return Path.Combine(tcFolder, "UserCommandSettings.xml");
        }

        public UserCommandSettings()
        {
            Commands = new List<UserCommand>();
        }

        /// <summary>
        /// 명령 목록 가져오기
        /// </summary>
        public List<UserCommand> GetCommands()
        {
            return Commands;
        }

        /// <summary>
        /// 이름으로 명령 찾기
        /// </summary>
        public UserCommand GetCommandByName(string name)
        {
            foreach (var command in Commands)
            {
                if (command.Name == name)
                    return command;
            }
            return null;
        }

        /// <summary>
        /// 명령 추가
        /// </summary>
        public void AddCommand(UserCommand command)
        {
            // 이미 존재하는 이름이면 업데이트
            UserCommand existing = GetCommandByName(command.Name);
            if (existing != null)
            {
                existing.Path = command.Path;
                existing.Parameters = command.Parameters;
            }
            else
            {
                Commands.Add(command);
            }
        }

        /// <summary>
        /// 명령 제거
        /// </summary>
        public void RemoveCommand(UserCommand command)
        {
            Commands.Remove(command);
        }

        /// <summary>
        /// 명령 제거 (이름으로)
        /// </summary>
        public bool RemoveCommand(string name)
        {
            UserCommand command = GetCommandByName(name);
            if (command != null)
            {
                Commands.Remove(command);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 설정 저장
        /// </summary>
        public void Save()
        {
            try
            {
                // 디렉토리 확인 및 생성
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // XML 직렬화
                XmlSerializer serializer = new XmlSerializer(typeof(UserCommandSettings));
                using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"사용자 명령 설정 저장 중 오류가 발생했습니다: {ex.Message}", 
                    "설정 저장 오류", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        public static UserCommandSettings Load()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new UserCommandSettings();
                }

                // XML 역직렬화
                XmlSerializer serializer = new XmlSerializer(typeof(UserCommandSettings));
                using (FileStream fs = new FileStream(SettingsFilePath, FileMode.Open))
                {
                    return (UserCommandSettings)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"사용자 명령 설정 로드 중 오류가 발생했습니다: {ex.Message}", 
                    "설정 로드 오류", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
                
                return new UserCommandSettings();
            }
        }
    }
} 