namespace YTMediaControllerUpdaterSrv
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.YTMediaControllerUpdaterSrv = new System.ServiceProcess.ServiceProcessInstaller();
            this.YTMediaControllerUpdaterSrvInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // YTMediaControllerUpdaterSrv
            // 
            this.YTMediaControllerUpdaterSrv.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.YTMediaControllerUpdaterSrv.Password = null;
            this.YTMediaControllerUpdaterSrv.Username = null;
            this.YTMediaControllerUpdaterSrv.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceProcessInstaller1_AfterInstall);
            // 
            // YTMediaControllerUpdaterSrvInstaller
            // 
            this.YTMediaControllerUpdaterSrvInstaller.Description = "Updater for YTMediaController service and browser extension";
            this.YTMediaControllerUpdaterSrvInstaller.DisplayName = "YTMediaControllerUpdaterSrv";
            this.YTMediaControllerUpdaterSrvInstaller.ServiceName = "Service1";
            this.YTMediaControllerUpdaterSrvInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.YTMediaControllerUpdaterSrv,
            this.YTMediaControllerUpdaterSrvInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller YTMediaControllerUpdaterSrv;
        private System.ServiceProcess.ServiceInstaller YTMediaControllerUpdaterSrvInstaller;
    }
}