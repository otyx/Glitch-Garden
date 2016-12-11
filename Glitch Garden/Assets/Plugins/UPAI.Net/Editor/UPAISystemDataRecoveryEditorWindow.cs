using RKGamesDev.Systems.UPAI.Editor;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using RKGamesDev.Systems.UPAI.Searchers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UPAISystemDataRecoveryEditorWindow : EditorWindow {
  void OnGUI() {
    EditorGUILayout.LabelField("Compiling:", EditorApplication.isCompiling ? "Yes" : "No");
  }

  void Update() {
    if (EditorApplication.isCompiling) {
      EditorApplication.isPlaying = false;
    } else {
      try {
        if (UPAIMenuItems.SystemIsInitialized() && UPAIUnityEditorRecoveryManager.CanBeRecovered()) {
          var recoveryData = UPAIUnityEditorRecoveryManager.RecoverFromRebuild();
          if (recoveryData != null) {
            if (recoveryData.IndexStatusWindowIsOpen)
              EditorApplication.ExecuteMenuItem("UPAI/System/Maintenance/View Index Status");

            if (recoveryData.SystemStatusWindowIsOpen)
              EditorApplication.ExecuteMenuItem("UPAI/System/View System Status");

            if (recoveryData.ProjectStatusWindowIsOpen)
              EditorApplication.ExecuteMenuItem("UPAI/Current Project/View Status");

            if (!string.IsNullOrEmpty(recoveryData.LastPackageIdImported)) {
              if (recoveryData.PackageSearchWindowIsOpen)
                EditorApplication.ExecuteMenuItem("UPAI/Search Packages... #F3");

              if (recoveryData.ActivePackageWindows != null && recoveryData.ActivePackageWindows.Count > 0) {
                foreach (KeyValuePair<string, bool> packageWindowData in recoveryData.ActiveAssetWindows) {
                  if (string.IsNullOrEmpty(packageWindowData.Key))
                    continue;

                  if (string.Compare(packageWindowData.Key, recoveryData.LastAssetIdImported, true) == 0)
                    continue;

                  var packageDocument = UPAIIndexSearcher.SearchPackages(Application.unityVersion, new Dictionary<string, string>() {
                  { UPAIDocumentFieldNames.ASSET_ID_FIELDNAME, packageWindowData.Key }
                }, 1).FirstOrDefault();

                  if (packageDocument == null)
                    continue;

                  UPAIPackageAssetDataWindow.CreatePackageAssetDataWindow(packageDocument);
                }

                var importedPackageDocument = UPAIIndexSearcher.SearchPackages(Application.unityVersion, new Dictionary<string, string>() {
                  { UPAIDocumentFieldNames.ASSET_ID_FIELDNAME, recoveryData.LastAssetIdImported }
                }, 1).FirstOrDefault();

                if (importedPackageDocument != null)
                  UPAIPackageAssetDataWindow.CreatePackageAssetDataWindow(importedPackageDocument);
              }
            }

            if (!string.IsNullOrEmpty(recoveryData.LastAssetIdImported)) {
              if (recoveryData.AssetSearchWindowIsOpen)
                EditorApplication.ExecuteMenuItem("UPAI/Search Assets... _F3");

              if (recoveryData.ActiveAssetWindows != null && recoveryData.ActiveAssetWindows.Count > 0) {
                var importedAssetAllowedPackageView = false;
                foreach (KeyValuePair<string, bool> assetWindowData in recoveryData.ActiveAssetWindows) {
                  if (string.IsNullOrEmpty(assetWindowData.Key))
                    continue;

                  if (string.Compare(assetWindowData.Key, recoveryData.LastAssetIdImported, true) == 0) {
                    importedAssetAllowedPackageView = assetWindowData.Value;
                    continue;
                  }

                  var assetdocument = UPAIIndexSearcher.SearchAssets(Application.unityVersion, new Dictionary<string, string>() {
                    { UPAIDocumentFieldNames.ASSET_ID_FIELDNAME, assetWindowData.Key }
                  }, 1).FirstOrDefault();

                  if (assetdocument == null)
                    continue;

                  UPAIAssetDataWindow.CreateAssetDataWindow(assetdocument, assetWindowData.Value);
                }

                var importedAssetdocument = UPAIIndexSearcher.SearchAssets(Application.unityVersion, new Dictionary<string, string>() {
                  { UPAIDocumentFieldNames.ASSET_ID_FIELDNAME, recoveryData.LastAssetIdImported }
                }, 1).FirstOrDefault();

                if (importedAssetdocument != null)
                  UPAIAssetDataWindow.CreateAssetDataWindow(importedAssetdocument, importedAssetAllowedPackageView, true);
              }
            }
          }

          UPAIUnityEditorRecoveryManager.SystemRecovered(true);
        }

        Close();
      } catch {
        
      }
    }
  }
}