using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ImgServer.UU;

namespace ImgServer
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        public static ImgMainForm mainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            log4net.Config.XmlConfigurator.Configure();
            //#if DEBUG  
                AllocConsole();  
            //#endif
            //AttachConsole(ATTACH_PARENT_PROCESS);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mainForm = new ImgMainForm();
            try{
                //UUCodeClient codeClient = new UUCodeClient();
                //int core = codeClient.GetLeftScore();
                //mainForm.SetDebugInfo("剩余题分：" + core.ToString());
                //mainForm._uuClient = codeClient;

            }catch(Exception e){
                mainForm.SetDebugInfo(e.Message);
            }
            Application.Run(mainForm);

#if DEBUG
            FreeConsole();
#endif
        }
    }
}
