using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class FilterBarLLScrollButtons
    {
        internal static bool Constructed { get; private set; }

        internal static ArrowButton LeftButton;
        internal static ArrowButton RightButton;

        internal static void Construct(MainForm form, Control container)
        {
            if (Constructed) return;

            LeftButton = new ArrowButton();
            container.Controls.Add(LeftButton);
            LeftButton.FlatStyle = FlatStyle.Flat;
            LeftButton.ArrowDirection = Direction.Left;
            LeftButton.Size = new Size(14, 24);
            LeftButton.TabIndex = 2;
            LeftButton.UseVisualStyleBackColor = true;
            LeftButton.Visible = false;
            LeftButton.EnabledChanged += form.FilterBarScrollButtons_EnabledChanged;
            LeftButton.VisibleChanged += form.FilterBarScrollButtons_VisibleChanged;
            LeftButton.Click += form.FilterBarScrollButtons_Click;
            LeftButton.MouseDown += form.FilterBarScrollButtons_MouseDown;
            LeftButton.MouseUp += form.FilterBarScrollButtons_MouseUp;

            RightButton = new ArrowButton();
            container.Controls.Add(RightButton);
            RightButton.FlatStyle = FlatStyle.Flat;
            RightButton.ArrowDirection = Direction.Right;
            RightButton.Size = new Size(14, 24);
            RightButton.TabIndex = 10;
            RightButton.UseVisualStyleBackColor = true;
            RightButton.Visible = false;
            RightButton.EnabledChanged += form.FilterBarScrollButtons_EnabledChanged;
            RightButton.VisibleChanged += form.FilterBarScrollButtons_VisibleChanged;
            RightButton.Click += form.FilterBarScrollButtons_Click;
            RightButton.MouseDown += form.FilterBarScrollButtons_MouseDown;
            RightButton.MouseUp += form.FilterBarScrollButtons_MouseUp;

            LeftButton.BringToFront();
            RightButton.BringToFront();

            Constructed = true;
        }
    }
}
