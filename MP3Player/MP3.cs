using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3Player
{
    public class MP3
    {
        private string _title;
        private string _artist;
        private string _album;
        private uint _track;
        private uint _year;
        private string _genre;
        private string _path;

        public MP3(string title, string artist, string album, uint track, uint year, string genre, string path)
        {
            this._title = title;
            this._artist = artist;
            this._album = album;
            this._track = track;
            this._year = year;
            this._genre = genre;
            this._path = path;
        }

        public string Title { get { return _title; } }
        public string Artist { get { return _artist; } }
        public string Album { get { return _album; } }
        public uint Track { get { return _track; } }
        public uint Year { get { return _year; } }
        public string Genre { get { return _genre; } }
        public string Path { get { return _path; } }

        public override string ToString()
        {
            return String.Format("{0}  by  {1}  on  {2}   Track: {3}", Title, Artist, Album, Track);
        }
    }
}
