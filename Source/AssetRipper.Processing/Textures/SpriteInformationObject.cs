using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Traversal;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using System.Diagnostics;

namespace AssetRipper.Processing.Textures;

public sealed class SpriteInformationObject : AssetGroup, INamed
{
	public SpriteInformationObject(AssetInfo assetInfo, ITexture2D texture) : base(assetInfo)
	{
		Texture = texture ?? throw new ArgumentNullException(nameof(texture));
		
		if (texture.Collection is null)
		{
			System.Diagnostics.Debug.WriteLine(
				$"Warning: SpriteInformationObject created for texture '{(texture as IUnityObjectBase)?.GetBestName() ?? texture.GetType().Name}' (Type: {texture.GetType().Name}, ClassID: {texture.ClassID}, PathID: {texture.PathID}) " +
				$"but texture has no collection assigned. This may cause export issues.");
		}
	}

	public ITexture2D Texture { get; }
	public IReadOnlyDictionary<ISprite, ISpriteAtlas?> Sprites => dictionary;
	private readonly Dictionary<ISprite, ISpriteAtlas?> dictionary = new();

	Utf8String INamed.Name
	{
		get => Texture.Name;
		set { }
	}

	public override IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			yield return Texture;
			foreach ((ISprite sprite, ISpriteAtlas? atlas) in dictionary)
			{
				yield return sprite;
				if (atlas is not null)
				{
					yield return atlas;
				}
			}
		}
	}

	public override void WalkStandard(AssetWalker walker)
	{
		if (walker.EnterAsset(this))
		{
			this.WalkPPtrField(walker, Texture);
			walker.DivideAsset(this);
			this.WalkDictionaryPPtrField(walker, Sprites);
			walker.ExitAsset(this);
		}
	}

	public override IEnumerable<(string, PPtr)> FetchDependencies()
	{
		yield return (nameof(Texture), AssetToPPtr(Texture));
		foreach ((ISprite sprite, ISpriteAtlas? atlas) in dictionary)
		{
			yield return (nameof(Sprites) + "[].Key", AssetToPPtr(sprite));
			if (atlas is not null)
			{
				yield return (nameof(Sprites) + "[].Value", AssetToPPtr(atlas));
			}
		}
	}

	internal void AddToDictionary(ISprite sprite, ISpriteAtlas? atlas)
	{
		if (dictionary.TryGetValue(sprite, out ISpriteAtlas? mappedAtlas))
		{
			if (mappedAtlas is null)
			{
				dictionary[sprite] = atlas;
			}
			else if (atlas is not null && atlas != mappedAtlas)
			{
				throw new Exception($"{nameof(atlas)} is not the same as {nameof(mappedAtlas)}");
			}
		}
		else
		{
			dictionary.Add(sprite, atlas);
		}
	}

	public override void SetMainAsset()
	{
		if (Texture.MainAsset is not null && Texture.MainAsset != this)
		{
			System.Diagnostics.Debug.WriteLine(
				$"Warning: Texture '{(Texture as IUnityObjectBase)?.GetBestName() ?? Texture.GetType().Name}' (Type: {Texture.GetType().Name}, ClassID: {Texture.ClassID}, PathID: {Texture.PathID}) " +
				$"in collection '{Texture.Collection.Name}' (Bundle: {Texture.Collection.Bundle.Name}) " +
				$"already has a main asset assigned: '{(Texture.MainAsset as IUnityObjectBase)?.GetBestName() ?? Texture.MainAsset.GetType().Name}' (Type: {Texture.MainAsset.GetType().Name}, ClassID: {Texture.MainAsset.ClassID}, PathID: {Texture.MainAsset.PathID}) " +
				$"in collection '{Texture.MainAsset.Collection.Name}' (Bundle: {Texture.MainAsset.Collection.Bundle.Name}). " +
				$"Skipping main asset assignment for SpriteInformationObject '{(this as IUnityObjectBase)?.GetBestName() ?? this.GetType().Name}' (Type: {this.GetType().Name}, ClassID: {this.ClassID}, PathID: {this.PathID}) " +
				$"in collection '{this.Collection.Name}' (Bundle: {this.Collection.Bundle.Name}).");
			return;
		}
		
		base.SetMainAsset();
	}
}
