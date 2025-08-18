using AssetRipper.Assets;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;
using AssetRipper.Import.Logging;

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
			if (asset.MainAsset is null)
			{
				asset.MainAsset = this;
			}
		}
	}

	protected PPtr AssetToPPtr(IUnityObjectBase? asset) => Collection.ForceCreatePPtr(asset);
}
