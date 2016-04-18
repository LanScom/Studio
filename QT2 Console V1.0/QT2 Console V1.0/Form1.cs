using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.IO;

namespace QT2_Console_V1._0
{
    public partial class Form1 : Form
    {
        //Thread _readThread;
        //bool _keepReading;
        #region Temp Data
        long intcount_pass = 0;
        long intcount_fail = 0;
        int timeout_cnt = 0;
        bool button1_status = false;
        string send_mcu_str = "";
        string receive_mcu_str = "";
        #endregion
        string tmp_str = "";
        string tmp_com = "";
        public Form1()
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();         // get the active serial port name;
            foreach (string port in ports)
            {
                tmp_com += port;

            }

            if (tmp_com == "")
            {
                MessageBox.Show("Could not detect serial port!");
            }
            else
            {
                w_serialPort.PortName = tmp_com;
                w_serialPort.BaudRate = 115200;
                w_serialPort.StopBits = StopBits.One;
                w_serialPort.WriteTimeout = 3000;
                w_serialPort.ReadTimeout = 3000;
                w_serialPort.ReceivedBytesThreshold = 1;
                w_serialPort.DataReceived += new SerialDataReceivedEventHandler(com_DataReceive);
                w_serialPort.Open();
                //_keepReading = true;
                //_readThread = new Thread(ReadPort);
                // _readThread.Start();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (tmp_com == "")
            {
                MessageBox.Show("Please Open the Serial Port Firstly!");
            }
            else
            {
                if (button1_status == false)
                {
                    button1_status = true;
                    button1.Text = "Stop";
                    send_mcu_str = textBox4.Text;
                    receive_mcu_str = "Code VER.\n\r\nCode VER.=1.0 \r\n@_@";
                    //receive_mcu_str = "Code VER.\n\r\nCode VER.=1.0 \r\n@_@";

                    w_serialPort.WriteLine(send_mcu_str);
                    timer1.Interval = (int)(double.Parse(textBox6.Text) * 1000);
                    timer1.Enabled = true;
                }
                else
                {
                    button1_status = false;
                    timer1.Enabled = false;
                    button1.Text = "Start";
                }
            }
        }

        //private void ReadPort()
        //{
        //    while(_keepReading)
        //    {
        //        if(w_serialPort.IsOpen)
        //        {
        //            byte[] readBuffer = new byte[w_serialPort.ReadBufferSize + 1];
        //            try
        //            {
        //                int count = w_serialPort.Read(readBuffer, 0, w_serialPort.ReadBufferSize);
        //                string serialIn = System.Text.Encoding.ASCII.GetString(readBuffer, 0, count);
        //                if(count != 0)
        //                {
        //                    //threadfun
        //                    //timer1.Enabled = false;
        //                  // textBox1.Text += serialIn;
        //                }
        //            }
        //            catch(TimeoutException)
        //            {
        //                timer1.Enabled = false;
        //            }
        //        }
        //        else
        //        {
        //            TimeSpan waittime = new TimeSpan(0, 0, 0, 0, 50);
        //            Thread.Sleep(waittime);
        //        }
        //    }
        //}

        private void com_DataReceive(object sender,SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(30);
            int count = w_serialPort.BytesToRead;
            if (count <= 0)
                return;
            byte[] readBuffer = new byte[count];
            byte[] readBuffer1 = new byte[count+1];
            Thread.Sleep(10);
            w_serialPort.Read(readBuffer, 0, count);
            string str = Encoding.ASCII.GetString(readBuffer);
            tmp_str = str;
            //textBox2.Text += str;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if(tmp_str == receive_mcu_str)       //"Code VER.\n\r\nCode VER.=1.0 \r\n@_@"   Code VER.\n\r\nCode VER.=1.0 \r\n@_@   Code VER.\r\n\r\nCode VER.=1.0 \r\n@_@
            {
                intcount_pass += 1; 
                tmp_str = "";                    
                textBox1.Text = intcount_pass.ToString();
                Thread.Sleep(2);
                w_serialPort.WriteLine(send_mcu_str);
                //Thread.Sleep(10);
                timer1.Enabled = true;
            }
            else if(tmp_str == "")
            {
                if (timeout_cnt < 5)
                {
                    timeout_cnt++;
                    timer1.Enabled = true;
                }
                else
                {
                    timeout_cnt = 0;
                    Thread.Sleep(2);
                    w_serialPort.WriteLine(send_mcu_str);
                    //Thread.Sleep(10);
                    timer1.Enabled = true;
                }

            }
            else
            {
                intcount_fail += 1;
                textBox2.Text = intcount_fail.ToString();
                FileStream fs = new FileStream(@"Failure Summary.csv", FileMode.Append, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(tmp_str+"\r\n");
                sw.Close();
                fs.Close();
                textBox3.Text += tmp_str+ "\r\n\r\n";
                tmp_str = "";
                Thread.Sleep(200);
                w_serialPort.WriteLine(send_mcu_str);
                timer1.Enabled = true;
         
            }
        }

    }
}
