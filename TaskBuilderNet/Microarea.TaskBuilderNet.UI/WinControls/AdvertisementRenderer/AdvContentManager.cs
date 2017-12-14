using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer
{
	/// <summary>
	/// AdvContentManager.
	/// </summary>
	//=========================================================================
	public class AdvContentManager : DefaultContentManager
	{
		private ClosableControl content;

		//---------------------------------------------------------------------
		public AdvContentManager(ClosableControl content)
		{
			this.content = content;
			this.StartColor = Color.White;
			this.EndColor = Color.Lavender;
		}

		//---------------------------------------------------------------------
		protected override Size GetContentArea(ref int titleYOffset, Balloon container)
		{
			titleYOffset = 0;
			return content.Size;
		}

       
        
        //---------------------------------------------------------------------
		protected override void AddContent(
			Balloon.Position p,
			Graphics bmpGraphics,
			Size contentArea,
			int titleYOffset, 
			Balloon container
			)
		{
            if (content == null || content.IsDisposed)
                content = new AdvRendererManager();//ad un cliente capitava l'anomalia TB://Document.PaiNet.PaiCore.Documents.Bugs?bugid:20343

			int y = YOffset;
			if ((p & Balloon.Position.Down) == Balloon.Position.Down)
				y += MinimumArrowHeight;

			content.Location = new Point(XOffset, y);
			container.Controls.Add(content);
		}
	}
}
