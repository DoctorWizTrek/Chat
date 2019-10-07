using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;

namespace TicTacToe
{
    /// <summary>
    /// Interaction logic for Network.xaml
    /// </summary>
    public partial class Network : Window
    {
        public MainWindow main;
        private bool NonCloseButtonClicked = false;
        public TcpClient client;
        public StreamReader STR;
        public StreamWriter STW;

        private readonly BackgroundWorker worker1;
        //private readonly BackgroundWorker worker2;

        public Network()
        {
            InitializeComponent();

            ExternalIP.Content = new WebClient().DownloadString("http://icanhazip.com");

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(IPAddress address in localIP)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ServerIPText.Text = address.ToString();
                }
            }

            worker1 = new BackgroundWorker();
            //worker2 = new BackgroundWorker();

            worker1.DoWork += worker1_DoWork;
            //worker2.DoWork += worker2_DoWork;

        }

        private void NetworkWindow_Closed(object sender, EventArgs e)
        {
            if (!NonCloseButtonClicked)
            {
                main.Close();
            }
        }

        public void worker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] parsedString;
            string playerTurnString;
            string buttonString;
            string CharacterString;
            string winnerString;
            bool playerTurnBool;
            int buttonInt;

            while (client.Connected)
            {
                try
                {
                    main.recieve = STR.ReadLine();

                    parsedString = main.recieve.Split('|');

                    playerTurnString = parsedString[0];
                    buttonString = parsedString[1];
                    CharacterString = parsedString[2];
                    winnerString = parsedString[3];

                    playerTurnBool = playerTurnString == "true" ? true : false;
                    if (!Int32.TryParse(buttonString, out buttonInt))
                    {
                        return;
                    }


                    main.Dispatcher.Invoke((Action)delegate ()
                    {
                        if (buttonString != "9")
                        {
                            main.player1Turn = !playerTurnBool;
                            main.board[buttonInt].button.Content = CharacterString;
                            main.board[buttonInt].symbol = CharacterString == "X" ? Symbol.cross : Symbol.circle;
                        }

                        if (winnerString == "Winner")
                        {
                            main.NewGame();
                        }

                        if ((main.player1Turn) && (buttonString != "9"))
                        {
                            main.board[buttonInt].button.Foreground = Brushes.Red;
                        }

                        main.checkForWinner();
                    });

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }

        public void SendString(string playerTurn, string button, string character, string winner)
        {
            string send;
            if (client.Connected)
            {
                send = playerTurn + "|" + button + "|" + character + "|" + winner;
                STW.WriteLine(send);
            }
            else
            {
                MessageBox.Show("Lost connection");
            }
        }

        //private void worker2_DoWork(object sender, DoWorkEventArgs e)
        //{}

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, int.Parse(ServerPortText.Text));

            listener.Start();

            client = listener.AcceptTcpClient();

            STR = new StreamReader(client.GetStream());
            STW = new StreamWriter(client.GetStream());
            STW.AutoFlush = true;


            worker1.RunWorkerAsync();

            if (client.Connected)
            {
                this.Hide();
                main.Show();
            }
            //worker2.WorkerSupportsCancellation = true;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            client = new TcpClient();

            try
            {
                IPEndPoint IPEnd = new IPEndPoint(IPAddress.Parse(ClientIPText.Text), int.Parse(ClientPortText.Text));
                client.Connect(IPEnd);
                if (client.Connected)
                {
                    STR = new StreamReader(client.GetStream());
                    STW = new StreamWriter(client.GetStream());
                    STW.AutoFlush = true;
                    worker1.RunWorkerAsync();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            if (client.Connected)
            {
                this.Hide();
                main.Show();
            }
        }
    }
}
