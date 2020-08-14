using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Media;
using MySql.Data.Common;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        VideoCapture capture;
        Mat frame;
        Bitmap image;
        private Thread camera;
        int isCameraRunning = 0;
        Font arialFont = new Font("Arial", 20);
        Rect[] faces;
        float[] val = new float[10];
        float standard = 110;
        bool flip = false;
        bool startMeasure = false;
        SoundPlayer simpleSound = new SoundPlayer(@"경보음2.wav");
        int framecnt = 0;
        int alarmcnt = 0;
        MySqlConnection conn;
        Mat good;
        Mat bad;
        DataTable dtRecord;
        string today = DateTime.Now.ToString("yyyy-MM-dd");


        public void OpenConnection()
        {
            string strconn = "Server=localhost;Database=edu;Uid=root;Pwd=1234;";
            conn = new MySqlConnection(strconn);
            conn.Open();
        }
        public void CloseConnection()
        {
            if (conn != null)
                conn.Close();
        }
        private void CaptureCamera()
        {

            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
        }

        private void CaptureCameraCallback()
        {
            frame = new Mat();
            capture = new VideoCapture();
            capture.FrameWidth = 680;
            capture.FrameHeight = 480;
            capture.Open(0);
            String filenameFaceCascade = "haarcascade_frontalface_alt.xml";
            CascadeClassifier faceCascade = new CascadeClassifier();

            if (!faceCascade.Load(filenameFaceCascade))
            {
                MessageBox.Show("haar error");
                return;
            }
            while (isCameraRunning == 1)
            {
                if (framecnt > 1000000) framecnt = 0;
                framecnt++;
                capture.Read(frame);
                if (flip)
                    Cv2.Flip(frame, frame, FlipMode.Y);
                if (!frame.Empty())
                {
                    faces = faceCascade.DetectMultiScale(frame);
                    if (faces.Length > 0 && startMeasure)
                    {
                        for (int i = 0; i < faces.Length; i++)
                        {

                            val[i] = (float)Cv2.Mean(frame.SubMat(faces[i]));
                            if (val[i] > standard)
                            {
                                Cv2.Rectangle(frame, faces[i], Scalar.Red); // add rectangle to the image
                            }
                            else
                            {
                                Cv2.Rectangle(frame, faces[i], Scalar.Green); // add rectangle to the image
                            }
                            //textBox1.Text = textBox1.Text + "\tfaces : " + faces[i];
                        }

                        image = BitmapConverter.ToBitmap(frame);

                        using (Graphics graphics = Graphics.FromImage(image))
                        {
                            for (int i = 0; i < faces.Length; i++)
                            {
                                PointF p = new PointF(faces[i].X + faces[i].Width / 2 - 10, faces[i].Y - 15);
                                if (val[i] > standard)
                                {
                                    graphics.DrawString(val[i].ToString(), arialFont, Brushes.Red, p);
                                }
                                else
                                {
                                    graphics.DrawString(val[i].ToString(), arialFont, Brushes.Green, p);
                                }
                            }

                        }
                        if (framecnt % 10 == 0)
                        {
                            for (int i = 0; i < faces.Length; i++)
                            {
                                try
                                {
                                    Mat dst = frame.SubMat(faces[i]);
                                    OpenCvSharp.Size s = new OpenCvSharp.Size(64, 64);
                                    Mat resized = dst.Resize(s);
                                    DateTime time = DateTime.Now;

                                    OpenConnection();
                                    MySqlCommand command = new MySqlCommand("", conn);
                                    command.CommandText = "INSERT INTO data VALUES(@Date, @Time, @Face, @Stand, @Measure, @Warn)";
                                    byte[] data = resized.ToBytes();
                                    command.Parameters.AddWithValue("@Date", time.ToString("yyyy/MM/dd"));
                                    command.Parameters.AddWithValue("@Time", time.ToString("hh:mm:ss"));
                                    MySqlParameter blob = new MySqlParameter("@Face", MySqlDbType.Blob, data.Length);
                                    blob.Value = data;
                                    command.Parameters.Add(blob);
                                    command.Parameters.AddWithValue("@Stand", standard);
                                    command.Parameters.AddWithValue("@Measure", val[i]);

                                    if (val[i] > standard)
                                    {
                                        byte[] icon = bad.ToBytes();
                                        MySqlParameter para = new MySqlParameter("@Warn", MySqlDbType.Blob, icon.Length);
                                        para.Value = icon;
                                        command.Parameters.Add(para);
                                    }
                                    else
                                    {
                                        byte[] icon = good.ToBytes();
                                        MySqlParameter para = new MySqlParameter("@Warn", MySqlDbType.Blob, icon.Length);
                                        para.Value = icon;
                                        command.Parameters.Add(para);
                                    }
                                    command.ExecuteNonQuery();

                                    if (val[i] > standard)
                                    {
                                        simpleSound.Play();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }

                            }
                            DisplayData(string.Format("SELECT * FROM data WHERE Date=\"{0}\"", today));
                        }
                    }
                    else
                    {
                        image = BitmapConverter.ToBitmap(frame);
                    }
                    pictureBox1.Image = image;
                }
                image = null;
            }

        }

        public Form1()
        {
            InitializeComponent();
            Mat tmpgood = Cv2.ImRead("../../good.png");
            Mat tmpbad = Cv2.ImRead("../../bad.png");
            OpenCvSharp.Size resize = new OpenCvSharp.Size(64, 64);
            good = tmpgood.Resize(resize);
            bad = tmpbad.Resize(resize);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text.Equals("카메라 On"))
            {
                CaptureCamera();
                button1.Text = "카메라 Off";
                isCameraRunning = 1;
            }
            else
            {
                button1.Text = "카메라 On";
                isCameraRunning = 0;

                if (capture.IsOpened())
                {
                    capture.Release();
                }

                pictureBox1.Image = null;
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (isCameraRunning == 0)
            {
                MessageBox.Show("카메라를 켜주세요.");
            }
            else
            {
                if (button2.Text.Equals("거울모드 On"))
                {
                    button2.Text = "거울모드 Off";
                    flip = true;
                }
                else
                {
                    button2.Text = "거울모드 On";
                    flip = false;
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            simpleSound.Stop();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (isCameraRunning == 0)
            {
                MessageBox.Show("카메라를 켜주세요.");
            }
            else
            {
                if (button4.Text.Equals("측정시작"))
                {
                    button4.Text = "측정종료";
                    startMeasure = true;

                }
                else
                {
                    simpleSound.Stop();
                    button4.Text = "측정시작";
                    startMeasure = false;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dtRecord = new DataTable();
            Form3 f = new Form3();
            if (f.ShowDialog() == DialogResult.OK)
            {
            }
            else
            {
                Application.Exit();
            }
            f.Close();
            OpenConnection();
            countAlarm(string.Format("SELECT * FROM data WHERE Stand<Measure AND Date=\"{0}\"", today));
            ReadDate();
            DisplayData(string.Format("SELECT * FROM data WHERE Date=\"{0}\"", today));

            textBox2.Text = DateTime.Now.ToString();
            label6.Text = string.Format("Count : {0}   Alarm : {1}", dataGridView1.Rows.Count, alarmcnt);
        }
        private void Timer1_Tick(object sender, EventArgs e)
        {
            textBox2.Text = DateTime.Now.ToString();
        }
        public void DisplayData(string query)
        {
            //dataGridView1.Rows.Clear();
            try
            {
                MySqlCommand sqlCmd = new MySqlCommand();
                sqlCmd.Connection = conn;
                sqlCmd.CommandType = CommandType.Text;
                sqlCmd.CommandText = query;
                MySqlDataAdapter sqlDataAdap = new MySqlDataAdapter(sqlCmd);
                dtRecord.Clear();
                sqlDataAdap.Fill(dtRecord);
                //sqlDataAdap.Update(dtRecord);
                dataGridView1.DataSource = dtRecord;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            standard = float.Parse(textBox1.Text);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = (string)comboBox2.SelectedItem;
            switch (comboBox1.SelectedIndex)
            {
                case 0: //전체
                    countAlarm("SELECT * FROM data WHERE Stand<Measure");
                    DisplayData("SELECT * FROM data");
                    break;
                case 1: //정상
                    countAlarm(string.Format("SELECT * FROM data WHERE Stand<Measure AND Date=\"{0}\"", str));
                    DisplayData(string.Format("SELECT * FROM data WHERE Stand>Measure AND Date=\"{0}\"", str));
                    break;
                case 2: //경고
                    countAlarm(string.Format("SELECT * FROM data WHERE Stand<Measure AND Date=\"{0}\"", str));
                    DisplayData(string.Format("SELECT * FROM data WHERE Stand<Measure AND Date=\"{0}\"", str));
                    break;
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            label6.Text = string.Format("Count : {0}   Alarm : {1}", dataGridView1.Rows.Count, alarmcnt);
        }
        private void 프로그램정보ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 info = new Form2();

            info.ShowDialog();
        }

        private void 로그아웃ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Restart();
        }

        private void 프로그램종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void ReadDate()
        {
            string str = "";
            try
            {
                MySqlCommand command = new MySqlCommand("SELECT DISTINCT Date FROM data", conn);
                MySqlDataReader rdr = command.ExecuteReader();

                string temp = string.Empty;
                if (rdr == null) temp = "No return";
                else
                {
                    while (rdr.Read())
                    {
                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            str = rdr[i].ToString();
                            comboBox2.Items.Add(str);
                        }
                    }
                }
                rdr.Close();
                rdr.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            if (comboBox2.Items.Contains(str)) comboBox2.SelectedItem = str;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            dtRecord.Dispose();
            CloseConnection();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string str = (string)comboBox2.SelectedItem;
            countAlarm(string.Format("SELECT * FROM data WHERE Stand<Measure AND Date=\"{0}\"", str));
            DisplayData(string.Format("SELECT * FROM data WHERE Date=\"{0}\"", str));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("정말 종료합니까?", "종료", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        private void countAlarm(string query)
        {
            alarmcnt = 0;
            try
            {
                MySqlCommand command = new MySqlCommand(query, conn);
                MySqlDataReader rdr = command.ExecuteReader();

                string temp = string.Empty;
                if (rdr == null) temp = "No return";
                else
                {
                    while (rdr.Read())
                    {
                        alarmcnt++;
                    }
                }
                rdr.Close();
                rdr.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}