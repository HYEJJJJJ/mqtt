using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mqtt_Client_Main
{
    public static class MsgBox
    {
        const string DefaultTitle = "";
        private static string 확인 = "확인";
        private static string 경고 = "경고";
        private static string 오류 = "오류";

        public static DialogResult YesNoCancel(string message, string title = "선택")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        public static DialogResult YesNo(string message, string title = DefaultTitle)
        {
            if (title.Length == 0) title = "선택";
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static DialogResult Confirm(string message)
        {
            return MessageBox.Show(message, 확인, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        }

        public static DialogResult Ok(string message)
        {
            return MessageBox.Show(message, 확인, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult Show(string message)
        {
            return MessageBox.Show(message, 경고, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult Show(Exception ex)
        {
            return MessageBox.Show(ex.Message, 오류, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static DialogResult Show(string message, Exception ex)
        {
            return MessageBox.Show($"{message}\r\n{ex.Message}", 오류, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}