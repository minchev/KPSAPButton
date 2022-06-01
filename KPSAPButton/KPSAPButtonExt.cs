using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;

using KeePass.Plugins;
using KeePass.Forms;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Ecas;
using KeePass.Util;
using KeePass.Util.Spr;
using KeePassLib;
using KeePassLib.Security;


namespace KPSAPButton
{
    public sealed class KPSAPButtonExt : Plugin
    {
        private IPluginHost m_host = null;
        //private System.Windows.Forms.ToolStripButton m_tbLBSAP;
        private ToolStripSeparator m_tsSeparator = null;
        private ToolStripMenuItem m_tsmiSAP = null;
        private string m_sSAPGUIPath = "";
        public static System.Diagnostics.Process SAPProcess = new System.Diagnostics.Process();

        public override bool Initialize(IPluginHost host)
        {
            Debug.Assert(host != null);
            if (host == null) return false;
            m_host = host;

            //Get SAPgui paths
            if (GetSAPGUIPath())
            {
                //Get a reference to the 'Tools' menu item container
                ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

                //// Add a separator at the bottom
                m_tsSeparator = new ToolStripSeparator();
                tsMenu.Add(m_tsSeparator);

                // Add the popup menu item
                m_tsmiSAP = new ToolStripMenuItem();
                m_tsmiSAP.Text = "Logon SAPgui";
                m_tsmiSAP.ShortcutKeys = Keys.F12;
                m_tsmiSAP.Click += OnKPSAP;
                m_tsmiSAP.Image = KPSAPButton.Resource.IconImage;
                tsMenu.Add(m_tsmiSAP);
                return true;
            };

            return false;
        } //Initialize

        public override void Terminate()
        {
        } //Terminate

        private void OnKPSAP(object sender, EventArgs e)
        {
            PwEntry[] selEntries;
            PwGroup pg;
            PwUuid pu;
            ProtectedString sTitle;
            ProtectedString sUserName;
            ProtectedString sPassword;

            if (!m_host.Database.IsOpen)
            {
                MessageBox.Show("You first need to open a database!", Resource.MessageTitleText);
                return;
            }

            pu = m_host.Database.LastSelectedGroup;
            pg = m_host.Database.RootGroup.FindGroup(pu, false);

            selEntries = m_host.MainWindow.GetSelectedEntries();

            if (selEntries == null)
            {
                MessageBox.Show("No selected entries!", Resource.MessageTitleText);
            }
            else
            {
                foreach (PwEntry pe in selEntries)
                {
                    sTitle = pe.Strings.Get(PwDefs.TitleField);
                    sUserName = pe.Strings.Get(PwDefs.UserNameField);
                    sPassword = pe.Strings.Get(PwDefs.PasswordField);
                    SAPLogon(sTitle.ReadString(), sUserName.ReadString(), sPassword.ReadString());
                }
            }
            return;
        } // OnKPSAP

        private void SAPLogon(string pTitle, string pUser, string pPass)
        {
            string sSysid = "";
            string sClient = "";
            int iCnt = 0;

            //Get splitted SYSID and Client
            string[] sSplit = pTitle.Split('.', '/');
            foreach (string w in sSplit)
            {
                iCnt++;
                switch (iCnt)
                {
                    case 1:
                        sSysid = w;
                        break;
                    case 2:
                        sClient = w;
                        break;
                }
            }

            //Check if SYSID and Client are filled
            if (sSysid == "" || sClient == "")
            {
                MessageBox.Show("Incorect format of the entry title!\n" +
                                "Format: <SYSID>[./]<CLIENT>\n" +
                                "Example: D02.400; E11/400",
                                Resource.MessageTitleText);
                return;
            }

            //TODO:Check against SAPLOGON.INI

            if (m_sSAPGUIPath == "") return;
            SAPProcess.StartInfo.Arguments = "";
            //SAPProcess.StartInfo.Arguments += " -sysname=" + sSysid; //MHM:20150611
            SAPProcess.StartInfo.Arguments += " -sysname=" + sSysid;   //MHM:20150611
            SAPProcess.StartInfo.Arguments += " -maxgui";
            SAPProcess.StartInfo.Arguments += " -client=" + sClient;
            SAPProcess.StartInfo.Arguments += " -user=" + pUser;
            SAPProcess.StartInfo.Arguments += " -pw=" + pPass;
            //MessageBox.Show(sSysid + "/" + sClient + "/" + pUser + "/" + pPass, Resource.MessageTitleText);
            try
            {
                SAPProcess.Start();
            }
            catch
            {
                MessageBox.Show("Error executing SAPSHCUT.EXE!", Resource.MessageTitleText);
            };

        } // SAPLogon

        private bool GetSAPGUIPath()
        {
            RegistryKey rootKey = RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, "");
            RegistryKey subKey = null;
            bool bOk = true;
            int i, Len;
            object objPath;
            string sPath = "";
            string resPath = "";

            m_sSAPGUIPath = "";
            SAPProcess.EnableRaisingEvents = false;
            try
            {
                // Check path from registry for x86
                subKey = rootKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\sapshcut.exe");
                objPath = subKey.GetValue("Path");
                sPath = objPath.ToString();
            }
            catch
            {
                bOk = false;
            };

            if (!bOk)
            {
                try
                {
                    // Check path from registry for 64bit
                    subKey = rootKey.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\App Paths\\sapshcut.exe");
                    objPath = subKey.GetValue("Path");
                    sPath = objPath.ToString();
                }
                catch
                {
                    bOk = false;
                }
            }

            if (bOk)
            {
                for (i = 0, Len = sPath.Length; ((i < Len) && (sPath[i] != ';')); i++)
                    resPath += sPath[i].ToString();

                if (resPath.Length < 3)  // "C:\"
                {
                    bOk = false;
                }
                else
                {
                    m_sSAPGUIPath = resPath;
                    CheckSAPGUIPath();
                }
            }
            else
            {
                MessageBox.Show("SAPgui not installed!", Resource.MessageTitleText);
            }
            return bOk;
        } //GetSAPGUIPath

        private bool CheckSAPGUIPath()
        {
            if ((m_sSAPGUIPath == null) ||
                (m_sSAPGUIPath.Length < 3) ||
                (!File.Exists(m_sSAPGUIPath + "\\sapshcut.exe")) ||
                (!File.Exists(m_sSAPGUIPath + "\\saplogon.exe")))
                return false;
            SAPProcess.StartInfo.FileName = m_sSAPGUIPath + "\\sapshcut.exe";
            return true;
        } //CheckSAPGUIPath

    }

}
