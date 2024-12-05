﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;

namespace AUV_UI
{
    public partial class AnaForm : Form
    {

        public string host;
        public string username;
        public string password;
        public string command;

        public SshClient RaspiSSHClient = null;
        public SftpClient RaspiSFTPClient = null;
        public ShellStream shellStream = null;

        public string currentProcessId;
        //private BackgroundWorker _backgroundWorker;

        public string raspi_dosya_yolu_Gonder;
        public string raspi_dosya_yolu_Al;
        public TextBoxMonitor yol_degis;
        public MotorConfigurationForm Konfigurasyon_Form;
        public ControlPanel kontrol_formu;
        public bool devamlilik_Sensor = false;
        public AnaForm()
        {
            InitializeComponent();

            ipadress_label.Text = Properties.Settings.Default.IP;
            password_label.Text = Properties.Settings.Default.Password;
            username_label.Text = Properties.Settings.Default.UserName;
            komut_satiri.Text = Properties.Settings.Default.KomutSatiri;

            //_backgroundWorker = new BackgroundWorker();
            //_backgroundWorker.WorkerSupportsCancellation = true; // İptal desteği sağla
            //_backgroundWorker.DoWork += BackgroundWorker_DoWork;
            //_backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        //private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //{
        //    SicaklikOku.ForeColor = Color.Black;
        //    devamlilik_Sensor = false;
        //}
        /*
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (!worker.CancellationPending)
            {
                // Uzun süren işleminiz burada
                var cmd = RaspiSSHClient.CreateCommand("vcgencmd measure_temp | sed \"s/temp=//;s/'C//\" | tr -d '\\n'");
                UpdateLabel("Raspberry Pi " + cmd.Execute() + " °C");
                Thread.Sleep(200); // Örnek: her saniye bir işlem yap
                // İşlemin ilerlemesini kontrol edebilir veya bir log atabilirsiniz
            }
            // İptal edildiğinde, metod burada tamamlanacak
        }

        
        private void UpdateLabel(string text)
        {
            try
            {
                if (label8.InvokeRequired)
                {
                    label8.Invoke(new MethodInvoker(() => label8.Text = text));
                }
                else
                {
                    label8.Text = text;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }
        */
        public void bilgi_geri_al(string _dosya_yolu, string tag)
        {
            if (tag == "al")
            {
                raspi_dosya_yolu_Al = _dosya_yolu;
            }

            else if (tag == "gonder")
            {
                raspi_dosya_yolu_Gonder = _dosya_yolu;
            }
        }

        private void baglan_Click(object sender, EventArgs e)
        {
            try
            {
                host = ipadress_label.Text;
                username = username_label.Text;
                password = password_label.Text;

                RaspiSSHClient = new SshClient(host, username, password);
                RaspiSFTPClient = new SftpClient(host, username, password);

                RaspiSSHClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(1);
                RaspiSFTPClient.ConnectionInfo.Timeout = TimeSpan.FromSeconds(1);

                RaspiSSHClient.Connect();
                RaspiSFTPClient.Connect();

                if (RaspiSSHClient.IsConnected && RaspiSSHClient.IsConnected)
                {
                    shellStream = RaspiSSHClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

                    var cmd = RaspiSSHClient.CreateCommand("echo -n $HOME/Desktop");
                    var result = cmd.Execute();
                    raspi_dosya_yolu_Gonder = result;
                    raspi_dosya_yolu_Al = result;
                    pictureBox2.Image = Properties.Resources.greentick;
                    desktop_listesi.Enabled = true;
                    dosya_gonder.Enabled = true;
                    baglantiyi_kes.Enabled = true;
                    komutu_calistir.Enabled = true;
                    alincak.Enabled = true;
                    baglan.Enabled = false;
                    //SicaklikOku.Enabled = true;
                    Dosya_Yolu_Degistir.Enabled = true;

                    shell_Baglan();
                }
                else
                {
                    pictureBox2.Image = Properties.Resources.rederror;
                    desktop_listesi.Enabled = false;
                    dosya_gonder.Enabled = false;
                    baglantiyi_kes.Enabled = false;
                    Dosya_Yolu_Degistir.Enabled = false;
                    MessageBox.Show("Bağlanamadı! \n");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
                    
        }

        private async void shell_Baglan()
        {
            try
            {
                string output;
                await Task.Run(() =>
                {
                    while (RaspiSSHClient != null)
                    {
                        output = shellStream.ReadLine() + "\n";
                        if (!string.IsNullOrEmpty(output))
                        {
                            Invoke(new Action(() =>
                            {
                                terminal.AppendText(output + Environment.NewLine);
                                //if (output == "%bitti%") { islemi_Durdur.Enabled = false; komutu_calistir.Enabled = true; }
                            }));
                        }
                    }
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void baglantiyi_kes_Click(object sender, EventArgs e)
        {
            try
            {
                shellStream.Close();
                RaspiSSHClient.Disconnect();
                RaspiSFTPClient.Disconnect();
                pictureBox2.Image = Properties.Resources.rederror;
                RaspiSSHClient = null;
                RaspiSFTPClient = null;
                shellStream = null;

                desktop_listesi.Enabled = false;
                dosya_gonder.Enabled = false;
                baglantiyi_kes.Enabled = false;
                komutu_calistir.Enabled = false;
                alincak.Enabled = false;
                baglan.Enabled = true;
                //SicaklikOku.Enabled = false;
                Dosya_Yolu_Degistir.Enabled = false;
            }
            catch(Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void desktop_listesi_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderDialog = new FolderBrowserDialog();
                folderDialog.Description = "Bir klasör seçin";
                folderDialog.ShowNewFolderButton = true;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {

                    string selectedPath = folderDialog.SelectedPath;
                    MessageBox.Show("Seçilen yol: " + selectedPath);
                    using (var file = File.Create(selectedPath + "/" + Path.GetFileName(raspi_dosya_yolu_Al)))
                    {
                        RaspiSFTPClient.DownloadFile(raspi_dosya_yolu_Al, file);
                        terminal.AppendText(Environment.NewLine + raspi_dosya_yolu_Al + Environment.NewLine + "Adresinden dosya başarı ile alındı ve " + Environment.NewLine + selectedPath + Path.GetFileName(raspi_dosya_yolu_Al) + Environment.NewLine + "Adresine kaydedildi " + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void alincak_Click(object sender, EventArgs e)
        {
            yol_degis = new TextBoxMonitor(this, raspi_dosya_yolu_Al, "al");
            yol_degis.ShowDialog();
        }

        private void dosya_gonder_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                dialog.RestoreDirectory = true;
                dialog.Multiselect = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (String file in dialog.FileNames)
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Open))
                        {
                            RaspiSFTPClient.ChangeDirectory(raspi_dosya_yolu_Gonder);
                            RaspiSFTPClient.UploadFile(fs, Path.GetFileName(file));
                            terminal.Text += file + " dosyası;" + Environment.NewLine + raspi_dosya_yolu_Gonder + " adresine yüklendi" + Environment.NewLine;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex + "");
            }
        }

        private void Dosya_Yolu_Degistir_Click(object sender, EventArgs e)
        {
            yol_degis = new TextBoxMonitor(this, raspi_dosya_yolu_Gonder, "gonder");
            yol_degis.ShowDialog();
        }

        private void config_button_Click(object sender, EventArgs e)
        {
            Konfigurasyon_Form = new MotorConfigurationForm(this);
            Konfigurasyon_Form.ShowDialog();
        }

        //private void SicaklikOku_Click(object sender, EventArgs e)
        //{
        //    Button button = sender as Button;
        //    if (devamlilik_Sensor)
        //    {
        //        _backgroundWorker.CancelAsync();
        //        button.ForeColor = Color.Black;
        //        devamlilik_Sensor = false;
        //        label8.Text = "Raspberry Pi °C";
        //    }
        //    else
        //    {
        //        button.ForeColor = Color.Green;
        //        devamlilik_Sensor = true;
        //        _backgroundWorker.RunWorkerAsync();
        //    }
        //}

        private void komutu_calistir_Click(object sender, EventArgs e)
        {
            try
            {
                if (komut_satiri.Text.EndsWith(".py") || komut_satiri.Text.EndsWith(".py & echo $!"))
                {
                    if (komut_satiri.Text.EndsWith(" & echo $!"))
                    {
                        shellStream.WriteLine(komut_satiri.Text.Substring(0, komut_satiri.Text.Length));
                    }
                    else
                    {
                        shellStream.WriteLine(komut_satiri.Text.Substring(0, komut_satiri.Text.Length) + " & echo $!");
                    }
                    string output;
                    Match match;

                    while (true)
                    {
                        output = shellStream.ReadLine() + "\n";
                        match = Regex.Match(output, @"%(\d+)%");
                        if (match.Success)
                        {
                            terminal.AppendText("islem kodu" + match.Groups[1].Value + Environment.NewLine);
                            currentProcessId = match.Groups[1].Value;
                            break;
                        }
                    }
                    islemi_Durdur.Enabled = true;
                    komutu_calistir.Enabled = false;
                }
                else
                {
                    shellStream.WriteLine(komut_satiri.Text);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }            
        }

        private void islemi_Durdur_Click(object sender, EventArgs e)
        {
            try
            {
                RaspiSSHClient.CreateCommand($"kill -9 {currentProcessId}").Execute();
                islemi_Durdur.Enabled = false;
                komutu_calistir.Enabled = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }            
        }

        private void AnaForm_Closing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.IP = ipadress_label.Text;
            Properties.Settings.Default.Password = password_label.Text;
            Properties.Settings.Default.UserName = username_label.Text;
            Properties.Settings.Default.KomutSatiri = komut_satiri.Text;
            Properties.Settings.Default.Save();
        }

        private void ControlPanelButton_Click(object sender, EventArgs e)
        {
            kontrol_formu = new ControlPanel(this);
            kontrol_formu.ShowDialog();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string url = "https://github.com/spacemonochrome/AUV_RBPI_STM32";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj göster
                MessageBox.Show("Bağlantı açılamadı: " + ex.Message);
            }
        }
    }
}
