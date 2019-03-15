using fNbt;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    ///
    public partial class MainWindow : Window {

        
        private Launcher launcher;


        private TextBox ov_pathBox, out_pathBox, threadsBox, jar_pathBox, render_nameBox;
        private ProgressBar bar;
        public RichTextBox console;
        private ListView worlds, regions;
        private CheckBox renderNormal, renderLighting, renderSmoothLighting,
            renderCave, renderNight, renderSmoothNight,
            renderNether, renderNetherLighting, renderNetherSmoothLighting;
        private Button launch, stop;

        private RadioButton defaultRenderName_worldName,
            defaultRenderName_lastPlayed, defaultRenderName_both;
        private RadioButton groupRenderBy_allTogether,
            groupRenderBy_worldName, groupRenderBy_renderType;
        private RadioButton orderRenderBy_listOrder,
            orderRenderBy_lastPlayed, orderRenderBy_worldName;
        private RadioButton order_ascendent, order_descendent;

        private TextBox x1, z1, x2, z2;
        private ComboBox type;

        public MainWindow() {
            InitializeComponent();

            launcher = new Launcher(this);

            ov_pathBox = (TextBox) FindName("ov_path_box");
            out_pathBox = (TextBox) FindName("out_path_box");
            jar_pathBox = (TextBox) FindName("jar_path_box");
            threadsBox = (TextBox) FindName("process_count");

            worlds = (ListView) FindName("world_list");

            regions = (ListView) FindName("region_list");
            x1 = (TextBox) FindName("rx1");
            z1 = (TextBox) FindName("rz1");
            x2 = (TextBox) FindName("rx2");
            z2 = (TextBox) FindName("rz2");
            type = (ComboBox) FindName("rcb");

            console = (RichTextBox) FindName("console_box");
            bar = (ProgressBar) FindName("progress");


            launch = (Button) FindName("launch_button");
            stop = (Button) FindName("stop_button");
            Button loadConfig = (Button) FindName("load_config");
            Button saveConfig = (Button)FindName("save_config");

            Button browseOv = (Button) FindName("browse_ov");
            Button downloadOv = (Button)FindName("download_ov");
            Button browseInput = (Button) FindName("browse_in");
            Button browseOutput = (Button) FindName("browse_out");
            Button add = (Button) FindName("add_region");
            Button removeRegion = (Button) FindName("remove_region");
            Button removeWorld = (Button) FindName("remove_world");

            browseOv.Click += BrowseOvPath;
            browseInput.Click += BrowseInputPath;
            browseOutput.Click += BrowseOutputPath;
            add.Click += AddRegion;
            removeRegion.Click += RemoveRegion;
            removeWorld.Click += RemoveWorld;
            launch.Click += Launch;
            stop.Click += Stop;

            bar.Maximum = 100;

            //TEMP  VVVVVVVVVVVVVVVV

            // retrieve settings if any
            
            OverviewerPath = UserSettings.OverviewerPath;
            DefaultRenderName = UserSettings.DefaultRenderName;

            //out_pathBox.Text = "E:\\Escritorio\\OUT\\test137";
            //ov_pathBox.Text = "E:\\Documentos\\Minecraft tools\\overviewer-0.12.137";
            //
            //AddWorlds("E:\\Escritorio\\worldtest");

        }

        public RenderItem SelectedRender {
            get { return (RenderItem) worlds.SelectedItem; }
        }

        public string OutputPath {
            get { return out_pathBox.Text; }
            set { out_pathBox.Text = value; }
        }

        public string MinecraftJar {
            get { return jar_pathBox.Text; }
            set { jar_pathBox.Text = value; }
        }

        public int ThreadCount {
            get {
                if (!Int32.TryParse(threadsBox.Text, out int count))
                    return -1;
                else if (count < 1)
                    return -1;
                return count;
            }
            set {
                threadsBox.Text = value + "";
            }
        }

        public GroupRenderByType GroupRenderBy {
            get {
                if (groupRenderBy_allTogether.IsChecked.Value)
                    return GroupRenderByType.all_together;
                else if (groupRenderBy_worldName.IsChecked.Value)
                    return GroupRenderByType.world_name;
                else if (groupRenderBy_renderType.IsChecked.Value)
                    return GroupRenderByType.render_type;
                else return GroupRenderByType.all_together;
            }
            set {
                switch (value) {
                    case GroupRenderByType.all_together: groupRenderBy_allTogether.IsChecked = true; break;
                    case GroupRenderByType.world_name: groupRenderBy_worldName.IsChecked = true; break;
                    case GroupRenderByType.render_type: groupRenderBy_renderType.IsChecked = true; break;
                    default: groupRenderBy_allTogether.IsChecked = true; break;
                }
            }
        }

        public OrderRenderByType OrderRenderBy {
            get {
                if (orderRenderBy_listOrder.IsChecked.Value)
                    return OrderRenderByType.list_order;
                else if (orderRenderBy_lastPlayed.IsChecked.Value)
                    return OrderRenderByType.last_played;
                else if (orderRenderBy_worldName.IsChecked.Value)
                    return OrderRenderByType.world_name;
                else
                    return OrderRenderByType.last_played;
            }
            set {
                switch (value) {
                    case OrderRenderByType.list_order: orderRenderBy_listOrder.IsChecked = true; break;
                    case OrderRenderByType.last_played: orderRenderBy_lastPlayed.IsChecked = true; break;
                    case OrderRenderByType.world_name: orderRenderBy_worldName.IsChecked = true; break;
                    default: orderRenderBy_lastPlayed.IsChecked = true; break;
                }
            }
        }

        public bool OrderReverse {
            get {
                return order_ascendent.IsChecked.Value;
            }
            set {
                if (value) order_ascendent.IsChecked = true;
                else order_descendent.IsChecked = true;
            }
        }

        public string RenderName {
            get { return render_nameBox.Text; }
            set { render_nameBox.Text = value; }
        }

        public bool RenderNormal {
            get { return renderNormal.IsChecked.Value; }
            set { renderNormal.IsChecked = value; }
        }
        public bool RenderLighting {
            get { return renderLighting.IsChecked.Value; }
            set { renderLighting.IsChecked = value; }
        }
        public bool RenderSmoothLighting {
            get { return renderSmoothLighting.IsChecked.Value; }
            set { renderSmoothLighting.IsChecked = value; }
        }

        public bool RenderCave {
            get { return renderCave.IsChecked.Value; }
            set { renderCave.IsChecked = value; }
        }
        public bool RenderNight {
            get { return renderNight.IsChecked.Value; }
            set { renderNight.IsChecked = value; }
        }
        public bool RenderSmoothNight {
            get { return renderSmoothNight.IsChecked.Value; }
            set { renderSmoothNight.IsChecked = value; }
        }

        public bool RenderNether {
            get { return renderNether.IsChecked.Value; }
            set { renderNether.IsChecked = value; }
        }
        public bool RenderNetherLighting {
            get { return renderNetherLighting.IsChecked.Value; }
            set { renderNetherLighting.IsChecked = value; }
        }
        public bool RenderNetherSmoothLighting {
            get { return renderNetherSmoothLighting.IsChecked.Value; }
            set { renderNetherSmoothLighting.IsChecked = value; }
        }

        public BlockAreaType BlockSelectionMode {
            get {
                switch (type.SelectedValue.ToString()) {
                    case "Region": return BlockAreaType.region;
                    case "Chunk": return BlockAreaType.chunk;
                    case "Block": return BlockAreaType.block;
                    default: return BlockAreaType.block;
                }
            }
        }

        public int[] BlockSelectionCoords {
            get {
                if (Int32.TryParse(this.x1.Text, out int x1) && Int32.TryParse(this.z1.Text, out int z1) &&
                    Int32.TryParse(this.x2.Text, out int x2) && Int32.TryParse(this.z2.Text, out int z2)) {
                    return new int[] {
                        x1, z1, x2, z2
                    };
                }
                //parse failed
                 //console.AppendError("Couldn't parse values... Use only numbers please");
                return null;
            }
        }

        public RegionItem[] RegionList {
            get {
                ItemCollection items = regions.Items;
                RegionItem[] result = new RegionItem[items.Count];
                for (int i = 0; i < items.Count; i++)
                    result[i] = (RegionItem) items.GetItemAt(i);
                return result;
            }
            set {
                regions.Items.Clear();
                foreach (RegionItem region in value)
                    regions.Items.Add(region);
            }
        }

        public string OverviewerPath {
            get { return ov_pathBox.Text; }
            set { ov_pathBox.Text = value; }
        }

        public DefaultRenderNameType DefaultRenderName {
            get {
                if (defaultRenderName_worldName.IsChecked.Value)
                    return DefaultRenderNameType.world_name;
                else if (defaultRenderName_lastPlayed.IsChecked.Value)
                    return DefaultRenderNameType.last_played;
                else if (defaultRenderName_both.IsChecked.Value)
                    return DefaultRenderNameType.both;
                else return DefaultRenderNameType.both;
            }
            set {
                switch (value) {
                    case DefaultRenderNameType.world_name: defaultRenderName_worldName.IsChecked = true; break;
                    case DefaultRenderNameType.last_played: defaultRenderName_lastPlayed.IsChecked = true; break;
                    case DefaultRenderNameType.both: defaultRenderName_both.IsChecked = true; break;
                    default: defaultRenderName_both.IsChecked = true; break;
                }
            }
        }

        private void BrowseOutputPath(object sender, RoutedEventArgs e) {
            string path = Utils.BrowseFolder();
            if (path != null) {
                out_pathBox.Text = path;
                console.Append("Output path: " + path, ColorCode.MSG);
            }
        }

        private void BrowseJarPath(object sender, RoutedEventArgs e) {
            string path = Utils.BrowseFile();
            if (path != null) {
                if (path.EndsWith(".jar")) {
                    jar_pathBox.Text = path;
                    console.Append("Minecraft .jar: " + path, ColorCode.MSG);
                } else {
                    console.AppendError("Minecraft .jar not found");
                }
            }
        }

        private void BrowseWorlds(object sender, RoutedEventArgs e) {
            string path = Utils.BrowseFolder();
            if (path != null) {
                int browsed = AddWorlds(path);
            }
        }

        private void RemoveWorld(object sender, RoutedEventArgs e) {
            try {
                worlds.Items.RemoveAt(worlds.SelectedIndex);
            } catch (ArgumentOutOfRangeException) {
            }
        }

        private void CopyWorld(object sender, RoutedEventArgs e) {
            try {
                worlds.Items.Add(SelectedRender.Clone());
            } catch (Exception) {
            }
        }

        private void AddRegion(object sender, RoutedEventArgs e) {

            int[] co = BlockSelectionCoords;

            if (co == null) {
                console.AppendError("Invalid selection... Use only numbers please");
            } else {
                this.x1.Clear();
                this.x2.Clear();
                this.z1.Clear();
                this.z2.Clear();

                RegionItem item = new RegionItem(
                    co[0], co[1], co[2], co[3], BlockSelectionMode);

                regions.Items.Add(item);
                RenderItem render = SelectedRender;
                render.RegionItems.Add(item);
            }

        }

        private void RemoveRegion(object sender, RoutedEventArgs e) {
            try {
                regions.Items.RemoveAt(regions.SelectedIndex);
            } catch (ArgumentOutOfRangeException) {
                return;
            }
        }

        private void CopyToAll(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to replace the configuration in all " +
                worlds.Items.Count + " worlds with the current values?",
                "Copy world configuration", MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                RenderItem current = SelectedRender;
                foreach (RenderItem item in worlds.Items) {
                    item.RegionItems = current.RegionItems;
                    item.RenderNormal = current.RenderNormal;
                    item.RenderLighting = current.RenderLighting;
                    item.RenderSmoothLighting = current.RenderSmoothLighting;
                    item.RenderCave = current.RenderCave;
                    item.RenderNight = current.RenderNight;
                    item.RenderSmoothNight = current.RenderSmoothNight;
                    item.RenderNether = current.RenderNether;
                    item.RenderNetherLighting = current.RenderNetherLighting;
                    item.RenderNetherSmoothLighting = current.RenderNetherSmoothLighting;
                }
            }
        }

        public void BrowseOverviewerPath(object sender, RoutedEventArgs e) {
            string path = Utils.BrowseFolder();
            if (path != null) {
                foreach (FileInfo file in new DirectoryInfo(path).GetFiles()) {
                    if (file.Name.Equals("overviewer.exe")) {
                        OverviewerPath = path;
                        console.Append("Overviewer path: " + path, ColorCode.MSG);
                        return;
                    }
                }

                ov_pathBox.WriteError("overviewer.exe not found");
            }
        }


        public async Task DownloadOverviewerAsync(object sender, RoutedEventArgs e) {
            JObject downloads = JObject.Parse(await new WebClient()
                .DownloadStringTaskAsync("http://overviewer.org/downloads.json"));
            string url = (string) downloads["win64"]["url"];
            System.Diagnostics.Process.Start(url);
        }

        public void Launch(object sender, RoutedEventArgs e) {

            if (!Int32.TryParse(this.threadsBox.Text, out int pCount)){
                console.AppendError("Invalid thread count");
                return;
            }
            string code = "";
            try {
                code = launcher.Launch(ov_pathBox.Text, out_pathBox.Text, pCount);
                if (!code.Equals(ErrorCode.CORRECT))
                    console.AppendError(code);
                else
                    ToggleLaunchStop(true);
            } catch (Exception ex) {
                Stop(null, null);
            }

        }

        public void Stop(object sender, RoutedEventArgs e) {
            launcher.Stop();
        }

        /// <summary>
        /// Toggles the launch stop buttons
        /// </summary>
        /// <param name="state">true if launching, false if stopping</param>
        public void ToggleLaunchStop(bool state) {

            Image emeraldOn = (Image) launch.Template.FindName("img0", launch);
            Image emeraldOff = (Image) launch.Template.FindName("img1", launch);

            Image redstoneOn = (Image) stop.Template.FindName("img0", stop);
            Image redstoneOff = (Image) stop.Template.FindName("img1", stop);

            Visibility vis0 = Visibility.Collapsed, vis1 = Visibility.Visible;

            if (state) {
                Visibility aux = vis0;
                vis0 = vis1;
                vis1 = aux;
            }

            emeraldOn.Visibility = vis1;
            emeraldOff.Visibility = vis0;

            redstoneOn.Visibility = vis0;
            redstoneOff.Visibility = vis1;
        }
        

        /// <summary>
        /// Inserts all worlds contained inside the directory
        /// It cascades over all of its sub directories
        /// </summary>
        /// <param name="path">Initial folder path</param>
        /// <returns>Total browsed worlds</returns>
        public int AddWorlds(string path) {
            int result = 0;
            if (IsWorld(path)) {
                try {
                    RenderItem item = new RenderItem(path);
                    worlds.Items.Add(item);
                    //console.Append("World added: " + item.Content, ColorCode.MSG);
                    result++;
                } catch (Exception e) {
                    console.AppendError("World at " + path + " seems corrupt. Is the level.dat correct?");
                }
            } else {
                foreach (DirectoryInfo dir in new DirectoryInfo(path).GetDirectories())
                    result += AddWorlds(dir.FullName);
            }
            return result;
        }


        private void SortWorlds() {
            List<WorldItem> ret = new List<WorldItem>(worlds.Items.Count);
            for (int i = 0; i < worlds.Items.Count; i++)
                ret.Add((WorldItem)worlds.Items.GetItemAt(i));

            ret.Sort(
                delegate (WorldItem i1, WorldItem i2) {
                        //date gets reversed (yyyy-mm-dd) so it gets ordered correctly
                        string[] a1 = i1.Date.Split(new char[] { '-' });
                    string[] a2 = i2.Date.Split(new char[] { '-' });

                    string d1 = a1[2] + "-" + a1[1] + "-" + a1[0];
                    string d2 = a2[2] + "-" + a2[1] + "-" + a2[0];

                    return d1.CompareTo(d2);
                }
            );

            worlds.Items.Clear();
            foreach (WorldItem world in ret)
                worlds.Items.Add(world);

        }

        //PLACEHOLDER-------- TODO: ADD CHECKBOX
        private bool checkDupes = true;


        public bool IsWorld(string path) {
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
                if (file.Name.Equals("level.dat")) // check if contains level.dat therefore = world file
                    return true;
            return false;
        }
        


        public string[] GenRegions() {
            ItemCollection items = regions.Items;
            List<string> regs = new List<string>();

            for (int i = 0; i < items.Count; i++) {
                RegionItem item = (RegionItem)items.GetItemAt(i);

                int x1 = item.x1,
                    z1 = item.z1,
                    x2 = item.x2,
                    z2 = item.z2;


                //minregion and max region
                int fromX = x1 < x2 ? x1 : x2;
                int toX = x1 < x2 ? x2 : x1;
                int fromZ = z1 < z2 ? z1 : z2;
                int toZ = z1 < z2 ? z2 : z1;

                // always start from the smaller one so it must go ++
                for (int x = fromX; x <= toX; x++)
                    for (int z = fromZ; z <= toZ; z++) {
                        string r = "r." + x + "." + z + ".mca";
                        if (!regs.Contains(r))
                            regs.Add(r);
                    }
            }
            return regs.ToArray();
        }

        public List<WorldItem> Worlds {
            get {
                List<WorldItem> ret = new List<WorldItem>(worlds.Items.Count);
                for (int i = 0; i < worlds.Items.Count; i++) 
                    ret.Add((WorldItem) worlds.Items.GetItemAt(i));
                return ret;
            }
        }

        private void WorldListKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                RemoveWorld(null, null);
            }
        }

        public void AddProgress(int value = 1) {
            bar.Value += value;
        }

        public void SetProgress(int value) {
            bar.Value = value;
        }

        public void FinishProgress() {
            bar.Value = bar.Maximum;
        }

        public void ResetProgress() {
            bar.Value = 0;
        }
        
        
    }
}
