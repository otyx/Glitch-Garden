using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using System;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.EditorWindowVersion)]
  public class UPAISettingsWindow : UPAIBaseEditorWindow {
    public const int SETTINGSWINDOW_FOLDERS = 0;
    public const int SETTINGSWINDOW_SEARCH = 1;
    public const int SETTINGSWINDOW_INDEXING = 2;
    public const int SETTINGSWINDOW_OPTIMIZATION = 3;
    public const int SETTINGSWINDOW_PROCESSING = 4;
    public const int SETTINGSWINDOW_LOGGING = 5;
    public const int SETTINGSWINDOW_ABOUT = 6;

    private const string CLASS_NAME = "UPAISettingsWindow";

    private bool _isDirty = false;
    private bool _isDisabled = false;
    private bool _forceRefresh = false;
    private bool _forceSave = false;    
    private bool _isInitialized = false;
    private bool _isRefreshing = false;
    private bool _isSaving = false;
    private int _selectedFolderTabIndex = 0;
    private int _selectedTabIndex = 0;
    private UPAISettingsOptions _systemSettings = null;
    private bool _upgradeDialogDisplayed = false;
    private Vector2 _scrollPosition_AlternateUserFolders = Vector2.zero;

    public static void CreateSettingsWindow(int selectedTabIndex = 0) {
      var settingsWindow = (UPAISettingsWindow)EditorWindow.GetWindow<UPAISettingsWindow>(true, "UPAI System Settings...", true);

      if (!settingsWindow.Initialize(selectedTabIndex)) {
        settingsWindow.Close();
        settingsWindow = null;
        return;
      }

      try {
        UPAIUnityEditorRecoveryManager.SystemSettingsWindowOpened();
      } catch {

      }

      settingsWindow.minSize = new Vector2(885f, 390f);
      settingsWindow.maxSize = new Vector2(885f, 390f);
      settingsWindow.CenterOnMainWin();
      settingsWindow.ShowUtility();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Initialize(int selectedTabIndex = 0) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "Intitialize");

      if (_isInitialized)
        return true;

      try {
        _systemSettings = UPAISystem.SystemSettings;
        _selectedTabIndex = selectedTabIndex;
        _isInitialized = true;
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "Initialize", ex);
      }

      return _isInitialized;
    }

    void OnEnable() {
      _isDisabled = false;
    }

    void OnInspectorUpdate() {
      if (EditorApplication.isCompiling)
        Close();

      if (_isDisabled)
        return;

      Repaint();
    }

    void OnDestroy() {
      try {
        UPAIUnityEditorRecoveryManager.SystemSettingsWindowClosed();
      } catch {

      }

      _isDirty = false;
      _forceRefresh = false;
      _forceSave = false;
      _isRefreshing = false;
      _isSaving = false;
      _systemSettings = null;
    }

    void OnDisable() {
      _isDisabled = true;
    }

    void OnGUI() {
      _DrawUI();
    }

    void Update() {
      if (EditorApplication.isCompiling)
        Close();

      if (_isDisabled)
        return;

      Repaint();
    }

    void _ForceSettingsRefresh() {
      _forceRefresh = false;
      _isRefreshing = true;

      Debug.Log("Refreshing settings...");
      _systemSettings = UPAISystem.SystemSettings;
      
      _isRefreshing = false;
    }

    void _ForceSettingsSave() {
      _forceSave = false;
      _isSaving = true;

      Debug.Log("Saving settings...");
      UPAISystem.SystemSettings = _systemSettings;

      _isDirty = false;
      _isSaving = false;
      
      Close();
    }

    void _DrawUI() {
      if (_isDisabled)
        return;

      try {
        var tabIndex = _DrawHorizontalTabs(new string[] { "Folders", "Search", "Indexing", "Optimization", "Processing", "Logging", "About" }, _selectedTabIndex);
        if (tabIndex != _selectedTabIndex) {
          _selectedTabIndex = tabIndex;
          return;
        }
        _DrawSpaces(1);
        switch (_selectedTabIndex) {
          case 0:
            _BeginHorizontal();
            _BeginVertical();                     
            var tabFolderIndex = _DrawHorizontalTabs(new string[] { "System", "User" }, _selectedFolderTabIndex);
            if (tabFolderIndex != _selectedFolderTabIndex) {
              _selectedFolderTabIndex = tabFolderIndex;
              return;
            }
            switch (_selectedFolderTabIndex) {
              case 0:
                _DrawHorizontalLabelField("System Folders");
                if (_systemSettings == null || _isSaving || _isRefreshing) {
                  _DrawHorizontalLabelValueField("Data Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Index Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Index Backup Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Index Stats Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("JSON Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Logs Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Icons Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Package Icons Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Asset Icons Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("System Stats Folder", "N/A", 250, 650);
                  _DrawHorizontalLabelValueField("Temp Folder", "N/A", 250, 650);
                } else {
                  _DrawHorizontalLabelValueField("Data Folder", _systemSettings.FolderOptions.DataFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Index Folder", _systemSettings.FolderOptions.IndexFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Index Backup Folder", _systemSettings.FolderOptions.IndexBackupFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Index Stats Folder", _systemSettings.FolderOptions.IndexStatsFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("JSON Folder", _systemSettings.FolderOptions.JsonFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Logs Folder", _systemSettings.FolderOptions.LogsFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Icons Folder", _systemSettings.FolderOptions.DataFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Package Icons Folder", _systemSettings.FolderOptions.PackageIconFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Asset Icons Folder", _systemSettings.FolderOptions.PreviewIconFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("System Stats Folder", _systemSettings.FolderOptions.SystemStatsFolderPath, 250, 650);
                  _DrawHorizontalLabelValueField("Temp Folder", _systemSettings.FolderOptions.TempFolderPath, 250, 650);
                }
                _EndVertical();
                _EndHorizontal();
                _DrawSpaces(5);

                break;
              case 1:
                _DrawHorizontalLabelField("User Folders");
                if (_systemSettings == null || _isSaving || _isRefreshing) {
                  _DrawVerticalLabelTextAreaField("Alternate Package Folders", "N/A", _scrollPosition_AlternateUserFolders, 250, 865, 150);
                } else {
                  _scrollPosition_AlternateUserFolders = _DrawVerticalLabelTextAreaField("Alternate Package Folders", "Coming Soon...", _scrollPosition_AlternateUserFolders, 250, 865, 150);
                }
                _EndVertical();
                _EndHorizontal();
                _DrawSpaces(1);

                break;
            }            

            break;
          case 1:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("Search Options");
            if (_systemSettings == null || _isSaving || _isRefreshing) {
              _DrawHorizontalToggleField("Always search using wildcards", false);              
            } else {              
              var alwaysSearchUsingWildcards = _DrawHorizontalToggleField("Always search using wildcards", _systemSettings.SearchOptions.AlwaysSearchUsingWildcards);
              if (alwaysSearchUsingWildcards.HasValue && alwaysSearchUsingWildcards.Value != _systemSettings.SearchOptions.AlwaysSearchUsingWildcards) {
                _systemSettings.SearchOptions.AlwaysSearchUsingWildcards = alwaysSearchUsingWildcards.Value;
                _isDirty = true;
              }
            }
            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(40);

            break;
          case 2:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("Indexing Options");
            if (_systemSettings == null || _isSaving || _isRefreshing) {              
              _DrawHorizontalIntField("Common Filename Limit", 0, 250, 50);
              _DrawHorizontalToggleField("Optimize After Updating", false);
              _DrawHorizontalToggleField("Process Assets Asynchronously", false);
              _DrawHorizontalToggleField("Process Packages Asynchronously", false);
            } else {              
              var fileNameInstanceLimit = _DrawHorizontalIntField("Common Filename Limit", _systemSettings.IndexingOptions.CommonFileNameLimit, 250, 50);
              if (fileNameInstanceLimit != _systemSettings.IndexingOptions.CommonFileNameLimit) {
                _systemSettings.IndexingOptions.CommonFileNameLimit = fileNameInstanceLimit;
                _isDirty = true;
              }
              var optimizeAfterUpdating = _DrawHorizontalToggleField("Optimize After Updating", _systemSettings.IndexingOptions.OptimizeAfterUpdating);
              if (optimizeAfterUpdating.HasValue && optimizeAfterUpdating.Value != _systemSettings.IndexingOptions.OptimizeAfterUpdating) {
                _systemSettings.IndexingOptions.OptimizeAfterUpdating = optimizeAfterUpdating.Value;
                _isDirty = true;
              }
              var processMultipleAssets = _DrawHorizontalToggleField("Process Assets Asynchronously", _systemSettings.IndexingOptions.ProcessAssetsAsynchronously);
              if (processMultipleAssets.HasValue && processMultipleAssets.Value != _systemSettings.IndexingOptions.ProcessAssetsAsynchronously) {
                _systemSettings.IndexingOptions.ProcessAssetsAsynchronously = processMultipleAssets.Value;
                _isDirty = true;
              }
              var processMultiplePackages = _DrawHorizontalToggleField("Process Packages Asynchronously", _systemSettings.IndexingOptions.ProcessPackagesAsynchronously);
              if (processMultiplePackages.HasValue && processMultiplePackages.Value != _systemSettings.IndexingOptions.ProcessPackagesAsynchronously) {
                _systemSettings.IndexingOptions.ProcessPackagesAsynchronously = processMultiplePackages.Value;
                _isDirty = true;
              }
            }

            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(31);

            break;
          case 3:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("Optimization Options");
            if (_systemSettings == null || _isSaving || _isRefreshing) {              
              _DrawHorizontalToggleField("Backup Before Optimizing", false);
            } else {              
              var backupBeforeOptimizing = _DrawHorizontalToggleField("Backup Before Optimizing", _systemSettings.OptimizationOptions.BackupBeforeOptimizing);
              if (backupBeforeOptimizing.HasValue && backupBeforeOptimizing.Value != _systemSettings.OptimizationOptions.BackupBeforeOptimizing) {
                _systemSettings.OptimizationOptions.BackupBeforeOptimizing = backupBeforeOptimizing.Value;
                _isDirty = true;
              }              
            }
            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(40);

            break;
          case 4:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("Processing Options");
            if (_systemSettings == null || _isSaving || _isRefreshing) {
              _DrawHorizontalEnumPopupField<System.Threading.ThreadPriority>("Thread Priority", System.Threading.ThreadPriority.BelowNormal, 250, 150);              
            } else {              
              var systemThreadPriority = _DrawHorizontalEnumPopupField<System.Threading.ThreadPriority>("Thread Priority", _systemSettings.ProcessingOptions.SystemThreadPriority, 250, 150);
              if (systemThreadPriority != _systemSettings.ProcessingOptions.SystemThreadPriority) {
                _systemSettings.ProcessingOptions.SystemThreadPriority = systemThreadPriority;
                _isDirty = true;
              }
            }
            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(40);

            break;
          case 5:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("Logging Options");
            if (_systemSettings == null || _isSaving || _isRefreshing) {              
              _DrawHorizontalEnumPopupField<UPAILoggingLevelTypeEnum>("Logging Level", UPAILoggingLevelTypeEnum.AllMessages, 250, 150);
              _DrawHorizontalToggleField("Log Debug Messages", false);
            } else {              
              var loggingLevel = _DrawHorizontalEnumPopupField<UPAILoggingLevelTypeEnum>("Logging Level", _systemSettings.LoggingOptions.LoggingLevel, 250, 150);
              if (loggingLevel != _systemSettings.LoggingOptions.LoggingLevel) {
                _systemSettings.LoggingOptions.LoggingLevel = loggingLevel;
                _isDirty = true;
              }
              var includeDebugMessages = _DrawHorizontalToggleField("Log Debug Messages", _systemSettings.LoggingOptions.IncludeDebugMessages);
              if (includeDebugMessages.HasValue && includeDebugMessages.Value != _systemSettings.LoggingOptions.IncludeDebugMessages) {
                _systemSettings.LoggingOptions.IncludeDebugMessages = includeDebugMessages.Value;
                _isDirty = true;
              }              
            }
            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(37);

            break;
          case 6:
            _BeginHorizontal();
            _BeginVertical();
            _DrawSpaces(1);
            _DrawHorizontalLabelField("System Information");
            if (_systemSettings == null || _isSaving || _isRefreshing) {
              _DrawHorizontalLabelValueField("UPAI Version", "N/A", 250, 650);
              _DrawHorizontalLabelValueField("UPAI Document Version", "N/A", 250, 650);
              _DrawHorizontalLabelValueField("Unity Version", "N/A", 250, 650);
              _DrawHorizontalLabelValueField("Lucene Version", "N/A", 250, 650);
              _DrawHorizontalLabelValueField("Created On", "N/A", 250, 650);              
            } else {              
              _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion, 250, 650);
              _DrawHorizontalLabelValueField("UPAI Document Version", _systemSettings.UPAIDocumentVersion, 250, 650);
              _DrawHorizontalLabelValueField("Unity Version", _systemSettings.UnityVersion, 250, 650);
              _DrawHorizontalLabelValueField("Lucene Version", _systemSettings.IndexVersion.ToString(), 250, 650);
              _DrawHorizontalLabelValueField("Created On", _systemSettings.CreatedOn.ToString("g"), 250, 650);
            }
            _EndVertical();
            _EndHorizontal();
            _DrawSpaces(28);

            break;
        }

        _BeginHorizontal();        
        _forceRefresh = GUILayout.Button("Refresh", GUILayout.Width(150));
        if (_forceRefresh && !_isRefreshing) {
          if (_isDirty) {
            var dirtySave = EditorUtility.DisplayDialog("Save Settings?",
              "You have unsaved changes.  Do you want to save them, before continuing?", "Yes", "No");
            if (dirtySave) {              
              _EndHorizontal();
              _ForceSettingsSave();

              return;
            }
          }
        }
        _forceSave = GUILayout.Button("Save", GUILayout.Width(150));
        if (_forceSave && !_isSaving) {          
          _EndHorizontal();
          _ForceSettingsSave();

          return;
        }
        if (UPAISystem.IsSettingsFileUpgraded() && !_upgradeDialogDisplayed) {
          _upgradeDialogDisplayed = true;
          EditorUtility.DisplayDialog("UPAI Settings File Upgraded", "The settings file has been upgraded to the latest version.  Please click on the Save button to continue.", "Ok");
          _isDirty = true;
        }
        _EndHorizontal();
        _BeginHorizontal();
        _BeginVertical();   
        _DrawSpaces(2);
        _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
        _EndVertical();
        _EndHorizontal();
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "_DrawUI", ex);
      }
    }
  }
}