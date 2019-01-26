﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using WindowsInput.Native;
using WindowsInput;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public decimal numericUpDown1_init, numericUpDown2_init, numericUpDown3_init, numericUpDown4_init, numericUpDown5_init, numericUpDown6_init;
        public Color OriginalBackground;
        public int UpDownSymb2, UpDownSymb3, UpDownSymb5;
        public string timeString;
        public string pwrshllin1, pwrshllin2;
        public string tempPath = System.IO.Path.GetTempPath();
        private Point mouseOffset;
        private bool isMouseDown = false;

        public Form1()
        {
            InitializeComponent();
            close_window.MouseEnter += label_MouseEnter;
            close_window.MouseLeave += label_MouseLeave;
            background.MouseDown += Form1_MouseDown;
            background.MouseMove += Form1_MouseMove;
            background.MouseUp += Form1_MouseUp;
            this.trayicon.MouseClick += new MouseEventHandler(trayicon_MouseClick);
            this.Resize += new System.EventHandler(this.Form1_Resize);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            int xOffset;
            int yOffset;

            if (e.Button == MouseButtons.Left)
            {
                xOffset = e.X;
                yOffset = e.Y;
                mouseOffset = new Point(xOffset, yOffset);
                isMouseDown = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - mouseOffset.X, currentScreenPos.Y - mouseOffset.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            schtasks_read();
        }

        private void schtasks_read()
        {
            PowerShell PowerShellInstance = PowerShell.Create();
            PowerShellInstance.AddScript(@"Schtasks /Query /NH /TN alarm_1.1 | Out-String");
            PowerShellInstance.Invoke();
            Collection<PSObject> pSObjects = PowerShellInstance.Invoke();
            foreach (PSObject p in pSObjects)
            {
                StreamWriter sw = new StreamWriter(tempPath + @"test.txt");
                sw.WriteLine(p.ToString());
                sw.Close();
            }
            schtasks_fileread();
            numericUpDown1.Value = numericUpDown1_init = Convert.ToDecimal(timeString.Substring(0, UpDownSymb2));
            numericUpDown2.Value = numericUpDown2_init = Convert.ToDecimal(timeString.Substring(UpDownSymb3, 2));
            numericUpDown3.Value = numericUpDown3_init = Convert.ToDecimal(timeString.Substring(UpDownSymb5));
            PowerShellInstance.AddScript(@"Schtasks /Query /NH /TN alarm_1.2 | Out-String");
            PowerShellInstance.Invoke();
            pSObjects = PowerShellInstance.Invoke();
            foreach (PSObject p in pSObjects)
            {
                StreamWriter sw = new StreamWriter(tempPath + @"test.txt");
                sw.WriteLine(p.ToString());
                sw.Close();
            }
            schtasks_fileread();
            numericUpDown4.Value = numericUpDown4_init = Convert.ToDecimal(timeString.Substring(0, UpDownSymb2));
            numericUpDown5.Value = numericUpDown5_init = Convert.ToDecimal(timeString.Substring(UpDownSymb3, 2));
            numericUpDown6.Value = numericUpDown6_init = Convert.ToDecimal(timeString.Substring(UpDownSymb5));
            File.Delete(tempPath + @"test.txt");
        }

        private void schtasks_fileread()
        {
            var lines = System.IO.File.ReadAllLines(tempPath + @"test.txt");
            string[] words = lines[2].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            timeString = words[2];
            if ((timeString.Substring(1, 1)) != ":")
            {
                UpDownSymb2 = 2;
                UpDownSymb3 = 3;
                UpDownSymb5 = 6;
            }
            else
            {
                UpDownSymb2 = 1;
                UpDownSymb3 = 2;
                UpDownSymb5 = 5;
            }
        }

        private void label_MouseEnter(object sender, EventArgs e)
        {
            OriginalBackground = ((Label)sender).ForeColor;
            ((Label)sender).ForeColor = Color.Red;
        }

        private void label_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).ForeColor = OriginalBackground;
        }

        private void apply_button_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void ApplyChanges()
        {
            
            int[] updownarr = { Convert.ToInt32(numericUpDown1.Value), Convert.ToInt32(numericUpDown2.Value), Convert.ToInt32(numericUpDown3.Value), Convert.ToInt32(numericUpDown4.Value), Convert.ToInt32(numericUpDown5.Value), Convert.ToInt32(numericUpDown6.Value) };
            string[] timearr = new string[6];

            for (int i = 0; i < 6; i++)
            {
                if ((updownarr[i] >= 0) && (updownarr[i] < 10))
                    timearr[i] = "0";
                else
                    timearr[i] = "";
            }
            pwrshllin1 = "Schtasks /Change /TN alarm_1.1 /ST " + timearr[0] + numericUpDown1.Value + ":" + timearr[1] + numericUpDown2.Value + ":" + timearr[2] + numericUpDown3.Value;
            pwrshllin2 = "Schtasks /Change /TN alarm_1.2 /ST " + timearr[3] + numericUpDown4.Value + ":" + timearr[4] + numericUpDown5.Value + ":" + timearr[5] + numericUpDown6.Value;
            uacSolver(pwrshllin1);
            uacSolver(pwrshllin2);
            numericUpDown1_init = numericUpDown1.Value;
            numericUpDown2_init = numericUpDown2.Value;
            numericUpDown3_init = numericUpDown3.Value;
            numericUpDown4_init = numericUpDown4.Value;
            numericUpDown5_init = numericUpDown5.Value;
            numericUpDown6_init = numericUpDown6.Value;
        }

        private void uacSolver(string sender)
        {
            Process p = new Process();
            p.StartInfo.FileName = "powershell.exe";
            p.StartInfo.Arguments = sender;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.Verb = "runas";
            p.Start();
            InputSimulator Simulator = new InputSimulator();
            Simulator.Mouse.MoveMouseTo(30000, 35000);
            Simulator.Mouse.Sleep(30);
            Simulator.Mouse.LeftButtonClick();
            Simulator.Mouse.Sleep(30);
            Simulator.Keyboard.TextEntry("193203");
            Simulator.Mouse.Sleep(30);
            Simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            Simulator.Mouse.Sleep(30);
            /*PowerShell PowerShellInstance = PowerShell.Create();
            PowerShellInstance.AddScript(@"Invoke-Command -ComputerName s4sh-desk -ScriptBlock {Schtasks /Change /TN alarm_1.1 /ST 00:00:00} -Credential s4sh-desk\s4sh | Out-String");
            PowerShellInstance.Invoke();*/
        }

        private void close_window_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(numericUpDown1.Value) != numericUpDown1_init || Convert.ToInt32(numericUpDown2.Value) != numericUpDown2_init || Convert.ToInt32(numericUpDown3.Value) != numericUpDown3_init || Convert.ToInt32(numericUpDown4.Value) != numericUpDown4_init || Convert.ToInt32(numericUpDown5.Value) != numericUpDown5_init || Convert.ToInt32(numericUpDown6.Value) != numericUpDown6_init)
            {
                DialogResult result = MessageBox.Show("Были внесены изменения, произвести сохранение?", "Подтвердите действие", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ApplyChanges();
                    this.Close();
                }
                else if (result == DialogResult.No)
                {
                    this.Close();
                }
                else if (result == DialogResult.Cancel)
                {

                }
            }
            else
                this.Close();
        }

        private void minimized_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                trayicon.Visible = true;
            }
        }

        private void trayicon_MouseClick(object sender, MouseEventArgs e)
        {
            trayicon.Visible = false;
            this.ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }
    }
}
