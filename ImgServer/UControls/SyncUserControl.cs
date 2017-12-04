using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.VisualBasic;

namespace ImgServer.UControls
{
    public partial class SyncUserControl : UserControl
    {
        [Serializable]
        private class NodeConfig
        {
            private List<String> _otherServerList;
            /// <summary>
            /// 本地侦听端口
            /// </summary>
            public int LocalPort { get; set; }

            /// <summary>
            /// 相邻的节点列表
            /// </summary>
            public List<String> OtherServerList
            {
                get
                {
                    return _otherServerList;
                }
            }

            public bool AutoSync { get; set; }

            public NodeConfig()
            {
                _otherServerList = new List<string>();
                AutoSync = false;
                LocalPort = 7000;
            }

        }
        private string _configFileName = "nodeConfig.dat";
        private NodeConfig configData = null;

        public SyncUserControl()
        {
            InitializeComponent();            
            configData = loadConfig();
            fillUI();
        }

        #region save and load
        private NodeConfig loadConfig()
        {
            if (File.Exists(_configFileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(_configFileName, FileMode.Open);
                try
                {
                    NodeConfig setting = (NodeConfig)(bf.Deserialize(fs));
                    //setting.OnCoordSettingChangedEvent += new OnCoordSettingChanged(settingChangeEvent);
                    //setting.RaiseCoordChangedEvent();
                    return setting;
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                NodeConfig setting = new NodeConfig();
                //setting.OnCoordSettingChangedEvent += new OnCoordSettingChanged(settingChangeEvent);
                //setting.RaiseCoordChangedEvent();
                return setting;
            }
        }

        private void saveConfig()
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (File.Exists(_configFileName)) File.Delete(_configFileName);
            FileStream fs = new FileStream(_configFileName, FileMode.CreateNew);
            try
            {
                bf.Serialize(fs, configData);
            }
            finally
            {
                fs.Close();
            }

        }
        #endregion

        private void fillUI()
        {
            this.tbPort.Text = Convert.ToString(configData.LocalPort);
            foreach(string nodeUrl in configData.OtherServerList){
                ListViewItem item = new ListViewItem() { Text = nodeUrl };
                item.SubItems.Add("未连接");
                lvServers.Items.Add(item);
            }
            ckSync.Checked = configData.AutoSync;
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            saveConfig();
        }

        private void ckSync_CheckedChanged(object sender, EventArgs e)
        {
            configData.AutoSync = ckSync.Checked;
        }

        private void tbPort_TextChanged(object sender, EventArgs e)
        {
            configData.LocalPort = Convert.ToInt32(tbPort.Text.Trim());
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            //Interaction.InputBox("输入相邻节点信息ip:port", "输入", "", Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2);
            String url = Interaction.InputBox("输入相邻节点信息ip:port", "输入", "", 100, 100);
            if (url.Trim().Length > 0)
            {
                configData.OtherServerList.Add(url);
                fillUI();
            }
            
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (lvServers.FocusedItem == null)
            {
                MessageBox.Show("必须先选择一个要删除的条目");
            }
            else
            {
                lvServers.Items.Remove(lvServers.FocusedItem);
            }
        }
    }
}
