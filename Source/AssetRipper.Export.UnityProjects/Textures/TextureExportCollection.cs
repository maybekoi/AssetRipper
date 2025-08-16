using AssetRipper.Assets;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.SpriteMetaData;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Textures;

public class TextureExportCollection : AssetsExportCollection<ITexture2D>
{
	public TextureExportCollection(TextureAssetExporter assetExporter, SpriteInformationObject spriteInformationObject, bool exportSprites)
		: base(assetExporter, spriteInformationObject.Texture)
	{
		m_exportSprites = exportSprites;

		if (exportSprites && spriteInformationObject.Sprites.Count > 0)
		{
			System.Diagnostics.Debug.WriteLine(
				$"Processing {spriteInformationObject.Sprites.Count} sprites for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID})");
			
			foreach ((ISprite? sprite, ISpriteAtlas? _) in spriteInformationObject.Sprites)
			{
				ITexture2D? spriteTexture = sprite.TryGetTexture();
				if (spriteTexture != Asset)
				{
					System.Diagnostics.Debug.WriteLine(
						$"Warning: Sprite '{sprite.GetBestName()}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
						$"references texture '{spriteTexture?.GetBestName() ?? "null"}' (Type: {spriteTexture?.GetType().Name ?? "null"}, ClassID: {spriteTexture?.ClassID ?? -1}, PathID: {spriteTexture?.PathID ?? -1}) " +
						$"but SpriteInformationObject was created for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}). " +
						$"This may indicate a processing issue. Continuing with export...");
				}
				
				if (sprite.AssetInfo == Asset.AssetInfo)
				{
					System.Diagnostics.Debug.WriteLine(
						$"Warning: Skipping sprite '{sprite.GetBestName()}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
						$"because it has the same AssetInfo as the main texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}). " +
						$"This would create a duplicate asset reference.");
					continue;
				}
				
				if (spriteTexture is not null && spriteTexture.AssetInfo == Asset.AssetInfo)
				{
					System.Diagnostics.Debug.WriteLine(
						$"Warning: Skipping sprite '{sprite.GetBestName()}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
						$"because it references a texture with the same AssetInfo as the main texture.");
					continue;
				}
				
				if (!ContainsByAssetInfo(sprite))
				{
					System.Diagnostics.Debug.WriteLine(
						$"Adding sprite to collection: '{sprite.GetBestName()}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
						$"for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID})");
					AddAsset(sprite);
				}
				else
				{
					System.Diagnostics.Debug.WriteLine(
						$"Sprite already in collection: '{sprite.GetBestName()}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
						$"for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID})");
				}
			}
		}
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		ITexture2D texture = Asset;
		if (m_convert)
		{
			ITextureImporter importer = ImporterFactory.GenerateTextureImporter(container, texture);
			AddSprites(container, importer, ((SpriteInformationObject?)Asset.MainAsset)!.Sprites);
			return importer;
		}
		else
		{
			return ImporterFactory.GenerateIHVImporter(container, texture);
		}
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return AssetExporter.Export(container, Asset, filePath, fileSystem);
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		if (m_convert)
		{
			return ((TextureAssetExporter)AssetExporter).ImageExportFormat.GetFileExtension();
		}
		return base.GetExportExtension(asset);
	}

	protected override long GenerateExportID(IUnityObjectBase asset)
	{
		long exportID = ExportIdHandler.GetMainExportID(asset, m_nextExportID);
		m_nextExportID += 2;
		return exportID;
	}

	private void AddSprites(IExportContainer container, ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?>? textureSpriteInformation)
	{
		if (textureSpriteInformation == null || textureSpriteInformation.Count == 0)
		{
			importer.SpriteModeE = SpriteImportMode.Single;
			importer.SpriteExtrude = 1;
			importer.SpriteMeshType = (int)SpriteMeshType.FullRect;//See pull request #306
			importer.Alignment = (int)SpriteAlignment.Center;
			if (importer.Has_SpritePivot())
			{
				importer.SpritePivot.SetValues(0.5f, 0.5f);
			}
			importer.SpritePixelsToUnits = 100.0f;
		}
		else if (textureSpriteInformation.Count == 1)
		{
			ISprite sprite = textureSpriteInformation.Keys.First();
			ITexture2D texture = Asset;
			if (sprite.Rect == sprite.RD.TextureRect && sprite.Name == texture.Name)
			{
				importer.SpriteModeE = SpriteImportMode.Single;
			}
			else
			{
				importer.SpriteModeE = SpriteImportMode.Multiple;
				importer.TextureTypeE = TextureImporterType.Sprite;
			}
			importer.SpriteExtrude = sprite.Extrude;
			importer.SpriteMeshType = (int)sprite.RD.GetMeshType();
			importer.Alignment = (int)SpriteAlignment.Custom;
			if (importer.Has_SpritePivot() && sprite.Has_Pivot())
			{
				importer.SpritePivot.CopyValues(sprite.Pivot);
			}
			if (importer.Has_SpriteBorder() && sprite.Has_Border())
			{
				importer.SpriteBorder.CopyValues(sprite.Border);
			}
			importer.SpritePixelsToUnits = sprite.PixelsToUnits;
			importer.TextureTypeE = TextureImporterType.Sprite;
			if (m_exportSprites)
			{
				AddSpriteSheet(importer, textureSpriteInformation);
				AddIDToName(container, importer, textureSpriteInformation);
			}
		}
		else
		{
			ISprite sprite = textureSpriteInformation.Keys.First();
			importer.TextureTypeE = TextureImporterType.Sprite;
			importer.SpriteModeE = SpriteImportMode.Multiple;
			importer.SpriteExtrude = sprite.Extrude;
			importer.SpriteMeshType = (int)sprite.RD.GetMeshType();
			importer.Alignment = (int)SpriteAlignment.Center;
			if (importer.Has_SpritePivot())
			{
				importer.SpritePivot.SetValues(0.5f, 0.5f);
			}
			importer.SpritePixelsToUnits = sprite.PixelsToUnits;
			importer.TextureTypeE = TextureImporterType.Sprite;
			if (m_exportSprites)
			{
				AddSpriteSheet(importer, textureSpriteInformation);
				AddIDToName(container, importer, textureSpriteInformation);
			}
		}
	}

