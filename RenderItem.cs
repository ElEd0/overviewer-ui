using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1 {
    public class RenderItem : ListViewItem, ICloneable {

        private string renderName, name, path;
        private long lastPlayed;

        private bool renderNormal, renderLighting, renderSmoothLighting,
            renderCave, renderNight, renderSmoothNight,
            renderNether, renderNetherLighting, renderNetherSmoothLighting;
        
        private List<RegionItem> regions;

        public RenderItem(string path) {
            this.path = path;
            regions = new List<RegionItem>();
            
            NbtFile file = new NbtFile();
            file.LoadFromFile(path + "\\level.dat");
            NbtCompound root = file.RootTag;


            //clean level name so it doesnt contain any non valid symbols
            name = "";
            string levelName = root["Data"]["LevelName"].StringValue;
            for (int i = 0; i < levelName.Length; i++) {
                int c = levelName[i];
                if (c < 128 && !Utils.invalidWinChars.Contains(c))
                    name += (char) c;
            }

            lastPlayed = root["Data"]["LastPlayed"].LongValue;

            DateTime when = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(lastPlayed).ToLocalTime();
            
            Content = name + " - " + when.ToString("dd/MM/yyyy H:mm:ss");

            renderName = name + " - " + when.ToString("dd/MM/yyyy");
        }


        public string Name {
            get { return name; }
            set { this.name = value; }
        }

        public string Path {
            get { return path; }
        }

        public long LastPlayed {
            get { return lastPlayed; }
        }
        
        public string[] RenderModes {
            get {
                List<string> modes = new List<string>();
                if (renderNormal) modes.Add("normal");
                if (renderLighting) modes.Add("lighting");
                if (renderSmoothLighting) modes.Add("smooth_lighting");
                if (renderCave) modes.Add("cave");
                if (renderNight) modes.Add("night");
                if (renderNether) modes.Add("nether");
                if (renderNetherLighting) modes.Add("nether_lighting");
                if (renderNetherSmoothLighting) modes.Add("nether_smooth_lighting");
                return modes.ToArray<string>();
            }
        }

        public bool RenderNormal {
            get { return renderNormal; }
            set { this.renderNormal = value; }
        }
        public bool RenderLighting {
            get { return renderLighting; }
            set { this.renderLighting = value; }
        }
        public bool RenderSmoothLighting {
            get { return renderSmoothLighting; }
            set { this.renderSmoothLighting = value; }
        }
        public bool RenderCave {
            get { return renderCave; }
            set { this.renderCave = value; }
        }
        public bool RenderNight {
            get { return renderNight; }
            set { this.renderNight = value; }
        }
        public bool RenderSmoothNight {
            get { return renderSmoothNight; }
            set { this.renderSmoothNight = value; }
        }
        public bool RenderNether {
            get { return renderNether; }
            set { this.renderNether = value; }
        }
        public bool RenderNetherLighting {
            get { return renderNetherLighting; }
            set { this.renderNetherLighting = value; }
        }
        public bool RenderNetherSmoothLighting {
            get { return renderNetherSmoothLighting; }
            set { this.renderNetherSmoothLighting = value; }
        }

        public List<RegionItem> RegionItems {
            get { return regions; }
            set { this.regions = value; }
        }

        public string RenderName {
            get { return renderName; }
            set { this.renderName = value; }
        }

        public object Clone() {
            return new RenderItem(Path);
        }
    }
}
