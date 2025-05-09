using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace windowsgallartapp
{
    public partial class Form1 : Form
    {
        private string imagesDirectory;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Define application images directory (create if not exists)
            imagesDirectory = Path.Combine(Application.StartupPath, "Images");
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
            }
            // Optionally, preload images
            DisplayAllPhotos();
        }

        private void btnAlbums_Click(object sender, EventArgs e)
        {
            // Show the album list box
            listBoxAlbums.Visible = true;
            listBoxAlbums.Items.Clear();

            // Assume each sub-folder under the Images folder is an album.
            string[] albumDirs = Directory.GetDirectories(imagesDirectory);
            foreach (string albumDir in albumDirs)
            {
                listBoxAlbums.Items.Add(Path.GetFileName(albumDir));
            }

            // Clear gallery
            flowLayoutPanelGallery.Controls.Clear();
        }

        private void btnAllPhotos_Click(object sender, EventArgs e)
        {
            // Hide album list if visible.
            listBoxAlbums.Visible = false;
            DisplayAllPhotos();
        }

        private void listBoxAlbums_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAlbums.SelectedItem == null)
                return;
            string albumName = listBoxAlbums.SelectedItem.ToString();
            string albumPath = Path.Combine(imagesDirectory, albumName);
            DisplayPhotos(albumPath);
        }

        // Displays all photos from all albums and the base Images folder.
        private void DisplayAllPhotos()
        {
            flowLayoutPanelGallery.Controls.Clear();

            // Add images directly in the Images folder
            DisplayPhotos(imagesDirectory);

            // Then add images from sub-folders (albums)
            foreach (string dir in Directory.GetDirectories(imagesDirectory))
            {
                DisplayPhotos(dir);
            }
        }

        // Displays images from a given directory.
        private void DisplayPhotos(string folderPath)
        {
            string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string imagePath in imageFiles)
            {
                if (IsImageFile(imagePath))
                {
                    try
                    {
                        PictureBox pic = new PictureBox();
                        pic.SizeMode = PictureBoxSizeMode.Zoom;
                        pic.Size = new Size(100, 100);
                        pic.Padding = new Padding(5);
                        pic.Image = Image.FromFile(imagePath);
                        pic.Tag = imagePath;
                        // Optional: Add click event to show full image, etc.
                        flowLayoutPanelGallery.Controls.Add(pic);
                    }
                    catch (Exception ex)
                    {
                        // Optionally log or handle exceptions (for example, file in use).
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private bool IsImageFile(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".gif";
        }
    }
}
