using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace EADesigner.ActivitiesDesigner {
	// Interaction logic for EAActivityControl.xaml
	public partial class EAActivityControlApprove {


		private void EAActivityControl_Loaded(object sender, System.Windows.RoutedEventArgs e) {

		}
		
		
		public EAActivityControlApprove() {
            InitializeComponent();
			Loaded += EAActivityControl_Loaded;
            //Con il codice seguente imposto una nuova icona con il logo Microarea.
            //Quindi può essere rimossa la riga corrispondente dallo xaml, che invece impostava quella di default
            this.Icon = new DrawingBrush
            {
                Drawing = new ImageDrawing
                {
                    Rect = new System.Windows.Rect(0, 0, 16, 16),
                    ImageSource = Utilities.ConvertToBitmapIntoImageSource(EADesigner.Properties.Resources.MicroLogo)
                }
            };
		}
	}
}
