﻿namespace VRCImageHelper.UI;

using VRCImageHelper.Core;
using VRCImageHelper.Tools;

public partial class ConfigWindow : Form
{
    public ConfigWindow()
    {
        InitializeComponent();
        _config = new Config();

        Icon = new Icon($"{Path.GetDirectoryName(Application.ExecutablePath)}\\icon.ico");
        comboBoxFileFormat.Items.AddRange(new object[] { "PNG", "JPEG", "AVIF" });
        comboBoxEncoder.Items.AddRange(FFMpeg.GetSupportedEncoder("av1"));
        comboBoxAlphaFileFormat.Items.AddRange(new object[] { "PNG", "AVIF" });
        comboBoxAlphaEncoder.Items.AddRange(FFMpeg.GetSupportedEncoder("av1"));
    }

    private Config _config;

    private void ConfigWindow_Load(object sender, EventArgs e)
    {
        _config = ConfigManager.Config;

        numericUpDownQuality.Value = _config.Quality;
        textBoxDir.Text = _config.DestDir;

        comboBoxFileFormat.SelectedItem = _config.Format;
        comboBoxEncoder.SelectedItem = _config.Encoder;

        textBoxFilePattern.Text = _config.FilePattern;
        textBoxEncoderOption.Text = _config.EncoderOption;

        comboBoxAlphaFileFormat.SelectedItem = _config.AlphaFormat;
        comboBoxAlphaEncoder.SelectedItem = _config.AlphaEncoder;

        textBoxAlphaFilePattern.Text = _config.AlphaFilePattern;
        textBoxAlphaEncoderOption.Text = _config.AlphaEncoderOption;

        ComboBoxFileFormat_SelectedIndexChanged(comboBoxFileFormat, EventArgs.Empty);
        ComboBoxFileFormat_SelectedIndexChanged(comboBoxAlphaFileFormat, EventArgs.Empty);
    }

    private void ButtonSelectDir_Click(object sender, EventArgs e)
    {
        var selectDirectoryDialog = new SaveFileDialog()
        {
            Filter = "Directory|保存先のフォルダ",
            FileName = Path.GetFileName(textBoxFilePattern.Text)
        };
        if (selectDirectoryDialog is not null && selectDirectoryDialog.ShowDialog() == DialogResult.OK)
        {
            var path = Path.GetDirectoryName(selectDirectoryDialog.FileName);
            if (path is not null)
            {
                _config.DestDir = path;
                textBoxDir.Text = path;
            }
        }
    }


    private void ButtonResetFilePattern_Click(object sender, EventArgs e)
    {
        var alpha = false;
        if (((Control)sender).Name.Contains("Alpha")) alpha = true;

        var controls = ((Control)sender).Parent.Parent.Controls;
        var textBox = (TextBox)controls.Find("textBoxFilePattern", true)[0];
        var comboBox = (ComboBox)controls.Find("fileFormat", true)[0];

        if (textBox is not null && comboBox is not null)
        {
            var filePattern = alpha ? Config.Default.AlphaFilePattern : Config.Default.FilePattern;
            var ext = comboBox.SelectedItem.ToString()?.ToLower();
            if (ext is not null)
            {
                textBox.Text = Path.ChangeExtension(filePattern, ext);
            }
        }
    }
    public delegate void FFMpegDownloadEnd();
    private void ComboBoxFileFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
        var alpha = "";
        if (((Control)sender).Name.Contains("Alpha")) alpha = "Alpha";
        var controls = ((Control)sender).Parent.Parent.Controls;
        var textBox = (TextBox)controls.Find($"textBox{alpha}FilePattern", true)[0];
        var fileFormat = (ComboBox)controls.Find($"comboBox{alpha}FileFormat", true)[0];
        var encoder = (ComboBox)controls.Find($"comboBox{alpha}Encoder", true)[0];
        var encoderOption = (TextBox)controls.Find($"textBox{alpha}EncoderOption", true)[0];
        var quality = (NumericUpDown)controls.Find($"numericUpDown{alpha}Quality", true)[0];

        var format = fileFormat.SelectedItem.ToString();

        if (format is not null && format != _config.Format)
        {
            textBox.Text = Path.ChangeExtension(textBoxFilePattern.Text, format.ToLower());
            _config.Format = format;
        }

        if (format == "AVIF" && FFMpeg.GetSupportedEncoder("av1").Length == 0)
        {
            var res = MessageBox.Show("この形式で圧縮するためにはffmpegが必要です。\nダウンロードしますか？", "VRCImageHelper/ffmpegのダウンロード", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                var downloadingDialog = new DownloadProgressDialog()
                {
                    Text = "Downloading...",
                };
                var downloading = true;
                downloadingDialog.FormClosing += (sender, e) =>
                {
                    if (downloading)
                        e.Cancel = true;
                };
                downloadingDialog.Load += (sender, e) =>
                {
                    new Task(() =>
                    {
                        FFMpeg.Download();
                        BeginInvoke(new FFMpegDownloadEnd(() =>
                        {
                            comboBoxEncoder.Items.AddRange(FFMpeg.GetSupportedEncoder("av1"));
                            comboBoxAlphaEncoder.Items.AddRange(FFMpeg.GetSupportedEncoder("av1"));

                            encoder.SelectedItem = alpha == "" ? Config.Default.Encoder : Config.Default.AlphaEncoder;
                            downloading = false;
                            downloadingDialog.Close();
                        }));
                    }).Start();
                };
                downloadingDialog.ShowDialog();
            }
            else
            {
                if (alpha == "")
                    fileFormat.SelectedItem = Config.Default.Format;
                else
                    fileFormat.SelectedItem = Config.Default.AlphaFormat;
            }
        }

        switch (format)
        {
            case "AVIF":
                encoder.Enabled = true;
                encoderOption.Enabled = true;
                quality.Enabled = true;
                break;
            case "JPEG":
                encoder.Enabled = false;
                encoderOption.Enabled = false;
                quality.Enabled = true;
                break;
            default:
                encoder.Enabled = false;
                encoderOption.Enabled = false;
                quality.Enabled = false;
                break;
        }
    }

    private void ButtonSave_Click(object sender, EventArgs e)
    {
        var format = comboBoxFileFormat.SelectedItem.ToString();
        if (format is not null) _config.Format = format;

        var encoder = comboBoxEncoder.SelectedItem?.ToString();
        if (encoder is not null) _config.Encoder = encoder;

        var alphaFormat = comboBoxAlphaFileFormat.SelectedItem.ToString();
        if (alphaFormat is not null) _config.AlphaFormat = alphaFormat;

        var alphaEncoder = comboBoxAlphaEncoder.SelectedItem?.ToString();
        if (alphaEncoder is not null) _config.AlphaEncoder = alphaEncoder;

        _config.Quality = Convert.ToInt32(numericUpDownQuality.Value);
        _config.EncoderOption = textBoxEncoderOption.Text;
        _config.FilePattern = textBoxFilePattern.Text;
        _config.AlphaQuality = Convert.ToInt32(numericUpDownAlphaQuality.Value);
        _config.AlphaEncoderOption = textBoxAlphaEncoderOption.Text;
        _config.AlphaFilePattern = textBoxAlphaFilePattern.Text;

        ConfigManager.Save(_config);
        Dispose();
    }
}