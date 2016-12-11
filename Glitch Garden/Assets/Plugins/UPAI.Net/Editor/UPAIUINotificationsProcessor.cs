using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Events.Abstract;
using RKGamesDev.Systems.UPAI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.ScriptFileVersion)]
  public class UPAIUINotificationsProcessor {
    /// <summary>
    /// Processing UI Notifications
    /// </summary>
    private List<Action> _actionsForEngineRunning = null;
    private List<Action> _actionsForEngineNotRunning = null;
    private bool _assetDatabaseIsRefreshing = false;
    private string _assetGuid = null;
    private Type _assetType = null;
    private bool _clearTimedProgressBar = false;
    private string _dialogTitle = "";
    private string _dialogMessage = "";
    private string _dialogButtonCaption = "Ok";
    private bool _displayDialog = false;    
    private bool _displayProgressBar = false;
    private bool _isDisabled = false;
    private bool _isInitialized = false;
    private bool _isTimedProcessRunning = false;
    private UPAIProcessEventArgs _lastUPAIProcessEventArgs = null;
    private UPAIProgressBarStatus _lastProgressBarStatus = null;
    private bool _pingAsset = false;
    private List<UPAIProcessTypeEnum> _processTypesToMonitor = null;
    private bool _refreshAssetDatabase = false;
    private UPAIProcessTypeEnum _timedProgressProcessType = UPAIProcessTypeEnum.Unknown;
    private string _timedProgressBarDescription = null;
    private UPAIProgressBarStatus _timedProgressBarStatus = null;
    private string _timedProgressBarTitle = null;
    private TimeSpan _timedProgressLastProcessingTime = new TimeSpan();
    private Stopwatch _timedProgressStopwatch = new Stopwatch();

    /// <summary>
    /// 
    /// </summary>
    public void ClearProgressBar() {
      if (_displayProgressBar) {
        _lastUPAIProcessEventArgs = null;
        _lastProgressBarStatus = null;
        _timedProgressBarStatus = null;
        _displayProgressBar = false;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disable() {
      if (_isDisabled)
        return;

      EditorApplication.playmodeStateChanged -= EditorApp_PlayModeStateChanged;
      EditorApplication.update -= EditorApp_Update;
      _isDisabled = true;
    }

    public void DisplayDialog(string title, string message, string buttonCaption = "Ok") {
      _dialogButtonCaption = buttonCaption;
      if (string.IsNullOrEmpty(_dialogButtonCaption))
        _dialogButtonCaption = "Ok";

      if (string.IsNullOrEmpty(title)) {
        message = "Title cannot be null or empty.";
        UnityEngine.Debug.LogError(message);
        _dialogTitle = "Display Dialog Error";
        _dialogMessage = message;
        _displayDialog = true;
      } else if (string.IsNullOrEmpty(message)) {
        message = "Message cannot be null or empty.";
        UnityEngine.Debug.LogError(message);
        _dialogTitle = "Display Dialog Error";
        _dialogMessage = message;       
        _displayDialog = true;
      } else {
        _dialogTitle = title;
        _dialogMessage = message;
        _displayDialog = true;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Enable() {
      if (!_isDisabled)
        return;

      EditorApplication.playmodeStateChanged += EditorApp_PlayModeStateChanged;
      EditorApplication.update += EditorApp_Update;
      _isDisabled = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsEnabled() {
      return !_isDisabled;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsInitialized() {
      return _isInitialized;
    }

    public void PingAsset(string assetGuid, Type assetType) {
      var message = "";
      if (string.IsNullOrEmpty(assetGuid)) {
        message = "AssetGuid cannot be null or empty.";
        UnityEngine.Debug.LogError(message);
        _dialogTitle = "Ping Asset Error";
        _dialogMessage = message;
        _displayDialog = true;
        return;
      }

      if (assetType == null) {
        message = "AssetType cannot be null.";
        UnityEngine.Debug.LogError(message);
        _dialogTitle = "Ping Asset Error";
        _dialogMessage = message;
        _displayDialog = true;
        return;
      }

      _assetGuid = assetGuid;
      _assetType = assetType;
      _pingAsset = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventArgs"></param>
    public void ProcessUINotification(UPAIProcessEventArgs eventArgs) {
      if (eventArgs == null || !_IsMonitoredProcessType(eventArgs.ProcessType))
        return;

      _lastUPAIProcessEventArgs = eventArgs;

      if (!string.IsNullOrEmpty(eventArgs.NotificationMessage)) {
        switch (eventArgs.ProcessNotificationType) {
          case UPAIProcessUINotificationTypeEnum.Console:
            UnityEngine.Debug.Log(eventArgs.NotificationMessage);
            break;
          case UPAIProcessUINotificationTypeEnum.Dialog:
            _dialogMessage = eventArgs.NotificationMessage;
            _dialogTitle = (!string.IsNullOrEmpty(eventArgs.ProgressBarTitle))
              ? eventArgs.ProgressBarTitle
              : eventArgs.ProcessType.ToString();
            _displayDialog = true;

            break;
          case UPAIProcessUINotificationTypeEnum.ProgressBar:
            _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(eventArgs.ProcessType);
            _displayProgressBar = true;

            break;
          default:
            break;
        }
      }
    }

    public void ProgressTrackingStarting(object sender, UPAIProgressTrackingStartingEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      _displayProgressBar = true;
    }

    public void ProgressTrackingStarted(object sender, UPAIProgressTrackingStartedEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      if (e.ProgressTrackingType == UPAIProgressTrackingTypeEnum.Timed) {
        _timedProgressBarStatus = _lastProgressBarStatus;
        _timedProgressProcessType = e.ProcessType;
        _timedProgressBarDescription = "Process Running...";
        _timedProgressBarTitle = e.ProgressBarTitle;
        _timedProgressLastProcessingTime = e.LastTimeElapsed;
      } else {
        _displayProgressBar = true;
      }
    }

    public void ProgressTrackingCompleting(object sender, UPAIProgressTrackingCompletingEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      _displayProgressBar = true;
    }

    public void ProgressTrackingCompleted(object sender, UPAIProgressTrackingCompletedEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = null;
      if (_isTimedProcessRunning && e.ProcessType == _timedProgressProcessType) {
        _timedProgressBarStatus = null;
        _timedProgressProcessType = UPAIProcessTypeEnum.Unknown;
        _timedProgressBarDescription = null;
        _timedProgressBarTitle = null;
        _timedProgressLastProcessingTime = new TimeSpan();
        _clearTimedProgressBar = true;
      } else {
        _displayProgressBar = true;
      }
    }

    public void ProgressTrackingStepStarting(object sender, UPAIProgressTrackingStepStartingEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      _displayProgressBar = true;
    }

    public void ProgressTrackingStepStarted(object sender, UPAIProgressTrackingStepStartedEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      if (!_isTimedProcessRunning && e.ProcessType == _timedProgressProcessType) {
        _isTimedProcessRunning = true;
        _timedProgressBarDescription = e.ProgressBarDescription;
        _timedProgressStopwatch = new Stopwatch();
        _timedProgressStopwatch.Start();        
      } else {
        _displayProgressBar = true;
      }
    }

    public void ProgressTrackingStepCompleting(object sender, UPAIProgressTrackingStepCompletingEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      _displayProgressBar = true;
    }

    public void ProgressTrackingStepCompleted(object sender, UPAIProgressTrackingStepCompletedEventArgs e) {
      if (e == null || !_IsMonitoredProcessType(e.ProcessType))
        return;

      _lastProgressBarStatus = UPAISystem.GetProgressBarStatus(e.ProcessType);
      if (_isTimedProcessRunning && e.ProcessType == _timedProgressProcessType) {
        _isTimedProcessRunning = false;
        _timedProgressStopwatch.Stop();
        _timedProgressStopwatch = new Stopwatch();       
      } else {
        _displayProgressBar = true;
      }
    }

    public void RefreshAssetDatabase() {
      if (_assetDatabaseIsRefreshing)
        return;

      _refreshAssetDatabase = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actionsForEngineRunning">List of Actions to execute while playsate mode changes to compiling or running</param>
    /// <param name="executeOnEngineNotProcessing">List of Actions to execute while playsate mode changes to not compiling and running</param>
    public UPAIUINotificationsProcessor(
      List<UPAIProcessTypeEnum> processTypesToMonitor,
      List<Action> actionsForEngineRunning = null, 
      List<Action> actionsForEngineNotRunning = null) {

      if (processTypesToMonitor == null)
        throw new ArgumentNullException("ProcessTypesToMonitor");

      _processTypesToMonitor = processTypesToMonitor;
      _actionsForEngineRunning = actionsForEngineRunning;
      _actionsForEngineNotRunning = actionsForEngineNotRunning;
      
      EditorApplication.playmodeStateChanged += EditorApp_PlayModeStateChanged;
      EditorApplication.update += EditorApp_Update;

      _isInitialized = true;
    }

    private void EditorApp_Update() {
      if (_displayDialog) {
        EditorUtility.DisplayDialog(_dialogTitle, _dialogMessage, _dialogButtonCaption);
        _displayDialog = false;
      }

      if (_displayProgressBar) {
        if (_lastProgressBarStatus != null) {
          EditorUtility.DisplayProgressBar(_lastProgressBarStatus.ProgressBarTitle, _lastProgressBarStatus.Description, _lastProgressBarStatus.Progress);
        } else {
          EditorUtility.ClearProgressBar();
        }

        _displayProgressBar = false;
        _lastProgressBarStatus = null;
      } else if (_isTimedProcessRunning) {
        var timeElapsedTicks = _timedProgressStopwatch.Elapsed.Ticks;
        var lastElapsedTotalTicks = _timedProgressLastProcessingTime.Ticks;
        if (lastElapsedTotalTicks == 0L)
          lastElapsedTotalTicks = TimeSpan.TicksPerMinute * 3;
        var timedProgress = ((float)timeElapsedTicks / (float)lastElapsedTotalTicks);        

        EditorUtility.DisplayProgressBar(
          (!string.IsNullOrEmpty(_timedProgressBarTitle))
            ? (_timedProgressBarTitle.Contains("{{{DisplayTimeElapsed}}}")) 
              ? _timedProgressBarTitle.Replace("{{{DisplayTimeElapsed}}}", _timedProgressStopwatch.Elapsed.ToString()) 
              : _timedProgressBarTitle
            : _timedProgressProcessType.ToString(),
          _timedProgressBarDescription, timedProgress);
      } else if (_clearTimedProgressBar) {
        EditorUtility.ClearProgressBar();
        _clearTimedProgressBar = false;
      }

      if (_pingAsset) {        
        var currentAssetPath = AssetDatabase.GUIDToAssetPath(_assetGuid);        
        AssetDatabase.GetDependencies(currentAssetPath, true);
        if (!string.IsNullOrEmpty(currentAssetPath)) {
          var currentObjectToPing = AssetDatabase.LoadAssetAtPath(currentAssetPath, _assetType);
          if (currentObjectToPing != null) {
            Selection.activeObject = currentObjectToPing;
            EditorGUIUtility.PingObject(currentObjectToPing);            
          }
        }
        _pingAsset = false;
      }

      if (_refreshAssetDatabase) {
        _assetDatabaseIsRefreshing = true;
        AssetDatabase.Refresh();
        _assetDatabaseIsRefreshing = false;
        _refreshAssetDatabase = false;
      }

      _lastUPAIProcessEventArgs = null;
    }

    private void EditorApp_PlayModeStateChanged() {
      if (EditorApplication.isCompiling
        || EditorApplication.isPlaying) {
        if (_actionsForEngineRunning == null)
          return;

        _PerformActions(_actionsForEngineRunning);
      }

      if (!EditorApplication.isCompiling
        && !EditorApplication.isPlaying) {
        if (_actionsForEngineNotRunning == null)
          return;

        _PerformActions(_actionsForEngineNotRunning);
      }      
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processType"></param>
    /// <returns></returns>
    private bool _IsMonitoredProcessType(UPAIProcessTypeEnum processType) {
      if (processType == UPAIProcessTypeEnum.Unknown)
        return false;

      if (_processTypesToMonitor == null)
        return true;

      return _processTypesToMonitor.Contains(processType);
    }

    private void _PerformActions(List<Action> actionsToPerform) {
      foreach (var performAction in actionsToPerform) {
        try {
          if (performAction == null)
            continue;

          performAction();
        } catch (Exception ex) {
          UPAISystem.Debug.LogException("UPAIUINotificationsProcessor", "_PerformActions", ex);
        }
      }
    }

    /// <summary>
    /// Temporarilly ignore unsed variables for future development.
    /// </summary>
    private void IgnoreUnusedVariables() {
      if (_lastUPAIProcessEventArgs == null) {}
      if (_lastProgressBarStatus == null) {}
      if (_timedProgressBarStatus == null) {}
    }
  }
}