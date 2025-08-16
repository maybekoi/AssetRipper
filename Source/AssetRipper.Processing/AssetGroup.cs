using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.Processing;

public abstract class AssetGroup : UnityObjectBase
{
	protected AssetGroup(AssetInfo assetInfo) : base(assetInfo)
	{
	}

	public abstract IEnumerable<IUnityObjectBase> Assets { get; }

	public virtual void SetMainAsset()
	{
		MainAsset = this;
		foreach (IUnityObjectBase asset in Assets)
		{
			if (asset.MainAsset is not null && asset.MainAsset != this)
			{
				System.Diagnostics.Debug.WriteLine(
					$"Warning: Asset '{asset.GetBestName()}' (Type: {asset.GetType().Name}, ClassID: {asset.ClassID}, PathID: {asset.PathID}) " +
					$"in collection '{asset.Collection.Name}' (Bundle: {asset.Collection.Bundle.Name}) " +
					$"already has a main asset assigned: '{asset.MainAsset.GetBestName()}' (Type: {asset.MainAsset.GetType().Name}, ClassID: {asset.MainAsset.ClassID}, PathID: {asset.MainAsset.PathID}) " +
					$"in collection '{asset.MainAsset.Collection.Name}' (Bundle: {asset.MainAsset.Collection.Bundle.Name}). " +
					$"Skipping assignment of '{(this as IUnityObjectBase)?.GetBestName() ?? this.GetType().Name}' (Type: {this.GetType().Name}, ClassID: {this.ClassID}, PathID: {this.PathID}) " +
					$"in collection '{this.Collection.Name}' (Bundle: {this.Collection.Bundle.Name}) as main asset.");
				continue;
			}
			asset.MainAsset = this;
		}
	}

	protected PPtr AssetToPPtr(IUnityObjectBase? asset) => Collection.ForceCreatePPtr(asset);
}
