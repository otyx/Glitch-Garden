using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RKGamesDev.Systems.UPAI.Editor {
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.ScriptFileVersion)]
  public class UPAISystemCleaner {
    private const string CLASS_NAME = "UPAISystemCleaner";

    /// <summary>
    /// 
    /// </summary>
    public static List<string> AssetsToDelete {
      get;
      private set;
    }

    /// <summary>
    /// 
    /// </summary>
    public static List<string> DeletedAssets {
      get;
      private set;
    }

    /// <summary>
    /// 
    /// </summary>
    public static List<string> FailedAssetDeletions {
      get;
      private set;
    }

    /// <summary>
    /// 
    /// </summary>
    private struct AssetCleanupCheck {
      /// <summary>
      /// 
      /// </summary>
      public string Version {
        get;
        set;
      }

      /// <summary>
      /// 
      /// </summary>
      public string SearchFilter {
        get;
        set;
      }

      public string[] SearchFolders {
        get;
        set;
      }

      public string[] GetMatchingAssets() {
        if (string.IsNullOrEmpty(SearchFilter))
          return null;

        if (SearchFolders.Length == 0) {
          UPAISystem.Debug.LogDebugMessage(string.Format("AssetCleanupCheck.GetMatchingAssets(Version: {0}, SearchFilter:{1}, SearchFolders: All)",
            Version, SearchFilter));

          return AssetDatabase.FindAssets(SearchFilter);
        } else {
          UPAISystem.Debug.LogDebugMessage(string.Format("AssetCleanupCheck.GetMatchingAssets(Version: {0}, SearchFilter:{1}, SearchFolders[{2}]: {3})",
            Version, SearchFilter, SearchFolders.Length, string.Join(";", SearchFolders)));

          return AssetDatabase.FindAssets(SearchFilter, SearchFolders);
        }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="version"></param>
      /// <param name="searchFilter"></param>
      /// <param name="searchFolders"></param>
      public AssetCleanupCheck(string version, string searchFilter, string[] searchFolders = null) {
        Version = version;
        SearchFilter = searchFilter;
        SearchFolders = searchFolders;
        if (SearchFolders == null)
          SearchFolders = new string[0];
      }
    }

    private static List<AssetCleanupCheck> _assetsToCheck = new List<AssetCleanupCheck> {
      new AssetCleanupCheck("0.9.9.2", "UPAIPackageDataWindow", new string[] { "Assets/Plugins/UPAI.Net/Editor" }),      
      new AssetCleanupCheck("0.9.9.2", "Newtonsoft.Json", new string[] { "Assets/Plugins/Newtonsoft.Json.Net" }),
      new AssetCleanupCheck("0.9.9.2", "Newtonsoft.Json.Net", new string[] { "Assets/Plugins" }),
    };

    public static int CheckForCleanup() {
      if (_assetsToCheck.Count == 0)
        return 0;

      UPAISystem.Debug.LogDebugMessage("UPAISystemCleaner.CheckForCleanup()");

      AssetsToDelete = new List<string>();
      foreach (var assetCheck in _assetsToCheck) {
        var matchingAssets = assetCheck.GetMatchingAssets();
        if (matchingAssets == null || matchingAssets.Length == 0)
          continue;

        AssetsToDelete.AddRange(matchingAssets
          .Select(x => {
            return AssetDatabase.GUIDToAssetPath(x);
          })
          .ToArray());
      }

      if (AssetsToDelete.Count == 0)
        return 0;

      return AssetsToDelete.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processCleanup"></param>
    /// <returns></returns>
    public static bool CleanupSystem() {
      if (AssetsToDelete.Count == 0) {
        if (CheckForCleanup() == 0) {
          return false;
        }
      }

      UPAISystem.Debug.LogDebugMessage("UPAISystemCleaner.CleanupSystem()");

      DeletedAssets = new List<string>();
      foreach (var assetToDelete in AssetsToDelete) {
        if (string.IsNullOrEmpty(assetToDelete))
          continue;

        UPAISystem.Debug.LogDebugMessage("Deleting prior asset file {0}...", assetToDelete);

        if (AssetDatabase.DeleteAsset(assetToDelete)) {
          DeletedAssets.Add(assetToDelete);
        } else {
          if (FailedAssetDeletions == null)
            FailedAssetDeletions = new List<string>();

          UPAISystem.Debug.LogErrorFormat("Unable to cleanup prior asset file {0}...", assetToDelete);

          FailedAssetDeletions.Add(assetToDelete);
        }
      }

      if (DeletedAssets.Count > 0) {
        AssetDatabase.Refresh();
        EditorApplication.RepaintProjectWindow();
      }

      return (AssetsToDelete.Count == DeletedAssets.Count);
    }
  }
}