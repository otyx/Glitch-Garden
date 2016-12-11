using Lucene.Net.Documents;
using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
using RKGamesDev.Systems.UPAI.Managers;
using RKGamesDev.Systems.UPAI.Models;
using RKGamesDev.Systems.UPAI.Searchers;
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
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.EditorWindowVersion)]
  public class UPAIBaseEditorWindow : EditorWindow {
    private const string CLASS_NAME = "UPAIBaseEditorWindow";

    protected static bool _processIsRunning = false;
    protected static string _processName = "";
    
    protected void _DrawHorizontalSortedFieldDictionary(Dictionary<string, int> assetTypeCounts, int labelWidth = 250, int valueWidth = 300, string toolTip = "") {
      if (assetTypeCounts == null || assetTypeCounts.Count == 0)
        return;

      var sortedDictionary = assetTypeCounts.OrderBy(x => x.Key);
      foreach (var entry in sortedDictionary)
        _DrawHorizontalLabelValueField(entry.Key, entry.Value.ToString(), labelWidth, valueWidth);

    }

    protected void _BeginHorizontal() {
      EditorGUILayout.BeginHorizontal();
    }

    protected void _BeginVertical() {
      EditorGUILayout.BeginVertical();
    }

    protected void _DrawSpaces(int count = 1) {
      if (count < 1)
        count = 1;

      if (count > 120)
        count = 120;

      for (int i = 0; i < count; i++) {
        EditorGUILayout.Space();
      }
    }

    protected void _EndHorizontal() {
      EditorGUILayout.EndHorizontal();
    }

    protected void _EndVertical() {
      EditorGUILayout.EndVertical();
    }

    protected T _DrawHorizontalEnumPopupField<T>(string caption, System.Enum selected, int labelWidth = 250, int valueWidth = 300, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return default(T);

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      var newValue = EditorGUILayout.EnumPopup(selected, GUILayout.Width(valueWidth));
      EditorGUILayout.EndHorizontal();

      return (T)(object)newValue;
    }

    protected int _DrawHorizontalIntField(string caption, int value, int labelWidth = 250, int valueWidth = 300, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return -1;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      var newValue = EditorGUILayout.IntField(value, GUILayout.Width(valueWidth));
      EditorGUILayout.EndHorizontal();

      return newValue;
    }

    protected void _DrawHorizontalOrderedFieldArray(string[] array, int labelWidth = 600, string toolTip = "") {      
      if (array == null || array.Length == 0)
        return;

      int nextNumber = 1;
      foreach (var data in array) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent(string.Format("{0}. {1}", nextNumber, data), toolTip), GUILayout.Width(labelWidth));
        EditorGUILayout.EndHorizontal();
        nextNumber++;
      }
    }

    protected void _DrawHorizontalLabelField(string caption, int labelWidth = 600, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("--- {0} ---", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      EditorGUILayout.EndHorizontal();
    }

    protected virtual bool _DrawHorizontalLabelButtonField(string caption, string buttonCaption = "Button", int labelWidth = 600, int buttonWidth = 75, string toolTip = "") {
      var buttonClicked = false;

      if (string.IsNullOrEmpty(caption))
        return buttonClicked;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("--- {0} ---", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      buttonClicked = GUILayout.Button(buttonCaption, GUILayout.Width(buttonWidth));
      EditorGUILayout.EndHorizontal();

      return buttonClicked;
    }

    protected void _DrawHorizontalLabelTextAreaField(string caption, string value, int labelWidth = 250, int valueWidth = 300, int valueHeight = 150, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return;

      if (string.IsNullOrEmpty(value))
        value = "N/A";

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      EditorGUILayout.TextArea(value, GUILayout.Width(valueWidth), GUILayout.Height(valueHeight));
      EditorGUILayout.EndHorizontal();
    }

    protected void _DrawHorizontalLabelValueField(string caption, string value, int labelWidth = 250, int valueWidth = 300, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return;

      if (string.IsNullOrEmpty(value))
        value = "N/A";

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField(value, GUILayout.Width(valueWidth));
      EditorGUILayout.EndHorizontal();
    }

    protected bool _DrawHorizontalLabelValueButtonField(string caption, string value, string buttonCaption = "Button", int labelWidth = 250, int valueWidth = 300, int buttonWidth = 75, string toolTip = "") {
      var buttonClicked = false;

      if (string.IsNullOrEmpty(caption))
        return buttonClicked;

      if (string.IsNullOrEmpty(value))
        value = "N/A";

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      EditorGUILayout.LabelField(value, GUILayout.Width(valueWidth));
      buttonClicked = GUILayout.Button(buttonCaption, GUILayout.Width(buttonWidth));
      EditorGUILayout.EndHorizontal();

      return buttonClicked;
    }

    /// <summary>
    /// Creates tabs from buttons, with their bottom edge removed by the magic of Haxx
    /// </summary>
    /// <remarks>
    /// The line will be misplaced if other elements is drawn before this
    /// </remarks>
    /// <returns>Selected tab</returns>
    protected int _DrawHorizontalTabs(string[] options, int selected) {
      const float DarkGray = 0.65f;
      const float LightGray = 0.9f;
      const float StartSpace = 10;

      GUILayout.Space(StartSpace);
      Color storeColor = GUI.backgroundColor;
      Color highlightCol = new Color(LightGray, LightGray, LightGray);
      Color bgCol = new Color(DarkGray, DarkGray, DarkGray);

      GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
      buttonStyle.padding.bottom = 7;

      GUILayout.BeginHorizontal();
      {   //Create a row of buttons
        for (int i = 0; i < options.Length; ++i) {
          GUI.backgroundColor = i == selected ? highlightCol : bgCol;
          if (GUILayout.Button(options[i], buttonStyle, GUILayout.Width(120))) {
            selected = i; //Tab click
          }
        }
      }
      GUILayout.EndHorizontal();
      //Restore color
      GUI.backgroundColor = storeColor;
      //Draw a line over the bottom part of the buttons (ugly haxx)
      var texture = new Texture2D(1, 1);
      texture.SetPixel(0, 0, highlightCol);
      texture.Apply();
      GUI.DrawTexture(new Rect(0, buttonStyle.lineHeight + buttonStyle.border.top + buttonStyle.margin.top + StartSpace, Screen.width, 4), texture);

      return selected;
    }

    protected bool? _DrawHorizontalToggleField(string caption, bool value, int labelWidth = 250, int valueWidth = 300, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return null;

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0} :", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      var currentValue = EditorGUILayout.Toggle(value, GUILayout.Width(valueWidth));      
      EditorGUILayout.EndHorizontal();

      return currentValue;
    }

    protected Vector2 _DrawVerticalLabelTextAreaField(string caption, string value, Vector2 scrollPostion, int labelWidth = 250, int valueWidth = 300, int valueHeight = 150, string toolTip = "") {
      if (string.IsNullOrEmpty(caption))
        return Vector2.zero;

      if (string.IsNullOrEmpty(value))
        value = "N/A";

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical();
      EditorGUILayout.LabelField(new GUIContent(string.Format("{0}: ", caption), toolTip), EditorStyles.boldLabel, GUILayout.Width(labelWidth));
      scrollPostion = GUILayout.BeginScrollView(scrollPostion);
      EditorGUILayout.TextArea(value, GUILayout.Width(valueWidth), GUILayout.Height(valueHeight));
      GUILayout.EndScrollView();
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();

      return scrollPostion;
    }

    protected string _GetDocumentSize(IFieldable value) {
      if (value == null || string.IsNullOrEmpty(value.StringValue))
        return "N/A";

      var fileSize = 0L;
      if (long.TryParse(value.StringValue, out fileSize)) {
        return UPAIFileInfoManager.FormatFileSize(fileSize);
      }

      return "N/A";
    }

    protected string _GetDocumentDate(IFieldable value) {
      if (value == null || string.IsNullOrEmpty(value.StringValue))
        return "N/A";

      var ticks = 0L;
      if (long.TryParse(value.StringValue, out ticks)) {
        return new DateTime(ticks).ToString("g");
      }

      return "N/A";
    }

    protected string _GetDocumentString(IFieldable value) {
      if (value == null || string.IsNullOrEmpty(value.StringValue))
        return "N/A";

      return value.StringValue;
    }
    
    protected void _ImportPackage(string packageId) {
      UPAISystem.RefreshSystemData(true, false);

      if (string.IsNullOrEmpty(packageId)) {
        UPAISystem.Debug.LogException("UPAIAssetDataWindow", "_ImportPackage", new ArgumentNullException("PackageID"));
        return;
      }

      var packageDocument = UPAIIndexSearcher
        .SearchPackages(Application.unityVersion, UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME, packageId, 1).FirstOrDefault();
      if (packageDocument == null) {
        UPAISystem.Debug.LogException("UPAIAssetDataWindow", "_ImportPackage", 
          new Exception(string.Format("Unable to locate package document for {0}...", packageId)));
        return;
      }

      var packageFileLocation = UPAIPackageFileInfoManager.GeneratePackageDocumentFilePath(UPAISystem.UnityVersion, packageDocument);
      if (string.IsNullOrEmpty(packageFileLocation)) {
        UPAISystem.Debug.LogException("UPAIAssetDataWindow", "_ImportPackage",
          new Exception(string.Format("Unable to determine package source file location for {0}...", packageId)));
        return;
      }

      if (!File.Exists(packageFileLocation)) {
        UPAISystem.Debug.LogException("UPAIAssetDataWindow", "_ImportPackage",
          new Exception(string.Format("Package source doesn't exist for {0}...", packageFileLocation)));
        return;
      }

      try {
        EditorUtility.DisplayDialog("Script File Import Warning...",
          "If any scripts are included in the current import, " +
          "this will cause the Unity Editor to begin recompiling, " +
          "and may cause the UPAI system to shut down all data windows." +
          "Should this occur, once Unity has completed it's post asset import cleanup process, " +
          "click on \"Recover UPAI system...\" from the \"UPAI => System\" menu.", "Ok");
        UPAIUnityEditorRecoveryManager.SetLastPackageIdImported(packageId);
        UPAIUnityEditorRecoveryManager.PrepareForRebuild();
      } catch {

      }

      AssetDatabase.ImportPackage(packageFileLocation, true);
    }

    protected void _ViewAssetPackage(string packageId) {
      UPAISystem.RefreshSystemData();

      if (string.IsNullOrEmpty(packageId)) {
        UPAISystem.Debug.LogException("UPAIBaseEditorWindow", "_ViewAssetPackage", new ArgumentNullException("PackageID"));
        return;
      }

      var packageDocument = UPAIIndexSearcher
        .SearchPackages(Application.unityVersion, UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME, packageId, 1).FirstOrDefault();
      if (packageDocument == null) {
        UPAISystem.Debug.LogException("UPAIBaseEditorWindow", "_ViewAssetPackage",
          new Exception(string.Format("Unable to locate package document for {0}...", packageId)));
        return;
      }

      UPAIPackageAssetDataWindow.CreatePackageAssetDataWindow(packageDocument);
    }
  }
}
