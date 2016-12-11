using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Helpers;
using System;
using UnityEditor;
using UnityEngine;

namespace RKGamesDev.Systems.UPAI.Editor {
  /// <summary>
  /// 
  /// </summary>
  [ExecuteInEditMode()]
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.ScriptFileVersion)]
  public class UPAIWebClient : MonoBehaviour {
    private static UPAIWebClientHelper _instance = null;
    private static UPAIWebClientHelper WebClientHelper {
      get {
        if (!UPAIMenuItems.SystemIsInitialized())
          return null;

        if (_instance == null)
          _instance = new UPAIWebClientHelper();

        return _instance;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetAssetStoreUrl() {
      if (!UPAIMenuItems.SystemIsInitialized() || WebClientHelper == null)
        return null;

      return _GetResourceValue(WebClientHelper.GetAssetStoreUrlServiceURI());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentVersion() {
      if (!UPAIMenuItems.SystemIsInitialized() || WebClientHelper == null)
        return null;

      return _GetResourceValue(WebClientHelper.GetCurrentVersionServiceURI());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetProductDocumentUrl() {
      if (!UPAIMenuItems.SystemIsInitialized() || WebClientHelper == null)
        return null;

      return _GetResourceValue(WebClientHelper.GetProductDocumentUrlServiceURI());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetProductHelpUrl() {
      if (!UPAIMenuItems.SystemIsInitialized() || WebClientHelper == null)
        return null;

      return _GetResourceValue(WebClientHelper.GetHelpUrlServiceURI());
    }

    public static string GetProductRegistrationUrl() {
      return "http://www.rk-softwaredev.net/site-registration?returnurl=http%3a%2f%2fwww.rk-softwaredev.net%2f";
    }

    public static string GetProductSupportContactUrl() {
      return "mailto:robert.keown@rk-softwaredev.net?subject=UPAI Support";
    }

    private static string _GetResourceValue(string serviceUri) {
      var webSecurityEmulationEnabled = EditorSettings.webSecurityEmulationEnabled;
      var webSecurityEmulationHostUrl = EditorSettings.webSecurityEmulationHostUrl;

      var resourceValue = "";

      try {
        EditorSettings.webSecurityEmulationEnabled = true;
        EditorSettings.webSecurityEmulationHostUrl = "https://webservices.rk-softwaredev.net";

        var unityWebRequest = new WWW(serviceUri);

        while (!unityWebRequest.isDone) {
        }

        // Strip excess double-quotes from resource value retrieval
        resourceValue = unityWebRequest.text.Replace("\"", "");

        EditorSettings.webSecurityEmulationEnabled = webSecurityEmulationEnabled;
        EditorSettings.webSecurityEmulationHostUrl = webSecurityEmulationHostUrl;
      } catch (Exception ex) {
        UPAISystem.Debug.LogException("UPAIWebClient", "_GetResourceValue", ex);
      }

      return resourceValue;
    }
  }
}