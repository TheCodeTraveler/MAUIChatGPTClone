#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Items;
using UIKit;

namespace AiChatClient.Maui;

public class CollectionViewNoScrollBarsHandler : CollectionViewHandler
{
	protected override void ConnectHandler(UIView platformView)
	{
		base.ConnectHandler(platformView);

		if (platformView is UICollectionView collectionView)
		{
			collectionView.ShowsVerticalScrollIndicator = false;
			collectionView.ShowsHorizontalScrollIndicator = false;
		}
	}
}
#endif