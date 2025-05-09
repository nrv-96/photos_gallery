using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.VisualBasic;

namespace windowsgallartapp
{
    public partial class Form1 : Form
    {
        private string imagesDirectory = @"D:\Photos\BabyGirl";
        private Dictionary<int, List<string>> photoIndex; // Index photos by year
        private const string INDEX_FILE_NAME = "photo_index.dat"; // File to store the index data
        private string indexFilePath = @"C:\photo_index"; // Full path to the index file
        private DateTime lastIndexTime; // Last time the index was updated
        private HashSet<string> indexedDirectories; // Keep track of indexed directories

        public Form1()
        {
            InitializeComponent();
            flowLayoutPanelGallery.BackColor = Color.Black;
            InitializeCopyMoveButtons(); // Initialize Copy/Move buttons            
            // Set the index file path in the application's directory
            indexFilePath = Path.Combine(Application.StartupPath, INDEX_FILE_NAME);
            indexedDirectories = new HashSet<string>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Try to load existing index first
            if (LoadPhotoIndex())
            {
                DisplayAllPhotosGroupedByYear(); // Display photos from loaded index
            }
            else
            {
                // If loading failed, get directories and create a new index
                List<string> directories = GetUserDirectories(); // Prompt user for directories
                IndexAllPhotos(directories); // Index photos using multi-threading
                SavePhotoIndex(); // Save the newly created index
                DisplayAllPhotosGroupedByYear(); // Display photos
            }
        }

        // Save photo index to file
        private void SavePhotoIndex()
        {
            try
            {
                using (FileStream fs = new FileStream(indexFilePath, FileMode.Create))
                {
                    var formatter = new BinaryFormatter();
                    // Create a serializable data structure to save
                    var dataToSave = new IndexData
                    {
                        Photos = photoIndex,
                        IndexedDirectories = indexedDirectories.ToList(),
                        LastIndexTime = DateTime.Now
                    };
                    formatter.Serialize(fs, dataToSave);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving photo index: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load photo index from file
        private bool LoadPhotoIndex()
        {
            if (!File.Exists(indexFilePath))
                return false;

            try
            {
                using (FileStream fs = new FileStream(indexFilePath, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    var loadedData = (IndexData)formatter.Deserialize(fs);

                    // Restore data from file
                    photoIndex = loadedData.Photos;
                    indexedDirectories = new HashSet<string>(loadedData.IndexedDirectories);
                    lastIndexTime = loadedData.LastIndexTime;

                    // Validate the index (check if files still exist)
                    if (ValidateIndex())
                    {
                        return true;
                    }
                    else
                    {
                        // If validation failed, clear the index and return false
                        photoIndex = null;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading photo index: {ex.Message}. A new index will be created.",
                    "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }

        // Validate that the indexed files still exist
        private bool ValidateIndex()
        {
            if (photoIndex == null)
                return false;

            bool needsUpdate = false;

            // Check if directories still exist
            foreach (string dir in indexedDirectories.ToList())
            {
                if (!Directory.Exists(dir))
                {
                    needsUpdate = true;
                    indexedDirectories.Remove(dir);
                }
            }

            // Check for missing files and create a new clean dictionary
            var validatedIndex = new Dictionary<int, List<string>>();

            foreach (var yearGroup in photoIndex)
            {
                var validFiles = yearGroup.Value.Where(File.Exists).ToList();

                if (validFiles.Count > 0)
                {
                    validatedIndex[yearGroup.Key] = validFiles;
                }

                if (validFiles.Count != yearGroup.Value.Count)
                {
                    needsUpdate = true;
                }
            }

            // If we removed any files or directories, update the index
            if (needsUpdate)
            {
                photoIndex = validatedIndex;
                SavePhotoIndex(); // Save the cleaned-up index
                MessageBox.Show("Some photos in the index were no longer found and have been removed.",
                    "Index Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return photoIndex.Count > 0;
        }

        // Check if any directories have been modified since last indexing
        private bool NeedsReindexing(List<string> directories)
        {
            // If we don't have an index yet or it's empty, we need to index
            if (photoIndex == null || photoIndex.Count == 0)
                return true;

            // Check if any directory's last write time is newer than our index time
            foreach (var dir in directories)
            {
                if (!indexedDirectories.Contains(dir))
                    return true; // New directory that wasn't previously indexed

                var dirInfo = new DirectoryInfo(dir);
                if (dirInfo.LastWriteTime > lastIndexTime)
                    return true; // Directory was modified after our last indexing
            }

            return false;
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
            // If we already have indexed directories, use them as default
            string defaultDirs = indexedDirectories.Count > 0 ?
                string.Join(",", indexedDirectories) :
                @"D:\Photos\BabyGirl";

            string input = ShowInputDialog(
                "Select Directories",
                "Enter directories separated by commas (e.g., D:\\photos,C:\\photos\\test):",
                defaultDirs
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
            // Check if we need to create a new photo index or if we can use the existing one
            if (photoIndex == null)
            {
                photoIndex = new Dictionary<int, List<string>>();
            }

            object lockObject = new object(); // For thread safety

            // Show loading indicator
            Cursor = Cursors.WaitCursor;
            Text = "Indexing photos... Please wait.";
            Application.DoEvents();

            try
            {
                Parallel.ForEach(directories, directory =>
                {
                    var imageFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                                              .Where(IsImageFile);

                    lock (lockObject)
                    {
                        indexedDirectories.Add(directory); // Track indexed directories
                    }

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

                // Update the last index time
                lastIndexTime = DateTime.Now;
            }
            finally
            {
                // Restore cursor and form title
                Cursor = Cursors.Default;
                Text = "Photo Gallery";
            }
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

                    // Re-index to update the photo index
                    List<string> dirs = new List<string>(indexedDirectories);
                    photoIndex = new Dictionary<int, List<string>>();
                    IndexAllPhotos(dirs);
                    SavePhotoIndex(); // Save the updated index
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
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to add to the gallery";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;

                    if (!Directory.Exists(selectedPath))
                    {
                        MessageBox.Show("The selected directory does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Index only the new directory
                    List<string> newDirectories = new List<string> { selectedPath };
                    IndexAllPhotos(newDirectories);

                    // Save the updated index
                    SavePhotoIndex();

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

        private void btnRefreshIndex_Click(object sender, EventArgs e)
        {
            // Force a reindex of all directories
            if (indexedDirectories != null && indexedDirectories.Count > 0)
            {
                List<string> dirs = new List<string>(indexedDirectories);
                photoIndex = new Dictionary<int, List<string>>();
                IndexAllPhotos(dirs);
                SavePhotoIndex();
                DisplayAllPhotosGroupedByYear();
                MessageBox.Show("Photo index has been refreshed.", "Index Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                List<string> directories = GetUserDirectories(); // Prompt user for directories
                IndexAllPhotos(directories); // Index photos using multi-threading
                SavePhotoIndex(); // Save the newly created index
                DisplayAllPhotosGroupedByYear(); // Display photos
            }
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

    // Serializable class to store the index data
    [Serializable]
    public class IndexData
    {
        public Dictionary<int, List<string>> Photos { get; set; }
        public List<string> IndexedDirectories { get; set; }
        public DateTime LastIndexTime { get; set; }
    }
}