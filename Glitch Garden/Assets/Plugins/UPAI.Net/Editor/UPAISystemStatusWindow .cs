using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.EditorWindowVersion)]
  public class UPAISystemStatusWindow : UPAIBaseEditorWindow {
    private static UPAISystemStatus _systemStatus = null;
    private static bool _forceOptionsWindow = false;
    private static bool _forceRefresh = false;
    private static bool _expandAssetStoreDirectory = false;
    private static bool _expandSystemDirectory = true;
    private static bool _isDisabled = false;
    private static bool _isRefreshing = false;
    private static UPAISystemStatusWindow _instance = null;
    private static Vector2 _scrollPosition = new Vector2();
    private static bool _lastSelectAllBackupsValue = false;
    private static bool _lastSelectAllLogsValue = false;
    private static bool? _selectAllBackupEntries = null;
    private static bool? _selectAllLogEntries = null;
    private static Dictionary<UPAIFileInfo, bool> _selectedBackupEntries = null;
    private static Dictionary<UPAIFileInfo, bool> _selectedLogEntries = null;
    private static UPAIUINotificationsProcessor _uiNotificationProcessor = null;

    /// <summary>
    /// 
    /// </summary>
    public static void ShowWindow() {
      _instance = (UPAISystemStatusWindow)EditorWindow.GetWindow<UPAISystemStatusWindow>("System Status", true, typeof(UnityEditor.SceneView));

      UPAISystem.SystemStatusRefreshAborted += UPAISystem_SystemStatusRefreshAborted;
      UPAISystem.SystemStatusRefreshAborting += UPAISystem_SystemStatusRefreshAborting;
      UPAISystem.SystemStatusRefreshCompleted += UPAISystem_SystemStatusRefreshCompleted;
      UPAISystem.SystemStatusRefreshCompleting += UPAISystem_SystemStatusRefreshCompleting;
      UPAISystem.SystemStatusRefreshStarted += UPAISystem_SystemStatusRefreshStarted;
      UPAISystem.SystemStatusRefreshStarting += UPAISystem_SystemStatusRefreshStarting;

      UPAISystem.ProgressTrackingCompleted += UPAISystem_ProgressTrackingCompleted;
      UPAISystem.ProgressTrackingCompleting += UPAISystem_ProgressTrackingCompleting;
      UPAISystem.ProgressTrackingStarted += UPAISystem_ProgressTrackingStarted;
      UPAISystem.ProgressTrackingStarting += UPAISystem_ProgressTrackingStarting;
      UPAISystem.ProgressTrackingStepCompleted += UPAISystem_ProgressTrackingStepCompleted;
      UPAISystem.ProgressTrackingStepCompleting += UPAISystem_ProgressTrackingStepCompleting;
      UPAISystem.ProgressTrackingStepStarted += UPAISystem_ProgressTrackingStepStarted;
      UPAISystem.ProgressTrackingStepStarting += UPAISystem_ProgressTrackingStepStarting;

      _uiNotificationProcessor = new UPAIUINotificationsProcessor(
        new List<UPAIProcessTypeEnum>() {
          UPAIProcessTypeEnum.SystemStatusRefresh,
        }, 
        null,
        null);

      try {
        UPAIUnityEditorRecoveryManager.SystemStatusWindowOpened();
      } catch {

      }

      _selectedBackupEntries = new Dictionary<UPAIFileInfo, bool>();
      _selectedLogEntries = new Dictionary<UPAIFileInfo, bool>();

      _systemStatus = UPAISystem.GetSystemStatus(false);
    }

    private static void UPAISystem_SystemStatusRefreshStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_SystemStatusRefreshStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
      _processIsRunning = true;
      _processName = e.ProgressBarTitle;
      _isRefreshing = true;
    }

    private static void UPAISystem_SystemStatusRefreshCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_SystemStatusRefreshCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
      _systemStatus = UPAISystem.GetSystemStatus(false);
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
    }

    private static void UPAISystem_SystemStatusRefreshAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_SystemStatusRefreshAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
      _systemStatus = UPAISystem.GetSystemStatus(false);
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
    }

    private static void UPAISystem_ProgressTrackingStepStarting(object sender, UPAIProgressTrackingStepStartingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepStarting(sender, e);
    }

    private static void UPAISystem_ProgressTrackingStepStarted(object sender, UPAIProgressTrackingStepStartedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepStarted(sender, e);
    }

    private static void UPAISystem_ProgressTrackingStepCompleting(object sender, UPAIProgressTrackingStepCompletingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepCompleting(sender, e);
    }

    private static void UPAISystem_ProgressTrackingStepCompleted(object sender, UPAIProgressTrackingStepCompletedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepCompleted(sender, e);
    }

    private static void UPAISystem_ProgressTrackingStarting(object sender, UPAIProgressTrackingStartingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStarting(sender, e);
    }

    private static void UPAISystem_ProgressTrackingStarted(object sender, UPAIProgressTrackingStartedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStarted(sender, e);
    }

    private static void UPAISystem_ProgressTrackingCompleting(object sender, UPAIProgressTrackingCompletingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingCompleting(sender, e);
    }

    private static void UPAISystem_ProgressTrackingCompleted(object sender, UPAIProgressTrackingCompletedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingCompleted(sender, e);
    }

    void OnEnable() {
      UPAISystem.SystemStatusRefreshAborted += UPAISystem_SystemStatusRefreshAborted;
      UPAISystem.SystemStatusRefreshAborting += UPAISystem_SystemStatusRefreshAborting;
      UPAISystem.SystemStatusRefreshCompleted += UPAISystem_SystemStatusRefreshCompleted;
      UPAISystem.SystemStatusRefreshCompleting += UPAISystem_SystemStatusRefreshCompleting;
      UPAISystem.SystemStatusRefreshStarted += UPAISystem_SystemStatusRefreshStarted;
      UPAISystem.SystemStatusRefreshStarting += UPAISystem_SystemStatusRefreshStarting;

      UPAISystem.ProgressTrackingCompleted += UPAISystem_ProgressTrackingCompleted;
      UPAISystem.ProgressTrackingCompleting += UPAISystem_ProgressTrackingCompleting;
      UPAISystem.ProgressTrackingStarted += UPAISystem_ProgressTrackingStarted;
      UPAISystem.ProgressTrackingStarting += UPAISystem_ProgressTrackingStarting;
      UPAISystem.ProgressTrackingStepCompleted += UPAISystem_ProgressTrackingStepCompleted;
      UPAISystem.ProgressTrackingStepCompleting += UPAISystem_ProgressTrackingStepCompleting;
      UPAISystem.ProgressTrackingStepStarted += UPAISystem_ProgressTrackingStepStarted;
      UPAISystem.ProgressTrackingStepStarting += UPAISystem_ProgressTrackingStepStarting;

      if (_uiNotificationProcessor != null && !_uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Enable();

      _isDisabled = false;
    }

    void OnInspectorUpdate() {
      if (EditorApplication.isCompiling)
        Close();

      if (_systemStatus == null && (_isRefreshing || _isDisabled))
        return;

      Repaint();
    }

    void OnDestroy() {
      try {
        UPAIUnityEditorRecoveryManager.SystemStatusWindowClosed();
      } catch {

      }

      _systemStatus = null;
      _forceRefresh = false;
      _expandAssetStoreDirectory = false;
      _expandSystemDirectory = true;
      _isDisabled = false;
      _isRefreshing = false;
      _instance = null;
      _scrollPosition = Vector2.zero;
      _lastSelectAllBackupsValue = false;
      _lastSelectAllLogsValue = false;
      _selectAllBackupEntries = null;
      _selectAllLogEntries = null;
      _selectedBackupEntries = null;
      _selectedLogEntries = null;
      _uiNotificationProcessor = null;
    }

    void OnDisable() {
      UPAISystem.SystemStatusRefreshAborted -= UPAISystem_SystemStatusRefreshAborted;
      UPAISystem.SystemStatusRefreshAborting -= UPAISystem_SystemStatusRefreshAborting;
      UPAISystem.SystemStatusRefreshCompleted -= UPAISystem_SystemStatusRefreshCompleted;
      UPAISystem.SystemStatusRefreshCompleting -= UPAISystem_SystemStatusRefreshCompleting;
      UPAISystem.SystemStatusRefreshStarted -= UPAISystem_SystemStatusRefreshStarted;
      UPAISystem.SystemStatusRefreshStarting -= UPAISystem_SystemStatusRefreshStarting;

      UPAISystem.ProgressTrackingCompleted -= UPAISystem_ProgressTrackingCompleted;
      UPAISystem.ProgressTrackingCompleting -= UPAISystem_ProgressTrackingCompleting;
      UPAISystem.ProgressTrackingStarted -= UPAISystem_ProgressTrackingStarted;
      UPAISystem.ProgressTrackingStarting -= UPAISystem_ProgressTrackingStarting;
      UPAISystem.ProgressTrackingStepCompleted -= UPAISystem_ProgressTrackingStepCompleted;
      UPAISystem.ProgressTrackingStepCompleting -= UPAISystem_ProgressTrackingStepCompleting;
      UPAISystem.ProgressTrackingStepStarted -= UPAISystem_ProgressTrackingStepStarted;
      UPAISystem.ProgressTrackingStepStarting -= UPAISystem_ProgressTrackingStepStarting;

      if (_uiNotificationProcessor != null && _uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Disable();

      _isDisabled = true;
    }

    void OnGUI() {
      _DrawUI();
    }

    void Update() {
      if (EditorApplication.isCompiling)
        Close();

      if (_systemStatus == null && (_isRefreshing || _isDisabled))
        return;

      Repaint();
    }

    void _ForceSystemStatusRefresh() {
      if (_processIsRunning) {
        var message = string.Format("{0} is already running, please wait for it to complete.", _processName);
        UPAISystem.Debug.LogWarning(message);
        _uiNotificationProcessor.DisplayDialog("Status Refresh Process Running", message, _processName);
        return;
      }

      _processName = "System Status Refresh";
      _forceRefresh = false;
      _systemStatus = null;
      _isRefreshing = true;
      UPAISystem.GetSystemStatus(true);
    }

    void _DrawUI() {
      try {
        if (_systemStatus == null || _isRefreshing || _isDisabled) {
          _BeginHorizontal();
          _BeginVertical();
          _DrawSpaces(2);
          _DrawHorizontalLabelField("System Status Information");
          _DrawHorizontalLabelValueField("Generated On", "N/A");
          _DrawHorizontalLabelValueField("Last generation timespan", "N/A");

          _DrawSpaces(2);
          _BeginHorizontal();
          _forceRefresh = GUILayout.Button("Refresh Status", GUILayout.Width(150));
          if (_forceRefresh && !_isRefreshing) {
            _EndVertical();
            _EndHorizontal();
            _ForceSystemStatusRefresh();
            return;
          }
          _forceOptionsWindow = GUILayout.Button("Logging Options", GUILayout.Width(150));
          if (_forceOptionsWindow && !_isRefreshing) {
            _forceOptionsWindow = false;
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_LOGGING);
            return;
          }
          _forceOptionsWindow = GUILayout.Button("Processing Options", GUILayout.Width(150));
          if (_forceOptionsWindow && !_isRefreshing) {
            _forceOptionsWindow = false;
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_PROCESSING);
            return;
          }
          _EndHorizontal();

          _DrawSpaces(3);
          _DrawHorizontalLabelField("System Directory Information");
          _DrawHorizontalLabelValueField("System Root", "N/A");

          _DrawSpaces(2);
          _DrawHorizontalLabelField("System Drive Space Totals");
          _DrawHorizontalLabelValueField("Total System Folders", "N/A");
          _DrawHorizontalLabelValueField("Total System Folders", "N/A");
          _DrawHorizontalLabelValueField("Total System Size", "N/A");

          _DrawSpaces(3);
          _DrawHorizontalLabelField("Asset Store Directory Information");
          _DrawHorizontalLabelValueField("System Root", "N/A");

          _DrawSpaces(2);
          _DrawHorizontalLabelField("Asset Store Drive Space Totals");
          _DrawHorizontalLabelValueField("Total Asset Store Folders", "N/A");
          _DrawHorizontalLabelValueField("Total Asset Store Folders", "N/A");
          _DrawHorizontalLabelValueField("Total Asset Store Size", "N/A");

          _DrawSpaces(3);
          _DrawHorizontalLabelField("Backup File Information");
          _DrawHorizontalLabelValueField("Is backed up", "N/A");
          _DrawHorizontalLabelValueField("Last backed up on", "N/A");
          _DrawHorizontalLabelValueField("Last backup timespan", "N/A");

          _DrawSpaces(3);
          _DrawHorizontalLabelField("Restoration Information");
          _DrawHorizontalLabelValueField("Is restored", "N/A");
          _DrawHorizontalLabelValueField("Last restored on", "N/A");
          _DrawHorizontalLabelValueField("Last restoration timespan", "N/A");

          _DrawSpaces(3);
          _DrawHorizontalLabelField("Log File Information");
          _DrawHorizontalLabelValueField("Log File Count", "N/A");

          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _DrawSpaces(2);
          _EndVertical();
          _EndHorizontal();
          return;
        }

        _BeginHorizontal();

        try {
          _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        } catch {
          _EndHorizontal();
          return;
        }

        _DrawSpaces(2);
        _DrawHorizontalLabelField("System Status Information");
        _DrawHorizontalLabelValueField("Generated On", _systemStatus.GeneratedOn.ToString("g"));
        _DrawHorizontalLabelValueField("Last generation timespan", _systemStatus.SystemStatusGenerationTimeSpan.ToString());

        _DrawSpaces(2);
        _BeginHorizontal();
        _forceRefresh = GUILayout.Button("Refresh Status", GUILayout.Width(150));
        if (_forceRefresh && !_isRefreshing) {
          _EndHorizontal();
          _ForceSystemStatusRefresh();
          return;
        }
        _forceOptionsWindow = GUILayout.Button("Logging Options", GUILayout.Width(150));
        if (_forceOptionsWindow && !_isRefreshing) {
          _forceOptionsWindow = false;          
          _EndHorizontal();
          UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_LOGGING);
          return;
        }
        _forceOptionsWindow = GUILayout.Button("Processing Options", GUILayout.Width(150));
        if (_forceOptionsWindow && !_isRefreshing) {
          _forceOptionsWindow = false;          
          _EndHorizontal();
          UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_PROCESSING);
          return;
        }
        _EndHorizontal();

        _DrawSpaces(3);
        _DrawFileSystemInformation(_systemStatus, true, _expandSystemDirectory);

        _DrawSpaces(3);
        _DrawFileSystemInformation(_systemStatus, false, _expandAssetStoreDirectory);

        _DrawSpaces(3);
        _DrawBackupFileInformation(_systemStatus);

        _DrawSpaces(3);
        _DrawHorizontalLabelField("Restoration Information");
        if (!_systemStatus.LastRestoredOn.HasValue) {
          _DrawHorizontalLabelValueField("Is restored", "No");
          _DrawHorizontalLabelValueField("Last restored on", "N/A");
          _DrawHorizontalLabelValueField("Last restoration timespan", "N/A");
        } else {
          _DrawHorizontalLabelValueField("Is restored", "yes");
          _DrawHorizontalLabelValueField("Last restored on",
            _systemStatus.LastRestoredOn.Value.ToString("g"));
          _DrawHorizontalLabelValueField("Last restoration timespan",
            _systemStatus.LastRestorationTimeSpan.ToString());
        }

        _DrawSpaces(3);
        _DrawLogFileInformation(_systemStatus);

        _DrawSpaces(3);
        _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
        _DrawSpaces(2);

        EditorGUILayout.EndScrollView();
        _EndHorizontal();
      } catch (Exception ex) {
        UPAISystem.Debug.LogException("UPAISystemStatusWindow", "_DrawUI", ex);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int _GetSelectedBackupFileCount() {
      var selectedFileCount = 0;

      if (_selectedBackupEntries == null)
        return selectedFileCount;

      foreach (var entry in _selectedBackupEntries) {
        if (entry.Value)
          selectedFileCount++;
      }

      return selectedFileCount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int _GetSelectedLogFileCount() {
      var selectedFileCount = 0;

      if (_selectedLogEntries == null)
        return selectedFileCount;

      foreach (var entry in _selectedLogEntries) {
        if (entry.Value)
          selectedFileCount++;
      }

      return selectedFileCount;
    }

    private Dictionary<UPAIFileInfo, string> _DrawBackupFileInformation(UPAISystemStatus systemStatus) {
      var currentBackupEntries = new Dictionary<UPAIFileInfo, string>();

      if (systemStatus == null)
        return currentBackupEntries;

      _DrawHorizontalLabelField("Backup File Information");
      _DrawHorizontalLabelValueField("Is backed up", (
          _systemStatus.BackupFilesList != null) ? "Yes" : "No");
      if (!systemStatus.LastBackedUpOn.HasValue) {
        _DrawHorizontalLabelValueField("Last backed up on", "N/A");
        _DrawHorizontalLabelValueField("Last backup timespan", "N/A");
      } else {
        _DrawHorizontalLabelValueField("Last backed up on",
          systemStatus.LastBackedUpOn.Value.ToString("g"));
        _DrawHorizontalLabelValueField("Last backup timespan",
          systemStatus.LastBackupTimeSpan.ToString());
      }

      if (systemStatus.BackupFilesList == null) {
        _DrawHorizontalLabelValueField("Backup File Count", "0");
      } else {
        var toggleAllBackups = _DrawHorizontalLabelValueButtonField("Backup File Count", 
          _systemStatus.BackupFilesList.Count.ToString(), 
          (!_lastSelectAllBackupsValue) ? "All" : "None",
          250,
          425);
        if (toggleAllBackups) {
          _lastSelectAllBackupsValue = !_lastSelectAllBackupsValue;
          _selectAllBackupEntries = _lastSelectAllBackupsValue;
        }
        _DrawSpaces(2);

        var sortedBackupFiles = systemStatus.BackupFilesList
          .OrderBy(x => x.LastSavedOn).ToArray();
        for (int i = 0; i < sortedBackupFiles.Count(); i++) {
          var entry = sortedBackupFiles[i];
          var canBeDeleted = UPAIBackupFileManager.CanBackupFileBeDeleted(UPAISystem.UnityVersion, entry);
          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField(string.Format("{0}. {1}, {2}, {3}", (i + 1),
            entry.Name,
            entry.LastSavedOn.ToString("g"),
            UPAIFileInfoManager.FormatFileSize(entry.FileSize)), GUILayout.Width(600));
          var restoreEntry = GUILayout.Button("Restore", GUILayout.Width(75));
          if (restoreEntry) {
            EditorUtility.DisplayDialog("Feature In Development", "This feature hasn't been implemented yet.", "Ok");
          }
          if (canBeDeleted) {
            var deleteEntry = GUILayout.Button("Delete", GUILayout.Width(75));
            if (deleteEntry) {
              var selectedFileCount = _GetSelectedBackupFileCount();
              if (selectedFileCount > 0) {
                var confirmDelete = EditorUtility.DisplayDialog("Delete Backup Files", string.Format("Are you sure you want to delete the {0} selected files?", selectedFileCount), "Yes", "No");
                if (confirmDelete) {
                  foreach (var selectedEntry in _selectedBackupEntries) {
                    if (!selectedEntry.Value)
                      continue;

                    entry = selectedEntry.Key;
                    Debug.LogFormat("Deleting {0}...", entry.Name);
                    UPAIBackupFileManager.DeleteBackupFile(UPAISystem.UnityVersion, entry);
                  }

                  _selectedBackupEntries = new Dictionary<UPAIFileInfo, bool>();
                  _ForceSystemStatusRefresh();
                  return currentBackupEntries;
                }
              } else {
                var confirmDelete = EditorUtility.DisplayDialog("Delete Backup File", "Are you sure you want to delete this file?", "Yes", "No");
                if (confirmDelete) {
                  Debug.LogFormat("Deleting {0}...", entry.Name);
                  if (UPAIBackupFileManager.DeleteBackupFile(UPAISystem.UnityVersion, entry)) {
                    _ForceSystemStatusRefresh();
                    return currentBackupEntries;
                  }
                }
              }
            }

            if (!_selectedBackupEntries.ContainsKey(entry))
              _selectedBackupEntries[entry] = false;

            if (_selectAllBackupEntries.HasValue && _selectedBackupEntries[entry] != _selectAllBackupEntries.Value)
              _selectedBackupEntries[entry] = _selectAllBackupEntries.Value;

            var selectEntry = _DrawHorizontalToggleField("-", _selectedBackupEntries[entry], 5, 30);
            if (selectEntry.HasValue) {
              _selectedBackupEntries[entry] = selectEntry.Value;
            }
          }
          EditorGUILayout.EndHorizontal();
        }
        _selectAllBackupEntries = null;
        _DrawSpaces(3);
      }

      return currentBackupEntries;
    }

    private Dictionary<UPAIFileInfo, string> _DrawLogFileInformation(UPAISystemStatus systemStatus) {
      var currentLogEntries = new Dictionary<UPAIFileInfo, string>();

      if (systemStatus == null)
        return currentLogEntries;

      _DrawHorizontalLabelField("Log File Information");
      if (systemStatus.LogFilesList == null) {
        _DrawHorizontalLabelValueField("Log File Count", "N/A");
      } else {
        var toggleAllLogFiles = _DrawHorizontalLabelValueButtonField("Log File Count",
          _systemStatus.LogFilesList.Count.ToString(),
          (!_lastSelectAllLogsValue) ? "All" : "None",
          250,
          425);
        if (toggleAllLogFiles) {
          _lastSelectAllLogsValue = !_lastSelectAllLogsValue;
          _selectAllLogEntries = _lastSelectAllLogsValue;
        }
        
        var sortedLogFiles = systemStatus.LogFilesList
          .OrderBy(x => x.LastSavedOn).ToArray();

        _DrawSpaces(2);
        for (int i = 0; i < sortedLogFiles.Count(); i++) {
          var entry = sortedLogFiles[i];
          var canBeDeleted = UPAILogFileManager.CanLogFileBeDeleted(UPAISystem.UnityVersion, entry);
          currentLogEntries[entry] = entry.Name;
          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField(string.Format("{0}. {1}, {2}, {3}", (i + 1),
            entry.Name,
            entry.LastSavedOn.ToString("g"),
            UPAIFileInfoManager.FormatFileSize(entry.FileSize)), GUILayout.Width(600));
          var viewEntry = GUILayout.Button("View", GUILayout.Width(75));
          if (viewEntry) {
            UPAILogFileManager.OpenLogFileViewer(UPAISystem.UnityVersion, entry.Name);
            return currentLogEntries;
          }
          if (canBeDeleted) {
            var deleteEntry = GUILayout.Button("Delete", GUILayout.Width(75));
            if (deleteEntry) {
              var selectedFileCount = _GetSelectedLogFileCount();
              if (selectedFileCount > 0) {
                var confirmDelete = EditorUtility.DisplayDialog("Delete Log Files", string.Format("Are you sure you want to delete the {0} selected log files?", selectedFileCount), "Yes", "No");
                if (confirmDelete) {
                  foreach (var selectedEntry in _selectedLogEntries) {
                    if (!selectedEntry.Value)
                      continue;

                    entry = selectedEntry.Key;
                    Debug.LogFormat("Deleting {0}...", entry.Name);
                    UPAILogFileManager.DeleteLogFile(UPAISystem.UnityVersion, entry);
                  }

                  _selectedLogEntries = new Dictionary<UPAIFileInfo, bool>();
                  _ForceSystemStatusRefresh();
                  return currentLogEntries;
                }
              } else {
                var confirmDelete = EditorUtility.DisplayDialog("Delete Log File", "Are you sure you want to delete this file?", "Yes", "No");
                if (confirmDelete) {
                  Debug.LogFormat("Deleting {0}...", entry.Name);
                  if (UPAILogFileManager.DeleteLogFile(UPAISystem.UnityVersion, entry)) {
                    _ForceSystemStatusRefresh();
                    return currentLogEntries;
                  }
                }
              }
            }

            if (!_selectedLogEntries.ContainsKey(entry))
              _selectedLogEntries[entry] = false;

            if (_selectAllLogEntries.HasValue && _selectedLogEntries[entry] != _selectAllLogEntries.Value)
              _selectedLogEntries[entry] = _selectAllLogEntries.Value;

            var selectEntry = _DrawHorizontalToggleField("-", _selectedLogEntries[entry], 5, 30);
            if (selectEntry.HasValue) {
              _selectedLogEntries[entry] = selectEntry.Value;
            }
          }
          EditorGUILayout.EndHorizontal();

        }
        _selectAllLogEntries = null;
        _DrawSpaces(3);
      }

      return currentLogEntries;
    }

    private void _DrawFileSystemInformation(UPAISystemStatus systemStatus,
      bool isSystemDirectoryStructure = true,
      bool includeDirectoryTree = true) {
      if (systemStatus == null || systemStatus.SystemDirectoryList == null || systemStatus.SystemDirectoryList.Count == 0) {
        _DrawHorizontalLabelField("System Directory Information");
        _DrawHorizontalLabelValueField("[root]", "N/A", 600, 250);
        return;
      }

      var root = (isSystemDirectoryStructure)
        ? systemStatus.SystemDirectoryList[0]
        : systemStatus.PackageDirectoryList[0];

      var totalFolderCount = 0;
      var totalFileCount = root.TotalFileCount;
      var totalSize = root.TotalFileSize;

      
      var toggleDirectoryTree = _DrawHorizontalLabelButtonField(string.Format("{0} Directory Information",
          (isSystemDirectoryStructure) ? "System" : "Asset Store"), (includeDirectoryTree) ? "Collapse" : "Expand", 600, 150);
      if (toggleDirectoryTree) {
        if (isSystemDirectoryStructure) {
          _expandSystemDirectory = !_expandSystemDirectory;
          includeDirectoryTree = _expandSystemDirectory;
        } else {
          _expandAssetStoreDirectory = !_expandAssetStoreDirectory;
          includeDirectoryTree = _expandAssetStoreDirectory;
        }
      }
      if (includeDirectoryTree) {      
        _DrawHorizontalLabelValueField(@"\" + root.Name, string.Format("Files: {0}  -  Size: {1}",
          totalFileCount.ToString("N0"), UPAIFileInfoManager.FormatFileSize(totalSize)), 600, 250);
      }

      if (root.SubDirectories == null || root.SubDirectories.Count == 0)
        return;

      _DrawNextFileSystemLevel(root, 0, ref totalFolderCount, ref totalFileCount, ref totalSize, includeDirectoryTree);

      _DrawSpaces(2);
      _DrawHorizontalLabelField(string.Format("{0} Drive Space Totals", 
        (isSystemDirectoryStructure) ? "System": "Asset Store"));
      _DrawHorizontalLabelValueField(string.Format("Total {0} Folders", 
        (isSystemDirectoryStructure) ? "System" : "Asset Store"), totalFolderCount.ToString("N0"));
      _DrawHorizontalLabelValueField(string.Format("Total {0} Files", 
        (isSystemDirectoryStructure) ? "System" : "Asset Store"), totalFileCount.ToString("N0"));
      _DrawHorizontalLabelValueField(string.Format("Total {0} Size", 
        (isSystemDirectoryStructure) ? "System" : "Asset Store"), UPAIFileInfoManager.FormatFileSize(totalSize));
    }

    private void _DrawNextFileSystemLevel(
      UPAIDirectoryInfo parentDirInfo,
      int parentLevel,
      ref int totalFolderCount,
      ref int totalFileCount,
      ref long totalSize,
      bool includeDirectoryTree = true) {
      if (parentDirInfo == null || parentDirInfo.SubDirectories == null || parentDirInfo.SubDirectories.Count == 0)
        return;

      var subFolders = parentDirInfo.SubDirectories;
      var currentLevel = parentLevel + 1;
      var padding = currentLevel * 10;   
      foreach (var folder in subFolders) {
        if (folder == null)
          continue;

        totalFolderCount++;
        totalFileCount += folder.TotalFileCount;
        totalSize += folder.TotalFileSize;

        if (includeDirectoryTree) {
          _DrawHorizontalLabelValueField(((@"\".PadLeft(padding, ' ') + folder.Name)), string.Format("Files: {0}  -  Size: {1}",
            folder.TotalFileCount.ToString("N0"), UPAIFileInfoManager.FormatFileSize(folder.TotalFileSize)), 600, 250);
        }

        if (folder.SubDirectories == null || folder.SubDirectories.Count == 0)
          continue;

        _DrawNextFileSystemLevel(folder, currentLevel, ref totalFolderCount, ref totalFileCount, ref totalSize, includeDirectoryTree);
      }
    }

    /// <summary>
    /// Temporarilly ignore unsed variables for future development.
    /// </summary>
    private void IgnoreUnusedVariables() {
      if (_instance == null) {}
    }
  }
}