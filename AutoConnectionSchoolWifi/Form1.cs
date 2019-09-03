using AutoConnectionSchoolWifi.src;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoConnectionSchoolWifi
{
    public partial class Form1 : Form , ConnectionStateCallBack
    {

        SynchronizationContext _syncContext = null;

        public Form1()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
            Start();
        }

        private void SetText(object text)
        {
            this.tb.Text += text.ToString() + "\r\n";
            MoveCurorLast();
        }

        private void MoveCurorLast()
        {
            //让文本框获取焦点 
            this.tb.Focus();
            //设置光标的位置到文本尾 
            this.tb.Select(this.tb.TextLength, 0);
            //滚动到控件光标处 
            this.tb.ScrollToCaret();
        }

        public void StateChange(string state)
        {
            _syncContext.Post(SetText, state);
        }

        private void Start()
        {
            Thread th = new Thread(ThreadChild);
            th.Start(this);
            
        }

        static void ThreadChild(object callback)
        {
            var conn = new DefaultAutoConnection((ConnectionStateCallBack)callback);
            conn.Connection();
        }
    }
}
