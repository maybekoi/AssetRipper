using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Processing.Textures;

public sealed partial class SpriteProcessor
{
	private readonly struct ObjectFactory
	{
		private readonly ProcessedAssetCollection processedCollection;
		private readonly Dictionary<AssetInfo, SpriteInformationObject> dictionary = new();

		public IEnumerable<SpriteInformationObject> Assets => dictionary.Values;

		public ObjectFactory(GameData gameData)
		{
			processedCollection = gameData.AddNewProcessedCollection("Sprite Data Storage");
		}

		public SpriteInformationObject GetOrCreate(ITexture2D texture)
		{
			if (!dictionary.TryGetValue(texture.AssetInfo, out SpriteInformationObject? result))
			{
				result = MakeSpriteInformationObject(texture);
				dictionary.Add(texture.AssetInfo, result);
			}
			return result;
		}

		SpriteInformationObject MakeSpriteInformationObject(ITexture2D texture)
		{
			return processedCollection.CreateAsset(-1, texture, static (assetInfo, texture) => new SpriteInformationObject(assetInfo, texture));
		}
	}
}
