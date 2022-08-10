using System;
using System.Windows.Forms;

namespace DemoWinForm
{
    public partial class ConvertProjectionAndCoordinatesForm : Form
    {
        public ConvertProjectionAndCoordinatesForm()
        {
            InitializeComponent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            MainSplitContainer.SplitterDistance = (Width - MainSplitContainer.SplitterWidth) / 2;
        }
    }
}