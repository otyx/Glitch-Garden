using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events.Abstract;
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
  [ExecuteInEditMode()]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.EditorWindowVersion)]
  public class UPAIProjectStatusWindow : UPAIBaseEditorWindow {
    private static Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>> _changedOptions;
    private static UPAIProjectStatus _projectStatus = null;
    private static bool _forceRescanAndAnalyze = false;
    private static bool _forceRefresh = false;
    private static bool _isDirty = false;
    private static bool _isRefreshing = false;
    private static bool _isSaving = false;
    private static UPAIProjectStatusWindow _instance = null;
    private static Dictionary<string, bool> _selectedIgnoredPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
    private static Dictionary<string, bool> _selectedImportedPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
    private static Dictionary<string, bool> _selectedDetectedPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
    private static Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>> _trackingOptions;

    public static void ShowWindow() {
      _instance = (UPAIProjectStatusWindow)EditorWindow.GetWindow<UPAIProjectStatusWindow>("Project Status", true, typeof(UnityEditor.SceneView));

      _changedOptions = new Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>>();
      _trackingOptions = new Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>>();

      _projectStatus = UPAISystem.GetProjectStatus(false);

      UPAISystem.ProjectStatusRefreshAborted += UPAISystem_ProjectStatusRefreshAborted;
      UPAISystem.ProjectStatusRefreshAborting += UPAISystem_ProjectStatusRefreshAborting;
      UPAISystem.ProjectStatusRefreshCompleted += UPAISystem_ProjectStatusRefreshCompleted;
      UPAISystem.ProjectStatusRefreshCompleting += UPAISystem_ProjectStatusRefreshCompleting;
      UPAISystem.ProjectStatusRefreshStarted += UPAISystem_ProjectStatusRefreshStarted;
      UPAISystem.ProjectStatusRefreshStarting += UPAISystem_ProjectStatusRefreshStarting;

      try {
        UPAIUnityEditorRecoveryManager.ProjectStatusWindowOpened();
      } catch {

      }
    }

    private static void _ProcessUINotification(UPAIProcessEventArgs eventArgs) {
      if (!string.IsNullOrEmpty(eventArgs.NotificationMessage)) {
        switch (eventArgs.ProcessNotificationType) {
          case Enumerations.UPAIProcessUINotificationTypeEnum.Console:
            Debug.Log(eventArgs.NotificationMessage);
            break;
          case Enumerations.UPAIProcessUINotificationTypeEnum.Dialog:
            EditorUtility.DisplayDialog("Project Status Refresh", eventArgs.NotificationMessage, "Ok");
            break;
          case Enumerations.UPAIProcessUINotificationTypeEnum.ProgressBar:
            break;
          default:
            break;
        }
      }
    }

    private static void UPAISystem_ProjectStatusRefreshStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_ProjectStatusRefreshStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_ProjectStatusRefreshCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_ProjectStatusRefreshCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
      _forceRescanAndAnalyze = false;
      _projectStatus = UPAISystem.GetProjectStatus(false);
      Debug.Log("Refreshing index status...");
      _ProcessUINotification(e);
    }

    private static void UPAISystem_ProjectStatusRefreshAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_ProjectStatusRefreshAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      _processIsRunning = false;
      _processName = "";
      _isRefreshing = false;
      _forceRescanAndAnalyze = false;
      _ProcessUINotification(e);
    }


    void OnEnable() {
      UPAISystem.ProjectStatusRefreshAborted += UPAISystem_ProjectStatusRefreshAborted;
      UPAISystem.ProjectStatusRefreshAborting += UPAISystem_ProjectStatusRefreshAborting;
      UPAISystem.ProjectStatusRefreshCompleted += UPAISystem_ProjectStatusRefreshCompleted;
      UPAISystem.ProjectStatusRefreshCompleting += UPAISystem_ProjectStatusRefreshCompleting;
      UPAISystem.ProjectStatusRefreshStarted += UPAISystem_ProjectStatusRefreshStarted;
      UPAISystem.ProjectStatusRefreshStarting += UPAISystem_ProjectStatusRefreshStarting;
      _projectStatus = UPAISystem.GetProjectStatus(false);
    }

    void OnInspectorUpdate() {
      if (EditorApplication.isCompiling)
        Close();

      if (_projectStatus == null && _isRefreshing)
        return;

      Repaint();
    }

    void OnDestroy() {
      try {
        UPAIUnityEditorRecoveryManager.ProjectStatusWindowClosed();
      } catch {

      }

      UPAISystem.ProjectStatusRefreshAborted -= UPAISystem_ProjectStatusRefreshAborted;
      UPAISystem.ProjectStatusRefreshAborting -= UPAISystem_ProjectStatusRefreshAborting;
      UPAISystem.ProjectStatusRefreshCompleted -= UPAISystem_ProjectStatusRefreshCompleted;
      UPAISystem.ProjectStatusRefreshCompleting -= UPAISystem_ProjectStatusRefreshCompleting;
      UPAISystem.ProjectStatusRefreshStarted -= UPAISystem_ProjectStatusRefreshStarted;
      UPAISystem.ProjectStatusRefreshStarting -= UPAISystem_ProjectStatusRefreshStarting;
      _projectStatus = null;
      _isRefreshing = false;
      _forceRescanAndAnalyze = false;
      _instance = null;
    }

    void OnDisable() {
      UPAISystem.ProjectStatusRefreshAborted -= UPAISystem_ProjectStatusRefreshAborted;
      UPAISystem.ProjectStatusRefreshAborting -= UPAISystem_ProjectStatusRefreshAborting;
      UPAISystem.ProjectStatusRefreshCompleted -= UPAISystem_ProjectStatusRefreshCompleted;
      UPAISystem.ProjectStatusRefreshCompleting -= UPAISystem_ProjectStatusRefreshCompleting;
      UPAISystem.ProjectStatusRefreshStarted -= UPAISystem_ProjectStatusRefreshStarted;
      UPAISystem.ProjectStatusRefreshStarting -= UPAISystem_ProjectStatusRefreshStarting;
      _projectStatus = null;      
    }

    void OnGUI() {
      _DrawUI();
    }

    void Update() {
      if (EditorApplication.isCompiling)
        Close();

      if (_projectStatus == null && _isRefreshing)
        return;

      Repaint();
    }

    void _CancelProjectStatusReanalysis() {
      if (!_processIsRunning || _processIsRunning && _processName.Equals("Project Status Refresh", StringComparison.InvariantCultureIgnoreCase)) {
        Debug.LogWarningFormat("No index refresh process is currently running...");
        return;
      }
      Debug.Log("Cancelling index refresh...");
      _projectStatus = UPAISystem.GetProjectStatus(false);
      _isRefreshing = false;
    }

    void _ForceNewAssetCheck() {
      _isRefreshing = true;
      _processName = "Project New Asset Check";
      Debug.Log("Beginning check for new, un-imported assets...");
      EditorUtility.DisplayDialog("Feature In Development", "This functionality hasn't been implemented yet.", "Ok");
      _isRefreshing = false;
    }

    void _ForceProjectStatusReanalysis() {
      _isRefreshing = true;
      EditorUtility.DisplayDialog("Feature In Development", "This functionality hasn't been implemented yet.", "Ok");
      _isRefreshing = false;
    }

    void _ForceRefresh() {
      _isRefreshing = true;      
      EditorUtility.DisplayDialog("Feature In Development", "This functionality hasn't been implemented yet.", "Ok");
      _isRefreshing = false;
    }

    void _ForceSave() {
      if (_projectStatus == null || _isSaving) {
        Debug.LogWarningFormat("Unable to save project status in its current state...", _processName);
        return;
      }

      if (!_isDirty || _changedOptions.Count == 0) {
        Debug.LogWarning("No changes were found...");
        return;
      }

      Debug.Log("Saving Project Settings...");
      _isSaving = true;      
      foreach (var change in _changedOptions) {
        var trackingEntry = _projectStatus.DetectedPackages[change.Key];
        if (trackingEntry == null) {
          UPAISystem.Debug.LogException("UPAIProjectStatusWindow.cs", "_ForceSave", 
            new Exception(string.Format("Unable to locate source tracking entry for {0}.", change.Key)));
          continue;
        }

        UPAISystem.Debug.LogFormat("Updating {0} from {1} to {2}.", 
          trackingEntry.PackageName, trackingEntry.TrackingType.ToString(), change.Value.Value.ToString());
        trackingEntry.TrackingType = change.Value.Value;
      }

      _changedOptions = new Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>>();

      var detectedPackageKeys = _projectStatus.DetectedPackages.Keys.ToList();
      foreach (var packageKey in detectedPackageKeys) {      
        var packageTrackingEntry = _projectStatus.DetectedPackages[packageKey];
        if (packageTrackingEntry == null)
          continue;

        bool okToRemove = true;
        switch (packageTrackingEntry.TrackingType) {          
          case UPAIPackageTrackingTypeEnum.Imported:
            if (_projectStatus.ImportedPackages == null) {
              _projectStatus.ImportedPackages =
                new Dictionary<string, UPAIPackageTrackingEntry>(StringComparer.InvariantCultureIgnoreCase);
            }

            _projectStatus.ImportedPackages[packageKey] = packageTrackingEntry;            

            break;
          case UPAIPackageTrackingTypeEnum.Ignored:
            if (_projectStatus.IgnoredPackages == null) {
              _projectStatus.IgnoredPackages =
                new Dictionary<string, UPAIPackageTrackingEntry>(StringComparer.InvariantCultureIgnoreCase);
            }

            _projectStatus.IgnoredPackages[packageKey] = packageTrackingEntry;
            
            break;
          default:
            okToRemove = false;

            break;
        }

        if (okToRemove) {
          _projectStatus.DetectedPackages.Remove(packageKey);
        }
      }
     
      _trackingOptions = new Dictionary<string, KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>>();
      UPAISystem.SaveProjectStatusFile(_projectStatus);      
      _isSaving = false;
      _isDirty = false;
      _isRefreshing = false;
    }

    void _ForceShowUpdatedAssets() {
      _isRefreshing = true;
      _processName = "Display Updated Assets";
      Debug.Log("Displaying updated/changed assets...");
      EditorUtility.DisplayDialog("Feature In Development", "This functionality hasn't been implmented yet.", "Ok");
      _isRefreshing = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected int _GetSelectedPackageCount() {
      var selectedPackageCount = 0;

      if (_selectedDetectedPackageEntries == null)
        return selectedPackageCount;

      foreach (var entry in _selectedDetectedPackageEntries) {
        if (entry.Value)
          selectedPackageCount++;
      }

      return selectedPackageCount;
    }

    void _DrawUI() {
      _BeginHorizontal();
      _BeginVertical();
      _DrawSpaces(3);
      _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
      _DrawSpaces(2);
      _DrawHorizontalLabelField("General Project Information");
      _DrawHorizontalLabelValueField("Project Version", "N/A");
      _DrawHorizontalLabelValueField("Asset Count", "N/A");
      _DrawHorizontalLabelValueField("Non-Indexed Asset Count", "N/A");
      _DrawHorizontalLabelValueField("Indexed Asset Count", "N/A");
      _DrawHorizontalLabelValueField("New Asset Count", "N/A");
      _DrawHorizontalLabelValueField("Updatable Asset Count", "N/A");
      _DrawHorizontalLabelValueField("Refreshed On", "N/A");
      _DrawHorizontalLabelValueField("Refresh Time", "N/A");
      _DrawHorizontalLabelValueField("Refreshed With", "N/A");
      _DrawSpaces(2);
      _BeginHorizontal();
      _forceRefresh = GUILayout.Button("Refresh", GUILayout.Width(75));
      if (_forceRefresh && !_isRefreshing) {
        _EndHorizontal();
        _EndVertical();
        _EndHorizontal();
        _ForceRefresh();
        return;
      }
      _forceRescanAndAnalyze = GUILayout.Button("Detect Packages...", GUILayout.Width(225));
      if (_forceRescanAndAnalyze && !_isRefreshing) {
        _EndHorizontal();
        _EndVertical();
        _EndHorizontal();
        _ForceProjectStatusReanalysis();
        return;
      }
      _EndHorizontal();
      _EndVertical();
      _EndHorizontal();
    }

    void _DrawProjectIgnoredPackages(Dictionary<string, UPAIPackageTrackingEntry> ignoredPackages, int labelWidth = 375, int valueWidth = 90, int selectionWidth = 100) {
      if (ignoredPackages == null || ignoredPackages.Count == 0)
        return;

      if (_selectedIgnoredPackageEntries == null)
        _selectedIgnoredPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

      EditorGUILayout.BeginHorizontal();      
      EditorGUILayout.LabelField("Package Name", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("Project Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Indexed Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("----------------", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.EndHorizontal();

      foreach (var pair in ignoredPackages.OrderBy(x => x.Key)) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} :", pair.Value.PackageName), GUILayout.Width(labelWidth));
        EditorGUILayout.LabelField(pair.Value.NewAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(pair.Value.ProjectAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(pair.Value.IndexedAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.EndHorizontal();
      }
    }

    void _DrawProjectImportedPackages(Dictionary<string, UPAIPackageTrackingEntry> importedPackages, int labelWidth = 375, int valueWidth = 90, int selectionWidth = 100) {
      if (importedPackages == null || importedPackages.Count == 0)
        return;

      if (_selectedImportedPackageEntries == null)
        _selectedImportedPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Package Name", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("New Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Project Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Indexed Assets", GUILayout.Width(valueWidth));      
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("----------------", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("-------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));      
      EditorGUILayout.EndHorizontal();

      foreach (var pair in importedPackages.OrderBy(x => x.Key)) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} :", pair.Value.PackageName), GUILayout.Width(labelWidth));
        EditorGUILayout.LabelField(pair.Value.NewAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(pair.Value.ProjectAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(pair.Value.IndexedAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.EndHorizontal();
      }
    }

    void _DrawProjectDetectedPackages(Dictionary<string, UPAIPackageTrackingEntry> detectedPackages, int labelWidth = 375, int valueWidth = 90, int selectionWidth = 100) {
      if (detectedPackages == null || detectedPackages.Count == 0)
        return;

      if (_selectedDetectedPackageEntries == null)
        _selectedDetectedPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Package Name", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("Project Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Package Assets", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Probability", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("Tracking Type", GUILayout.Width(selectionWidth));
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("----------------", GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(valueWidth));
      EditorGUILayout.LabelField("----------------", GUILayout.Width(selectionWidth));
      EditorGUILayout.EndHorizontal();
           
      foreach (var pair in detectedPackages.OrderBy(x => x.Key)) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} :", pair.Value.PackageName), GUILayout.Width(labelWidth));
        EditorGUILayout.LabelField(pair.Value.ProjectAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(pair.Value.IndexedAssetCount.ToString(), GUILayout.Width(valueWidth));
        EditorGUILayout.LabelField(string.Format("{0}%", pair.Value.Probability.ToString("N2"), GUILayout.Width(valueWidth)));
        _trackingOptions[pair.Key] = new KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>(
          pair.Value.TrackingType,(UPAIPackageTrackingTypeEnum)EditorGUILayout.EnumPopup(
            new GUIContent("", string.Format("Change {0}...", pair.Value.PackageName)), pair.Value.TrackingType, GUILayout.Width(selectionWidth)));
        if (_trackingOptions[pair.Key].Key != _trackingOptions[pair.Key].Value  && !_isSaving && !_isRefreshing) {
          var newTrackingValue = _trackingOptions[pair.Key].Value;
          var selectedPackageCount = _GetSelectedPackageCount();
          if (selectedPackageCount > 0) {
            foreach (var packageEntry in _selectedDetectedPackageEntries) {
              if (!packageEntry.Value)
                continue;

              var entryToUpdate = detectedPackages[packageEntry.Key];
              if (entryToUpdate == null)
                continue;

              if (entryToUpdate.TrackingType != newTrackingValue) {
                _changedOptions[entryToUpdate.PackageName] = new KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>(_trackingOptions[entryToUpdate.PackageName].Key, newTrackingValue);
                entryToUpdate.TrackingType = newTrackingValue;
                _isDirty = true;
              }
            }

            _selectedDetectedPackageEntries = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
          } else {
            _changedOptions[pair.Value.PackageName] = new KeyValuePair<UPAIPackageTrackingTypeEnum, UPAIPackageTrackingTypeEnum>(_trackingOptions[pair.Key].Key, newTrackingValue);
            pair.Value.TrackingType = newTrackingValue;
            _isDirty = true;
          }
        }

        if (!_selectedDetectedPackageEntries.ContainsKey(pair.Key))
          _selectedDetectedPackageEntries[pair.Key] = false;

        var selectedPackage = _DrawHorizontalToggleField((_changedOptions.ContainsKey(pair.Value.PackageName))
          ? "[+]  "
          : "[-]  ", _selectedDetectedPackageEntries[pair.Key], 30, 30, string.Format("Select {0}...", pair.Key));
        if (selectedPackage.HasValue) {
          _selectedDetectedPackageEntries[pair.Key] = selectedPackage.Value;
        }
        EditorGUILayout.EndHorizontal();
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