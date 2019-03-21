using fNbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1 {
    public class RenderItem : ListViewItem, ICloneable {

        public RenderItem(string path) {
            this.Path = path;
            
            NbtFile file = new NbtFile();
            file.LoadFromFile(path + "\\level.dat");
            NbtCompound root = file.RootTag;


            //clean level name so it doesnt contain any non valid symbols
            Name = "";
            string levelName = root["Data"]["LevelName"].StringValue;
            for (int i = 0; i < levelName.Length; i++) {
                int c = levelName[i];
                if (c < 128 && !Utils.invalidWinChars.Contains(c))
                    Name += (char) c;
            }

            LastPlayed = root["Data"]["LastPlayed"].LongValue;

            DateTime when = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(LastPlayed).ToLocalTime();
            
            Content = Name + " - " + when.ToString("dd/MM/yyyy H:mm:ss");

            RenderName = Name + " - " + when.ToString("dd/MM/yyyy");
        }


        public string Name { get; set; }

        public string Path { get; }

        public long LastPlayed { get; }

        public string[] RenderModes {
            get {
                List<string> modes = new List<string>();
                if (RenderNormal) modes.Add("normal");
                if (RenderLighting) modes.Add("lighting");
                if (RenderSmoothLighting) modes.Add("smooth_lighting");
                if (RenderCave) modes.Add("cave");
                if (RenderNight) modes.Add("night");
                if (RenderNether) modes.Add("nether");
                if (RenderNetherLighting) modes.Add("nether_lighting");
                if (RenderNetherSmoothLighting) modes.Add("nether_smooth_lighting");
                return modes.ToArray<string>();
            }
        }

        public bool RenderNormal { get; set; }
        public bool RenderLighting { get; set; }
        public bool RenderSmoothLighting { get; set; }
        public bool RenderCave { get; set; }
        public bool RenderNight { get; set; }
        public bool RenderSmoothNight { get; set; }
        public bool RenderNether { get; set; }
        public bool RenderNetherLighting { get; set; }
        public bool RenderNetherSmoothLighting { get; set; }

        public List<RegionItem> RegionItems { get; set; }

        public string RenderName { get; set; }

        public object Clone() {
            return new RenderItem(Path);
        }
    }
}
