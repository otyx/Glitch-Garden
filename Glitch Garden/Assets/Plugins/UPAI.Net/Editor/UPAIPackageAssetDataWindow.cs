using Lucene.Net.Documents;
using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Helpers;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using RKGamesDev.Systems.UPAI.Searchers;
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
  public class UPAIPackageAssetDataWindow : UPAIBaseEditorWindow {
    private const string CLASS_NAME = "UPAIPackageAssetDataWindow";
    private const float MAX_IMAGE_DIMENSION = 128f;

    private string[] _assetTypes = null;
    private GUIContent[] _contentsToDisplay = null;
    private List<Document> _currentDocuments = null;
    private Document _currentDocument = null;
    private Document _packageDocument = null;
    private string _selectedPackageId = null;
    private string[] _fileExtensions;
    private bool _forcePageReload = false;
    private bool _forceSearch = false;
    private bool _forceAssetView = false;
    private bool _isDisabled = false;
    private bool _isFiltering = false;
    private bool _isInitialized = false;
    private bool _isSearching = false;
    private bool _isSorting = false;
    private UPAIIndexablePackageAssetPagingHelper _pagingHelper = null;
    private Vector2 _scrollPosition_Description = Vector2.zero;
    private Vector2 _scrollPosition_PubNotes = Vector2.zero;
    private Vector2 _scrollPosition_TagData = Vector2.zero;
    private int _searchRotation = 0;
    private string _searchString = "";
    private int _selectedAssetIndex = -1;
    private UPAIAssetTypeEnum _selectedAssetType;
    private int _selectedFileIndex = -1;
    private int _selectedGridIndex = -1;
    private int _selectedSortIndex = -1;
    private int _selectedTabIndex = 0;
    private int _selectionAssetTypeIndex = -1;
    private int _selectionFileExtensionIndex = -1;
    private int _selectionGridIndex = -1;
    private string[] _sortTypes = null;
    private UPAIIndexablePackageAssetPagingHelper _tempPagingHelper = null;
    private UPAIUINotificationsProcessor _uiNotificationProcessor = null;

    public static GUIContent[] ConvertLuceneDocuments(List<Document> documents, int itemsPerPage, UPAIAssetTypeEnum selectedAssetType) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "ConvertLuceneDocuments");

      if (documents == null || documents.Count == 0) {
        var emptyContent = new List<GUIContent>();
        for (int i = 0; i < itemsPerPage; i++) {
          emptyContent.Add(new GUIContent() {
            text = ""
          });
        }
        return emptyContent.ToArray();
      }

      List<GUIContent> guiContents = new List<GUIContent>();
      foreach (var doc in documents) {
        if (doc == null)
          continue;

        GUIContent guiContent = new GUIContent();
        guiContent.text = _GetField(doc, UPAIDocumentFieldNames.ASSET_FILENAME_FIELDNAME);
        guiContent.image = UPAIIconFileRetriever.GetAssetPreviewIcon(doc);
        guiContents.Add(guiContent);
      }

      if (documents.Count < itemsPerPage) {
        for (int i = 0; i < itemsPerPage - documents.Count; i++) {
          guiContents.Add(new GUIContent() {
            text = "",
          });
        }
      }

      return guiContents.ToArray();
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsImporting() {
      return UPAISystem.IsImporting();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void CreatePackageAssetDataWindow(Document packageDocument) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "CreatePackageAssetDataWindow");

      if (packageDocument == null)
        return;

      var packageFileName = _GetField(packageDocument, UPAIDocumentFieldNames.PACKAGE_FILENAME_FIELDNAME);
      if (string.IsNullOrEmpty(packageFileName))
        packageFileName = "Package";

      var newPackageAssetDataWindow = (UPAIPackageAssetDataWindow)EditorWindow.CreateInstance<UPAIPackageAssetDataWindow>();
      newPackageAssetDataWindow.titleContent = new GUIContent() {
        text = string.Format("{0} Data...", packageFileName)
      };
      if (!newPackageAssetDataWindow.Intitialize(packageDocument)) {
        newPackageAssetDataWindow.Close();
        newPackageAssetDataWindow = null;
        return;
      }

      newPackageAssetDataWindow.minSize = new Vector2(805f, 650f);
      newPackageAssetDataWindow.maxSize = new Vector2(805f, 650f);
      newPackageAssetDataWindow.CenterOnMainWin();
      newPackageAssetDataWindow.ShowUtility();
    }

    void OnEnable() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnEnable");

      if (_uiNotificationProcessor != null && !_uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Enable();

      UPAISystem.ProgressTrackingCompleted += UPAISystem_ProgressTrackingCompleted;
      UPAISystem.ProgressTrackingCompleting += UPAISystem_ProgressTrackingCompleting;
      UPAISystem.ProgressTrackingStarted += UPAISystem_ProgressTrackingStarted;
      UPAISystem.ProgressTrackingStarting += UPAISystem_ProgressTrackingStarting;
      UPAISystem.ProgressTrackingStepCompleted += UPAISystem_ProgressTrackingStepCompleted;
      UPAISystem.ProgressTrackingStepCompleting += UPAISystem_ProgressTrackingStepCompleting;
      UPAISystem.ProgressTrackingStepStarted += UPAISystem_ProgressTrackingStepStarted;
      UPAISystem.ProgressTrackingStepStarting += UPAISystem_ProgressTrackingStepStarting;

      if (_pagingHelper != null) {
        _pagingHelper.IndexSearchAborted += UPAIPagingHelper_IndexSearchAborted;
        _pagingHelper.IndexSearchAborting += UPAIPagingHelper_IndexSearchAborting;
        _pagingHelper.IndexSearchCompleted += UPAIPagingHelper_IndexSearchCompleted;
        _pagingHelper.IndexSearchCompleting += UPAIPagingHelper_IndexSearchCompleting;
        _pagingHelper.IndexSearchStarted += UPAIPagingHelper_IndexSearchStarted;
        _pagingHelper.IndexSearchStarting += UPAIPagingHelper_IndexSearchStarting;
      }

      _isDisabled = false;
    }

    void OnInspectorUpdate() {
      if (_isDisabled)
        return;

      Repaint();
    }

    void OnDestroy() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnDestroy");

      _assetTypes = null;
      _contentsToDisplay = null;
      _currentDocuments = null;
      _currentDocument = null;
      _packageDocument = null;
      _selectedPackageId = null;
      _fileExtensions = null;
      _searchString = null;
      _pagingHelper = null;
      _tempPagingHelper = null;
      _uiNotificationProcessor = null;
    }

    void OnDisable() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnDisable");

      if (_uiNotificationProcessor != null && _uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Disable();

      UPAISystem.ProgressTrackingCompleted -= UPAISystem_ProgressTrackingCompleted;
      UPAISystem.ProgressTrackingCompleting -= UPAISystem_ProgressTrackingCompleting;
      UPAISystem.ProgressTrackingStarted -= UPAISystem_ProgressTrackingStarted;
      UPAISystem.ProgressTrackingStarting -= UPAISystem_ProgressTrackingStarting;
      UPAISystem.ProgressTrackingStepCompleted -= UPAISystem_ProgressTrackingStepCompleted;
      UPAISystem.ProgressTrackingStepCompleting -= UPAISystem_ProgressTrackingStepCompleting;
      UPAISystem.ProgressTrackingStepStarted -= UPAISystem_ProgressTrackingStepStarted;
      UPAISystem.ProgressTrackingStepStarting -= UPAISystem_ProgressTrackingStepStarting;

      if (_pagingHelper != null) {
        _pagingHelper.IndexSearchAborted -= UPAIPagingHelper_IndexSearchAborted;
        _pagingHelper.IndexSearchAborting -= UPAIPagingHelper_IndexSearchAborting;
        _pagingHelper.IndexSearchCompleted -= UPAIPagingHelper_IndexSearchCompleted;
        _pagingHelper.IndexSearchCompleting -= UPAIPagingHelper_IndexSearchCompleting;
        _pagingHelper.IndexSearchStarted -= UPAIPagingHelper_IndexSearchStarted;
        _pagingHelper.IndexSearchStarting -= UPAIPagingHelper_IndexSearchStarting;
      }

      _isDisabled = true;
    }

    void OnGUI() {
      _DrawUI();
    }

    void _ChangeAssetType(int selectedAssetIndex) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ChangeAssetType");

      _selectedAssetType = (UPAIAssetTypeEnum)Enum.Parse(
        typeof(UPAIAssetTypeEnum), _assetTypes[selectedAssetIndex]);

      if (_selectedAssetType == UPAIAssetTypeEnum.Folder
        || _selectedAssetType == UPAIAssetTypeEnum.Unknown
        || selectedAssetIndex == _selectionAssetTypeIndex)
        return;

      _searchString = "";

      _selectionAssetTypeIndex = selectedAssetIndex;
      var assetTypeDefinition = UPAIAssetDataManager
        .AssetTypeDefintions.FirstOrDefault(x => x.Key == _selectedAssetType);
      if (assetTypeDefinition.Value != null)
        _fileExtensions = assetTypeDefinition.Value.KnownExtensions.ToArray();
            
      _pagingHelper = new UPAIIndexablePackageAssetPagingHelper(
        UPAISystem.UnityVersion,
        _packageDocument.GetField(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME).StringValue,
        _selectedAssetType,
        _searchString);

      _pagingHelper.IndexSearchAborted += UPAIPagingHelper_IndexSearchAborted;
      _pagingHelper.IndexSearchAborting += UPAIPagingHelper_IndexSearchAborting;
      _pagingHelper.IndexSearchCompleted += UPAIPagingHelper_IndexSearchCompleted;
      _pagingHelper.IndexSearchCompleting += UPAIPagingHelper_IndexSearchCompleting;
      _pagingHelper.IndexSearchStarted += UPAIPagingHelper_IndexSearchStarted;
      _pagingHelper.IndexSearchStarting += UPAIPagingHelper_IndexSearchStarting;

      _pagingHelper.PopulatePages();
    }

    private void UPAIPagingHelper_FilterResultsStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_FilterResultsStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_FilterResultsStarted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _forceSearch = false;
      _isFiltering = true;
    }

    private void UPAIPagingHelper_FilterResultsCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_FilterResultsCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_FilterResultsCompleted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isFiltering = false;
      _forcePageReload = true;
    }

    private void UPAIPagingHelper_FilterResultsAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_FilterResultsAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_FilterResultsAborted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isFiltering = false;
      _forcePageReload = true;
    }

    private void UPAIPagingHelper_IndexSearchStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_IndexSearchStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_IndexSearchStarted");

      _pagingHelper.FilterResultsAborted -= UPAIPagingHelper_FilterResultsAborted;
      _pagingHelper.FilterResultsAborting -= UPAIPagingHelper_FilterResultsAborting;
      _pagingHelper.FilterResultsCompleted -= UPAIPagingHelper_FilterResultsCompleted;
      _pagingHelper.FilterResultsCompleting -= UPAIPagingHelper_FilterResultsCompleting;
      _pagingHelper.FilterResultsStarted -= UPAIPagingHelper_FilterResultsStarted;
      _pagingHelper.FilterResultsStarting -= UPAIPagingHelper_FilterResultsStarting;

      _pagingHelper.SortResultsAborted -= UPAIPagingHelper_SortResultsAborted;
      _pagingHelper.SortResultsAborting -= UPAIPagingHelper_SortResultsAborting;
      _pagingHelper.SortResultsCompleted -= UPAIPagingHelper_SortResultsCompleted;
      _pagingHelper.SortResultsCompleting -= UPAIPagingHelper_SortResultsCompleting;
      _pagingHelper.SortResultsStarted -= UPAIPagingHelper_SortResultsStarted;
      _pagingHelper.SortResultsStarting -= UPAIPagingHelper_SortResultsStarting;

      _uiNotificationProcessor.ProcessUINotification(e);
      _forceSearch = false;
      _isSearching = true;
    }

    private void UPAIPagingHelper_IndexSearchCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_IndexSearchCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_IndexSearchCompleted");

      if (_pagingHelper.TotalItems == 0 && !string.IsNullOrEmpty(_searchString)) {
        e.NotificationMessage = string.Format("No results found for \"{0}\"...", _searchString);
      } else {
        _pagingHelper.FilterResultsAborted += UPAIPagingHelper_FilterResultsAborted;
        _pagingHelper.FilterResultsAborting += UPAIPagingHelper_FilterResultsAborting;
        _pagingHelper.FilterResultsCompleted += UPAIPagingHelper_FilterResultsCompleted;
        _pagingHelper.FilterResultsCompleting += UPAIPagingHelper_FilterResultsCompleting;
        _pagingHelper.FilterResultsStarted += UPAIPagingHelper_FilterResultsStarted;
        _pagingHelper.FilterResultsStarting += UPAIPagingHelper_FilterResultsStarting;

        _pagingHelper.SortResultsAborted += UPAIPagingHelper_SortResultsAborted;
        _pagingHelper.SortResultsAborting += UPAIPagingHelper_SortResultsAborting;
        _pagingHelper.SortResultsCompleted += UPAIPagingHelper_SortResultsCompleted;
        _pagingHelper.SortResultsCompleting += UPAIPagingHelper_SortResultsCompleting;
        _pagingHelper.SortResultsStarted += UPAIPagingHelper_SortResultsStarted;
        _pagingHelper.SortResultsStarting += UPAIPagingHelper_SortResultsStarting;

        e.ProcessNotificationType = UPAIProcessUINotificationTypeEnum.Console;

        if (_pagingHelper.AvailableFileExtentions != null && _pagingHelper.AvailableFileExtentions.Count > 0) {
          try {
            _fileExtensions = _pagingHelper.AvailableFileExtentions
              .OrderByDescending(x => x.Value)
              .Select(x => {
                return string.Format("{0} [{1}]", x.Key, x.Value);
              }).ToArray();

            if (_selectedFileIndex != -1) {
              var selectedFileIndex = _selectedFileIndex;
              _selectedFileIndex = -1;
              _FilterOnFileExtension(selectedFileIndex);
            }
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_IndexSearchCompleted", ex);
          }
        }

        if (_pagingHelper.FieldSorts != null && _pagingHelper.FieldSorts.Count > 0) {
          _sortTypes = _pagingHelper.FieldSorts.ToArray();

          _selectedSortIndex = Array.IndexOf<string>(_sortTypes, _pagingHelper.CurrentPageSort);
        }
      }

      _uiNotificationProcessor.ProcessUINotification(e);
      _isSearching = false;
      _forcePageReload = true;
    }

    private void UPAIPagingHelper_IndexSearchAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_IndexSearchAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_IndexSearchAborted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isSearching = false;
      _forcePageReload = true;
    }

    private void UPAISystem_ProgressTrackingStepStarting(object sender, UPAIProgressTrackingStepStartingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepStarting(sender, e);
    }

    private void UPAISystem_ProgressTrackingStepStarted(object sender, UPAIProgressTrackingStepStartedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepStarted(sender, e);
    }

    private void UPAISystem_ProgressTrackingStepCompleting(object sender, UPAIProgressTrackingStepCompletingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepCompleting(sender, e);
    }

    private void UPAISystem_ProgressTrackingStepCompleted(object sender, UPAIProgressTrackingStepCompletedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStepCompleted(sender, e);
    }

    private void UPAISystem_ProgressTrackingStarting(object sender, UPAIProgressTrackingStartingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStarting(sender, e);
    }

    private void UPAISystem_ProgressTrackingStarted(object sender, UPAIProgressTrackingStartedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingStarted(sender, e);
    }

    private void UPAISystem_ProgressTrackingCompleting(object sender, UPAIProgressTrackingCompletingEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingCompleting(sender, e);
    }

    private void UPAISystem_ProgressTrackingCompleted(object sender, UPAIProgressTrackingCompletedEventArgs e) {
      _uiNotificationProcessor.ProgressTrackingCompleted(sender, e);
    }

    private void UPAIPagingHelper_SortResultsStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_SortResultsStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_SortResultsStarted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _forceSearch = false;
      _isSorting = true;
    }

    private void UPAIPagingHelper_SortResultsCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_SortResultsCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_SortResultsCompleted");

      _selectedSortIndex = Array.IndexOf<string>(_sortTypes, _pagingHelper.CurrentPageSort);
      _uiNotificationProcessor.ProcessUINotification(e);
      _isSorting = false;
      _forcePageReload = true;
    }

    private void UPAIPagingHelper_SortResultsAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_SortResultsAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_SortResultsAborted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isSorting = false;
      _forcePageReload = true;
    }

    void _ChangePages(Action pageMethod) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ChangePages");
            
      pageMethod();
      _selectionGridIndex = -1;      
    }

    void _ChangeSortType(int selectedSortIndex) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ChangeSortType");

      if (_selectedSortIndex == selectedSortIndex)
        return;

      _selectedSortIndex = selectedSortIndex;

      var sortType = _sortTypes[_selectedSortIndex];

      _tempPagingHelper = _pagingHelper;

      _pagingHelper.CurrentPageSort = sortType;
      _pagingHelper.ApplySort();
    }

    void _FilterOnFileExtension(int selectedFileExtensionIndex) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_FilterOnFileExtension");

      if (_selectionFileExtensionIndex == selectedFileExtensionIndex)
        return;

      _tempPagingHelper = _pagingHelper;

      _selectionFileExtensionIndex = selectedFileExtensionIndex;

      _pagingHelper.FileExtension = _fileExtensions[selectedFileExtensionIndex];
      _pagingHelper.ApplyFilters();
    }

    /// <summary>
    /// Temporarilly Ignore Unused Fields
    /// </summary>
    void _IgnoreUnusedFields() {
      if (_tempPagingHelper == null) {}
    }

    void _Search() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_Search");

      if (string.IsNullOrEmpty(_searchString))
        return;
      
      _selectedAssetType = (UPAIAssetTypeEnum)Enum.Parse(
        typeof(UPAIAssetTypeEnum), _assetTypes[_selectionAssetTypeIndex]);

      _tempPagingHelper = _pagingHelper;

      _isSearching = true;

      _pagingHelper = new UPAIIndexablePackageAssetPagingHelper(
        UPAISystem.UnityVersion,
        _packageDocument.GetField(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME).StringValue,
        _selectedAssetType,
        _searchString);

      _pagingHelper.IndexSearchAborted += UPAIPagingHelper_IndexSearchAborted;
      _pagingHelper.IndexSearchAborting += UPAIPagingHelper_IndexSearchAborting;
      _pagingHelper.IndexSearchCompleted += UPAIPagingHelper_IndexSearchCompleted;
      _pagingHelper.IndexSearchCompleting += UPAIPagingHelper_IndexSearchCompleting;
      _pagingHelper.IndexSearchStarted += UPAIPagingHelper_IndexSearchStarted;
      _pagingHelper.IndexSearchStarting += UPAIPagingHelper_IndexSearchStarting;

      _pagingHelper.PopulatePages();
    }

    void _Reset() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_SelectItem");

      _isSearching = true;

      _forceSearch = false;
      _forceAssetView = false;
      _isDisabled = false;
      _searchString = "";
      _selectedFileIndex = -1;
      _selectedGridIndex = -1;
      _selectionFileExtensionIndex = -1;
      _selectionGridIndex = -1;

      _selectedAssetType = (UPAIAssetTypeEnum)Enum.Parse(
        typeof(UPAIAssetTypeEnum), _assetTypes[_selectionAssetTypeIndex], true);

      _pagingHelper = new UPAIIndexablePackageAssetPagingHelper(
        UPAISystem.UnityVersion,
        _packageDocument.GetField(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME).StringValue,
        _selectedAssetType,
        _searchString);

      _pagingHelper.IndexSearchAborted += UPAIPagingHelper_IndexSearchAborted;
      _pagingHelper.IndexSearchAborting += UPAIPagingHelper_IndexSearchAborting;
      _pagingHelper.IndexSearchCompleted += UPAIPagingHelper_IndexSearchCompleted;
      _pagingHelper.IndexSearchCompleting += UPAIPagingHelper_IndexSearchCompleting;
      _pagingHelper.IndexSearchStarted += UPAIPagingHelper_IndexSearchStarted;
      _pagingHelper.IndexSearchStarting += UPAIPagingHelper_IndexSearchStarting;

      _pagingHelper.PopulatePages();
    }

    Document _SelectItem(int index) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_SelectItem");

      if (index == -1)
        return new Document();

      if (index > (ItemsPerPage - 1))
        return new Document();

      _selectionGridIndex = index;
      _currentDocuments = _pagingHelper.Page(_pagingHelper.CurrentPageId);

      try {
        return _currentDocuments.ToArray()[index];
      } catch {
        _selectionGridIndex = -1;
        _selectedGridIndex = -1;
        return new Document();
      }
    }

    void _DrawUI() {
      if (_isDisabled)
        return;

      Event e = Event.current;

      var defaultLabelWidth = 110;
      var tabIndex = _DrawHorizontalTabs(new string[] { "Description", string.Format("Assets ({0})",
        _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_INDEXEDASSETCOUNT_FIELDNAME).StringValue),
        "Notes", "Tags (0)", "File Data", "Index Data" }, _selectedTabIndex);
      if (tabIndex != _selectedTabIndex) {
        _selectedTabIndex = tabIndex;
        return;
      }
      _DrawSpaces(1);
      switch (_selectedTabIndex) {
        case 0:
          _BeginHorizontal();
          _BeginVertical();
          _DrawHorizontalLabelField("Package Icon", 150);
          try {
            var packageIconTexture = UPAIIconFileRetriever.GetPackageIcon(_packageDocument);
            if (packageIconTexture != null) {
              EditorGUI.DrawPreviewTexture(new Rect(20, 65, 128, 128), packageIconTexture);
            }
          } catch (Exception ex) {
            Debug.LogError(ex.Message);
          }
          _EndVertical();
          _BeginVertical();
          _DrawSpaces(2);
          _DrawHorizontalLabelValueField("Title", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_TITLE_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Publisher", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_PUBLISHER_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Published On", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_PUBLISHDATE_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Category", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_CATEGORY_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Included Assets", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_ASSETCOUNT_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Indexed Assets", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_INDEXEDASSETCOUNT_FIELDNAME)), defaultLabelWidth);
          _EndVertical();
          _EndHorizontal();
          _BeginHorizontal();
          _BeginVertical();
          _DrawSpaces(5);
          _scrollPosition_Description = _DrawVerticalLabelTextAreaField("Description", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_DESCRIPTION_FIELDNAME)), 
            _scrollPosition_Description, defaultLabelWidth, 795, 350);
          _DrawSpaces(2);
          var importPackage = GUILayout.Button("Import Package...", GUILayout.Width(150));
          if (importPackage) {
            _EndVertical();
            _EndHorizontal();
            _ImportPackage(_GetDocumentString(
              _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME)));
            return;
          }
          _DrawSpaces(5);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();

          return;
        case 1:
          _BeginHorizontal();
          _BeginVertical();
          _selectedAssetIndex = EditorGUILayout.Popup("Asset Type: ", _selectionAssetTypeIndex, _assetTypes, GUILayout.MaxWidth(350));
          if (_selectionAssetTypeIndex != _selectedAssetIndex) {
            UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-AssetType", _selectedAssetIndex);

            _EndVertical();
            _EndHorizontal();
            _ChangeAssetType(_selectedAssetIndex);
            return;
          }

          if (_fileExtensions != null && _fileExtensions.Length <= 2) {
            _fileExtensions = null;
          }

          if (_fileExtensions != null && _fileExtensions.Length > 1) {
            _selectedFileIndex = EditorGUILayout.Popup("File Type: ", _selectionFileExtensionIndex, _fileExtensions, GUILayout.MaxWidth(350));
            if (_selectionFileExtensionIndex != _selectedFileIndex && !_isFiltering) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-FileExtensionType", _selectedFileIndex);

              _EndVertical();
              _EndHorizontal();
              _FilterOnFileExtension(_selectedFileIndex);
              return;
            }
          } else {
            _selectionFileExtensionIndex = -1;
          }

          _BeginHorizontal();
          var searchWords = EditorGUILayout.TextField("Search: ", _searchString, GUILayout.Width(450));
          if (_searchString != searchWords) {
            _searchString = searchWords;
          }
          _EndHorizontal();

          _BeginHorizontal();
          _forceSearch = GUILayout.Button("Search...", GUILayout.Width(75));
          if (e.type == EventType.KeyDown &&
            (e.keyCode == KeyCode.KeypadEnter ||
              e.keyCode == KeyCode.Return)) {
            if (!_forceSearch) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "KeyDown-EnterOrReturn", _searchString);

              _forceSearch = true;
            }
          }
          if (_forceSearch && !_isSearching) {
            UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-Search", _searchString);

            _EndVertical();
            _EndHorizontal();
            _Search();
            return;
          }
          _forceSearch = GUILayout.Button("Reset", GUILayout.Width(75));
          if (_forceSearch && !_isSearching) {
            UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-Reset", "");

            _EndVertical();
            _EndHorizontal();
            _Reset();
            return;
          }
          if (_sortTypes != null && _sortTypes.Length > 0) {
            var selectedSort = EditorGUILayout.Popup(_selectedSortIndex, _sortTypes, GUILayout.MaxWidth(150));
            if (selectedSort != _selectedSortIndex) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-Sort", selectedSort);

              _EndVertical();
              _EndHorizontal();
              _ChangeSortType(selectedSort);
              return;
            }
          } else {
            _selectedSortIndex = -1;
          }
          _EndHorizontal();

          if (_isSearching) {
            _contentsToDisplay = _GetAlternateContentPlaceHolder("Searching");
          } else if (_isFiltering) {
            _contentsToDisplay = _GetAlternateContentPlaceHolder("Filtering");
          } else if (_isSorting) {
            _contentsToDisplay = _GetAlternateContentPlaceHolder("Sorting");
          }

          _DrawSpaces(2);
          if (_pagingHelper != null || _pagingHelper.TotalItems > 0 || _contentsToDisplay.Length > 0) {
            if (_forcePageReload || (_pagingHelper.TotalItems > 0 && _contentsToDisplay.Length == 0)) {
              _forcePageReload = false;
              _EndVertical();
              _EndHorizontal();
              _ChangePages(FirstPage);
              return;
            }

            try {
              if (_isSearching) {
                _DrawHorizontalLabelField("Searching...");
              } else if (_isFiltering) {
                _DrawHorizontalLabelField("Filtering...");
              } else if (_isSorting) {
                _DrawHorizontalLabelField("Sorting...");
              } else {
                _DrawHorizontalLabelField(string.Format("Selection - {0} {1}'s...", TotalItems.ToString("N0"), _assetTypes[_selectionAssetTypeIndex]));
              }

              _selectedGridIndex = GUILayout.SelectionGrid(_selectionGridIndex, _contentsToDisplay, 5, _GetGUIStyle(), GUILayout.MaxWidth((1.25f * 5f) * 128f));
              if (_selectedGridIndex != _selectionGridIndex) {
                _currentDocument = _SelectItem(_selectedGridIndex);
              }
            } catch {
              _currentDocument = null;
              _EndVertical();
              _EndHorizontal();
              return;
            }

            _BeginHorizontal();
            _forceSearch = GUILayout.Button("<<", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-FirstPage", "");

              _EndHorizontal();
              _EndVertical();
              _ChangePages(FirstPage);
              return;
            }
            _forceSearch = GUILayout.Button("<", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-PreviousPage", "");

              _EndHorizontal();
              _EndVertical();
              _ChangePages(PreviousPage);
              return;
            }
            EditorGUILayout.LabelField(string.Format("Page {0} of {1}...", _pagingHelper.CurrentPageId, _pagingHelper.TotalPages), GUILayout.Width(200));
            _forceSearch = GUILayout.Button(">", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-NextPage", "");

              _EndHorizontal();
              _EndVertical();
              _ChangePages(NextPage);
              return;
            }
            _forceSearch = GUILayout.Button(">>", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-LastPage", "");

              _EndHorizontal();
              _EndVertical();
              _ChangePages(LastPage);
              return;
            }
            _forceAssetView = GUILayout.Button("View Asset...", GUILayout.Width(200));
            if (_selectedGridIndex != -1 && _forceAssetView) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-ViewAsset", _selectedGridIndex);

              _EndHorizontal();
              _EndVertical();
              _currentDocument = _SelectItem(_selectedGridIndex);
              UPAIAssetDataWindow.CreateAssetDataWindow(_currentDocument, false);
              return;
            }
            _EndHorizontal();
          } else {
            _DrawHorizontalLabelField("No results found...");
            _EndHorizontal();
            _EndVertical();
          }
          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();

          return;
        case 2: // Notes          
          _BeginHorizontal();
          _BeginVertical();
          _scrollPosition_PubNotes = _DrawVerticalLabelTextAreaField("Publisher Notes", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_PUBLISHNOTES_FIELDNAME)), 
            _scrollPosition_PubNotes,
            defaultLabelWidth, 795, 550);
          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();
          return;
          
        case 3: // Tags
          _BeginHorizontal();
          _BeginVertical();
          _scrollPosition_TagData =_DrawVerticalLabelTextAreaField("Tags", "Coming Soon...",
            _scrollPosition_TagData,
            defaultLabelWidth, 795, 550);
          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();
          return;

        case 4:
          _BeginHorizontal();
          _BeginVertical();          
          _DrawHorizontalLabelValueField("Path", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_LOCATION_FIELDNAME)), defaultLabelWidth, 300);
          _DrawHorizontalLabelValueField("Size", _GetDocumentSize(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_FILESIZE_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("Downloaded On", _GetDocumentDate(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_FILEDATE_FIELDNAME)), defaultLabelWidth);
          _DrawHorizontalLabelValueField("MD5 Hash", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_FILEMD5HASH_FIELDNAME)), defaultLabelWidth);
          _DrawSpaces(86);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();
          return;
                    
        case 5:
          _BeginHorizontal();
          _BeginVertical();
          _DrawHorizontalLabelValueField("Document ID", _GetDocumentString(
            _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME)), defaultLabelWidth);
          _DrawSpaces(95);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();
          return;

        default:
          _BeginHorizontal();
          _BeginVertical();
          _DrawSpaces(98);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();
          return;
      }
    }

    void Update() {
      if (EditorApplication.isCompiling)
        Close();

      if (_isDisabled)
        return;

      Repaint();
    }

    /// <summary>
    /// 
    /// </summary>
    public int ItemsPerPage {
      get {
        if (_pagingHelper == null)
          return 0;

        return _pagingHelper.ItemsPerPage;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int TotalItems {
      get {
        if (_pagingHelper == null)
          return 0;

        return _pagingHelper.TotalItems;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public int TotalPages {
      get {
        if (_pagingHelper == null)
          return 0;

        return _pagingHelper.TotalPages;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void FirstPage() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "FirstPage");

      if (_pagingHelper == null)
        _contentsToDisplay = null;

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.FirstPage(), ItemsPerPage, GetSelectedAssetType());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public UPAIAssetTypeEnum GetSelectedAssetType() {
      try {
        return ((UPAIAssetTypeEnum)Enum.Parse(
          typeof(UPAIAssetTypeEnum), _assetTypes[_selectionAssetTypeIndex], true));
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "GetSelectedAssetType", ex);
      }

      return UPAIAssetTypeEnum.Unknown;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packageDocument"></param>
    /// <returns></returns>
    public bool Intitialize(Document packageDocument) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "Intitialize");

      if (packageDocument == null)
        throw new ArgumentNullException("PackageDocument");

      if (_isInitialized)
        return true;

      try {
        _packageDocument = packageDocument;

        _uiNotificationProcessor = new UPAIUINotificationsProcessor(
          new List<UPAIProcessTypeEnum>() {
            UPAIProcessTypeEnum.IndexSearch,
          },
          null,
          null);

        _selectedPackageId = _packageDocument.GetFieldable(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME).StringValue;
        try {
          _assetTypes = UPAIIndexSearcher.GetPackageAssetTypes(
            Application.unityVersion, _selectedPackageId, true).Keys.OrderBy(x => x).ToArray();
          _ChangeAssetType(0);
        } catch {
          _assetTypes = null;
        }

        _isInitialized = true;
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "Initialize", ex);
      }

      return _isInitialized;
    }

    public bool IsPageable() {
      if (_pagingHelper == null)
        return false;

      return _pagingHelper.IsPageable();
    }

    public void LastPage() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "LastPage");

      if (_pagingHelper == null)
        _contentsToDisplay = null;

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.LastPage(), ItemsPerPage, GetSelectedAssetType());
    }

    public void NextPage() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "NextPage");

      if (_pagingHelper == null)
        _contentsToDisplay = null;

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.NextPage(), ItemsPerPage, GetSelectedAssetType());
    }

    public void Page(int id) {
      if (_pagingHelper == null)
        _contentsToDisplay = null;

      if (_pagingHelper.CurrentPageId == id)
        return;

      _currentDocuments = _pagingHelper.Page(id);
      _contentsToDisplay = ConvertLuceneDocuments(_currentDocuments, ItemsPerPage, GetSelectedAssetType());
    }

    public bool PagesExist() {
      if (_pagingHelper == null)
        return false;

      return _pagingHelper.PagesExist();
    }

    public void PreviousPage() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "PreviousPage");

      if (_pagingHelper == null)
        _contentsToDisplay = null;

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.PreviousPage(), ItemsPerPage, GetSelectedAssetType());
    }

    private GUIStyle _GetGUIStyle() {
      GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
      guiStyle.alignment = TextAnchor.MiddleCenter;
      var backgroundImagess = AssetDatabase.FindAssets("t:Texture2d UPAI_Default_Background_Image");
      if (backgroundImagess.Length == 1) {
        guiStyle.normal.background = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(backgroundImagess[0]));
      }
      guiStyle.fixedHeight = 150f;
      guiStyle.fixedWidth = 152f;
      guiStyle.fontSize = 9;
      guiStyle.imagePosition = ImagePosition.ImageAbove;
      guiStyle.wordWrap = true;
      return guiStyle;
    }

    private GUIContent[] _GetAlternateContentPlaceHolder(string message = "Searching") {
      if (_searchRotation > 480)
        _searchRotation = 0;

      var searchingContent = new List<GUIContent>();
      for (int i = 0; i < 15; i++) {
        if (i == 15 / 2) {
          searchingContent.Add(new GUIContent() {
            image = Resources.Load<Texture2D>(string.Format("Icons/Search_{0}", _searchRotation / 60)),
            text = string.Format("{0}...", message)
          });
        } else {
          searchingContent.Add(new GUIContent() {
            text = "-",
          });
        }
      }
      _searchRotation++;
      return searchingContent.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="document"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    private static string _GetField(Document document, string fieldName) {
      if (document == null
        || string.IsNullOrEmpty(fieldName)
        || document.GetField(fieldName) == null)
        return "";

      return document.GetField(fieldName).StringValue;
    }
  }
}