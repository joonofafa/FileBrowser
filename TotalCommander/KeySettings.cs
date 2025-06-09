using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using TotalCommander;

namespace TotalCommander
{
    /// <summary>
    /// List of available key actions
    /// </summary>
    public enum KeyAction
    {
        None,
        View,               // View file
        Edit,               // Edit file
        Copy,               // Copy
        Cut,                // Cut
        Paste,              // Paste
        Delete,             // Delete
        CreateFolder,       // Create new folder
        Properties,         // Properties
        Refresh,            // Refresh
        GoParent,           // Go to parent folder
        GoBack,             // Go back
        GoForward,          // Go forward
        Exit,               // Exit
        Rename,             // Rename
        UserExecute         // User-defined execution option
    }

    /// <summary>
    /// User-defined execution option class
    /// </summary>
    [Serializable]
    public class UserExecuteOption
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public string Parameters { get; set; }

        public UserExecuteOption()
        {
            // Default constructor (for serialization)
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
        /// Executes the user-defined option
        /// </summary>
        /// <param name="leftExplorerFullPath">Full path of the selected item in the left file list</param>
        /// <param name="rightExplorerFullPath">Full path of the selected item in the right file list</param>
        /// <param name="focusingExplorerFullPath">Full path of the selected item in the currently focused file list</param>
        public void Execute(string leftExplorerFullPath, string rightExplorerFullPath, string focusingExplorerFullPath = null)
        {
            try
            {
                // Debugging information - check each path
                System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
                debugInfo.AppendLine(StringResources.GetString("ParameterDebugInfo"));
                debugInfo.AppendLine(StringResources.GetString("LeftExplorerPath", leftExplorerFullPath ?? "None"));
                debugInfo.AppendLine(StringResources.GetString("RightExplorerPath", rightExplorerFullPath ?? "None"));
                debugInfo.AppendLine(StringResources.GetString("FocusingExplorerPath", focusingExplorerFullPath ?? "None"));
                debugInfo.AppendLine(StringResources.GetString("OriginalParameters", Parameters));

                // Parameter variable substitution
                string parameters = Parameters;
                
                // Path validation - replace null with empty string
                leftExplorerFullPath = leftExplorerFullPath ?? string.Empty;
                rightExplorerFullPath = rightExplorerFullPath ?? string.Empty;
                focusingExplorerFullPath = focusingExplorerFullPath ?? string.Empty;
                
                // Pre-calculate directory paths
                string leftExplorerDirPath = string.Empty;
                string rightExplorerDirPath = string.Empty;
                string focusingExplorerDirPath = string.Empty;
                
                // Safely extract directory paths
                if (!string.IsNullOrEmpty(leftExplorerFullPath))
                {
                    try {
                        leftExplorerDirPath = Path.GetDirectoryName(leftExplorerFullPath) ?? leftExplorerFullPath;
                    } catch {
                        leftExplorerDirPath = leftExplorerFullPath;
                    }
                    debugInfo.AppendLine(StringResources.GetString("LeftDirectoryPath", leftExplorerDirPath));
                }
                
                if (!string.IsNullOrEmpty(rightExplorerFullPath))
                {
                    try {
                        rightExplorerDirPath = Path.GetDirectoryName(rightExplorerFullPath) ?? rightExplorerFullPath;
                    } catch {
                        rightExplorerDirPath = rightExplorerFullPath;
                    }
                    debugInfo.AppendLine(StringResources.GetString("RightDirectoryPath", rightExplorerDirPath));
                }
                
                if (!string.IsNullOrEmpty(focusingExplorerFullPath))
                {
                    try {
                        focusingExplorerDirPath = Path.GetDirectoryName(focusingExplorerFullPath) ?? focusingExplorerFullPath;
                    } catch {
                        focusingExplorerDirPath = focusingExplorerFullPath;
                    }
                    debugInfo.AppendLine(StringResources.GetString("FocusingDirectoryPath", focusingExplorerDirPath));
                }
                
                // Substitute all variables - start with the longest variable names 
                // (to handle cases where a short variable name is part of a longer one)
                // Left explorer variable substitution
                parameters = parameters.Replace("{SelectedItemFullPath:LeftExplorer}", leftExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:LeftExplorer}", leftExplorerDirPath);
                
                // Right explorer variable substitution
                parameters = parameters.Replace("{SelectedItemFullPath:RightExplorer}", rightExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:RightExplorer}", rightExplorerDirPath);
                
                // Focusing explorer variable substitution
                parameters = parameters.Replace("{SelectedItemFullPath:FocusingExplorer}", focusingExplorerFullPath);
                parameters = parameters.Replace("{SelectedItemDirPath:FocusingExplorer}", focusingExplorerDirPath);
                
                // Legacy variable names for backward compatibility
                parameters = parameters.Replace("{Explorer1:SelectedItem}", leftExplorerFullPath);
                parameters = parameters.Replace("{Explorer2:SelectedItem}", rightExplorerFullPath);
                parameters = parameters.Replace("{ActiveExplorer:SelectedItem}", focusingExplorerFullPath);
                
                debugInfo.AppendLine(StringResources.GetString("ParametersAfterSubstitution", parameters));

                // Log debug information
                Logger.DebugMultiline($"UserExecuteOption({Name})", debugInfo.ToString());
                
                // Start process
                Logger.Information($"Executing: {ExecutablePath} {parameters}");
                System.Diagnostics.Process.Start(ExecutablePath, parameters);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"UserExecuteOption({Name}) execution failed");
                System.Windows.Forms.MessageBox.Show(
                    StringResources.GetString("ExecutionErrorMessage", ex.Message), 
                    StringResources.GetString("ExecutionError"), 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }

    /// <summary>
    /// Single key setting item
    /// </summary>
    [Serializable]
    public class KeySetting
    {
        public Keys Key { get; set; }
        public KeyAction Action { get; set; }
        public string UserExecuteOptionName { get; set; }

        public KeySetting()
        {
            // Default constructor (for serialization)
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
    /// Class to manage all key settings
    /// </summary>
    [Serializable]
    public class KeySettings
    {
        public List<KeySetting> Settings { get; set; }
        public List<UserExecuteOption> UserExecuteOptions { get; set; }

        private static string SettingsFilePath => GetSettingsFilePath();

        /// <summary>
        /// Returns the full path of the settings file
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
            // Default settings method removed (not automatically called in constructor)
            // SetDefaults();
        }

        /// <summary>
        /// Initializes with default key settings
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
        /// Finds the action corresponding to a specific key
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
        /// Finds the user-defined execution option name corresponding to a specific key
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
        /// Finds the user-defined execution option corresponding to a specific key
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
        /// Finds the user-defined execution option by name
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
        /// Adds or updates a user-defined execution option
        /// </summary>
        public void AddOrUpdateUserExecuteOption(UserExecuteOption option)
        {
            // Find existing option
            for (int i = 0; i < UserExecuteOptions.Count; i++)
            {
                if (UserExecuteOptions[i].Name == option.Name)
                {
                    // If an option with the same name exists, update it
                    UserExecuteOptions[i] = option;
                    return;
                }
            }

            // Add new option
            UserExecuteOptions.Add(option);
        }

        /// <summary>
        /// Removes a user-defined execution option
        /// </summary>
        public bool RemoveUserExecuteOption(string name)
        {
            // Find option to remove
            for (int i = 0; i < UserExecuteOptions.Count; i++)
            {
                if (UserExecuteOptions[i].Name == name)
                {
                    // Remove option
                    UserExecuteOptions.RemoveAt(i);

                    // Update key settings that use this option
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
        /// Saves settings to file
        /// </summary>
        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // Debugging message - check F5 key setting before saving
                string f5Debug = "";
                foreach (var setting in Settings)
                {
                    if (setting.Key == Keys.F5)
                    {
                        f5Debug = $"Saving F5 key setting: Action={setting.Action}";
                        if (setting.Action == KeyAction.UserExecute)
                            f5Debug += $", Option={setting.UserExecuteOptionName}";
                        // MessageBox replaced with log
                        Logger.Information(f5Debug);
                        break;
                    }
                }

                XmlSerializer serializer = new XmlSerializer(typeof(KeySettings));
                using (StreamWriter writer = new StreamWriter(SettingsFilePath))
                {
                    serializer.Serialize(writer, this);
                }

                // Immediately read file after saving to verify
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
                                    string verifyMsg = $"Saved F5 key setting verification: Action={setting.Action}";
                                    if (setting.Action == KeyAction.UserExecute)
                                        verifyMsg += $", Option={setting.UserExecuteOptionName}";
                                    // MessageBox replaced with log
                                    Logger.Information(verifyMsg);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log and show message box
                        Logger.Error(ex, "Saved settings file verification failed");
                        MessageBox.Show(
                            StringResources.GetString("SavedSettingsVerificationError", ex.Message),
                            StringResources.GetString("SettingsVerificationError"), 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log and show message box
                Logger.Error(ex, "Error saving settings");
                MessageBox.Show(
                    StringResources.GetString("ErrorSavingSettings", ex.Message),
                    StringResources.GetString("SettingsSaveError"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads settings from file
        /// </summary>
        public static KeySettings Load()
        {
            // Settings file path
            string settingsPath = SettingsFilePath;
            
            // Debugging - check settings file path (MessageBox replaced with log)
            Logger.Information($"Settings file path: {settingsPath}");
            Logger.Information($"File existence: {File.Exists(settingsPath)}");
            
            if (!File.Exists(settingsPath))
            {
                KeySettings newSettings = new KeySettings();
                // Important: Do not set defaults here - remove SetDefaults() call from constructor
                // newSettings.SetDefaults(); 
                newSettings.Save(); // Save empty settings if file doesn't exist
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

                // Debugging message - check loaded F5 key setting
                if (loadedSettings != null)
                {
                    // Important: Do not initialize empty settings list to defaults
                    if (loadedSettings.Settings == null)
                        loadedSettings.Settings = new List<KeySetting>();
                        
                    if (loadedSettings.UserExecuteOptions == null)
                        loadedSettings.UserExecuteOptions = new List<UserExecuteOption>();
                        
                    // F5 key setting verification
                    bool hasF5Setting = false;
                    foreach (var setting in loadedSettings.Settings)
                    {
                        if (setting.Key == Keys.F5)
                        {
                            hasF5Setting = true;
                            string loadMsg = $"Loaded F5 key setting: Action={setting.Action}";
                            if (setting.Action == KeyAction.UserExecute)
                                loadMsg += $", Option={setting.UserExecuteOptionName}";
                            // MessageBox replaced with log
                            Logger.Information(loadMsg);
                            break;
                        }
                    }
                    
                    // Warning if no F5 setting
                    if (!hasF5Setting)
                    {
                        // MessageBox replaced with log
                        Logger.Warning("Loaded settings have no F5 key setting!");
                    }
                    
                    return loadedSettings;
                }
                else
                {
                    // Log and show message box
                    Logger.Error("Loaded settings but null returned");
                    MessageBox.Show(
                        StringResources.GetString("NullSettingsReturned"),
                        StringResources.GetString("SettingsLoadError"), 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                    return new KeySettings();
                }
            }
            catch (Exception ex)
            {
                // Log and show message box
                Logger.Error(ex, "Error loading settings");
                MessageBox.Show(
                    StringResources.GetString("ErrorLoadingSettings", ex.Message),
                    StringResources.GetString("SettingsLoadError"), 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                
                // Error occurs, backup settings file and create new settings
                try
                {
                    string backupPath = settingsPath + ".bak";
                    if (File.Exists(settingsPath))
                    {
                        File.Copy(settingsPath, backupPath, true);
                        File.Delete(settingsPath);
                    }
                }
                catch { /* Backup failure ignored */ }

                return new KeySettings(); // Return default settings on error
            }
        }
    }
} 
