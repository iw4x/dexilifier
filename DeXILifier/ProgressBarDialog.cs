namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class ProgressBarDialog : Form
    {
        private Form owner;

        public ProgressBarDialog()
        {
            InitializeComponent();
        }

        public void HideProgressDialog()
        {
            Invoke((Action)(()=>
            {
                Hide();
                owner.Enabled = true;
                Dispose();
            }));
        }

        public static ProgressBarDialog ShowProgressDialog(Form owner, Func<float> progressGetter01=null)
        {
            var dial = new ProgressBarDialog();

            dial.progressBar1.Style = progressGetter01 == null ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;

            void OnTick(object sender, System.Timers.ElapsedEventArgs e)
            {
                dial.Invoke((Action)(() =>
                {
                    dial.progressBar1.Value = (int)(progressGetter01() * 100);
                }));
            }

            var timer = new System.Timers.Timer();
            if (progressGetter01 != null)
            {
                timer.Interval = 100; // every second
                timer.Elapsed += OnTick;
            }

            owner.Enabled = false;
            dial.owner = owner;
            dial.Show(owner);

            if (progressGetter01 != null)
            {
                timer.Elapsed -= OnTick;
            }

            return dial;
        }
    }
}
