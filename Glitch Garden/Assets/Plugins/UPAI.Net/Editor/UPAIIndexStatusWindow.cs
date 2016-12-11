using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.EditorWindowVersion)]
  public class UPAIIndexStatusWindow : UPAIBaseEditorWindow {
    private const string CLASS_NAME = "UPAIIndexStatusWindow";

    private static UPAIIndexStatus _indexStatus = null;
    private static bool _isDisabled = false;
    private static bool _forceOptionsWindow = false;
    private static bool _forceRefresh = false;
    private static bool _isRefreshing = false;
    private static UPAIIndexStatusWindow _instance = null;
    private static Vector2 _scrollPosition = new Vector2();
    private static UPAIUINotificationsProcessor _uiNotificationProcessor = null;

    /// <summary>
    /// 
    /// </summary>
    public static void ShowWindow() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "ShowWindow");

      _instance = (UPAIIndexStatusWindow)EditorWindow.GetWindow<UPAIIndexStatusWindow>("Index Status", true, typeof(UnityEditor.SceneView));
      
      UPAISystem.IndexStatusRefreshAborted += UPAISystem_IndexStatusRefreshAborted;
      UPAISystem.IndexStatusRefreshAborting += UPAISystem_IndexStatusRefreshAborting;
      UPAISystem.IndexStatusRefreshCompleted += UPAISystem_IndexStatusRefreshCompleted;
      UPAISystem.IndexStatusRefreshCompleting += UPAISystem_IndexStatusRefreshCompleting;
      UPAISystem.IndexStatusRefreshStarted += UPAISystem_IndexStatusRefreshStarted;
      UPAISystem.IndexStatusRefreshStarting += UPAISystem_IndexStatusRefreshStarting;

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
          UPAIProcessTypeEnum.IndexStatusRefresh,
        },
        null,
        null);

      try {
        UPAIUnityEditorRecoveryManager.IndexStatusWindowOpened();
      } catch {

      }

      _indexStatus = UPAISystem.GetIndexStatus(false);
    }

    private static void UPAISystem_IndexStatusRefreshStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_IndexStatusRefreshStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_IndexStatusRefreshCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_IndexStatusRefreshCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
      _indexStatus = UPAISystem.GetIndexStatus(false);
      UPAISystem.Debug.Log("Refreshing index status...");
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_IndexStatusRefreshAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private static void UPAISystem_IndexStatusRefreshAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
      _uiNotificationProcessor.ProcessUINotification(e);
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
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnEnable");

      UPAISystem.IndexStatusRefreshAborted += UPAISystem_IndexStatusRefreshAborted;
      UPAISystem.IndexStatusRefreshAborting += UPAISystem_IndexStatusRefreshAborting;
      UPAISystem.IndexStatusRefreshCompleted += UPAISystem_IndexStatusRefreshCompleted;
      UPAISystem.IndexStatusRefreshCompleting += UPAISystem_IndexStatusRefreshCompleting;
      UPAISystem.IndexStatusRefreshStarted += UPAISystem_IndexStatusRefreshStarted;
      UPAISystem.IndexStatusRefreshStarting += UPAISystem_IndexStatusRefreshStarting;

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

      if (_indexStatus == null && _isRefreshing)
        return;

      Repaint();
    }

    void OnDestroy() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnDestroy");

      try {
        UPAIUnityEditorRecoveryManager.IndexStatusWindowClosed();
      } catch {

      }

      _indexStatus = null;
      _isDisabled = false;
      _forceRefresh = false;
      _isRefreshing = false;
      _instance = null;
      _scrollPosition = Vector2.zero;
      _uiNotificationProcessor = null;
  }

    void OnDisable() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnDisable");

      UPAISystem.IndexStatusRefreshAborted -= UPAISystem_IndexStatusRefreshAborted;
      UPAISystem.IndexStatusRefreshAborting -= UPAISystem_IndexStatusRefreshAborting;
      UPAISystem.IndexStatusRefreshCompleted -= UPAISystem_IndexStatusRefreshCompleted;
      UPAISystem.IndexStatusRefreshCompleting -= UPAISystem_IndexStatusRefreshCompleting;
      UPAISystem.IndexStatusRefreshStarted -= UPAISystem_IndexStatusRefreshStarted;
      UPAISystem.IndexStatusRefreshStarting -= UPAISystem_IndexStatusRefreshStarting;

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

      if (_indexStatus == null && (_isRefreshing || _isDisabled))
        return;

      Repaint();
    }

    void _DeleteEntry(string path, string packageId) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_DeleteEntry");

      if (string.IsNullOrEmpty(path))
        throw new ArgumentNullException("Path");

      if (string.IsNullOrEmpty(packageId))
        throw new ArgumentNullException("PackageId");

      var confirmDelete = EditorUtility.DisplayDialog("Delete Missing Entry", string.Format(
        "You are about to delete all entries for {0} entires from the index. " +
        "Do you want to continue?", Path.GetFileName(path)), "Yes", "No");
      if (!confirmDelete)
        return;

      if (_processIsRunning) {
        UPAISystem.Debug.LogWarningFormat("{0} is already running, please wait for it to complete...", _processName);
        return;
      }

      _processName = "Deleting Missing Package Entry";
      UPAISystem.Debug.Log("Deleting missing package entry...");
      _indexStatus = null;
      try {
        _processIsRunning = true;
        if (UPAISystem.DeletePackageEntry(packageId)) {
          _processIsRunning = false;
          _processName = null;
          _ForceIndexRefresh();
        }
      } catch (Exception ex) {
        UPAISystem.Debug.LogError(ex.Message);
      } finally {
        _processIsRunning = false;
        _processName = null;
        _isRefreshing = false;
      }
    }

    void _DeleteAllMissingEntries() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_DeleteAllMissingEntries");

      var packageEntriesToDelete = new List<string>();
      try {
        packageEntriesToDelete = _indexStatus.MissingPackageFiles
          .Where(x => !string.IsNullOrEmpty(x.Key))
          .Select(x => x.Value).ToList();
      } catch (Exception ex) {
        UPAISystem.Debug.LogError(ex.Message);
        return;
      }

      if (packageEntriesToDelete == null || packageEntriesToDelete.Count == 0) {
        UPAISystem.Debug.LogWarning("No package ids were found to delete from the index");
        return;
      }

      var confirmDelete = EditorUtility.DisplayDialog("Delete Missing Entries", string.Format(
        "You are about to delete {0} missing package entires from the index. " +
        "Do you want to continue?", packageEntriesToDelete.Count), "Yes", "No");
      if (!confirmDelete)
        return;

      if (_processIsRunning) {
        UPAISystem.Debug.LogWarningFormat("{0} is already running, please wait for it to complete...", _processName);
        return;
      }

      _processName = "Package Index Cleanup";
      UPAISystem.Debug.Log("Beginning package index cleanup...");
      _indexStatus = null;
      var entriesDeleted = 0;
      try {
        _processIsRunning = true;
        entriesDeleted = UPAISystem.DeletePackageEntries(packageEntriesToDelete);
        if (entriesDeleted > 0) {
          _processIsRunning = false;
          _processName = null;
          _ForceIndexRefresh();
        }
      } catch (Exception ex) {
        UPAISystem.Debug.LogError(ex.Message);
      } finally {
        _processIsRunning = false;
        _processName = null;
        _isRefreshing = false;
      }
    }

    void _ForceIndexRefresh() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ForceIndexRefresh");

      if (_processIsRunning) {
        UPAISystem.Debug.LogWarningFormat("{0} is already running, please wait for it to complete...", _processName);
        return;
      }
      
      _processName = "Index Status Refresh";
      _forceRefresh = false;
      UPAISystem.Debug.Log("Beginning index refresh...");
      _indexStatus = null;
      _isRefreshing = true;
      UPAISystem.GetIndexStatus(true);      
    }

    void _DrawUI() {
      try {
        if (_indexStatus == null || _isRefreshing || _isDisabled) {
          _BeginHorizontal();
          _BeginVertical();
          _indexStatus = UPAISystem.GetIndexStatus(false);
          _DrawSpaces(2);
          _DrawHorizontalLabelField("General Index Information");
          _DrawHorizontalLabelValueField("Index Document Count", "N/A");
          _DrawHorizontalLabelValueField("Packages Indexed", "N/A");
          _DrawHorizontalLabelValueField("Assets Indexed", "N/A");
          _DrawHorizontalLabelValueField("Refreshed On", "N/A");
          _DrawHorizontalLabelValueField("Refresh Time", "N/A");
          _BeginHorizontal();
          _forceRefresh = GUILayout.Button("Refresh Status", GUILayout.Width(150));
          if (_forceRefresh && !_isRefreshing) {
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            _ForceIndexRefresh();
            return;
          }
          _forceOptionsWindow = GUILayout.Button("Indexing Options", GUILayout.Width(150));
          if (_forceOptionsWindow && !_isRefreshing) {
            _forceOptionsWindow = false;
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_INDEXING);
            return;
          }
          _forceOptionsWindow = GUILayout.Button("Optimization Options", GUILayout.Width(150));
          if (_forceOptionsWindow && !_isRefreshing) {
            _forceOptionsWindow = false;
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_OPTIMIZATION);
            return;
          }
          _EndHorizontal();
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
        _DrawHorizontalLabelField("General Index Information");
        _DrawHorizontalLabelValueField("Index Document Count",
          _indexStatus.DocumentCount.ToString());
        _DrawHorizontalLabelValueField("Packages Indexed",
          _indexStatus.PackagesIndexed.ToString());
        _DrawHorizontalLabelValueField("Assets Indexed",
          _indexStatus.AssetsIndexed.ToString());
        _DrawHorizontalLabelValueField("Refreshed On", string.Format(
          "{0} {1}",
          _indexStatus.GeneratedOn.ToShortDateString(),
          _indexStatus.GeneratedOn.ToShortTimeString()));
        _DrawHorizontalLabelValueField("Refresh Time",
          _indexStatus.IndexStatusGenerationTimeSpan.ToString());
        _BeginHorizontal();
        _forceRefresh = GUILayout.Button("Refresh Status", GUILayout.Width(150));
        if (_forceRefresh && !_isRefreshing) {
          _EndHorizontal();         
          _ForceIndexRefresh();
          return;
        }
        _forceOptionsWindow = GUILayout.Button("Indexing Options", GUILayout.Width(150));
        if (_forceOptionsWindow && !_isRefreshing) {
          _forceOptionsWindow = false;
          _EndHorizontal();          
          UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_INDEXING);
          return;
        }
        _forceOptionsWindow = GUILayout.Button("Optimization Options", GUILayout.Width(150));
        if (_forceOptionsWindow && !_isRefreshing) {
          _forceOptionsWindow = false;          
          _EndHorizontal();
          UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_OPTIMIZATION);
          return;
        }
        _EndHorizontal();
        _DrawSpaces(2);
        _DrawHorizontalLabelField("Package Data");

        if (_indexStatus.LastDownloadedOn == null) {
          _DrawHorizontalLabelValueField("Total Packages", "N/A");
          _DrawHorizontalLabelValueField("Last downloaded on", "N/A");
        } else {
          _DrawHorizontalLabelValueField("Total Packages",
            _indexStatus.TotalPackageFiles.ToString());
          _DrawHorizontalLabelValueField("Last Download Date",
            _indexStatus.LastDownloadedOn.Value.ToString("g"));
        }

        if (_indexStatus.MissingPackageDocuments != null &&
          _indexStatus.MissingPackageDocuments.Count > 0) {
          _DrawSpaces(2);
          _DrawHorizontalLabelField("Index - Missing Package Entries");
          _DrawHorizontalLabelValueField("Missing Entries", _indexStatus.MissingPackageDocuments.Count.ToString());
          var sortedPathList = _indexStatus.MissingPackageDocuments.Keys.OrderBy(x => x);
          foreach (var path in sortedPathList) {
            var analyzePackage = _DrawHorizontalLabelButtonField(path.Replace(UPAISystem.UnityAssetStorePath, ".."), "Analyze Package", 750, 150);
            if (analyzePackage) {
              _uiNotificationProcessor.DisplayDialog("Analyze Package", "This has not been implemented yet.", "Ok");
            }
          }
        }

        if (_indexStatus.MissingPackageFiles != null &&
          _indexStatus.MissingPackageFiles.Count > 0) {
          _DrawSpaces(2);
          _DrawHorizontalLabelField("Files - Missing Unity Packages");
          var deleteAllEntries = _DrawHorizontalLabelValueButtonField("Missing Files", _indexStatus.MissingPackageFiles.Count.ToString(), "Delete All Entries", 250, 495, 150);
          if (deleteAllEntries) {
            _EndHorizontal();
            _DeleteAllMissingEntries();
            return;
          }
          var sortedPathList = _indexStatus.MissingPackageFiles.Keys.OrderBy(x => x);
          foreach (var path in sortedPathList) {
            var deleteEntry = _DrawHorizontalLabelButtonField(path.Replace(UPAISystem.UnityAssetStorePath, ".."), "Delete Entry", 750, 150);
            if (deleteEntry) {
              _EndHorizontal();
              var packageIdToDelete = _indexStatus.MissingPackageFiles[path];
              _DeleteEntry(path, packageIdToDelete);
              return;
            }
          }
        }

        _DrawSpaces(2);
        _DrawHorizontalLabelField("Asset Types/Counts");
        _DrawHorizontalSortedFieldDictionary(_indexStatus.AssetTypeCounts);

        _DrawSpaces(2);
        _DrawHorizontalLabelField("Index File Information");

        if (!_indexStatus.LastOptimizedOn.HasValue) {
          _DrawHorizontalLabelValueField("Is Optimized", "N/A");
          _DrawHorizontalLabelValueField("Last optimized on", "N/A");
        } else {
          _DrawHorizontalLabelValueField("Is Optimized", (
            _indexStatus.IsOptimized) ? "Yes" : "No");
          _DrawHorizontalLabelValueField("Last optimized on",
            _indexStatus.LastOptimizedOn.Value.ToString("g"));
          _DrawHorizontalLabelValueField("Last optimization timespan",
            _indexStatus.LastOptimizationTimespan.ToString());
        }

        if (!_indexStatus.LastUpdatedOn.HasValue) {
          _DrawHorizontalLabelValueField("Is Current", "N/A");
          _DrawHorizontalLabelValueField("Last updated on", "N/A");
        } else {
          _DrawHorizontalLabelValueField("Is Current", (_indexStatus.IsCurrent) ? "Yes" : "No");
          _DrawHorizontalLabelValueField("Last updated on",
            _indexStatus.LastUpdatedOn.Value.ToString("g"));
          _DrawHorizontalLabelValueField("Last update timespan",
            _indexStatus.LastUpdateTimeSpan.ToString());
        }
        _DrawHorizontalLabelValueField("Index File Count",
          _indexStatus.IndexFileCount.ToString());
        _DrawHorizontalLabelValueField("Index File Size",
          UPAIFileInfoManager.FormatFileSize(_indexStatus.IndexSize));
        _DrawSpaces(3);
        _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
        _DrawSpaces(2);

        EditorGUILayout.EndScrollView();

        _EndHorizontal();
      } catch (Exception ex) {
        UPAISystem.Debug.LogException("UPAIIndexStatusWindow", "_DrawUI", ex);
      }
    }

    protected override bool _DrawHorizontalLabelButtonField(string caption, string buttonCaption = "Button", int labelWidth = 600, int buttonWidth = 75, string toolTip = "") {
      var buttonClicked = false;

      if (string.IsNullOrEmpty(caption))
        return buttonClicked;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      buttonClicked = GUILayout.Button(buttonCaption, GUILayout.Width(buttonWidth));
      EditorGUILayout.EndHorizontal();

      return buttonClicked;
    }

    /// <summary>
    /// Temporarilly ignore unsed variables for future development.
    /// </summary>
    private void IgnoreUnusedVariables() {
      if (_instance == null) {}
    }
  }
}