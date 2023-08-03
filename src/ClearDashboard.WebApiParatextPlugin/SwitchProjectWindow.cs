using System;
using System.IO;
using System.Windows.Forms;

namespace ClearDashboard.WebApiParatextPlugin
{
    public partial class SwitchProjectWindow : Form
    {
        public SwitchProjectWindow()
        {
            InitializeComponent();


            // load in the animated gif
            string appPath = Path.GetDirectoryName(Application.ExecutablePath);
            string imagePath = appPath + @"\plugins\ClearDashboardWebApiPlugin\ChangeParatextProject.gif";
            if (File.Exists(imagePath))
            {
                pb.ImageLocation = imagePath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
