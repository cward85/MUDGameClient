using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace GameClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpClient m_objClient;
        Thread m_objConnectionThread;
        NetworkStream m_objClientStream;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
            txtInput.Focus();
        }

        private void ConnectToServer()
        {
            IPEndPoint objEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.3"),4000);
            m_objClient = new TcpClient();

            m_objClient.Connect(objEndpoint);

            m_objConnectionThread = new Thread(new ThreadStart(ListenToServer));
            m_objConnectionThread.Start();

        }

        private void ListenToServer()
        {
            try
            {

                using (m_objClientStream = m_objClient.GetStream())
                {
                    while (m_objClient.Connected)
                    {
                        byte[] byteArray = new byte[1024];
                        string strOutput = string.Empty;
                        do
                        {
                            if (m_objClientStream.Read(byteArray, 0, byteArray.Length) > 0)
                            {
                                char[] chars = new char[byteArray.Length / sizeof(char)];
                                System.Buffer.BlockCopy(byteArray, 0, chars, 0, byteArray.Length);
                                strOutput += new string(chars);
                            }
                        }
                        while (m_objClientStream.DataAvailable);

                        strOutput = strOutput.Replace("\0", string.Empty);
                                                
                        WriteOutputText(strOutput);

                        if (strOutput == "Exit")
                        {
                            break;
                        }
                    }
                }
            }
            catch(ObjectDisposedException exe)
            {
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("Client: " + ex.Message);
            }
        }

        private byte[] ConvertStringToBytes(string p_strInput)
        {
            byte[] bytes = new byte[p_strInput.Length * sizeof(char)];
            System.Buffer.BlockCopy(p_strInput.ToCharArray(), 0, bytes, 0, bytes.Length);

            return bytes;
        }

        private void WriteToServer(string p_strInput)
        {
            if (!String.IsNullOrEmpty(p_strInput))
            {
                byte[] bytes = ConvertStringToBytes(p_strInput);
                m_objClientStream.Write(bytes, 0, bytes.Length);
            }
        }

        private void CheckForEnter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                WriteToServer(txtInput.Text);
                txtInput.Text = string.Empty;
            }
        }

        private void SendKillConnection(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                WriteToServer("Exit");
                m_objClientStream.Close();
                m_objClient.Close();
            }
            catch
            {
                
            }
        }

        delegate void AppendRichTextInvoker(string p_strText);
        private void WriteOutputText(string p_strText)
        {
            if (this.rtbOutput.Dispatcher.Thread == Thread.CurrentThread)
            {
                rtbOutput.AppendText(p_strText.Trim() + Environment.NewLine);
                rtbOutput.ScrollToEnd();
            }
            else
            {
                this.rtbOutput.Dispatcher.BeginInvoke(new Action(() => WriteOutputText(p_strText)));
            }
        }
    }
}