	private static void AddSpriteSheet(ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
	{
		if (!importer.Has_SpriteSheet())
		{
		}
		else if (importer.SpriteModeE == SpriteImportMode.Single)
		{
			KeyValuePair<ISprite, ISpriteAtlas?> kvp = textureSpriteInformation.First();
			ISpriteMetaData smeta = SpriteMetaData.Create(kvp.Key.Collection.Version);
			smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
			importer.SpriteSheet.CopyFromSpriteMetaData(smeta);
		}
		else
		{
			AccessListBase<ISpriteMetaData> spriteSheetSprites = importer.SpriteSheet.Sprites;
			foreach (KeyValuePair<ISprite, ISpriteAtlas?> kvp in textureSpriteInformation)
			{
				ISpriteMetaData smeta = spriteSheetSprites.AddNew();
				smeta.FillSpriteMetaData(kvp.Key, kvp.Value);
				if (smeta.Has_InternalID())
				{
					smeta.InternalID = ExportIdHandler.GetInternalId();
				}
			}
		}
	}

	private void AddIDToName(IExportContainer container, ITextureImporter importer, IReadOnlyDictionary<ISprite, ISpriteAtlas?> textureSpriteInformation)
	{
		if (importer.SpriteModeE == SpriteImportMode.Multiple)
		{
			if (importer.Has_InternalIDToNameTable())
			{
				foreach (ISprite sprite in textureSpriteInformation.Keys)
				{
					try
					{
						long exportID = GetExportID(container, sprite);
						
						ISpriteMetaData? smeta = null;
						try
						{
							smeta = importer.SpriteSheet.GetSpriteMetaData(sprite.Name);
						}
						catch (KeyNotFoundException)
						{
							foreach (ISpriteMetaData metaData in importer.SpriteSheet.Sprites)
							{
								if (metaData.Name == sprite.Name)
								{
									smeta = metaData;
									break;
								}
							}
						}
						
						if (smeta is not null)
						{
							smeta.InternalID = exportID;
							AssetPair<AssetPair<int, long>, Utf8String> pair = importer.InternalIDToNameTable.AddNew();
							pair.Key.Key = (int)ClassIDType.Sprite;
							pair.Key.Value = exportID;
							pair.Value = sprite.Name;
						}
						else
						{
							System.Diagnostics.Debug.WriteLine(
								$"Warning: Could not find sprite meta data for sprite '{sprite.Name}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
								$"in texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}). " +
								$"Skipping InternalIDToNameTable entry for this sprite.");
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(
							$"Warning: Error processing sprite '{sprite.Name}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
							$"for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}): {ex.Message}");
					}
				}
			}
			else if (importer.Has_FileIDToRecycleName_AssetDictionary_Int64_Utf8String())
			{
				foreach (ISprite sprite in textureSpriteInformation.Keys)
				{
					try
					{
						long exportID = GetExportID(container, sprite);
						importer.FileIDToRecycleName_AssetDictionary_Int64_Utf8String.Add(exportID, sprite.Name);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(
							$"Warning: Error processing sprite '{sprite.Name}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
							$"for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}): {ex.Message}");
					}
				}
			}
			else if (importer.Has_FileIDToRecycleName_AssetDictionary_Int32_Utf8String())
			{
				foreach (ISprite sprite in textureSpriteInformation.Keys)
				{
					try
					{
						long exportID = GetExportID(container, sprite);
						importer.FileIDToRecycleName_AssetDictionary_Int32_Utf8String.Add((int)exportID, sprite.Name);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine(
							$"Warning: Error processing sprite '{sprite.Name}' (Type: {sprite.GetType().Name}, ClassID: {sprite.ClassID}, PathID: {sprite.PathID}) " +
							$"for texture '{Asset.GetBestName()}' (Type: {Asset.GetType().Name}, ClassID: {Asset.ClassID}, PathID: {Asset.PathID}): {ex.Message}");
					}
				}
			}
		}
	}

	/// <summary>
	/// If exportSprites is false, we do not generate sprite sheet into texture importer,
	/// yet we still need the sprites to properly set other texture importer settings.
	/// </summary>
	private readonly bool m_exportSprites;
	private readonly bool m_convert = true;
	private uint m_nextExportID = 0;
	
	/// <summary>
	/// Check if an asset is already in the collection by comparing AssetInfo rather than object references.
	/// This prevents duplicate assets that have the same logical identity but different object references.
	/// </summary>
	private bool ContainsByAssetInfo(IUnityObjectBase asset)
	{
		if (Asset.AssetInfo == asset.AssetInfo)
		{
			return true;
		}
		
		if (Contains(asset))
		{
			return true;
		}
		
		foreach (IUnityObjectBase existingAsset in Assets)
		{
			if (existingAsset.AssetInfo == asset.AssetInfo)
			{
				return true;
			}
		}
		
		return false;
	}
}
