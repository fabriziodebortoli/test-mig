using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Aga.Controls.Properties;
using System.Diagnostics;
using System.IO;

namespace Aga.Controls.Tree.NodeControls
{
	public class NodeStateIcon: NodeIcon
	{
		private Image _empty;
        private Hashtable _images;

        //4112
        public void AddImage(String key, String path)
        {
            try
            {
                if (_images[key.ToUpper()] == null)
                {
                    string diectory = Path.GetDirectoryName(path);
                    if (Directory.Exists(diectory))
                    {
                        if (File.Exists(path))
                        {
                            Image img = Bitmap.FromFile(path as String);
                            if (img != null)
                            {
                                // DPI calculate
                                float dpiScale = 1;
                                using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
                                {
                                    dpiScale = graphics.DpiY / 96;
                                }
                                int wNewImg = (int)(img.Width * dpiScale);
                                int hNewImg = (int)(img.Height * dpiScale);
                               
                                //Bitmap bitmap = new Bitmap(img);
                                Bitmap bitmap = new Bitmap(img, new Size(wNewImg, hNewImg));

                                if (bitmap != null)
                                    _images.Add(key, bitmap);
                            }
                        }   
                    }
                }
            }
            catch (Exception)
            { 

            }
        }

        //
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (Bitmap bmp in _images.Values)
					bmp.Dispose();
				_images.Clear();
				if (_empty != null)
					_empty.Dispose();
			}
			base.Dispose(disposing);
		}
		public NodeStateIcon()
		{
			_empty = MakeTransparent(Resources.empty);
            _images = new Hashtable();
        }

		private static Image MakeTransparent(Bitmap bitmap)
		{
			bitmap.MakeTransparent(bitmap.GetPixel(0,0));
			return bitmap;
		}

		protected override Image GetIcon(TreeNodeAdv node)
		{
            Image icon = base.GetIcon(node);

            //4112 - generalizzare
            if (icon == null && !string.IsNullOrEmpty((node.Tag as Node).NodeImageKey))
            {
				if (_images[(node.Tag as Node).NodeImageKey.ToUpper()] != null)
				{
					(node.Tag as Node).Icon = (Image)_images[(node.Tag as Node).NodeImageKey.ToUpper()];
					return (Image)_images[(node.Tag as Node).NodeImageKey.ToUpper()];
				}
				else
				{
					(node.Tag as Node).Icon = _empty;
					return _empty;
				}
            }
            
            //

            return icon;
        }
	}
}
