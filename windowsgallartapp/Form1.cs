using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace windowsgallartapp
{
    public partial class Form1 : Form
    {
        private string imagesDirectory = @"D:\Photos\BabyGirl";
        private Dictionary<int, List<string>> photoIndex; // Index photos by year

        public Form1()
        {
            InitializeComponent();
            flowLayoutPanelGallery.BackColor = Color.Black;
            InitializeCopyMoveButtons(); // Initialize Copy/Move buttons
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> directories = GetUserDirectories(); // Prompt user for directories
            IndexAllPhotos(directories); // Index photos using multi-threading
            DisplayAllPhotosGroupedByYear(); // Display photos
        }
        private string ShowInputDialog(string title, string prompt, string defaultValue = "")
        {
            Form inputForm = new Form
            {
                Width = 400,
                Height = 200,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label label = new Label
            {
                Text = prompt,
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                AutoSize = true
            };

            TextBox textBox = new TextBox
            {
                Text = defaultValue,
                Dock = DockStyle.Top,
                Margin = new Padding(10)
            };

            Button okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom
            };

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);

            inputForm.AcceptButton = okButton;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        private List<string> GetUserDirectories()
        {
            string input = ShowInputDialog(
                "Select Directories",
                "Enter directories separated by commas (e.g., D:\\photos,C:\\photos\\test):",
                @"D:\Photos\BabyGirl"
            );

            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("No directories provided. The application will exit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0); // Exit if no directories are provided
            }

            return input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(dir => dir.Trim())
                        .Where(Directory.Exists)
                        .ToList();
        }


        private void IndexAllPhotos(List<string> directories)
        {
            if (photoIndex == null)
            {
                photoIndex = new Dictionary<int, List<string>>();
            }

            object lockObject = new object(); // For thread safety

            Parallel.ForEach(directories, directory =>
            {
                var imageFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                                          .Where(IsImageFile);

                foreach (var file in imageFiles)
                {
                    int year = GetImageYear(file);

                    lock (lockObject) // Ensure thread safety when modifying the dictionary
                    {
                        if (!photoIndex.ContainsKey(year))
                        {
                            photoIndex[year] = new List<string>();
                        }

                        // Avoid adding duplicate files
                        if (!photoIndex[year].Contains(file))
                        {
                            photoIndex[year].Add(file);
                        }
                    }
                }
            });
        }


        private List<string> selectedImages = new List<string>(); // Store selected images
        private Button copyButton;
        private Button moveButton;
        private void CopySelectedImages()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string destination = dialog.SelectedPath;

                    foreach (var file in selectedImages)
                    {
                        try
                        {
                            string destFile = Path.Combine(destination, Path.GetFileName(file));
                            File.Copy(file, destFile, overwrite: true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error copying file {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    MessageBox.Show("Files copied successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedImages.Clear();
                    ShowCopyMoveButtons(false);
                }
            }
        }

        private void MoveSelectedImages()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string destination = dialog.SelectedPath;

                    foreach (var file in selectedImages)
                    {
                        try
                        {
                            string destFile = Path.Combine(destination, Path.GetFileName(file));
                            File.Move(file, destFile);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error moving file {file}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    MessageBox.Show("Files moved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    selectedImages.Clear();
                    ShowCopyMoveButtons(false);
                    //IndexAllPhotos(); // Re-index after moving files
                    DisplayAllPhotosGroupedByYear();
                }
            }
        }

        private void InitializeCopyMoveButtons()
        {
            copyButton = new Button
            {
                Text = "Copy",
                BackColor = Color.LightGray,
                Margin = new Padding(10),
                AutoSize = true
            };
            copyButton.Click += (s, e) => CopySelectedImages();

            moveButton = new Button
            {
                Text = "Move",
                BackColor = Color.LightGray,
                Margin = new Padding(10),
                AutoSize = true
            };
            moveButton.Click += (s, e) => MoveSelectedImages();
        }
        private void DisplayAllPhotosGroupedByYear()
        {
            flowLayoutPanelGallery.Controls.Clear();

            if (photoIndex == null || photoIndex.Count == 0)
                return;

            foreach (var group in photoIndex.OrderByDescending(g => g.Key))
            {
                // Add year label
                var yearLabel = new Label
                {
                    Text = group.Key.ToString(),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    AutoSize = true,
                    Margin = new Padding(10, 20, 10, 10)
                };
                flowLayoutPanelGallery.Controls.Add(yearLabel);

                // Use FlowLayoutPanel for dynamic wrapping
                var rowPanel = new FlowLayoutPanel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    FlowDirection = FlowDirection.LeftToRight,
                    BackColor = Color.Black,
                    Margin = new Padding(0, 0, 0, 20),
                    WrapContents = true,
                    Dock = DockStyle.Top
                };

                foreach (var imgPath in group.Value)
                {
                    try
                    {
                        PictureBox pic = new PictureBox
                        {
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Size = new Size(120, 80),
                            Margin = new Padding(4),
                            Image = CreateThumbnail(imgPath),
                            Tag = imgPath,
                            BackColor = Color.Black,
                            Cursor = Cursors.Hand,
                            BorderStyle = BorderStyle.None
                        };

                        // Handle selection with Ctrl + Click
                        pic.Click += (s, e) =>
                        {
                            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                            {
                                // Ctrl is pressed, toggle selection
                                if (selectedImages.Contains(imgPath))
                                {
                                    selectedImages.Remove(imgPath);
                                    pic.BorderStyle = BorderStyle.None; // Deselect
                                }
                                else
                                {
                                    selectedImages.Add(imgPath);
                                    pic.BorderStyle = BorderStyle.Fixed3D; // Select
                                }
                            }
                            else
                            {
                                // Ctrl is not pressed, single selection
                                foreach (var control in rowPanel.Controls.OfType<PictureBox>())
                                {
                                    control.BorderStyle = BorderStyle.None; // Deselect all
                                }
                                selectedImages.Clear();

                                selectedImages.Add(imgPath);
                                pic.BorderStyle = BorderStyle.Fixed3D; // Select current
                            }

                            // Show or hide buttons based on selection
                            ShowCopyMoveButtons(selectedImages.Count > 0);
                        };

                        // Handle double-click to open image
                        pic.DoubleClick += (s, e) =>
                        {
                            int idx = group.Value.IndexOf(imgPath);
                            ShowFullImageWithControls(group.Value, idx); // Open image viewer
                        };

                        rowPanel.Controls.Add(pic);
                    }
                    catch
                    {
                        // Ignore bad images
                    }
                }

                flowLayoutPanelGallery.Controls.Add(rowPanel);
            }
        }

        private void ShowCopyMoveButtons(bool show)
        {
            btnCopy.Visible = show;
            btnMove.Visible = show;
        }
        private void btnAddDirectory_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a Directory";
                dialog.Filter = "Folders|*.none"; // Dummy filter to show folders only
                dialog.CheckFileExists = false; // Allow folder selection
                dialog.ValidateNames = false; // Allow folder selection
                dialog.FileName = "Select Folder"; // Default text in the file name box

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = Path.GetDirectoryName(dialog.FileName);

                    if (!Directory.Exists(selectedPath))
                    {
                        MessageBox.Show("The selected directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Index only the new directory
                    List<string> newDirectories = new List<string> { selectedPath };
                    IndexAllPhotos(newDirectories);

                    // Refresh the gallery
                    DisplayAllPhotosGroupedByYear();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Refresh the layout without re-indexing
            flowLayoutPanelGallery.SuspendLayout();
            flowLayoutPanelGallery.ResumeLayout();
        }
        private Image CreateThumbnail(string filePath)
        {
            try
            {
                using (var img = Image.FromFile(filePath))
                {
                    return img.GetThumbnailImage(120, 80, null, IntPtr.Zero);
                }
            }
            catch
            {
                return null; // Return null if thumbnail creation fails
            }
        }

        private void ShowFullImageWithControls(List<string> images, int currentIndex)
        {
            if (images == null || images.Count == 0 || currentIndex < 0 || currentIndex >= images.Count)
                return;

            var form = new Form
            {
                Text = Path.GetFileName(images[currentIndex]),
                Size = new Size(900, 700),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.Black
            };

            Bitmap original = null;
            Bitmap current = null;
            float zoom = 1.0f;
            int rotation = 0;

            PictureBox pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.Black
            };

            void LoadImage(int idx)
            {
                try
                {
                    if (original != null) original.Dispose();
                    if (current != null) current.Dispose();
                    original = new Bitmap(images[idx]);
                    current = new Bitmap(original);
                    zoom = 1.0f;
                    rotation = 0;
                    UpdateImage();
                    form.Text = Path.GetFileName(images[idx]);
                }
                catch
                {
                    MessageBox.Show("Unable to load image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            void UpdateImage()
            {
                if (current != null) current.Dispose();
                current = new Bitmap(original);

                // Rotate
                switch (rotation % 360)
                {
                    case 90:
                    case -270:
                        current.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 180:
                    case -180:
                        current.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 270:
                    case -90:
                        current.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }

                // Zoom
                int w = (int)(current.Width * zoom);
                int h = (int)(current.Height * zoom);
                if (w > 0 && h > 0)
                {
                    var bmp = new Bitmap(current, new Size(w, h));
                    pictureBox.Image = bmp;
                    current.Dispose();
                    current = bmp;
                }
                else
                {
                    pictureBox.Image = current;
                }
            }

            // --- Controls with symbols ---
            // --- Controls with icons ---
            var btnPrev = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"previous.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            var btnNext = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"next.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            var btnRotateLeft = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"rotate_left.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            var btnRotateRight = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"rotate_right.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            var btnZoomIn = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"zoom_in.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            var btnZoomOut = new Button
            {
                Width = 40,
                Height = 40,
                BackgroundImage = Image.FromFile(@"zoom_out.png"),
                BackgroundImageLayout = ImageLayout.Stretch
            };

            btnPrev.Click += (s, e) =>
            {
                if (currentIndex > 0)
                {
                    currentIndex--;
                    LoadImage(currentIndex);
                }
            };
            btnNext.Click += (s, e) =>
            {
                if (currentIndex < images.Count - 1)
                {
                    currentIndex++;
                    LoadImage(currentIndex);
                }
            };
            btnRotateLeft.Click += (s, e) =>
            {
                rotation = (rotation - 90) % 360;
                UpdateImage();
            };
            btnRotateRight.Click += (s, e) =>
            {
                rotation = (rotation + 90) % 360;
                UpdateImage();
            };
            btnZoomIn.Click += (s, e) =>
            {
                zoom *= 1.2f;
                UpdateImage();
            };
            btnZoomOut.Click += (s, e) =>
            {
                zoom /= 1.2f;
                UpdateImage();
            };

            // --- Mouse wheel zoom (with Ctrl) ---
            pictureBox.MouseWheel += (s, e) =>
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    if (e.Delta > 0)
                        zoom *= 1.1f;
                    else
                        zoom /= 1.1f;
                    UpdateImage();
                }
            };
            // Enable mouse wheel events
            pictureBox.Focus();
            pictureBox.MouseEnter += (s, e) => pictureBox.Focus();

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Black
            };
            panel.Controls.Add(btnPrev);
            panel.Controls.Add(btnNext);
            panel.Controls.Add(btnRotateLeft);
            panel.Controls.Add(btnRotateRight);
            panel.Controls.Add(btnZoomIn);
            panel.Controls.Add(btnZoomOut);

            form.Controls.Add(pictureBox);
            form.Controls.Add(panel);

            form.FormClosed += (s, e) =>
            {
                if (original != null) original.Dispose();
                if (current != null) current.Dispose();
            };

            LoadImage(currentIndex);
            form.ShowDialog();
        }
        private void ShowFullImage(string filePath)
        {
            try
            {
                using (var img = Image.FromFile(filePath))
                {
                    var form = new Form
                    {
                        Text = Path.GetFileName(filePath),
                        Size = new Size(800, 600),
                        StartPosition = FormStartPosition.CenterScreen,
                        BackColor = Color.Black
                    };

                    var pictureBox = new PictureBox
                    {
                        Image = img,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Dock = DockStyle.Fill
                    };

                    form.Controls.Add(pictureBox);
                    form.ShowDialog();
                }
            }
            catch
            {
                MessageBox.Show("Unable to load image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopySelectedImages(); // Call the existing method to copy selected images
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            MoveSelectedImages(); // Call the existing method to move selected images
        }
        private int GetImageYear(string filePath)
        {
            try
            {
                using (var img = Image.FromFile(filePath))
                {
                    const int PropertyTagExifDTOrig = 0x9003;
                    if (img.PropertyIdList.Contains(PropertyTagExifDTOrig))
                    {
                        var prop = img.GetPropertyItem(PropertyTagExifDTOrig);
                        string dateTaken = System.Text.Encoding.ASCII.GetString(prop.Value).Trim('\0');
                        if (DateTime.TryParseExact(dateTaken, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                            return dt.Year;
                        if (DateTime.TryParse(dateTaken, out dt))
                            return dt.Year;
                    }
                }
            }
            catch { /* Ignore EXIF errors */ }

            return File.GetLastWriteTime(filePath).Year; // Fallback to file's last write time
        }
        private void btnAllPhotos_Click(object sender, EventArgs e)
        {
            // Logic to display all photos
            DisplayAllPhotosGroupedByYear();
        }

        private void btnAlbums_Click(object sender, EventArgs e)
        {
            // Logic to display albums (if implemented)
            MessageBox.Show("Albums functionality is not implemented yet.");
        }

        private void listBoxAlbums_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Logic to handle album selection (if implemented)
            MessageBox.Show("Album selection functionality is not implemented yet.");
        }

        private bool IsImageFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif";
        }
    }
}