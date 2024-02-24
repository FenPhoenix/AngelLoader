﻿// @ScreenshotDisplay: Watch screenshot folder and reload from disk

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class ScreenshotsTabPage : Lazy_TabsBase
{
    private Lazy_ScreenshotsPage _page = null!;

    /*
    Images loaded from files keep the file stream alive for their entire lifetime, insanely. This means the file
    is "in use" and will cause delete attempts (like FM uninstallation) to fail. So we need to use this workaround
    of loading the file into a memory stream first, so it's only the memory stream being kept alive. This does
    mean we carry around the full file bytes in memory as well as the displayed image, but since we're only
    displaying one at a time and they'll probably be a few megs at most, it's not a big deal.
    */
    private sealed class MemoryImage : IDisposable
    {
        private readonly MemoryStream _memoryStream;
        internal readonly Image Img;
        internal string Path { get; private set; }

        public MemoryImage(string path)
        {
            Path = path;
            byte[] bytes = File.ReadAllBytes(path);
            _memoryStream = new MemoryStream(bytes);
            Img = Image.FromStream(_memoryStream);
        }

        public void Dispose()
        {
            Path = "";
            Img.Dispose();
            _memoryStream.Dispose();
        }
    }

    private readonly List<string> ScreenshotFileNames = new();
    private string CurrentScreenshotFileName = "";
    private MemoryImage? _currentScreenshotStream;
    private bool _currentImageBroken;

    #region Theme

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override bool DarkModeEnabled
    {
        get => base.DarkModeEnabled;
        set
        {
            if (DarkModeEnabled == value) return;
            base.DarkModeEnabled = value;

            if (!_constructed) return;

            if (_currentImageBroken)
            {
                _page.ScreenshotsPictureBox.Image = Images.BrokenFile;
            }
        }
    }

    #endregion

    #region Public common

    public override void Construct()
    {
        if (_constructed) return;

        _page = ConstructPage<Lazy_ScreenshotsPage>();

        using (new DisableEvents(_owner))
        {
            Controls.Add(_page);

            _page.PrevButton.Click += ScreenshotsPrevButton_Click;
            _page.NextButton.Click += ScreenshotsNextButton_Click;
            _page.OpenScreenshotsFolderButton.PaintCustom += OpenScreenshotsFolderButton_PaintCustom;
            _page.OpenScreenshotsFolderButton.Click += OpenScreenshotsFolderButton_Click;

            FinishConstruct();
        }

        _page.Show();
    }

    public override void UpdatePage()
    {
        if (!_constructed) return;

        FanMission? fm = _owner.GetMainSelectedFMOrNull();

        Core.PopulateScreenshotFileNames(fm, ScreenshotFileNames);

        if (ScreenshotFileNames.Count == 0)
        {
            CurrentScreenshotFileName = "";
            ClearCurrentScreenshot();
            _page.ScreenshotsPictureBox.Enabled = false;
            _page.OpenScreenshotsFolderButton.Enabled = false;
            _page.PrevButton.Enabled = false;
            _page.NextButton.Enabled = false;
            SetNumberLabelText("");
        }
        // @ScreenshotDisplay: Should we save the selected screenshot in the FM object?
        else
        {
            CurrentScreenshotFileName = ScreenshotFileNames[0];
            DisplayCurrentScreenshot();
            _page.ScreenshotsPictureBox.Enabled = true;
            _page.OpenScreenshotsFolderButton.Enabled = true;
            _page.PrevButton.Enabled = ScreenshotFileNames.Count > 1;
            _page.NextButton.Enabled = ScreenshotFileNames.Count > 1;
        }
    }

    #endregion

    #region Page

    // Manual right-align to avoid needing a FlowLayoutPanel
    private void SetNumberLabelText(string text)
    {
        _page.NumberLabel.Text = text;
        _page.NumberLabel.Location = _page.NumberLabel.Location with
        {
            X = (_page.ClientSize.Width - 8) - _page.NumberLabel.Width
        };
    }

    private void ClearCurrentScreenshot()
    {
        _currentImageBroken = false;
        _page.ScreenshotsPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        _page.ScreenshotsPictureBox.Image = null;
        _page.ScreenshotsPictureBox.ImageLocation = "";
        _currentScreenshotStream?.Dispose();
    }

    /*
    The standard behavior for lazy loaded tabs is that they don't update until loaded, after which they always
    update. However, loading an image could take a significant amount of time, and we don't want users to pay the
    performance cost if they're not actually able to see the image. So we only update when visible (or when we
    become visible).
    */
    private void DisplayCurrentScreenshot()
    {
        if (!_constructed) return;
        if (!_owner.StartupState && !Visible) return;

        if (!CurrentScreenshotFileName.IsEmpty() &&
            // @TDM_CASE when FM is TDM
            _currentScreenshotStream?.Path.EqualsI(CurrentScreenshotFileName) != true)
        {
            try
            {
                _currentScreenshotStream?.Dispose();
                _currentScreenshotStream = new MemoryImage(CurrentScreenshotFileName);
                _page.ScreenshotsPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                _page.ScreenshotsPictureBox.Image = _currentScreenshotStream.Img;
                _currentImageBroken = false;
            }
            catch
            {
                ClearCurrentScreenshot();
                _page.ScreenshotsPictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                _page.ScreenshotsPictureBox.Image = Images.BrokenFile;
                _currentImageBroken = true;
            }
            finally
            {
                SetNumberLabelText(
                    (ScreenshotFileNames.IndexOf(CurrentScreenshotFileName) + 1).ToStrInv() + " / " +
                    ScreenshotFileNames.Count.ToStrInv()
                );
            }
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) DisplayCurrentScreenshot();
    }

    private void OpenScreenshotsFolderButton_Click(object sender, EventArgs e)
    {
        Core.OpenFMScreenshotsFolder(_owner.FMsDGV.GetMainSelectedFM(), CurrentScreenshotFileName);
    }

    private void OpenScreenshotsFolderButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Image image = Images.Folder;
        DarkButton button = _page.OpenScreenshotsFolderButton;
        Images.PaintBitmapButton(
            button,
            e,
            button.Enabled ? image : Images.GetDisabledImage(image),
            x: (button.Width - image.Width) / 2);
    }

    private void ScreenshotsPrevButton_Click(object sender, EventArgs e) => CycleScreenshot(step: -1);

    private void ScreenshotsNextButton_Click(object sender, EventArgs e) => CycleScreenshot(step: 1);

    private void CycleScreenshot(int step)
    {
        if (ScreenshotFileNames.Count <= 1) return;
        int index = ScreenshotFileNames.IndexOf(CurrentScreenshotFileName);

        if (index == -1) return;

        index =
            step == 1
                ? index == ScreenshotFileNames.Count - 1 ? 0 : index + 1
                : index == 0 ? ScreenshotFileNames.Count - 1 : index - 1;

        CurrentScreenshotFileName = ScreenshotFileNames[index];
        DisplayCurrentScreenshot();
    }

    #endregion
}
