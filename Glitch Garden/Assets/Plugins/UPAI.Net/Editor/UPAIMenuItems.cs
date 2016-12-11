using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Events.Abstract;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// Manages the menu item functionality for the UPAI
  /// Note: The menu items in this script depend on whether or not
  /// the system is enabled or disabled.
  /// The system should only be enabled for any index maintenace,
  /// Updating/Processing Package Data, Backing Up, Optimizing, and
  /// Restoring index information.  All other processing, such as status
  /// and searching, doesn't require maintenance to be enabled.
  /// </summary> 
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.ScriptFileVersion)]
  public class UPAIMenuItems {
    private static bool _callbacksEstablished = false;
    private static bool? _systemInitialized = null;   
    private static string _systemHelpUrl = "";
    private static string _systemManualPath = "";
    private static string _systemManualUrl = "";
    private static string _systemRegistrationUrl = "";
    private static string _systemProductSupportContactUrl = "";
    private static string _systemWebVersion = "";
    private static UPAIUINotificationsProcessor _uiNotificationProcessor = null;

    void StartUPAISystem() {
      ResetUPAISystem();
    }

    void StopUPAISystem() {
      DisableCallbackFunctions();
      UPAISystem.Exit(); 
      _systemInitialized = null;
      _systemHelpUrl = null;
      _systemManualPath = null;
      _systemManualUrl = null;
      _systemWebVersion = null;
    }

    void ResetUPAISystem() {
      _systemInitialized = null;
      UPAISystem.IsIntialized();     
      EnableCallbackFunctions();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool SystemIsInitialized() {
      var initializationStatus = UPAISystem.IsIntialized();
      return (!UPAISystem.IsCheckingInitialization() && initializationStatus.HasValue && initializationStatus.Value == true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static bool _IsInitialized() {
      if (UPAISystem.IsCheckingInitialization())
        return false;

      if (!_systemInitialized.HasValue && !_callbacksEstablished) {
        EnableCallbackFunctions();        
      }

      _systemInitialized = UPAISystem.IsIntialized();
      if (!_systemInitialized.HasValue)
        return false;

      return _systemInitialized.Value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsIndexMaintenanceEnbled() {
      return UPAISystem.IsIndexMaintenanceEnabled();
    }

    [MenuItem("UPAI/Help/Documentation/UPAI Manual (.pdf)")]
    static void UPAI_Open_SysManual_PDF() {
      var pdf = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_systemManualPath);
      if (!pdf) {
        UPAISystem.Debug.LogErrorFormat("Unable to load UPAI System Manual from asset path {0}", _systemManualPath);
        return;
      }

      AssetDatabase.OpenAsset(pdf);
    }

    [MenuItem("UPAI/Help/Documentation/UPAI Manual (.pdf)", true)]
    static bool Validate_UPAI_Open_SysManual_PDF() {
      if (string.IsNullOrEmpty(_systemManualPath)) {
        var manualGuid = AssetDatabase.FindAssets("UPAI System Manual", new string[] { "Assets/Plugins/UPAI.Net/Documentation" });
        if (manualGuid.Length == 0)
          return false;

        _systemManualPath = AssetDatabase.GUIDToAssetPath(manualGuid[0]);
      }

      return (!string.IsNullOrEmpty(_systemManualPath));
    }

    [MenuItem("UPAI/Help/Documentation/UPAI Manual (Web)")]
    static void UPAI_Open_SysManual_Online() {
      Application.OpenURL(_systemManualUrl);
    }

    [MenuItem("UPAI/Help/Documentation/UPAI Manual (Web)", true)]
    static bool Validate_UPAI_Open_SysManual_Online() {
      if (string.IsNullOrEmpty(_systemManualUrl)) {
        _systemManualUrl = UPAIWebClient.GetProductDocumentUrl();
      }

      return (!string.IsNullOrEmpty(_systemManualUrl));
    }

    [MenuItem("UPAI/Help/Contact Product Support...")]
    static void UPAI_Open_Help_ContactProductSupport_Online() {
      Application.OpenURL(_systemProductSupportContactUrl);
    }

    [MenuItem("UPAI/Help/Contact Product Support...", true)]
    static bool Validate_UPAI_Open_Help_ContactProductSupport_Online() {
      if (string.IsNullOrEmpty(_systemProductSupportContactUrl)) {
        _systemProductSupportContactUrl = UPAIWebClient.GetProductSupportContactUrl();
      }

      return (!string.IsNullOrEmpty(_systemProductSupportContactUrl));
    }

    [MenuItem("UPAI/Help/Online Product Registration")]
    static void UPAI_Open_Help_ProductRegistration_Online() {
      Application.OpenURL(_systemRegistrationUrl);
    }

    [MenuItem("UPAI/Help/Online Product Registration", true)]
    static bool Validate_UPAI_Open_Help_ProductRegistration_Online() {
      if (string.IsNullOrEmpty(_systemHelpUrl)) {
        _systemRegistrationUrl = UPAIWebClient.GetProductRegistrationUrl();
      }

      return (!string.IsNullOrEmpty(_systemRegistrationUrl));
    }

    [MenuItem("UPAI/Help/Online Support Forums")]
    static void UPAI_Open_Help_Online() {
      Application.OpenURL(_systemHelpUrl);
    }

    [MenuItem("UPAI/Help/Online Support Forums", true)]
    static bool Validate_UPAI_Open_Help_Online() {
      if (string.IsNullOrEmpty(_systemHelpUrl)) {
        _systemHelpUrl = UPAIWebClient.GetProductHelpUrl();
      }

      return (!string.IsNullOrEmpty(_systemHelpUrl));
    }

    [MenuItem("UPAI/Help/Check For Updates...")]
    static void UPAI_CheckForUpdates() {
      UPAISystem.RefreshSystemData();

      var upaiSystemVersion = new UPAIVersion(UPAISystem.UPAIVersion);
      var webSystemVersion = new UPAIVersion(_systemWebVersion);

      var messageToLog = string.Format("You are already running the latest version of the UPAI ({0}).", upaiSystemVersion.ToString());
      if (upaiSystemVersion >= webSystemVersion) {
        UPAISystem.Debug.Log(messageToLog);
        EditorUtility.DisplayDialog("UPAI Version Check", messageToLog, "Ok");
        return;
      } else if (upaiSystemVersion < webSystemVersion) {
        messageToLog = string.Format("An updated version of the UPAI ({0}) is available for {1}.", webSystemVersion.ToString(), upaiSystemVersion.ToString());
        UPAISystem.Debug.Log(messageToLog);
        var proceedToAssetPage = EditorUtility.DisplayDialog("UPAI Version Check - Update Availble", messageToLog + " Would you like to go to the Asset Store to download?", "Yes", "No");
        if (!proceedToAssetPage) {
          return;
        }

        var assetStorePageUrl = UPAIWebClient.GetAssetStoreUrl();
        if (string.IsNullOrEmpty(assetStorePageUrl)) {
          UPAISystem.Debug.LogError("Unable to retrieve the asset store page url fromthe web api.");
        }

        Application.OpenURL(UPAIWebClient.GetAssetStoreUrl());
      }
    }

    [MenuItem("UPAI/Help/Check For Updates...", true)]
    static bool Validate_UPAI_CheckForUpdates() {
      if (string.IsNullOrEmpty(_systemWebVersion)) {
        _systemWebVersion = UPAIWebClient.GetCurrentVersion();
      }

      return (!string.IsNullOrEmpty(_systemWebVersion));
    }

    [MenuItem("UPAI/Help/About...")]
    static void UPAI_Open_Help_About() {
      UPAISettingsWindow.CreateSettingsWindow(UPAISettingsWindow.SETTINGSWINDOW_ABOUT);
    }

    [MenuItem("UPAI/Help/About...", true)]
    static bool Validate_UPAI_Open_Help_About() {
      return (_IsInitialized());
    }

    [MenuItem("UPAI/System/Maintenance/Enable", false)]
    static void UPAI_IdxMain_EnableMaintenance() {
      UPAISystem.EnableIndexMaintenance();
    }

    [MenuItem("UPAI/System/Maintenance/Enable", true)]
    static bool Validate_UPAI_IdxMain_EnableMaintenance() {      
      return (_IsInitialized() && !IsIndexMaintenanceEnbled());
    }

    [MenuItem("UPAI/System/Maintenance/Disable", false)]
    static void UPAI_IdxMain_DisableMaintenance() {
      UPAISystem.DisableIndexMaintenance();
    }

    [MenuItem("UPAI/System/Maintenance/Disable", true)]
    static bool Validate_UPAI_IdxMain_DisableMaintenance() {      
      return (_IsInitialized() && IsIndexMaintenanceEnbled());
    }

    [MenuItem("UPAI/System/Maintenance/Backup Index", false)]
    static void UPAI_IdxMain_BackupIndex() {
      UPAISystem.BackupIndex();
    }

    [MenuItem("UPAI/System/Maintenance/Backup Index", true)]
    static bool Validate_UPAI_IdxMain_BackupIndex() {      
      return (_IsInitialized() && IsIndexMaintenanceEnbled() && UPAISystem.GetIndexCapabilities().CanBeBackedUp());
    }

    [MenuItem("UPAI/System/Maintenance/Optimize Index", false)]
    static void UPAI_IdxMain_OptimizeIndex() {
      UPAISystem.OptimizeIndex();        
    }

    [MenuItem("UPAI/System/Maintenance/Optimize Index", true)]
    static bool Validate_UPAI_IdxMain_OptimizeIndex() {      
      return (_IsInitialized() && IsIndexMaintenanceEnbled() && UPAISystem.GetIndexCapabilities().CanBeOptimized());
    }

    /// <summary>
    /// 
    /// </summary>
    [MenuItem("UPAI/System/Maintenance/View Index Status", false)]
    static void UPAI_GetStatus_Index() {
      UPAIIndexStatusWindow.ShowWindow();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [MenuItem("UPAI/System/Maintenance/View Index Status", true)]
    static bool Validate_UPAI_GetStatus_Index() {
      return _IsInitialized();
    }

    [MenuItem("UPAI/Packages/Check for Updates", false)]
    static void UPAI_IdxMain_UpdateIndex() {      
      UPAISystem.UpdateIndex();
    }

    [MenuItem("UPAI/Packages/Check for Updates", true)]
    static bool Validate_UPAI_IdxMain_UpdateIndex() {
      if (!_IsInitialized())
        return false;

      if (!IsIndexMaintenanceEnbled())
        UPAISystem.EnableIndexMaintenance();

      return (IsIndexMaintenanceEnbled() && UPAISystem.GetIndexCapabilities().CanBeUpdated());
    }

    /// <summary>
    /// 
    /// </summary>
    [MenuItem("UPAI/System/Initialize...")]
    static void UPAI_System_Initialize() {
      UPAISystem.Debug.LogFormat("Initializing Unity Package Asset Index for Unity {0}", Application.unityVersion);
      if (UPAISystem.Initialize()) {
        UPAISystem.Debug.Log("System initilaized successfully!!!");
        var analyzePackages = EditorUtility.DisplayDialog("Generate Unity Package Asset Index",
          "Would you like to start the process for analyzing assets in current unity package files?", "Yes", "No");
        if (!analyzePackages) {
          UPAISystem.Debug.Log("Cancelling analysis of unity package files.");
          return;
        }

        UPAISystem.EnableIndexMaintenance();
        if (!UPAISystem.IsIndexMaintenanceEnabled()) {
          UPAISystem.Debug.LogException(
            "UPAIInitializer",
            "InitializeUPAISystemFunction",
            new System.Exception("Unable to enable index maintenance functions, please contact support."));
          return;
        }
        
        if (!UPAISystem.GetIndexCapabilities().CanBeUpdated()) {
          UPAISystem.Debug.LogException(
            "UPAIInitializer",
            "InitializeUPAISystemFunction",
            new System.Exception("UPAI Index is not able to be updated at this time, please contact support."));
          return;
        }

        UPAISystem.UpdateIndex();

        _systemInitialized = true;
      } else {
        UPAISystem.Debug.LogError("Unable to initialize Unity Package Asset Index system, please contact support.");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [MenuItem("UPAI/System/Initialize...", true)]
    static bool Validate_UPAI_System_Initialize() {
      return (!_IsInitialized());
    }

    [MenuItem("UPAI/System/Recover UPAI System...")]
    static void UPAI_System_RecoverUPAISystem() {
      var systemDataRecoveryEditorWindow = EditorWindow.GetWindowWithRect<UPAISystemDataRecoveryEditorWindow>(new Rect(0, 0, 165, 40));
      if (systemDataRecoveryEditorWindow != null)
        systemDataRecoveryEditorWindow.Show();
    }

    [MenuItem("UPAI/System/Recover UPAI System...", true)]
    static bool Validate_UPAI_System_RecoverUPAISystem() {
      return (_IsInitialized() &&
        UPAIUnityEditorRecoveryManager.CanBeRecovered());
    }

    [MenuItem("UPAI/System/Settings...")]
    static void UPAI_Edit_Settings() {
      UPAISettingsWindow.CreateSettingsWindow();
    }

    [MenuItem("UPAI/System/Settings...", true)]
    static bool Validate_UPAI_Edit_Settings() {
      return _IsInitialized();
    }

    [MenuItem("UPAI/System/View System Status", false)]
    static void UPAI_GetStatus_System() {
      UPAISystemStatusWindow.ShowWindow();
    }

    [MenuItem("UPAI/System/View System Status", true)]
    static bool Validate_UPAI_GetStatus_System() {
      return _IsInitialized();
    }

    [MenuItem("UPAI/Current Project/View Status", false)]
    static void UPAI_GetStatus_Project() {
      UPAIProjectStatusWindow.ShowWindow();
    }

    [MenuItem("UPAI/Current Project/View Status", true)]
    static bool Validate_UPAI_GetStatus_Project() {
      return _IsInitialized();
    }

    /// <summary>
    /// 
    /// </summary>
    [MenuItem("UPAI/Search Assets... _F3", false)]
    static void UPAI_IdxSearch_Assets() {
      var indexStatus = UPAISystem.GetIndexStatus(false);
      if (indexStatus == null) {
        _uiNotificationProcessor.DisplayDialog("Asset Search Pre-Requesite",
          "Please open up the Index Status page, and click on Refresh button.  " +
            "Once it has refreshed successfully, please try re-opening the Asset Search Page.",
          "Close");
        return;
      } else if (indexStatus.AssetsIndexed == 0) {
        _uiNotificationProcessor.DisplayDialog("Asset Search Pre-Requesite",
          "No assets appear to have been indexed.  " +
            "Please run an Index Update from the UPAI Index Maintenance menu.",
          "Close");
        return;
      }
      
      UPAIAssetSearchWindow.CreateAssetSearchWindow();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [MenuItem("UPAI/Search Assets... _F3", true)]
    static bool Validate_UPAI_IdxSearch_Assets() {
      return _IsInitialized();
    }

    /// <summary>
    /// 
    /// </summary>
    [MenuItem("UPAI/Search Packages... #F3", false)]
    static void UPAI_IdxSearch_Packages() {
      var indexStatus = UPAISystem.GetIndexStatus(false);
      if (indexStatus == null) {
        _uiNotificationProcessor.DisplayDialog("Package Search Pre-Requesite",
          "Please open up the Index Status page, and click on Refresh button.  " +
            "Once it has refreshed successfully, please try re-opening the Package Search Page.",
          "Close");
        return;
      } else if (indexStatus.PackagesIndexed == 0) {
        _uiNotificationProcessor.DisplayDialog("Package Search Pre-Requesite",
          "No packages appear to have been indexed.  " +
            "Please run an Index Update from the UPAI Index Maintenance menu.",
          "Close");
        return;
      }

      UPAIPackageSearchWindow.CreatePackageSearchWindow();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [MenuItem("UPAI/Search Packages... #F3", true)]
    static bool Validate_UPAI_IdxSearch_Packages() {
      return _IsInitialized();
    }

    static void CheckForSystemCleanup() {
      var itemsToCleanup = UPAISystemCleaner.CheckForCleanup();
      if (itemsToCleanup == 0)
        return;

      var processCleanup = EditorUtility.DisplayDialog("UPAI System Cleanup", string.Format("{0} items were found from prior versions of the system.  Is it ok to delete these files now?", itemsToCleanup), "Yes", "No");
      if (!processCleanup)
        return;

      if (UPAISystemCleaner.CleanupSystem()) {
        EditorUtility.DisplayDialog("UPAI System Cleanup", "All items were successfully removed.", "Ok");
      } else {
        EditorUtility.DisplayDialog("UPAI System Cleanup (Warning)", "Not all items were able to be removed.  Please check the log file for more details.", "Ok");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    static void DisableCallbackFunctions() {
      if (_callbacksEstablished) {
        if (_uiNotificationProcessor != null && _uiNotificationProcessor.IsEnabled())
          _uiNotificationProcessor.Disable();

        UPAISystem.SystemDataRefreshAborted -= UPAISystem_SystemDataRefreshAborted;
        UPAISystem.SystemDataRefreshAborting -= UPAISystem_SystemDataRefreshAborting;
        UPAISystem.SystemDataRefreshCompleted -= UPAISystem_SystemDataRefreshCompleted;
        UPAISystem.SystemDataRefreshCompleting -= UPAISystem_SystemDataRefreshCompleting;
        UPAISystem.SystemDataRefreshStarted -= UPAISystem_SystemDataRefreshStarted;
        UPAISystem.SystemDataRefreshStarting -= UPAISystem_SystemDataRefreshStarting;

        UPAISystem.SystemInitializationAborted -= UPAISystem_SystemInitializationAborted;
        UPAISystem.SystemInitializationAborting -= UPAISystem_SystemInitializationAborting;
        UPAISystem.SystemInitializationCompleted -= UPAISystem_SystemInitializationCompleted;
        UPAISystem.SystemInitializationCompleting -= UPAISystem_SystemInitializationCompleting;
        UPAISystem.SystemInitializationStarted -= UPAISystem_SystemInitializationStarted;
        UPAISystem.SystemInitializationStarting -= UPAISystem_SystemInitializationStarting;

        UPAISystem.IndexMaintenanceChangeAborted -= UPAISystem_IndexMaintenanceChangeAborted;
        UPAISystem.IndexMaintenanceChangeAborting -= UPAISystem_IndexMaintenanceChangeAborting;
        UPAISystem.IndexMaintenanceChangeCompleted -= UPAISystem_IndexMaintenanceChangeCompleted;
        UPAISystem.IndexMaintenanceChangeCompleting -= UPAISystem_IndexMaintenanceChangeCompleting;
        UPAISystem.IndexMaintenanceChangeStarted -= UPAISystem_IndexMaintenanceChangeStarted;
        UPAISystem.IndexMaintenanceChangeStarting -= UPAISystem_IndexMaintenanceChangeStarting;

        UPAISystem.BackupProcessAborted -= UPAISystem_BackupProcessAborted;
        UPAISystem.BackupProcessAborting -= UPAISystem_BackupProcessAborting;
        UPAISystem.BackupProcessCompleted -= UPAISystem_BackupProcessCompleted;
        UPAISystem.BackupProcessCompleting -= UPAISystem_BackupProcessCompleting;
        UPAISystem.BackupProcessStarted -= UPAISystem_BackupProcessStarted;
        UPAISystem.BackupProcessStarting -= UPAISystem_BackupProcessStarting;

        UPAISystem.OptimizationProcessAborted -= UPAISystem_OptimizationProcessAborted;
        UPAISystem.OptimizationProcessAborting -= UPAISystem_OptimizationProcessAborting;
        UPAISystem.OptimizationProcessCompleted -= UPAISystem_OptimizationProcessCompleted;
        UPAISystem.OptimizationProcessCompleting -= UPAISystem_OptimizationProcessCompleting;
        UPAISystem.OptimizationProcessStarted -= UPAISystem_OptimizationProcessStarted;
        UPAISystem.OptimizationProcessStarting -= UPAISystem_OptimizationProcessStarting;

        UPAISystem.RestorationProcessAborted -= UPAISystem_RestorationProcessAborted;
        UPAISystem.RestorationProcessAborting -= UPAISystem_RestorationProcessAborting;
        UPAISystem.RestorationProcessCompleted -= UPAISystem_RestorationProcessCompleted;
        UPAISystem.RestorationProcessCompleting -= UPAISystem_RestorationProcessCompleting;
        UPAISystem.RestorationProcessStarted -= UPAISystem_RestorationProcessStarted;
        UPAISystem.RestorationProcessStarting -= UPAISystem_RestorationProcessStarting;

        UPAISystem.UpdateProcessAborted -= UPAISystem_UpdateProcessAborted;
        UPAISystem.UpdateProcessAborting -= UPAISystem_UpdateProcessAborting;
        UPAISystem.UpdateProcessCompleted -= UPAISystem_UpdateProcessCompleted;
        UPAISystem.UpdateProcessCompleting -= UPAISystem_UpdateProcessCompleting;
        UPAISystem.UpdateProcessStarted -= UPAISystem_UpdateProcessStarted;
        UPAISystem.UpdateProcessStarting -= UPAISystem_UpdateProcessStarting;

        UPAISystem.UpgradeProcessAborted -= UPAISystem_UpgradeProcessAborted;
        UPAISystem.UpgradeProcessAborting -= UPAISystem_UpgradeProcessAborting;
        UPAISystem.UpgradeProcessCompleted -= UPAISystem_UpgradeProcessCompleted;
        UPAISystem.UpgradeProcessCompleting -= UPAISystem_UpgradeProcessCompleting;
        UPAISystem.UpgradeProcessStarted -= UPAISystem_UpgradeProcessStarted;
        UPAISystem.UpgradeProcessStarting -= UPAISystem_UpgradeProcessStarting;

        _callbacksEstablished = false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void EnableCallbackFunctions() {
      if (!_callbacksEstablished) {  
        if (_uiNotificationProcessor == null) {
          _uiNotificationProcessor = new UPAIUINotificationsProcessor(
            new List<UPAIProcessTypeEnum>() {
              UPAIProcessTypeEnum.BuildAssetDependencyMap,
              UPAIProcessTypeEnum.BuildPackageAssetDependencies,
              UPAIProcessTypeEnum.BuildPackageFileMap,
              UPAIProcessTypeEnum.ExtractPackage,
              UPAIProcessTypeEnum.SystemDataRefresh,
              UPAIProcessTypeEnum.SystemInitialization,              
              UPAIProcessTypeEnum.IndexMaintenanceChange,
              UPAIProcessTypeEnum.IndexBackup,
              UPAIProcessTypeEnum.IndexOptimization,
              UPAIProcessTypeEnum.IndexUpdate,
              UPAIProcessTypeEnum.IndexUpgrade,              
              UPAIProcessTypeEnum.IndexWrite,
              UPAIProcessTypeEnum.PackageScan
            },
            new List<Action>() {
              DisableCallbackFunctions
            },
            new List<Action>() {
              EnableCallbackFunctions
            });
        } else if (!_uiNotificationProcessor.IsEnabled()) {
          _uiNotificationProcessor.Enable();
        }

        UPAISystem.SystemDataRefreshAborted += UPAISystem_SystemDataRefreshAborted;
        UPAISystem.SystemDataRefreshAborting += UPAISystem_SystemDataRefreshAborting;
        UPAISystem.SystemDataRefreshCompleted += UPAISystem_SystemDataRefreshCompleted;
        UPAISystem.SystemDataRefreshCompleting += UPAISystem_SystemDataRefreshCompleting;
        UPAISystem.SystemDataRefreshStarted += UPAISystem_SystemDataRefreshStarted;
        UPAISystem.SystemDataRefreshStarting += UPAISystem_SystemDataRefreshStarting;

        UPAISystem.SystemInitializationAborted += UPAISystem_SystemInitializationAborted;
        UPAISystem.SystemInitializationAborting += UPAISystem_SystemInitializationAborting;
        UPAISystem.SystemInitializationCompleted += UPAISystem_SystemInitializationCompleted;
        UPAISystem.SystemInitializationCompleting += UPAISystem_SystemInitializationCompleting;
        UPAISystem.SystemInitializationStarted += UPAISystem_SystemInitializationStarted;
        UPAISystem.SystemInitializationStarting += UPAISystem_SystemInitializationStarting;

        UPAISystem.IndexMaintenanceChangeAborted += UPAISystem_IndexMaintenanceChangeAborted;
        UPAISystem.IndexMaintenanceChangeAborting += UPAISystem_IndexMaintenanceChangeAborting;
        UPAISystem.IndexMaintenanceChangeCompleted += UPAISystem_IndexMaintenanceChangeCompleted;
        UPAISystem.IndexMaintenanceChangeCompleting += UPAISystem_IndexMaintenanceChangeCompleting;
        UPAISystem.IndexMaintenanceChangeStarted += UPAISystem_IndexMaintenanceChangeStarted;
        UPAISystem.IndexMaintenanceChangeStarting += UPAISystem_IndexMaintenanceChangeStarting;

        UPAISystem.BackupProcessAborted += UPAISystem_BackupProcessAborted;
        UPAISystem.BackupProcessAborting += UPAISystem_BackupProcessAborting;
        UPAISystem.BackupProcessCompleted += UPAISystem_BackupProcessCompleted;
        UPAISystem.BackupProcessCompleting += UPAISystem_BackupProcessCompleting;
        UPAISystem.BackupProcessStarted += UPAISystem_BackupProcessStarted;
        UPAISystem.BackupProcessStarting += UPAISystem_BackupProcessStarting;

        UPAISystem.OptimizationProcessAborted += UPAISystem_OptimizationProcessAborted;
        UPAISystem.OptimizationProcessAborting += UPAISystem_OptimizationProcessAborting;
        UPAISystem.OptimizationProcessCompleted += UPAISystem_OptimizationProcessCompleted;
        UPAISystem.OptimizationProcessCompleting += UPAISystem_OptimizationProcessCompleting;
        UPAISystem.OptimizationProcessStarted += UPAISystem_OptimizationProcessStarted;
        UPAISystem.OptimizationProcessStarting += UPAISystem_OptimizationProcessStarting;

        UPAISystem.ProgressTrackingCompleted += UPAISystem_ProgressTrackingCompleted;
        UPAISystem.ProgressTrackingCompleting += UPAISystem_ProgressTrackingCompleting;
        UPAISystem.ProgressTrackingStarted += UPAISystem_ProgressTrackingStarted;
        UPAISystem.ProgressTrackingStarting += UPAISystem_ProgressTrackingStarting;

        UPAISystem.ProgressTrackingStepCompleted += UPAISystem_ProgressTrackingStepCompleted;
        UPAISystem.ProgressTrackingStepCompleting += UPAISystem_ProgressTrackingStepCompleting;
        UPAISystem.ProgressTrackingStepStarted += UPAISystem_ProgressTrackingStepStarted;
        UPAISystem.ProgressTrackingStepStarting += UPAISystem_ProgressTrackingStepStarting;

        UPAISystem.RestorationProcessAborted += UPAISystem_RestorationProcessAborted;
        UPAISystem.RestorationProcessAborting += UPAISystem_RestorationProcessAborting;
        UPAISystem.RestorationProcessCompleted += UPAISystem_RestorationProcessCompleted;
        UPAISystem.RestorationProcessCompleting += UPAISystem_RestorationProcessCompleting;
        UPAISystem.RestorationProcessStarted += UPAISystem_RestorationProcessStarted;
        UPAISystem.RestorationProcessStarting += UPAISystem_RestorationProcessStarting;

        UPAISystem.UpdateProcessAborted += UPAISystem_UpdateProcessAborted;
        UPAISystem.UpdateProcessAborting += UPAISystem_UpdateProcessAborting;
        UPAISystem.UpdateProcessCompleted += UPAISystem_UpdateProcessCompleted;
        UPAISystem.UpdateProcessCompleting += UPAISystem_UpdateProcessCompleting;
        UPAISystem.UpdateProcessStarted += UPAISystem_UpdateProcessStarted;
        UPAISystem.UpdateProcessStarting += UPAISystem_UpdateProcessStarting;

        UPAISystem.UpgradeProcessAborted += UPAISystem_UpgradeProcessAborted;
        UPAISystem.UpgradeProcessAborting += UPAISystem_UpgradeProcessAborting;
        UPAISystem.UpgradeProcessCompleted += UPAISystem_UpgradeProcessCompleted;
        UPAISystem.UpgradeProcessCompleting += UPAISystem_UpgradeProcessCompleting;
        UPAISystem.UpgradeProcessStarted += UPAISystem_UpgradeProcessStarted;
        UPAISystem.UpgradeProcessStarting += UPAISystem_UpgradeProcessStarting;

        _callbacksEstablished = true;

        CheckForSystemCleanup();
      }
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

    private static void _ProcessUINotification(UPAIProcessEventArgs eventArgs) {
      _uiNotificationProcessor.ProcessUINotification(eventArgs);      
    }

    private static void UPAISystem_RestorationProcessStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_RestorationProcessStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_RestorationProcessCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_RestorationProcessCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_RestorationProcessAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_RestorationProcessAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpgradeProcessAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpdateProcessStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpdateProcessStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpdateProcessCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpdateProcessCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_UpdateProcessAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_UpdateProcessAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_OptimizationProcessStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_OptimizationProcessStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_OptimizationProcessCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_OptimizationProcessCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_OptimizationProcessAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_OptimizationProcessAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_BackupProcessStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_BackupProcessStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_BackupProcessCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_BackupProcessCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_BackupProcessAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_BackupProcessAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_SystemInitializationStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemInitializationStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemInitializationCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemInitializationCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
      if (!e.IsSubProcess)
        UPAI_IdxMain_DisableMaintenance();
    }

    private static void UPAISystem_SystemInitializationAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemInitializationAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_SystemDataRefreshAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeStarting(object sender, UPAIProcessStartingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeStarted(object sender, UPAIProcessStartedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeCompleting(object sender, UPAIProcessCompletingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeCompleted(object sender, UPAIProcessCompletedEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeAborting(object sender, UPAIProcessAbortingEventArgs e) {
      _ProcessUINotification(e);
    }

    private static void UPAISystem_IndexMaintenanceChangeAborted(object sender, UPAIProcessAbortedEventArgs e) {
      _ProcessUINotification(e);
    }
  }
}