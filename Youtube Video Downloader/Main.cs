using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using YoutubeExtractor;
using YoutubeSearch;
using System.Net;

namespace Youtube_Video_Downloader
{

    public partial class Main : Form
    {
        int querypages = 1; 

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;

            this.WindowState = FormWindowState.Maximized;

              }

        private void Downloader_DownloadProgressChanged(object sender, ProgressEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Value = (int)e.ProgressPercentage;
                lbpercent.Text = $"{string.Format("{0:0.##}", e.ProgressPercentage)}%";
                progressBar1.Update();

                if (lbpercent.Text == "100%")
                {
                    button1.Enabled = true;
                    MessageBox.Show("The video has been downloaded successfully!", "Downloaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }));
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Download ()
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;
                button1.Enabled = false;

                IEnumerable<VideoInfo> videos = DownloadUrlResolver.GetDownloadUrls(txtURL.Text);
                VideoInfo video = videos.First(p => p.VideoType == VideoType.Mp4 && p.Resolution == Convert.ToInt32(comboBox1.Text));
                if (video.RequiresDecryption)
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                VideoDownloader downloader = new VideoDownloader(video, Path.Combine(folderBrowserDialog1.SelectedPath + "\\", video.Title + video.VideoExtension));
                lbTitle.Text = video.Title;
                downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                Thread thread = new Thread(() => { downloader.Execute(); }) { IsBackground = true };
                thread.Start();
            }
        }

      
        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);

            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebReponse.GetResponseStream())
                {
                    return Image.FromStream(stream);
                }
            }
        }

        private void dataGridView1_RowHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            txtURL.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtURL.Text = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();

            var items = new VideoSearch();

            int i = 1;

            querypages = int.Parse(comboBox2.SelectedItem.ToString());

            foreach (var item in items.SearchQuery(textBox2.Text, querypages))
            {
                dataGridView1.Rows.Add(item.Title, item.Author, item.Description, item.Duration, item.Url, GetImageFromUrl(item.Thumbnail));
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (txtURL.Text != "")
            {
                if (txtURL.Text.StartsWith("https://"))
                {
                    txtURL.Text = txtURL.Text.Replace("https://", "http://");

                    Download();


                }
                else
                {
                    if (txtURL.Text.StartsWith("http://"))
                    {
                        Download();
                    }
                }

            }
            else
            {
                MessageBox.Show("Please insert a URL.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
    }
}
