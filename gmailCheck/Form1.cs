using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gmailCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bunifuFlatButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Browse Text Files";

            openFileDialog1.CheckFileExists = true;
            openFileDialog1.CheckPathExists = true;

            openFileDialog1.DefaultExt = "txt";
            openFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ReadOnlyChecked = true;
            openFileDialog1.ShowReadOnly = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog1.FileName;

                dataGridView1.Rows.RemoveAt(0);

                int i = 0;

                foreach (var line in File.ReadAllLines(filePath))
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = line;
                    dataGridView1.Rows[i].Cells[1].Value = "No proxys added";
                    dataGridView1.Rows[i].Cells[2].Value = "Not checked";
                    i++;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            dataGridView1.Rows[0].Cells[0].Value = "Please add a email list";
            dataGridView1.Rows[0].Cells[1].Value = "No proxys added";
            dataGridView1.Rows[0].Cells[2].Value = "Not checked";
        }

        private void bunifuFlatButton2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount <= 1)
            {
                MessageBox.Show("Please upload a googlemail list containing atleast 1 email address.", "Gmail checker Error", MessageBoxButtons.OK);
            }
            else
            {
                new Thread(new ThreadStart(check)) { IsBackground = true }.Start();
            }
        }

        private void check()
        {
            bool status = false;
            bool cancel = (bool)this.Invoke((Func<bool, bool>)DoCheapGuiAccess, status);
            int x = 0;

            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                string address = dataGridView1.Rows[i].Cells[0].Value.ToString();
                string proxy = dataGridView1.Rows[i].Cells[1].Value.ToString();
                string[] words = address.Split('@');

                try
                {
                    string post = "{\"input01\":{\"Input\":\"GmailAddress\",\"GmailAddress\":\"" + words[0] + "\",\"FirstName\":\"\",\"LastName\":\"\"},\"Locale\":\"de\"}";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://accounts.google.com/InputValidator?resource=SignUp&service=mail") as HttpWebRequest;
                    request.Proxy = new WebProxy(proxy);
                    request.Method = "POST";
                    request.Accept = "*/*";
                    request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
                    request.Host = "accounts.google.com";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
                    request.ContentType = "application/json; charset=utf-8";
                    request.KeepAlive = true;
                    request.Referer = "https://accounts.google.com/SignUp?service=mail&continue=https%3A%2F%2Fmail.google.com%2Fmail%2F&ltmpl=default";

                    byte[] postBytes = Encoding.ASCII.GetBytes(post);
                    request.ContentLength = postBytes.Length;
                    Stream requestStream = request.GetRequestStream();

                    requestStream.Write(postBytes, 0, postBytes.Length);
                    requestStream.Close();

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    string html = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    if (html.Contains("wird bereits verwendet"))
                    {
                        dataGridView1.Rows[i].Cells[2].Value = "Exists.";
                    }
                    else if(html.Contains("verwenden Sie 6 bis 30"))
                    {
                        dataGridView1.Rows[i].Cells[2].Value = "Less than 6 charackters.";
                    }
                    else
                    {
                        dataGridView1.Rows[i].Cells[2].Value = "Does not exist.";
                    }
                }
                catch
                {
                    dataGridView1.Rows[i].Cells[2].Value = "Error.";
                }
            }

            bool finished = true;
            bool finsihed = (bool)this.Invoke((Func<bool, bool>)DoCheapGuiAccess, finished);
        }

        bool DoCheapGuiAccess(bool status)
        {
            if (status == false)
            {
                bunifuCustomLabel8.Text = "Running.";
                return true;
            }
            else
            {
                bunifuCustomLabel8.Text = "Finished.";
                return true;
            }
        }

        private async void bunifuFlatButton3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows[0].Cells[1].Value.ToString() == "Undefined")
            {
                MessageBox.Show("Please lookup a gmail account before exporting data!", "Gmail checker Info", MessageBoxButtons.OK);
            }
            else
            {
                try
                {
                    await ExportUserData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task ExportUserData()
        {
            string file_name = AppDomain.CurrentDomain.BaseDirectory + "/export.txt";

            TextWriter writer = new StreamWriter(file_name);
            string line = "";

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    line += dataGridView1.Columns[j].HeaderText + ": " + dataGridView1.Rows[i].Cells[j].Value.ToString() + " | ";
                }

                line = line.Remove(line.Length - 3);
                await writer.WriteAsync(line);
                line = "";
                await writer.WriteLineAsync("");
            }

            writer.Close();
            MessageBox.Show("Data exported to the following text file: " + file_name + "!", "Gmail checker Info", MessageBoxButtons.OK);
        }

        private void bunifuFlatButton4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.InitialDirectory = @"C:\";
            openFileDialog2.Title = "Browse Text Files";

            openFileDialog2.CheckFileExists = true;
            openFileDialog2.CheckPathExists = true;

            openFileDialog2.DefaultExt = "txt";
            openFileDialog2.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog2.FilterIndex = 2;
            openFileDialog2.RestoreDirectory = true;

            openFileDialog2.ReadOnlyChecked = true;
            openFileDialog2.ShowReadOnly = true;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog2.FileName;
                string[] lines = File.ReadAllLines(filePath);

                bunifuCustomLabel4.Text = lines.Length.ToString();

                Random rand = new Random();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    string line = lines[rand.Next(lines.Length)];
                    
                    row.Cells[1].Value = line;
                }
            }
        }
    }
}
