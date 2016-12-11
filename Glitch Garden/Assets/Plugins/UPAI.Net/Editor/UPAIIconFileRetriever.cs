using Lucene.Net.Documents;
using RKGamesDev.Systems.UPAI.Attributes;
using RKGamesDev.Systems.UPAI.Enumerations;
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
  [UPAIFileVersion("0.9.9.2", UPAIVersionTypeEnum.ScriptFileVersion)]
  public class UPAIIconFileRetriever {
    private const string CLASS_NAME = "UPAIIconFileRetriever";
    private const float MAX_IMAGE_DIMENSION = 128f;

    private static Dictionary<string, Texture2D> _assetPreviewFilesCache;
    private static Dictionary<string, Texture2D> _packageIconFileCache;

    /// <summary>
    /// 
    /// </summary>
    public static Dictionary<string, Texture2D> AssetPreviewFileCache {
      get {
        if (_assetPreviewFilesCache == null)
          _assetPreviewFilesCache = new Dictionary<string, Texture2D>();

        return _assetPreviewFilesCache;
      }
      private set {
        if (value != null)
          _assetPreviewFilesCache = value;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public static Dictionary<string, Texture2D> PackageIconFileCache {
      get {
        if (_packageIconFileCache == null)
          _packageIconFileCache = new Dictionary<string, Texture2D>();

        return _packageIconFileCache;
      }
      private set {
        if (value != null)
          _packageIconFileCache = value;
      }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetDocument"></param>
    /// <returns></returns>
    public static Texture2D GetAssetPreviewIcon(Document assetDocument) {
      Texture2D texture = null;

      var documentType = "";
      try {
        documentType = assetDocument.GetField(UPAIDocumentFieldNames.COMMON_DOCUMENTTYPE_FIELDNAME).StringValue;
        if (string.IsNullOrEmpty(documentType))
          return null;
      } catch {
        return null;
      }
      
      try {
        var assetId = "";
        try {
          assetId = assetDocument.GetField(UPAIDocumentFieldNames.ASSET_ID_FIELDNAME).StringValue;
        } catch {
          throw new ArgumentException("Asset document does not contain asset id field.");
        }

        if (AssetPreviewFileCache.ContainsKey(assetId) &&
          AssetPreviewFileCache[assetId] != null)
          return AssetPreviewFileCache[assetId];

        var previewImagePath = _GetField(assetDocument, "assetImagePath");
        if (!string.IsNullOrEmpty(previewImagePath)) {
          var www = new WWW("file://" + previewImagePath);
          texture = www.texture;
        } else {
          try {
            var assetType = _GetField(assetDocument, UPAIDocumentFieldNames.ASSET_TYPE_FIELDNAME);
            var selectedAssetType = (UPAIAssetTypeEnum)Enum.Parse(
              typeof(UPAIAssetTypeEnum), assetType);

            var typeDefinition = UPAIAssetDataManager.AssetTypeDefintions
              .FirstOrDefault(x => x.Key == selectedAssetType);
            if (typeDefinition.Value != null)
              texture = typeDefinition.Value.Image as Texture2D;
            else
              texture = null;
          } catch {
            texture = null;
          }
          if (texture == null) {
            var textureAssets = AssetDatabase.FindAssets("t:Texture2d UPAI_Default_Missing_Image");
            if (textureAssets.Length == 1) {
              texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureAssets[0]));
            } else {
              texture = null;
            }
          }
        }
        if (texture != null
          && (texture.height > MAX_IMAGE_DIMENSION || texture.width > MAX_IMAGE_DIMENSION)) {
          var percentInReduction = 0f;
          float newHeight = texture.height;
          float newWidth = texture.width;
          var resizeAttempts = 0;
          while (resizeAttempts <= 5 && (newHeight > MAX_IMAGE_DIMENSION || newWidth > MAX_IMAGE_DIMENSION)) {
            if (texture.height > MAX_IMAGE_DIMENSION) {
              percentInReduction = (MAX_IMAGE_DIMENSION / texture.height);
            } else {
              percentInReduction = (MAX_IMAGE_DIMENSION / texture.width);
            }

            newHeight *= percentInReduction;
            newWidth *= percentInReduction;
            resizeAttempts++;
          }

          try {
            UPAITextureScaler.Bilinear(texture, (int)newWidth, (int)newHeight);
          } catch (Exception ex) {
            UPAISystem.Debug.LogException(CLASS_NAME, "GetAssetPreviewIcon", ex);
          }
        }

        AssetPreviewFileCache[assetId] = texture;
      } catch (Exception ex) {
        UPAISystem.Debug.LogException(CLASS_NAME, "GetAssetPreviewIcon", ex);
        texture = null;
      }

      return texture;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetDocument"></param>
    /// <returns></returns>
    public static Texture2D GetAssetDependencyMapStatusIcon(Document assetDocument) {
      var documentType = "";
      try {
        documentType = assetDocument.GetField(UPAIDocumentFieldNames.COMMON_DOCUMENTTYPE_FIELDNAME).StringValue;
        if (string.IsNullOrEmpty(documentType))
          return null;
      } catch {
        return null;
      }

      Texture2D assetDependencyMapStatusTexture = Resources.Load("Checkmarks/assetDependencyNotAvailable") as Texture2D;
      var assetType = (UPAIAssetTypeEnum)Enum.Parse(typeof(UPAIAssetTypeEnum), _GetField(assetDocument, UPAIDocumentFieldNames.ASSET_TYPE_FIELDNAME));
      if (UPAIDependencyDataManager.DetermineDependencyCheck(assetType)) {
        var assetDependencyMap = _GetField(assetDocument, UPAIDocumentFieldNames.ASSET_DEPENDENCYMAP_FIELDNAME);
        if (!string.IsNullOrEmpty(assetDependencyMap)) {
          assetDependencyMapStatusTexture = Resources.Load("Checkmarks/assetDependencyChecked") as Texture2D;
        } else {
          assetDependencyMapStatusTexture = Resources.Load("Checkmarks/assetDependencyNotChecked") as Texture2D;
        }
      }

      return assetDependencyMapStatusTexture;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packageDocument"></param>
    /// <returns></returns>
    public static Texture2D GetPackageIcon(Document packageDocument) {
      Texture2D texture = null;

      var documentType = "";
      try {
        documentType = packageDocument.GetField(UPAIDocumentFieldNames.COMMON_DOCUMENTTYPE_FIELDNAME).StringValue;
        if (string.IsNullOrEmpty(documentType))
          return null;
      } catch {
        return null;
      }

      var packageId = "";
      try {
        packageId = packageDocument.GetField(UPAIDocumentFieldNames.PACKAGE_ID_FIELDNAME).StringValue;
      } catch {
        throw new ArgumentException("Package document does not contain package id field.");
      }

      if (PackageIconFileCache.ContainsKey(packageId) &&
        PackageIconFileCache[packageId] != null)
        return PackageIconFileCache[packageId];

      var packageImagePath = _GetField(packageDocument, "packageImagePath");
      if (!string.IsNullOrEmpty(packageImagePath)) {
        var www = new WWW("file://" + packageImagePath);
        texture = www.texture;
      } else {
        var textureAssets = AssetDatabase.FindAssets("t:Texture2d UPAI_Default_UnityPackage_Image");
        if (textureAssets.Length == 1) {
          texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureAssets[0]));
        } else {
          texture = null;
        }
      }
      if (texture != null && (texture.height < 128 || texture.height > 128))
        try {
          UPAITextureScaler.Bilinear(texture, 128, 128);
        } catch {

        }

      PackageIconFileCache[packageId] = texture;

      return texture;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packageDocument"></param>
    /// <returns></returns>
    public static Texture2D GetPackageMapStatusIcon(Document packageDocument) {
      var documentType = "";
      try {
        documentType = packageDocument.GetField(UPAIDocumentFieldNames.COMMON_DOCUMENTTYPE_FIELDNAME).StringValue;
        if (string.IsNullOrEmpty(documentType))
          return null;
      } catch {
        return null;
      }

      Texture2D packageMapStatusTexture = Resources.Load("Checkmarks/packageMapNotPresent") as Texture2D;
      var packageMap = _GetField(packageDocument, UPAIDocumentFieldNames.PACKAGE_FILEMAP_FIELDNAME);
      if (!string.IsNullOrEmpty(packageMap))
        packageMapStatusTexture = Resources.Load("Checkmarks/packageMapPresent") as Texture2D;

      return packageMapStatusTexture;
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