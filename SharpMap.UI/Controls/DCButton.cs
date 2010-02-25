using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SharpMap.Controls
{
    public partial class DCButton : Button
    {
        private MouseEventArgs mArgs;

        public DCButton()
        {
            InitializeComponent();
            clickTimer.Interval = 250;
        }

        new public event MouseEventHandler MouseDoubleClick;
        new public event EventHandler DoubleClick;
        new public event MouseEventHandler MouseClick;
        new public event EventHandler Click;

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            mArgs = mevent;

            if (clickTimer.Enabled == false)
                clickTimer.Start();
            else
            {
                clickTimer.Stop();
                if (DoubleClick != null)
                    DoubleClick(this, null);
                if (MouseDoubleClick != null)
                    MouseDoubleClick(this, mArgs);

            }
        }

        private void clickTimer_Tick(object sender, EventArgs e)
        {
            clickTimer.Stop();
            if (Click != null)
                Click(this, e);
            if (MouseClick != null)
                MouseClick(this, mArgs);
        }
    }
}
