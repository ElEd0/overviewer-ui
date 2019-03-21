using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace WpfApp1 {

    public enum GroupRenderByType {
        all_together, world_name, render_type
    }

    public enum OrderRenderByType {
        list_order, last_played, world_name
    }

    public enum DefaultRenderNameType {
        world_name, last_played, both
    }

    public enum BlockAreaType {
        block, chunk, region
    }

    public static class ColorCode {
        public static SolidColorBrush ERROR = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFEA3E3E"));
        public static SolidColorBrush MSG = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF3CA625"));
        public static SolidColorBrush OV = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFCFCFCF"));
    }

    public static class ErrorCode {
        public static String CORRECT = "CODE0";
        public static String PARAM_ERROR = "Invalid parameters!";
        public static String NO_WORLDS = "No Worlds selected";
        public static String IO_ERROR = "World file not found";
    }

    public static class UserSettings {
        public static DefaultRenderNameType DefaultRenderName {
            get {
                switch (Properties.Settings.Default.default_render_name) {
                    case 0: return DefaultRenderNameType.world_name;
                    case 1: return DefaultRenderNameType.last_played;
                    case 2: default: return DefaultRenderNameType.both;
                }
            }
            set {
                switch (value) {
                    case DefaultRenderNameType.world_name:
                        Properties.Settings.Default.default_render_name = 0;
                        break;
                    case DefaultRenderNameType.last_played:
                        Properties.Settings.Default.default_render_name = 1;
                        break;
                    case DefaultRenderNameType.both:
                    default:
                        Properties.Settings.Default.default_render_name = 2;
                        break;
                }
            }
        }

        public static string OverviewerPath {
            get {
                return Properties.Settings.Default.overviewer_path;
            }
            set {
                Properties.Settings.Default.overviewer_path = value;
            }
        }
    }

    public static class ConfigParams {

        // TODO
        // imgformat
        // imgquality
        // optimizeimg
        // zoom (max min default)
        // markers
        // rerender
        // render customization

        public static string FormatRenderName(string renderName, string renderType) {
            if (renderType.Equals("normal"))
                return renderName;
            else
                return renderName + " - "
                    + renderType.Substring(0, 1).ToUpper()
                    + renderType.Substring(1).Replace("_", " ");
        }

        // TODO should aim towards a more customizable render selection
        // selected dimension -> then render type
        public static string DimensionForRenderType(string renderType) {
            return renderType.Contains("nether") ? "nether" : "overworld";
        }

        public static string WorldParam(string name, string path) {
            return "worlds[\"" + name + "\"] = \"" + path.ForwardSlash() + "\"\n";
        }

        public static string RenderParam(string name, string world, string title,
            string dimension, string rendermode, List<RegionItem> regions) {
            string render = "renders[\"" + name + "\"] = {\n" +
                "\t\"world\": \"" + world + "\",\n" +
                "\t\"title\": \"" + title + "\",\n" +
                "\t\"dimension\": \"" + dimension + "\",\n" +
                "\t\"rendermode\": \"" + rendermode + "\"";
            if (regions != null && regions.Count > 0) {
                render += ",\n\t\"crop:\"[";
                for (int i = 0; i < regions.Count; i++) {
                    if (i != 0) render += ",";
                    render += "(" + regions[i].x1 + ", " + regions[i].z1 + ", " +
                        regions[i].z1 + ", " + regions[i].z2 + ")";
                }
                render += "]";
            }
            render += "\n}";
            return render;
        }

        public static string OutputParam(string path) {
            return "outputdir = \"" + path.ForwardSlash() + "\"\n";
        }

        public static string TextureParam(string path) {
            return "texturepath = \"" + path.ForwardSlash() + "\"\n";
        }

        public static string ThreadsParam(int threads) {
            return "processes = \"" + threads + "\"\n";
        }
    }

    static class Utils {

        
        public static List<int> invalidWinChars = new List<int>() {
            "/"[0], ":"[0], "*"[0], "?"[0], "\""[0], "<"[0], ">"[0], "|"[0] };

        //chunk utils
        /**
         * gets coord, returns chunk index
         * @param chIndex
         * @return rx
         */
        public static int Block2chunk(int block) {
            return (block >= 0 ? block / 16 : ((block + 1) / 16) - 1);
        }

        /**
         * gets chIndex, return region index
         * @param chIndex
         * @return rx
         */
        public static int Chunk2region(int chIndex) {
            return (chIndex >= 0 ? chIndex / 32 : ((chIndex + 1) / 32) - 1);
        }

        /**
         * gets coord, returns region index
         * @param block
         * @return rx
         */
        public static int Block2region(int block) {
            return Chunk2region(Block2chunk(block));
        }

        public static int Region2Chunk(int region) {
            return region * 32;
        }

        public static int Chunk2Block(int chunk) {
            return chunk * 16;
        }

        public static int Region2Block(int region) {
            return region * 512;
        }

        public static string BrowseFolder() {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return null;
            return dialog.SelectedPath;
        }

        public static string BrowseFile(string defaultFileName = "") {
            var dialog = new OpenFileDialog();
            dialog.FileName = defaultFileName;
            if (dialog.ShowDialog() != DialogResult.OK)
                return null;
            return dialog.FileName;
        }

        public static string SaveFile(string defaultFileName = "config.json") {
            var dialog = new SaveFileDialog();
            dialog.FileName = defaultFileName;
            if (dialog.ShowDialog() != DialogResult.OK)
                return null;
            return dialog.FileName;
        }


        //EXTENDED METHODS VVVVVVVVVVVVV
        public static void Write(this System.Windows.Controls.RichTextBox box,
            string text, SolidColorBrush color) {
            Console.WriteLine(text);
            
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd) {
                Text = text
            };

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            box.ScrollToEnd();
        }

        public static void Append(this System.Windows.Controls.RichTextBox box, string text, SolidColorBrush color) {
            box.Write(text + "\n", color);
        }

        public static void AppendMsg(this System.Windows.Controls.RichTextBox box, string text) {
            box.Write(text + "\n", ColorCode.MSG);
        }

        public static void AppendError(this System.Windows.Controls.RichTextBox box, string text) {
            box.Write("ERROR: "+text+"\n", ColorCode.ERROR);
        }

        public static void WriteError(this System.Windows.Controls.TextBox box, string text) {
            box.Clear();
            box.Foreground = ColorCode.ERROR;
            box.AppendText(text);
        }

        public static void Write(this System.Windows.Controls.TextBox box, string text) {
            box.Clear();
            box.Foreground = Brushes.Black;
            box.AppendText(text);
        }

        public static string ForwardSlash(this String s) {
            return s.Replace("\\", "/");
        }

        public static bool IsValidPath(this String s) {
            for (int i = 0; i < s.Length; i++) {
                if (i == 1 && s.ToCharArray()[i].Equals(":"[0]))//skip ':' char after disk letter
                    continue;
                if (invalidWinChars.Contains(s.ToCharArray()[i]))
                    return false;
            }
            return true;
        }

    }

}
