using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

 public class TextureImport : AssetPostprocessor
 {
     // Import all textures as Sprites
     void OnPreprocessTexture()
     {
         TextureImporter textureImporter = (TextureImporter)assetImporter;
         textureImporter.maxTextureSize = 512;
         textureImporter.textureCompression = TextureImporterCompression.CompressedHQ;

         textureImporter.textureType = TextureImporterType.Sprite;
     }
 }