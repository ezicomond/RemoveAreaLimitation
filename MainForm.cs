using System;
using System.Diagnostics;
using System.Windows.Forms;

using Fiddler;

namespace RemoveAreaLimit
{
    public partial class MainForm : Form
    {
        private const string MATCH_URL = "http://auth.black.game.daum.net/api/starter/black/launch.json";
        private const string NOT_ABALIABLE = "NOT_AVAILABLE_COUNTRY";
        private const string RDT_URL = "http://black.game.daum.net/black/index.daum";

        private int port = 5973;

        static string sSecureEndpointHostname = "localhost";
        static int iSecureEndpointPort = 3965;

        static Proxy oSecureEndpoint;

        public MainForm()
        {
            InitializeComponent();

            FiddlerApplication.SetAppDisplayName("RemoveAreaLimit");
            FiddlerCoreStartupFlags oFCSF = FiddlerCoreStartupFlags.Default;

            FiddlerApplication.Startup(port, oFCSF);

            oSecureEndpoint = FiddlerApplication.CreateProxyEndpoint(iSecureEndpointPort, true, sSecureEndpointHostname);

            FiddlerApplication.BeforeRequest += OnBeforeRequest;
            FiddlerApplication.BeforeResponse += OnBeforeResponse;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            FiddlerApplication.Shutdown();
        }

        void OnBeforeRequest(Session session)
        {
            session.utilDecodeRequest();

            if (session.fullUrl.IndexOf(MATCH_URL) != -1)
                session.bBufferResponse = true;
        }

        void OnBeforeResponse(Session session)
        {
            session.utilDecodeResponse();

            if (session.fullUrl.IndexOf(MATCH_URL) != -1)
            {
                string response = session.GetResponseBodyAsString();

                int codeLoc = response.IndexOf(NOT_ABALIABLE);
                if (codeLoc != -1)
                {
                    response = response.Substring(0, codeLoc) + "PASS" +
                               response.Substring(codeLoc + NOT_ABALIABLE.Length,
                                   response.Length - codeLoc - NOT_ABALIABLE.Length);

                    int msgCtlLoc = response.IndexOf(RDT_URL);
                    response = response.Substring(0, msgCtlLoc) +
                               response.Substring(msgCtlLoc + RDT_URL.Length,
                                   response.Length - msgCtlLoc - RDT_URL.Length);

                    session.utilSetResponseBody(response);
                }

            }

        }

        bool ProcessExists(string PName)
        {
            Process[] proc = Process.GetProcessesByName(PName);

            return proc.Length != 0;
        }

        private void timerDetect_Tick(object sender, EventArgs e)
        {
            if (ProcessExists("BlackDesert_Launcher"))
            {
                this.Close();
            }
        }
    }
}
