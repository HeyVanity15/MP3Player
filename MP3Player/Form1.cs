using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//use these references for media player
using WMPLib;
using System.IO;

namespace MP3Player
{
    public partial class Form1 : Form
    {
        //======================================================================================
        //==============================GLOBAL VARIABLES========================================
        //======================================================================================

        WindowsMediaPlayer player = new WindowsMediaPlayer();
        //Create a list to hold entire library
        List<MP3> mp3Library = new List<MP3>();
        //create another list that will hold whatever
        //  currently appears in the listview display
        List<MP3> currentList = new List<MP3>();
        //list to hold distinct artists
        List<string> artistList = new List<string>();
        //Filename of the binary library file
        string library = "mp3Library.dat";
        //Reference to access the currently playing
        //  song at any given time
        MP3 currentSong;

        //======================================================================================
        //=====================================METHODS==========================================
        //======================================================================================

        //Select folder to add contents to a List
        public void AddFolderToLibrary()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            //open window and execute when user selects "ok"
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //save folder path
                string folder = dialog.SelectedPath;
                //save all paths of mp3 files within the selected folder
                string[] mp3Paths = Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories);
                //loop through files at saved paths and...
                foreach (string path in mp3Paths)
                {
                    //access and save mp3 metadata...
                    TagLib.File f = TagLib.File.Create(path);
                    if (f.Tag.Performers.Count() > 0 
                        && f.Tag.Genres.Count() > 0)
                    {
                        string title = f.Tag.Title;
                        string artist = f.Tag.Performers[0];
                        string album = f.Tag.Album;
                        uint track = f.Tag.Track;
                        uint year = f.Tag.Year;
                        string genre = f.Tag.Genres[0];
                        string filepath = path;
                        //to create mp3 objects...
                        MP3 mp3 = new MP3(title, artist, album, track, year, genre, filepath);
                        //that will be saved to the library list
                        mp3Library.Add(mp3);
                    }
                    else if (f.Tag.Genres.Count() < 1)
                    {
                        string title = f.Tag.Title;
                        string artist = f.Tag.Performers[0];
                        string album = f.Tag.Album;
                        uint track = f.Tag.Track;
                        uint year = f.Tag.Year;
                        string genre = "N/A";
                        string filepath = path;
                        //to create mp3 objects...
                        MP3 mp3 = new MP3(title, artist, album, track, year, genre, filepath);
                        //that will be saved to the library list
                        mp3Library.Add(mp3);
                    }
                }
            }
        }

        public void AddFilesToLibrary()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string[] files = dialog.FileNames;
                foreach (string path in files)
                {
                    TagLib.File f = TagLib.File.Create(path);
                    string title = f.Tag.Title;
                    string artist = f.Tag.Performers[0];
                    string album = f.Tag.Album;
                    uint track = f.Tag.Track;
                    uint year = f.Tag.Year;
                    string genre = f.Tag.Genres[0];
                    string filepath = path;
                    MP3 mp3 = new MP3(title, artist, album, track, year, genre, filepath);
                    mp3Library.Add(mp3);
                }
            }
        }

        //Alphabetize and order library
        private List<MP3> OrganizeLibrary(IEnumerable<MP3> list)
        {
            var ordList = mp3Library.OrderBy(x => x.Artist).ThenBy(x => x.Album).ThenBy(x => x.Track);
            return ordList.ToList();
        }

        //Display list
        private void DisplayList(List<MP3> list)
        {
            //First, clear items in display 
            lstvLibrary.Items.Clear();
            foreach (MP3 mp3 in list)
            {
                //Add each object property to the listview item
                ListViewItem lstvitem = new ListViewItem(mp3.Title);
                lstvitem.SubItems.Add(mp3.Artist);
                lstvitem.SubItems.Add(mp3.Album);
                lstvitem.SubItems.Add(mp3.Track.ToString());
                lstvitem.SubItems.Add(mp3.Year.ToString());
                lstvitem.SubItems.Add(mp3.Genre);
                //Add the listview item to the Listview display
                lstvLibrary.Items.Add(lstvitem);
            }
        }

        //Search and get list of matching items
        private List<MP3> SearchLibrary(List<MP3> lib, string searchType, string searchValue)
        {
            List<MP3> list = new List<MP3>();
            if (searchType == "artist")
                //search artists
                list = lib.FindAll(a => a.Artist.ToLower().StartsWith(searchValue));
            else if (searchType == "song")
                //search songs
                list = lib.FindAll(a => a.Title.ToLower().StartsWith(searchValue));
            else
                list = lib.FindAll(a => a.Album.ToLower().StartsWith(searchValue));
            return list;
        }

        //Save List to BinaryFile
        private void SaveLibraryToBinaryFile(List<MP3> lib)
        {
            FileStream fs = null;
            BinaryWriter bw = null;
            try
            {
                fs = new FileStream(library, FileMode.Create, FileAccess.Write);
                bw = new BinaryWriter(fs);
                foreach (MP3 mp3 in mp3Library)
                {
                    bw.Write(mp3.Title);
                    bw.Write(mp3.Artist);
                    bw.Write(mp3.Album);
                    bw.Write(mp3.Track);
                    bw.Write(mp3.Year);
                    bw.Write(mp3.Genre);
                    bw.Write(mp3.Path);
                }
            }
            catch (ArgumentException ae)
            {
                MessageBox.Show(ae.Message);
            }
            catch (FileNotFoundException fne)
            {
                MessageBox.Show(fne.Message);
            }
            catch (IOException ioe)
            {
                MessageBox.Show(ioe.Message);
            }
            finally
            {
                if (fs != null)
                    bw.Close();
                if (bw != null)
                    bw.Close();
            }
        }

        //Read List from BinaryFile
        private void ReadLibraryFromBinaryFile(List<MP3> lib)
        {
            if (File.Exists(library))
            {
                FileStream fs = null;
                BinaryReader br = null;
                try
                {
                    fs = new FileStream(library, FileMode.Open, FileAccess.Read);
                    br = new BinaryReader(fs);
                    while (br.BaseStream.Position != br.BaseStream.Length)
                    {
                        string title = br.ReadString();
                        string artist = br.ReadString();
                        string album = br.ReadString();
                        uint track = br.ReadUInt32();
                        uint year = br.ReadUInt32();
                        string genre = br.ReadString();
                        string path = br.ReadString();
                        MP3 mp3 = new MP3(title, artist, album, track, year, genre, path);
                        mp3Library.Add(mp3);
                    }
                }
                catch (ArgumentException ae)
                {
                    MessageBox.Show(ae.Message);
                }
                catch (FileNotFoundException fne)
                {
                    MessageBox.Show("Add folders to your music library.\n\n" + fne.Message);
                }
                catch (IOException ioe)
                {
                    MessageBox.Show(ioe.Message);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                    if (br != null)
                        br.Close();
                }
            }
        }

        //Display currently playing
        private void DisplayTrackInfo(MP3 mp3, Label lbl)
        {
            if (lbl.InvokeRequired)
            {
                Action<MP3, Label> action = DisplayTrackInfo;
                lbl.Invoke(action, mp3, lbl);
            }
            else
                lblCurrentlyPlaying.Text = mp3.ToString();
        }

        //Get each distinct artist from library
        //  and display in the listbox
        private List<string> GetArtistList(List<MP3> Library)
        {
            //TEMPORARY
            //CLEAR LISTBOX
            lstbArtists.Items.Clear();

            var artists =
                (
                from a in mp3Library
                select a.Artist
                ).Distinct();
            return artists.ToList();
        }

        //======================================================================================
        //=====================================EVENTS===========================================
        //======================================================================================

        //Form Load
        public Form1()
        {
            InitializeComponent();
            //Read library file to library list
            ReadLibraryFromBinaryFile(mp3Library);
            //Reflect library in current list
            currentList = mp3Library;
            //Display the current list
            DisplayList(currentList);
            //Get distinct artists from library
            artistList = GetArtistList(mp3Library);
            //display artists in listbox
            lstbArtists.Items.AddRange(artistList.ToArray());
            //Initialize the search combobox
            cboSearch.Items.Add("Artist");
            cboSearch.Items.Add("Album");
            cboSearch.Items.Add("Song");
            //Preselect first item to avoid exceptions
            cboSearch.SelectedIndex = 0;
            //load volume value into textbox
            player.settings.volume = 50;
            txtVolume.Text = player.settings.volume.ToString();
            //associate the listview with the context menu strip 
            lstvLibrary.ContextMenuStrip = contextMenuStrip1;
            lstvLibrary.Focus();
            btnNext.Enabled = false;
            btnPrevious.Enabled = false;
        }

        //Change Play button text when selection changes
        private void lstvLibrary_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (player.controls.get_isAvailable("pause"))
                btnPause.Text = "Play";
        }

        //Add folders to library
        private void btnOpen_Click(object sender, EventArgs e)
        {
            //Add user-selected tracks from folder to library
            AddFolderToLibrary();
            //Organize the updated library and replace the old one
            mp3Library = OrganizeLibrary(mp3Library);
            //Overwrite the library file with the new library list
            SaveLibraryToBinaryFile(mp3Library);
            //Reflect library changes in current list
            currentList = mp3Library;
            //Get artist list for listbox
            artistList = GetArtistList(mp3Library);
            //display artists in listbox
            lstbArtists.Items.AddRange(artistList.ToArray());
            //Redisplay the current list
            DisplayList(currentList);
        }


        //Play / Pause
        private void btnPause_Click(object sender, EventArgs e)
        {
            //If an item in the list is selected...
            if (lstvLibrary.SelectedIndices.Count > 0)
            {
                //And if the selected item is different from the
                //  currently playing track...
                int sindex = lstvLibrary.FocusedItem.Index;
                if (currentSong != currentList[sindex])
                {
                    //start the new track
                    player.URL = currentList[sindex].Path;
                    //Save reference to current track
                    currentSong = currentList[sindex];
                    DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
                    btnPause.Text = "Pause";
                    //Restart the timer
                    timer1.Start();
                }
                //Otherwise, if a track is playing...
                else if (player.controls.get_isAvailable("pause"))
                {
                    //pause the current track
                    player.controls.pause();
                    btnPause.Text = "Play";
                    timer1.Stop();
                }
                //Else (If the current track is paused
                //  and the user has not selected a different
                //  one...
                else
                {
                    //play the paused track
                    player.controls.play();
                    btnPause.Text = "Pause";
                    timer1.Start();
                }
            }
            //Play first song in current list
            else
            {
                currentSong = currentList[0];
                player.URL = currentSong.Path;
                lstvLibrary.Items[0].Focused = true;
                lstvLibrary.Items[0].Selected = true;
                DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
                timer1.Start();
            }
            lstvLibrary.Focus();
            btnNext.Enabled = true;
            btnPrevious.Enabled = true;
        }

        //Stop
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (player.controls.get_isAvailable("stop"))
            {
                player.controls.stop();
                lblCurrentlyPlaying.Text = "Nothing";
                lblPosition.Text = "00:00";
                lblDuration.Text = "00:00";
                btnPause.Text = "Play";
                timer1.Stop();
                btnNext.Enabled = false;
                btnPrevious.Enabled = false;
            }
            lstvLibrary.Focus();
        }

        //Perform search each time textbox updates
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string strSearchValue = txtSearch.Text.ToLower();
            string strSearchType = cboSearch.Text.ToLower();
            currentList = SearchLibrary(mp3Library, strSearchType, strSearchValue);
            DisplayList(currentList);
            btnPause.Text = "Play";
        }

        //context menu remove button for listview
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //Get all selected tracks with Linq statement
            List<MP3> sitems =
                (from int i in lstvLibrary.SelectedIndices
                 select currentList[i]).ToList();
            //Remove each from the currentlist and library
            foreach (MP3 track in sitems)
            {
                mp3Library.Remove(track);
                currentList.Remove(track);
            }
            //update artistlist and listbox
            artistList = GetArtistList(mp3Library);
            lstbArtists.Items.AddRange(artistList.ToArray());
            //update library file
            SaveLibraryToBinaryFile(mp3Library);
            //redisplay the current list
            DisplayList(currentList);
            lstvLibrary.Focus();
        }

        //Add selected files to library
        private void btnAddFilesToLibrary_Click(object sender, EventArgs e)
        {
            //add files to library list
            AddFilesToLibrary();
            //organize objects in list
            //  and save it that way
            mp3Library = OrganizeLibrary(mp3Library);
            //save list to library file
            SaveLibraryToBinaryFile(mp3Library);
            //reinitialize current list
            //  to reflect changes to library
            currentList = mp3Library;
            //extract artists from library
            artistList = GetArtistList(mp3Library);
            //display in listbox
            lstbArtists.Items.AddRange(artistList.ToArray());
            //redisplay the current list
            DisplayList(currentList);
            lstvLibrary.Focus();
        }

        //timer to show song position
        private void timer1_Tick(object sender, EventArgs e)
        {
            //update position and duration labels with timer tick
            //  **(duration only displays with timer tick for some reason)
            lblPosition.Text = player.controls.currentPositionString;
            lblDuration.Text = player.currentMedia.durationString;
            //When the song ends, and it is not the last song in the list...
            if (player.controls.currentPosition > player.currentMedia.duration - 2
                && currentList.IndexOf(currentSong) < currentList.Count - 1)
            {
                //get current track index
                int ctindex = currentList.IndexOf(currentSong);
                //get and play the next track
                currentSong = currentList[ctindex + 1];
                player.URL = currentSong.Path;
                //redisplay song info and select new song
                lstvLibrary.Items[ctindex].Focused = false;
                lstvLibrary.Items[ctindex].Selected = false;
                lstvLibrary.Items[ctindex + 1].Focused = true;
                lstvLibrary.Items[ctindex + 1].Selected = true;
                DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
            }
        }

        //raise volume
        private void btnVolumeUp_Click(object sender, EventArgs e)
        {
            if (player.settings.volume < 100)
            {
                player.settings.volume += 10;
                txtVolume.Text = player.settings.volume.ToString();
            }
            lstvLibrary.Focus();
        }

        //lower volume
        private void btnVolumeDown_Click(object sender, EventArgs e)
        {
            if (player.settings.volume > 0)
            {
                player.settings.volume -= 10;
                txtVolume.Text = player.settings.volume.ToString();
            }
            lstvLibrary.Focus();
        }

        //next song
        private void btnNext_Click(object sender, EventArgs e)
        {
            //When the current track is not the last in the current list
            if (currentList.IndexOf(currentSong) < currentList.Count - 1 &&
                player.controls.get_isAvailable("pause"))
            {
                //get the current track index
                int ctIndex = currentList.IndexOf(currentSong);
                //make next track the current track,
                //  select and play it
                currentSong = currentList[ctIndex + 1];
                lstvLibrary.Items[ctIndex + 1].Focused = true;
                lstvLibrary.Items[ctIndex].Selected = false;
                lstvLibrary.Items[ctIndex + 1].Selected = true;
                player.URL = currentSong.Path;
                DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
                btnPause.Text = "Pause";
            }
            lstvLibrary.Focus();
        }

        //previous song
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            //When the current track is not the first in the list
            if (currentList.IndexOf(currentSong) > 0
                && player.controls.get_isAvailable("pause"))
            {
                int ctindex = lstvLibrary.FocusedItem.Index;
                //play the previous one
                currentSong = currentList[ctindex - 1];
                lstvLibrary.Items[ctindex - 1].Focused = true;
                lstvLibrary.Items[ctindex].Selected = false;
                lstvLibrary.Items[ctindex - 1].Selected = true;
                player.URL = currentSong.Path;
                DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
                btnPause.Text = "Pause";
            }
            lstvLibrary.Focus();
        }

        //Change current list when artist is selected from artist listbox
        private void lstbArtists_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sindex = lstbArtists.SelectedIndex;
            if (sindex != -1)
            {
                string strArtist = artistList[sindex].ToLower();
                currentList = SearchLibrary(mp3Library, "artist", strArtist);
                DisplayList(currentList);
                btnNext.Enabled = false;
                btnPrevious.Enabled = false;
            }
            lstvLibrary.Focus();
            btnPause.Text = "Play";
        }

        //Reset current list to entire library
        private void button1_Click_1(object sender, EventArgs e)
        {
            currentList = mp3Library;
            DisplayList(currentList);
            lstvLibrary.Focus();
            btnPause.Text = "Play";
        }

        private void lstvLibrary_DoubleClick(object sender, EventArgs e)
        {
            int ctIndex = lstvLibrary.FocusedItem.Index;
            currentSong = currentList[ctIndex];
            player.URL = currentSong.Path;
            timer1.Start();
            DisplayTrackInfo(currentSong, lblCurrentlyPlaying);
            btnPause.Text = "Pause";
            btnNext.Enabled = true;
            btnPrevious.Enabled = true;
        }
    }
}
//To add TagLib# through NuGet "Install-Package taglib"
