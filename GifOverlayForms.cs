using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GifOverlayApp
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ManagerForm());
        }
    }

    public class ManagerForm : Form
    {
        private Button loadGifButton;
        private ListBox gifListBox;
        private TrackBar scaleTrackBar;
        private Label scaleLabel;
        private CheckBox clickThroughCheckBox;
        private Button showSelectedButton;
        private Button closeSelectedButton;
        private Button closeAllButton;
        private Label infoLabel;

        private List<string> gifPaths = new List<string>();
        private List<GifOverlayForm> gifForms = new List<GifOverlayForm>();

        public ManagerForm()
        {
            InitializeComponent();
            this.Text = "GIF Overlay Manager";
            this.MinimumSize = new Size(500, 400);
            
            // Register global hotkey (Ctrl+Q) to close all GIFs
            KeyPreview = true;
            this.KeyDown += (sender, e) => {
                if (e.Control && e.KeyCode == Keys.Q)
                {
                    CloseAllGifs();
                }
            };
        }

        private void InitializeComponent()
        {
            // Main layout
            this.SuspendLayout();
            
            // Load GIF Button
            loadGifButton = new Button();
            loadGifButton.Text = "Load GIF";
            loadGifButton.Location = new Point(10, 10);
            loadGifButton.Size = new Size(100, 30);
            loadGifButton.Click += LoadGifButton_Click;
            this.Controls.Add(loadGifButton);
            
            // GIF List Label
            Label gifListLabel = new Label();
            gifListLabel.Text = "Loaded GIFs:";
            gifListLabel.Location = new Point(10, 50);
            gifListLabel.AutoSize = true;
            this.Controls.Add(gifListLabel);
            
            // GIF List Box
            gifListBox = new ListBox();
            gifListBox.Location = new Point(10, 70);
            gifListBox.Size = new Size(480, 150);
            this.Controls.Add(gifListBox);
            
            // Scale Label
            Label scaleTextLabel = new Label();
            scaleTextLabel.Text = "Scale:";
            scaleTextLabel.Location = new Point(10, 230);
            scaleTextLabel.AutoSize = true;
            this.Controls.Add(scaleTextLabel);
            
            // Scale TrackBar
            scaleTrackBar = new TrackBar();
            scaleTrackBar.Location = new Point(60, 230);
            scaleTrackBar.Size = new Size(350, 45);
            scaleTrackBar.Minimum = 10;
            scaleTrackBar.Maximum = 300;
            scaleTrackBar.Value = 100;
            scaleTrackBar.TickFrequency = 10;
            scaleTrackBar.ValueChanged += ScaleTrackBar_ValueChanged;
            this.Controls.Add(scaleTrackBar);
            
            // Scale Value Label
            scaleLabel = new Label();
            scaleLabel.Text = "100%";
            scaleLabel.Location = new Point(420, 230);
            scaleLabel.AutoSize = true;
            this.Controls.Add(scaleLabel);
            
            // Click-through Checkbox
            clickThroughCheckBox = new CheckBox();
            clickThroughCheckBox.Text = "Enable Click-through";
            clickThroughCheckBox.Location = new Point(10, 280);
            clickThroughCheckBox.AutoSize = true;
            clickThroughCheckBox.Checked = true;
            clickThroughCheckBox.CheckedChanged += ClickThroughCheckBox_CheckedChanged;
            this.Controls.Add(clickThroughCheckBox);
            
            // Show Selected Button
            showSelectedButton = new Button();
            showSelectedButton.Text = "Show Selected";
            showSelectedButton.Location = new Point(10, 310);
            showSelectedButton.Size = new Size(150, 30);
            showSelectedButton.Click += ShowSelectedButton_Click;
            this.Controls.Add(showSelectedButton);
            
            // Close Selected Button
            closeSelectedButton = new Button();
            closeSelectedButton.Text = "Close Selected";
            closeSelectedButton.Location = new Point(170, 310);
            closeSelectedButton.Size = new Size(150, 30);
            closeSelectedButton.Click += CloseSelectedButton_Click;
            this.Controls.Add(closeSelectedButton);
            
            // Close All Button
            closeAllButton = new Button();
            closeAllButton.Text = "Close All GIFs";
            closeAllButton.Location = new Point(330, 310);
            closeAllButton.Size = new Size(150, 30);
            closeAllButton.Click += CloseAllButton_Click;
            this.Controls.Add(closeAllButton);
            
            // Info Label
            infoLabel = new Label();
            infoLabel.Text = "Keyboard Shortcuts:\nCtrl+Q: Close all GIFs\nLeft-click and drag: Move GIF";
            infoLabel.Location = new Point(10, 350);
            infoLabel.AutoSize = true;
            this.Controls.Add(infoLabel);
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadGifButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "GIF Files (*.gif)|*.gif";
                openFileDialog.Title = "Select a GIF File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    gifPaths.Add(filePath);
                    gifListBox.Items.Add(Path.GetFileName(filePath));
                }
            }
        }

        private void ScaleTrackBar_ValueChanged(object sender, EventArgs e)
        {
            float scale = scaleTrackBar.Value / 100.0f;
            scaleLabel.Text = scaleTrackBar.Value.ToString() + "%";
            
            // Update scale for all displayed GIFs
            foreach (GifOverlayForm form in gifForms)
            {
                if (form != null && !form.IsDisposed)
                {
                    form.SetScale(scale);
                }
            }
        }

        private void ClickThroughCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            bool clickThrough = clickThroughCheckBox.Checked;
            
            // Update click-through for all displayed GIFs
            foreach (GifOverlayForm form in gifForms)
            {
                if (form != null && !form.IsDisposed)
                {
                    form.SetClickThrough(clickThrough);
                }
            }
        }

        private void ShowSelectedButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = gifListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < gifPaths.Count)
            {
                string gifPath = gifPaths[selectedIndex];
                float scale = scaleTrackBar.Value / 100.0f;
                bool clickThrough = clickThroughCheckBox.Checked;
                
                GifOverlayForm gifForm = new GifOverlayForm(gifPath, scale, clickThrough);
                gifForms.Add(gifForm);
                gifForm.Show();
            }
            else
            {
                MessageBox.Show("Please select a GIF from the list.", "No GIF Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CloseSelectedButton_Click(object sender, EventArgs e)
        {
            int selectedIndex = gifListBox.SelectedIndex;
            if (selectedIndex >= 0 && selectedIndex < gifPaths.Count)
            {
                // Close all instances of the selected GIF
                string selectedGifPath = gifPaths[selectedIndex];
                for (int i = gifForms.Count - 1; i >= 0; i--)
                {
                    if (gifForms[i] != null && !gifForms[i].IsDisposed && gifForms[i].GifPath == selectedGifPath)
                    {
                        gifForms[i].Close();
                        gifForms.RemoveAt(i);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a GIF from the list.", "No GIF Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CloseAllButton_Click(object sender, EventArgs e)
        {
            CloseAllGifs();
        }
        
        private void CloseAllGifs()
        {
            // Close all GIF overlay forms
            for (int i = gifForms.Count - 1; i >= 0; i--)
            {
                if (gifForms[i] != null && !gifForms[i].IsDisposed)
                {
                    gifForms[i].Close();
                }
            }
            gifForms.Clear();
        }
    }

    // Native methods for window transparency
    internal static class NativeMethods
    {
        public static readonly int GWL_EXSTYLE = -20;
        public static readonly int WS_EX_LAYERED = 0x80000;
        public static readonly int WS_EX_TRANSPARENT = 0x20;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }

    public class GifOverlayForm : Form
    {
        public string GifPath { get; private set; }
        private PictureBox gifPictureBox;
        private float scaleFactor;
        private bool clickThrough;
        private Point dragStartPosition;
        private bool dragging;

        public GifOverlayForm(string gifPath, float scale, bool clickThrough)
        {
            this.GifPath = gifPath;
            this.scaleFactor = scale;
            this.clickThrough = clickThrough;
            
            // Set form properties for borderless and always on top
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            
            // Make form background transparent
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            
            // Create PictureBox for GIF
            gifPictureBox = new PictureBox();
            gifPictureBox.Dock = DockStyle.Fill;
            gifPictureBox.BackColor = Color.Transparent;
            
            // Load the GIF with animation support
            try
            {
                // Use a different approach for animated GIFs
                gifPictureBox.Image = Image.FromFile(gifPath);
                
                // This is important for GIF animation
                gifPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                
                this.Controls.Add(gifPictureBox);
                
                // Set initial size
                SetScale(scale);
                
                // Set click-through property
                SetClickThrough(clickThrough);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading GIF: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
            
            // Enable dragging
            gifPictureBox.MouseDown += GifPictureBox_MouseDown;
            gifPictureBox.MouseMove += GifPictureBox_MouseMove;
            gifPictureBox.MouseUp += GifPictureBox_MouseUp;
        }

        public void SetScale(float scale)
        {
            this.scaleFactor = scale;
            
            if (gifPictureBox.Image != null)
            {
                // Calculate new dimensions
                int originalWidth = gifPictureBox.Image.Width;
                int originalHeight = gifPictureBox.Image.Height;
                int newWidth = (int)(originalWidth * scale);
                int newHeight = (int)(originalHeight * scale);
                
                // Resize the form
                this.Size = new Size(newWidth, newHeight);
            }
        }

        public void SetClickThrough(bool enabled)
        {
            this.clickThrough = enabled;
            
            if (enabled)
            {
                // Make window transparent to mouse events
                int exStyle = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(
                    this.Handle,
                    NativeMethods.GWL_EXSTYLE,
                    exStyle | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT);
            }
            else
            {
                // Make window respond to mouse events
                int exStyle = NativeMethods.GetWindowLong(this.Handle, NativeMethods.GWL_EXSTYLE);
                NativeMethods.SetWindowLong(
                    this.Handle,
                    NativeMethods.GWL_EXSTYLE,
                    exStyle & ~NativeMethods.WS_EX_TRANSPARENT);
            }
        }

        private void GifPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                dragStartPosition = e.Location;
            }
        }

        private void GifPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                this.Location = new Point(
                    this.Location.X + e.X - dragStartPosition.X,
                    this.Location.Y + e.Y - dragStartPosition.Y);
            }
        }

        private void GifPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}