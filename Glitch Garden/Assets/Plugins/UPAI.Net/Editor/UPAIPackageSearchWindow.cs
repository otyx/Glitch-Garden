using Lucene.Net.Documents;
using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Events;
using RKGamesDev.Systems.UPAI.Helpers;
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
  public class UPAIPackageSearchWindow : UPAIBaseEditorWindow {
    private const string CLASS_NAME = "UPAIPackageSearchWindow";
    private const float MAX_IMAGE_DIMENSION = 128f;

    private GUIContent[] _contentsToDisplay = null;
    private List<Document> _currentDocuments = null;
    private Document _currentDocument = null;
    private bool _forcePackageView = false;
    private bool _forcePageReload = false;
    private bool _forceSearch = false;
    private bool _isDisabled = false;
    private bool _isFiltering = false;
    private bool _isInitialized = false;
    private bool _isSearching = false;
    private bool _isSorting = false;
    private string[] _nonFilteredCategories = null;
    private string[] _filteredPublishers = null;
    private string[] _filteredSources = null;
    private Dictionary<string, string> _organizedCategories = null;
    private Dictionary<string, string> _organizedPublishers = null;
    private UPAIIndexablePackagePagingHelper _pagingHelper = null;
    private Vector2 _scrollPosition_FavoritePackages = Vector2.zero;
    private Vector2 _scrollPosition_RecentPackages = Vector2.zero;
    private int _searchRotation = 0;
    private string _searchString = "";
    private int _selectedCategoryId = -1;    
    private int _selectedGridIndex = -1;
    private int _selectedPublisherId = -1;
    private int _selectedSourceId = -1;
    private int _selectedSortIndex = -1;
    private int _selectedTabIndex = 0;
    private int _selectionCategoryId = -1;
    private int _selectionGridIndex = -1;
    private int _selectionPublisherId = -1;
    private int _selectionSourceId = -1;
    private string[] _sortTypes = null;
    //private UPAIIndexablePackagePagingHelper _tempPagingHelper = null;
    private UPAIUINotificationsProcessor _uiNotificationProcessor = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documents"></param>
    /// <param name="itemsPerPage"></param>
    /// <returns></returns>
    public static GUIContent[] ConvertLuceneDocuments(List<Document> documents, int itemsPerPage) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "ConvertLuceneDocuments");

      if (documents == null || documents.Count == 0) {
        var emptyContent = new List<GUIContent>();
        for (int i = 0; i < itemsPerPage; i++) {
          emptyContent.Add(new GUIContent() {
            text = i.ToString(),
          });
        }
        return emptyContent.ToArray();
      }

      List<GUIContent> guiContents = new List<GUIContent>();
      foreach (var doc in documents) {
        if (doc == null)
          continue;

        GUIContent guiContent = new GUIContent();
        guiContent.text = _GetField(doc, UPAIDocumentFieldNames.PACKAGE_FILENAME_FIELDNAME);
        guiContent.image = UPAIIconFileRetriever.GetPackageIcon(doc);
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
    public static void CreatePackageSearchWindow() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "CreatePackageSearchWindow");

      var newPackageSearchWindow = (UPAIPackageSearchWindow)EditorWindow.GetWindow<UPAIPackageSearchWindow>(
        true, "Search Packages...", true);
      if (!newPackageSearchWindow.Initialize()) {
        newPackageSearchWindow.Close();
        newPackageSearchWindow = null;
        return;
      }

      try {
        UPAIUnityEditorRecoveryManager.PackageSearchWindowOpened();
      } catch {

      }

      newPackageSearchWindow.minSize = new Vector2(805f, 695f);
      newPackageSearchWindow.maxSize = new Vector2(805f, 695f);
      newPackageSearchWindow.CenterOnMainWin();
      newPackageSearchWindow.ShowUtility();
    }

    public bool Initialize() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "Intitialize");

      if (_isInitialized)
        return true;

      try {
        _uiNotificationProcessor = new UPAIUINotificationsProcessor(
          new List<UPAIProcessTypeEnum>() {
            UPAIProcessTypeEnum.IndexSearch,
          },
          null,
          null);

        _Search();

        _isInitialized = true;
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "Initialize", ex);
      }

      return _isInitialized;
    }
    
    void OnEnable() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnEnable");

      if (_uiNotificationProcessor != null && !_uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Enable();

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

      try {
        UPAIUnityEditorRecoveryManager.PackageSearchWindowClosed();
      } catch {

      }

      _selectedTabIndex = 0;
      _contentsToDisplay = null;
      _currentDocuments = null;
      _currentDocument = null;      
      _pagingHelper = null;
      _uiNotificationProcessor = null;
    }

    void OnDisable() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "OnDisable");

      if (_uiNotificationProcessor != null && _uiNotificationProcessor.IsEnabled())
        _uiNotificationProcessor.Disable();

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

    void _ChangeSortType(int selectedSortIndex) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ChangeSortType");

      if (_selectedSortIndex == selectedSortIndex)
        return;

      _selectedSortIndex = selectedSortIndex;

      var sortType = _sortTypes[_selectedSortIndex];

      //_tempPagingHelper = _pagingHelper;

      _pagingHelper.CurrentPageSort = sortType;
      _pagingHelper.ApplySort();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="category"></param>
    void _FilterOnCategory(string category) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_FilterOnCategory");

      if (string.IsNullOrEmpty(category))
        return;

      if (category.Contains(" \\ "))
        category = category.Replace(" \\ ", " / ");

      if (category.Contains(" and "))
        category = category.Replace(" and ", " & ");

      if (category.EndsWith("/(root)"))
        category = category.Substring(0, category.Length - 7);

      if (!_organizedCategories.ContainsKey(category))
        return;

      if (string.Compare(_pagingHelper.Category, category, true) == 0)
        return;

      _selectedCategoryId = _selectionCategoryId;

      //_tempPagingHelper = _pagingHelper;

      _pagingHelper.Category = category;
      _pagingHelper.ApplyFilters();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="publisher"></param>
    void _FilterOnPublisher(string publisher) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_FilterOnPublisher");

      if (string.IsNullOrEmpty(publisher))
        return;

      if (publisher.StartsWith("[0 - 9]/")) {
        publisher = publisher.Substring(10, publisher.Length - 10);
      } else {
        publisher = publisher.Substring(10, publisher.Length - 10);
      }

      if (publisher.Contains(" \\ "))
        publisher = publisher.Replace(" \\ ", " / ");

      if (publisher.Contains(" and "))
        publisher = publisher.Replace(" and ", " & ");

      if (!_organizedPublishers.ContainsKey(publisher))
        return;

      if (string.Compare(_pagingHelper.Publisher, publisher, true) == 0)
        return;

      _selectedPublisherId = 0;

      //_tempPagingHelper = _pagingHelper;

      _pagingHelper.Publisher = publisher;
      _pagingHelper.ApplyFilters();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    void _FilterOnSource(string source) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_FilterOnSource");

      if (string.IsNullOrEmpty(source))
        return;

      if (string.Compare(_pagingHelper.Source, source, true) == 0)
        return;

      _selectedSourceId = 0;

      //_tempPagingHelper = _pagingHelper;

      _pagingHelper.Source = source;
      _pagingHelper.ApplyFilters();
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
    void _ClearFilters(bool clearCategory = false, bool clearPublisher = true, bool clearSource = true) {
      if (clearCategory)
        _selectedCategoryId = -1;

      if (clearPublisher)
        _selectedPublisherId = -1;

      if (clearSource)
      _selectedSourceId = -1;

      _pagingHelper.ClearFilters(clearCategory, clearPublisher, clearSource);
    }

    private void UPAIPagingHelper_FilterResultsStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_FilterResultsStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_FilterResultsStarted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isFiltering = true;
    }

    private void UPAIPagingHelper_FilterResultsCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_FilterResultsCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_FilterResultsCompleted");

      if (_pagingHelper.TotalItems == 0) {
        e.NotificationMessage = "No results found for current filter(s)...";
        _nonFilteredCategories = null;
        _filteredPublishers = null;
        _filteredSources = null;
        _sortTypes = null;
      } else {
        if (_pagingHelper.AvailableCategories != null && _pagingHelper.AvailableCategories.Count > 0) {
          try {
            _OrganizeCategories(_pagingHelper.AvailableCategories.Keys.ToArray());
            _nonFilteredCategories = _RemoveRootOnlyCategories(_organizedCategories.Values.OrderBy(x => x).ToArray());
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_FilterResultsCompleted", ex);
          }
        }

        if (_pagingHelper.AvailablePublishers != null && _pagingHelper.AvailablePublishers.Count > 0) {
          try {
            _OrganizePublishers(_pagingHelper.AvailablePublishers.Keys.ToArray());
            _filteredPublishers = _organizedPublishers.Values.OrderBy(x => x).ToArray();
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_FilterResultsCompleted", ex);
          }
        }

        if (_pagingHelper.AvailableSources != null && _pagingHelper.AvailableSources.Count > 0) {
          try {
            _filteredSources = _pagingHelper.AvailableSources.Keys.OrderBy(x => x).ToArray();
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_FilterResultsCompleted", ex);
          }
        }

        if (_pagingHelper.FieldSorts != null && _pagingHelper.FieldSorts.Count > 0) {
          _sortTypes = _pagingHelper.FieldSorts.ToArray();

          _selectedSortIndex = Array.IndexOf<string>(_sortTypes, _pagingHelper.CurrentPageSort);
        }
      }

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

      if (_pagingHelper.TotalItems == 0) {
        e.NotificationMessage = string.Format("No results found for \"{0}\"...", _searchString);
        _nonFilteredCategories = null;
        _filteredPublishers = null;
        _filteredSources = null;
        _sortTypes = null;
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

        if (_pagingHelper.AvailableCategories != null && _pagingHelper.AvailableCategories.Count > 0) {
          try {
            _OrganizeCategories(_pagingHelper.AvailableCategories.Keys.ToArray());
            _nonFilteredCategories = _RemoveRootOnlyCategories(_organizedCategories.Values.OrderBy(x => x).ToArray());
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_IndexSearchCompleted", ex);
          }
        }

        if (_pagingHelper.AvailablePublishers != null && _pagingHelper.AvailablePublishers.Count > 0) {
          try {
            _OrganizePublishers(_pagingHelper.AvailablePublishers.Keys.ToArray());
            _filteredPublishers = _organizedPublishers.Values.OrderBy(x => x).ToArray();            
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "UPAIPagingHelper_IndexSearchCompleted", ex);
          }
        }

        if (_pagingHelper.AvailableSources != null && _pagingHelper.AvailableSources.Count > 0) {
          try {
            _filteredSources = _pagingHelper.AvailableSources.Keys.OrderBy(x => x).ToArray();
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

    private void UPAIPagingHelper_IndexSearchAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAIPagingHelper_IndexSearchAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "UPAIPagingHelper_IndexSearchAborted");

      _uiNotificationProcessor.ProcessUINotification(e);
      _isSearching = false;
      _forcePageReload = true;
    }

    private void UPAISystem_IndexStatusRefreshStarting(object sender, Events.UPAIProcessStartingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAISystem_IndexStatusRefreshStarted(object sender, Events.UPAIProcessStartedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAISystem_IndexStatusRefreshCompleting(object sender, Events.UPAIProcessCompletingEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAISystem_IndexStatusRefreshCompleted(object sender, Events.UPAIProcessCompletedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
    }

    private void UPAISystem_IndexStatusRefreshAborting(object sender, Events.UPAIProcessAbortingEventArgs e) {
      throw new NotImplementedException();
    }

    private void UPAISystem_IndexStatusRefreshAborted(object sender, Events.UPAIProcessAbortedEventArgs e) {
      _uiNotificationProcessor.ProcessUINotification(e);
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

    void _ChangePages(Action pageMethod) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_ChangePages");

      pageMethod();
      _selectionGridIndex = -1;
    }

    void _Search() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_Search");

      _pagingHelper = new UPAIIndexablePackagePagingHelper(
        UPAISystem.UnityVersion,
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

    void _Reset(int selectedCategoryId = -1) {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "_Reset");

      _nonFilteredCategories = null;
      _filteredPublishers = null;
      _filteredSources = null;
      _forceSearch = false;
      _searchString = "";
      _selectedCategoryId = selectedCategoryId;
      _selectedPublisherId = -1;
      _selectedSourceId = -1;
      _selectionCategoryId = -1;
      _selectionGridIndex = -1;
      _selectionPublisherId = -1;
      _selectionSourceId = -1;

      _pagingHelper = new UPAIIndexablePackagePagingHelper(
        UPAISystem.UnityVersion);

      _pagingHelper.IndexSearchAborted += UPAIPagingHelper_IndexSearchAborted;
      _pagingHelper.IndexSearchAborting += UPAIPagingHelper_IndexSearchAborting;
      _pagingHelper.IndexSearchCompleted += UPAIPagingHelper_IndexSearchCompleted;
      _pagingHelper.IndexSearchCompleting += UPAIPagingHelper_IndexSearchCompleting;
      _pagingHelper.IndexSearchStarted += UPAIPagingHelper_IndexSearchStarted;
      _pagingHelper.IndexSearchStarting += UPAIPagingHelper_IndexSearchStarting;

      _pagingHelper.PopulatePages();
    }

    void _DrawUI() {
      if (_isDisabled)
        return;

      Event e = Event.current;
      
      var tabIndex = _DrawHorizontalTabs(new string[] { "Search", "Favorites (0)", "Recent (0)" }, _selectedTabIndex);
      if (tabIndex != _selectedTabIndex) {
        UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "TabClick", tabIndex);

        _selectedTabIndex = tabIndex;
        return;
      }
      _DrawSpaces(1);
      switch (_selectedTabIndex) {
        case 0:
          _BeginHorizontal();
          _BeginVertical();

          if (_nonFilteredCategories != null && _nonFilteredCategories.Length > 0) {
            _selectionCategoryId = EditorGUILayout.Popup("Category: ", _selectedCategoryId, _nonFilteredCategories, GUILayout.MaxWidth(350));
            if (_selectionCategoryId != _selectedCategoryId && !_isFiltering) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-Category", _selectionCategoryId);
              
              _EndVertical();
              _EndHorizontal();
              _FilterOnCategory(_nonFilteredCategories[_selectionCategoryId]);

              return;
            }
          } else {
            _selectedCategoryId = -1;
          }

          if (_filteredPublishers != null && _filteredPublishers.Length > 0) {
            _BeginHorizontal();
            _selectionPublisherId = EditorGUILayout.Popup("Publisher: ", _selectedPublisherId, _filteredPublishers, GUILayout.MaxWidth(350));
            if (_selectionPublisherId != _selectedPublisherId && !_isFiltering) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-Publisher", _selectionPublisherId);

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _FilterOnPublisher(_filteredPublishers[_selectionPublisherId]);

              return;
            }
            if (_selectedPublisherId != -1) {
              var showAllPubsAndSources = GUILayout.Button("Clear", GUILayout.Width(75));
              if (showAllPubsAndSources && !_isFiltering) {
                _EndHorizontal();
                _EndVertical();
                _EndHorizontal();
                _ClearFilters();

                return;
              }
            }
            _EndHorizontal();
          } else {
            _selectedPublisherId = -1;
          }

          if (_filteredSources != null && _filteredSources.Length > 0) {
            _BeginHorizontal();
            _selectionSourceId = EditorGUILayout.Popup("Source: ", _selectedSourceId, _filteredSources, GUILayout.MaxWidth(350));
            if (_selectionSourceId != _selectedSourceId && !_isFiltering) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-Source", _selectionSourceId);

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _FilterOnSource(_filteredSources[_selectionSourceId]);

              return;
            }
            if (_selectedSourceId != -1) {
              var showAllPubsAndSources = GUILayout.Button("Clear", GUILayout.Width(75));
              if (showAllPubsAndSources && !_isFiltering) {
                _EndHorizontal();
                _EndVertical();
                _EndHorizontal();
                _ClearFilters();

                return;
              }
            }
            _EndHorizontal();
          } else {
            _selectedSourceId = -1;
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

            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            _Search();

            return;
          }
          _forceSearch = GUILayout.Button("Reset", GUILayout.Width(75));
          if (_forceSearch && !_isSearching) {
            UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-Reset", "");

            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
            _Reset();

            return;
          }
          if (_sortTypes != null && _sortTypes.Length > 0) {
            var selectedSort = EditorGUILayout.Popup(_selectedSortIndex, _sortTypes, GUILayout.MaxWidth(150));
            if (selectedSort != _selectedSortIndex) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "PopUpClick-Sort", selectedSort);

              _EndHorizontal();
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
          if ((_pagingHelper != null && _pagingHelper.TotalItems > 0) || (_contentsToDisplay != null && _contentsToDisplay.Length > 0)) {
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
                _DrawHorizontalLabelField(string.Format("Selection - {0} Packages's...", TotalItems.ToString("N0")));
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
              _EndHorizontal();
              _ChangePages(FirstPage);
              return;
            }
            _forceSearch = GUILayout.Button("<", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-PreviousPage", "");

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _ChangePages(PreviousPage);
              return;
            }
            EditorGUILayout.LabelField(string.Format("Page {0} of {1}...", _pagingHelper.CurrentPageId, _pagingHelper.TotalPages), GUILayout.Width(200));
            _forceSearch = GUILayout.Button(">", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-NextPage", "");

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _ChangePages(NextPage);
              return;
            }
            _forceSearch = GUILayout.Button(">>", GUILayout.Width(50));
            if (_forceSearch && !_isSearching) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-LastPage", "");

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _ChangePages(LastPage);

              return;
            }
            _forcePackageView = GUILayout.Button("View Package...", GUILayout.Width(200));
            if (_selectedGridIndex != -1 && _forcePackageView) {
              UPAISystem.Debug.LogDebugMessage("{0}.{1}(): User Action[{2}:{3}]", CLASS_NAME, "_DrawUI", "ButtonClick-ViewPackage", _selectedGridIndex);

              _EndHorizontal();
              _EndVertical();
              _EndHorizontal();
              _currentDocument = _SelectItem(_selectedGridIndex);
              UPAIPackageAssetDataWindow.CreatePackageAssetDataWindow(_currentDocument);

              return;
            }
            _EndHorizontal();
          } else {
            _DrawHorizontalLabelField("No results found...");
            _EndHorizontal();
            _EndVertical();
            _EndHorizontal();
          }
          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();

          return;
        case 1:
          _BeginHorizontal();
          _BeginVertical();
          _scrollPosition_FavoritePackages = _DrawVerticalLabelTextAreaField("Favorite Packages", "Coming Soon...",
            _scrollPosition_FavoritePackages, 130, 795, 550);
          _DrawSpaces(3);
          _DrawHorizontalLabelValueField("UPAI Version", UPAISystem.UPAIVersion);
          _EndVertical();
          _EndHorizontal();

          return;
        case 2:
          _BeginHorizontal();
          _BeginVertical();
          _scrollPosition_RecentPackages = _DrawVerticalLabelTextAreaField("Recent Packages", "Coming Soon...",
            _scrollPosition_RecentPackages, 130, 795, 550);
          _DrawSpaces(3);
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

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.FirstPage(), ItemsPerPage);
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

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.LastPage(), ItemsPerPage);
    }

    public void NextPage() {
      UPAISystem.Debug.LogDebugMessage("{0}.{1}()", CLASS_NAME, "NextPage");

      if (_pagingHelper == null)
        _contentsToDisplay = null;

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.NextPage(), ItemsPerPage);
    }

    public void Page(int id) {
      if (_pagingHelper == null)
        _contentsToDisplay = null;

      if (_pagingHelper.CurrentPageId == id)
        return;

      _currentDocuments = _pagingHelper.Page(id);
      _contentsToDisplay = ConvertLuceneDocuments(_currentDocuments, ItemsPerPage);
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

      _contentsToDisplay = ConvertLuceneDocuments(_pagingHelper.PreviousPage(), ItemsPerPage);
    }

    private GUIContent[] _GetSearchingContentPlaceHolder() {
      if (_searchRotation > 480)
        _searchRotation = 0;

      var searchingContent = new List<GUIContent>();
      for (int i = 0; i < 15; i++) {
        if (i == 15 / 2) {
          searchingContent.Add(new GUIContent() {
            image = Resources.Load<Texture2D>(string.Format("Icons/Search_{0}", _searchRotation / 60)),
            text = "Searching..."
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

    private string[] _RemoveRootOnlyCategories(string[] organizedCategories) {
      if (organizedCategories == null || organizedCategories.Length == 0)
        return null;

      var strippedCategories = new List<string>();
      var searchCategories = new string[organizedCategories.Length];
      organizedCategories.CopyTo(searchCategories, 0);

      foreach (var category in searchCategories) {
        try {
          var root = category.Substring(0, category.Length - 6);

          var rootMembers = organizedCategories
            .Where(x => x.StartsWith(root))
            .ToList();
          if (rootMembers.Count == 1) {
            strippedCategories.Add(root.Substring(0, root.Length - 1));
            continue;
          }

          strippedCategories.Add(category);          
        } catch {

        }
      }

      return strippedCategories.Distinct().OrderBy(x => x).ToArray();
    }

    private void _OrganizeCategories(string[] categories) {
      if (categories == null || categories.Length == 0)
        return;

      _organizedCategories = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var category in categories) {
        var organizedCategory = category;
        if (organizedCategory.Contains(" & "))
          organizedCategory = organizedCategory.Replace(" & ", " and ");

        if (organizedCategory.Contains(" / "))
          organizedCategory = organizedCategory.Replace(" / ", " \\ ");
        
        _organizedCategories[category] = string.Format("{0}/(root)", organizedCategory);        
      }
    }

    private void _OrganizePublishers(string[] publishers) {
      if (publishers == null || publishers.Length == 0)
        return;

      _organizedPublishers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
      foreach (var publisher in publishers) {
        var organizedPublisher = publisher;
        if (organizedPublisher.Contains(" & "))
          organizedPublisher = organizedPublisher.Replace(" & ", " and ");

        if (organizedPublisher.Contains(" / "))
          organizedPublisher = organizedPublisher.Replace(" / ", " \\ ");

        var startingChar = organizedPublisher.Substring(0, 1).ToUpper();
        var sortNumber = -1;
        if (int.TryParse(startingChar, out sortNumber)) {
          organizedPublisher = string.Format("[0 - 9]/{0}/{1}", organizedPublisher.Substring(0, 1).ToUpper(), organizedPublisher);
        } else {
          organizedPublisher = string.Format("[A - Z]/{0}/{1}", organizedPublisher.Substring(0, 1).ToUpper(), organizedPublisher);
        }

        _organizedPublishers[publisher] = organizedPublisher;
      }
    }
  }
}