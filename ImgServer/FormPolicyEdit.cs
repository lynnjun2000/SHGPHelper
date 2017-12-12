using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImgServer.Net;
using Wind.Comm.Expo4;

namespace ImgServer
{
    public partial class FormPolicyEdit : Form
    {
        public string UserID { get; set; }
        public string AdminUserID { get; set; }
        public UserAdmin TermAdmin { get; set; }

        public FormPolicyEdit()
        {
            InitializeComponent();
            TermAdmin = null;
        }

        private void FormPolicyEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        public void initParam(UserPolicyData policyData)
        {
            tbIncPrice.Text = policyData.PriceMarkup;
            resetTimePickerValue_1(dtAutoBid, policyData.OfferTime);
            resetTimePickerValue_2(dtAutoCommit400, tbAutoCommitMS400, policyData.Submit400);
            resetTimePickerValue_2(dtAutoCommit500, tbAutoCommitMS500, policyData.Submit500);
            resetTimePickerValue_2(dtAutoCommit2, tbAutoCommitMS2, policyData.SubmitForce);
        }

        /// <summary>
        /// 需要在主窗体中也使用该功能
        /// </summary>
        /// <param name="control"></param>
        /// <param name="offset"></param>
        public static void resetTimePickerValue_1(DateTimePicker control, double offset)
        {
            MyDateTime time1 = new MyDateTime(control.Value);
            int min = (int)(Math.Floor(offset));
            time1.Minute = min;
            int v1 = Convert.ToInt32(offset * 100);
            int v2 = min * 100;
            time1.Second = v1 - v2;
            control.Value = time1.ToDateTime();
        }

        /// <summary>
        /// 需要在主窗体中也使用该功能
        /// </summary>
        /// <param name="dateControl"></param>
        /// <param name="msControl"></param>
        /// <param name="offset"></param>
        public static void resetTimePickerValue_2(DateTimePicker dateControl, TextBox msControl, double offset)
        {
            MyDateTime time1 = new MyDateTime(dateControl.Value);
            int sec = (int)(Math.Floor(offset));
            time1.Second = sec;
            int v1 = Convert.ToInt32(offset * 1000);
            int v2 = sec * 1000;
            msControl.Text = Convert.ToString(v1 - v2);
            dateControl.Value = time1.ToDateTime();
        }


        private Wind.Comm.Expo4.Message buildRequest()
        {
            UserPolicyData policyData = new UserPolicyData();
            policyData.UserID = UserID;
            policyData.OfferTime = dtAutoBid.Value.Minute;
            policyData.OfferTime += dtAutoBid.Value.Second / 100.0;

            policyData.PriceMarkup = tbIncPrice.Text.Trim();

            policyData.Submit400 = dtAutoCommit400.Value.Second;
            policyData.Submit400 += Convert.ToInt32(tbAutoCommitMS400.Text) / 1000.0;

            policyData.Submit500 = dtAutoCommit500.Value.Second;
            policyData.Submit500 += Convert.ToInt32(tbAutoCommitMS500.Text) / 1000.0;

            policyData.SubmitForce = dtAutoCommit2.Value.Second;
            policyData.SubmitForce += Convert.ToInt32(tbAutoCommitMS2.Text) / 1000.0;
            policyData.ServerIP = "";

            Wind.Comm.Expo4.Message msg = new Wind.Comm.Expo4.Message();
            msg.SetCommand(3100, UserAdmin.CHANGEPOLICYMSGID);
            MessageV2BuilderHelper helper = new MessageV2BuilderHelper();
            helper.WriteString(AdminUserID);
            helper.WriteUserPolicyData2Msg(policyData);
            msg.setMsgBody(helper.GetMsgBody());

            return msg;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            Wind.Comm.Expo4.Message reqMsg = buildRequest();
            TermAdmin.Server.conBroadcast(reqMsg, PacketHeader.ExpoDealType.InterGroup);
            Hide();
        }
    }
}
