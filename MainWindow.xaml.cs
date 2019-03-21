using fNbt;
using Newtonsoft.Json;
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


        private TextBox out_pathBox, threadsBox, jar_pathBox,
            ov_pathBox, render_nameBox;
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

            out_pathBox = (TextBox) FindName("out_path_box");
            jar_pathBox = (TextBox) FindName("jar_path_box");
            threadsBox = (TextBox) FindName("process_count");

            groupRenderBy_allTogether = (RadioButton) FindName("groupBy_all_together");
            groupRenderBy_worldName = (RadioButton) FindName("groupBy_world_name");
            groupRenderBy_renderType = (RadioButton) FindName("groupBy_render_type");
            orderRenderBy_listOrder = (RadioButton)FindName("orderBy_list_order");
            orderRenderBy_lastPlayed = (RadioButton)FindName("orderBy_last_played");
            orderRenderBy_worldName = (RadioButton)FindName("orderBy_world_name");
            order_ascendent = (RadioButton)FindName("orderByOrder_asc");
            order_descendent = (RadioButton)FindName("orderByOrder_desc");

            worlds = (ListView) FindName("world_list");
            // drag and drop
            new WPF.JoshSmith.ServiceProviders.UI
                .ListViewDragDropManager<RenderItem>(worlds);

            render_nameBox = (TextBox)FindName("render_name");
            renderNormal = (CheckBox)FindName("render_normal");
            renderLighting = (CheckBox)FindName("render_lighting");
            renderSmoothLighting = (CheckBox)FindName("render_smooth_lighting");
            renderCave = (CheckBox)FindName("render_cave");
            renderNight = (CheckBox)FindName("render_night");
            renderSmoothNight = (CheckBox)FindName("render_smooth_night");
            renderNether = (CheckBox)FindName("render_nether");
            renderNetherLighting = (CheckBox)FindName("render_nether_lighting");
            renderNetherSmoothLighting = (CheckBox)FindName("render_nether_smooth_lighting");


            regions = (ListView) FindName("region_list");
            x1 = (TextBox) FindName("rx1");
            z1 = (TextBox) FindName("rz1");
            x2 = (TextBox) FindName("rx2");
            z2 = (TextBox) FindName("rz2");
            type = (ComboBox) FindName("rcb");

            ov_pathBox = (TextBox)FindName("ov_path_box");
            defaultRenderName_worldName = (RadioButton)FindName("defaultRenderName_world_name");
            defaultRenderName_lastPlayed = (RadioButton)FindName("defaultRenderName_last_played");
            defaultRenderName_both = (RadioButton)FindName("defaultRenderName_both_id");

            console = (RichTextBox) FindName("console_box");
            bar = (ProgressBar) FindName("progress");

            Button
                browseOutput = (Button)FindName("browse_out"),
                browseJar = (Button)FindName("browse_jar"),
                addWorlds = (Button)FindName("browse_in"),
                removeWorld = (Button)FindName("remove_world"),
                copyWorld = (Button)FindName("copy_world"),
                addBlockArea = (Button)FindName("add_region"),
                removeBlockArea = (Button)FindName("remove_region"),
                copyAllSettings = (Button)FindName("copy_settings"),
                browseOverviewer = (Button)FindName("browse_ov"),
                downloadOverviewer = (Button)FindName("download_ov"),
                loadConfig = (Button)FindName("load_config"),
                saveConfig = (Button)FindName("save_config");

            launch = (Button) FindName("launch_button");
            stop = (Button) FindName("stop_button");

            browseOutput.Click += BrowseOutputPath;
            browseJar.Click += BrowseJarPath;
            addWorlds.Click += BrowseWorlds;
            removeWorld.Click += RemoveWorld;
            copyWorld.Click += CopyWorld;
            addBlockArea.Click += AddRegion;
            removeBlockArea.Click += RemoveRegion;
            copyAllSettings.Click += CopyToAll;
            browseOverviewer.Click += BrowseOverviewerPath;
            downloadOverviewer.Click += DownloadOverviewerAsync;
            // loadConfig.Click += 
            // saveConfig.Click +=

            launch.Click += Launch;
            stop.Click += Stop;

            bar.Maximum = 100;

            // retrieve settings if any
            
            OverviewerPath = UserSettings.OverviewerPath;
            DefaultRenderName = UserSettings.DefaultRenderName;
            
        }

        private void SetFocusWorld(RenderItem item) {
            RenderName = item.RenderName;

            RenderNormal = item.RenderNormal;
            RenderLighting = item.RenderLighting;
            RenderSmoothLighting = item.RenderSmoothLighting;
            RenderCave = item.RenderCave;
            RenderNight = item.RenderNight;
            RenderSmoothNight = item.RenderSmoothNight;
            RenderNether = item.RenderNether;
            RenderNetherLighting = item.RenderNetherLighting;
            RenderNetherSmoothLighting = item.RenderNetherSmoothLighting;

            RegionList = item.RegionItems;
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
                    item.RenderNormal = current.RenderNormal;
                    item.RenderLighting = current.RenderLighting;
                    item.RenderSmoothLighting = current.RenderSmoothLighting;
                    item.RenderCave = current.RenderCave;
                    item.RenderNight = current.RenderNight;
                    item.RenderSmoothNight = current.RenderSmoothNight;
                    item.RenderNether = current.RenderNether;
                    item.RenderNetherLighting = current.RenderNetherLighting;
                    item.RenderNetherSmoothLighting = current.RenderNetherSmoothLighting;

                    item.RegionItems = current.RegionItems;
                }
            }
        }

        public void BrowseOverviewerPath(object sender, RoutedEventArgs e) {
            string path = Utils.BrowseFolder();
            if (path != null) {
                if (ValidateOverviewer(path)) {
                    OverviewerPath = path;
                    console.Append("Overviewer path: " + path, ColorCode.MSG);
                } else {
                    ov_pathBox.WriteError("overviewer.exe not found");
                }
            }
        }

        public bool ValidateOverviewer(string path) {
            if (path == null || path.Equals(""))
                return false;
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
                if (file.Name.Equals("overviewer.exe"))
                    return true;
            return false;
        }


        public async void DownloadOverviewerAsync(object sender, RoutedEventArgs e) {
            using (WebClient client = new WebClient()) {
                JObject downloads = JObject.Parse(await client
                    .DownloadStringTaskAsync("http://overviewer.org/downloads.json"));
                string url = (string) downloads["win64"]["url"];
                System.Diagnostics.Process.Start(url);
            }
        }

        public void SaveConfig() {
            RenderProfile prof = GenerateProfile();
            if (prof == null) {
                return;
            }
            string filePath = Utils.SaveFile("config.json");
            if (filePath != null) {
                string json = JsonConvert.SerializeObject(prof);
                File.WriteAllText(filePath, json);
            }
        }

        public void LoadConfig() {
            string filePath = Utils.BrowseFile("config.json");
            if (filePath != null) {
                string json = File.ReadAllText(filePath);
                RenderProfile saved = JsonConvert.DeserializeObject<RenderProfile>(json);
                OutputPath = saved.OutPath;
                MinecraftJar = saved.JarPath;
                ThreadCount = saved.ThreadCount;
                // TODO continue
            }
        }

        public RenderProfile GenerateProfile() {
            int pCount = ThreadCount;
            string outPath = OutputPath;
            List<RenderItem> worlds = Renders;
            if (pCount < 0) {
                console.AppendError("Invalid thread count");
                return null;
            }
            if (outPath == null || outPath.Equals("") || !outPath.IsValidPath()) {
                console.AppendError("An output folder must be specified");
                return null;
            }
            if (worlds.Count < 1) {
                console.AppendError("No worlds selected!");
                return null;
            }

            SortWorlds(worlds, OrderRenderBy, OrderReverse);

            return new RenderProfile(outPath, MinecraftJar, pCount, worlds);
        }

        public void Launch(object sender, RoutedEventArgs e) {

            if (!ValidateOverviewer(OverviewerPath)) {
                console.AppendError("overviewer.exe not found");
                return;
            }

            RenderProfile prof = GenerateProfile();
            if (prof == null) {
                return;
            }

            string code = "";
            try {
                code = launcher.Launch(prof);
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

        public void Finish(string finishMsg) {
            console.AppendMsg(finishMsg);
            ToggleLaunchStop(false);
            ResetProgress();
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

        private void SortWorlds(List<RenderItem> worlds,
            OrderRenderByType orderType, bool orderReversed) {

            switch (OrderRenderBy) {
                case OrderRenderByType.list_order:
                    break;
                case OrderRenderByType.world_name:
                    worlds.Sort(
                        delegate (RenderItem i1, RenderItem i2) {
                            string n1 = i1.Name, n2 = i2.Name;
                            if (orderReversed) {
                                string aux = n1;
                                n1 = n2;
                                n2 = aux;
                            }
                            return n1.CompareTo(n2);
                        }
                    );
                    break;
                case OrderRenderByType.last_played:
                    worlds.Sort(
                        delegate (RenderItem i1, RenderItem i2) {
                            long lp1 = i1.LastPlayed, lp2 = i2.LastPlayed;
                            if (orderReversed) {
                                long aux = lp1;
                                lp1 = lp2;
                                lp2 = aux;
                            }
                            return (int) (lp1 - lp2);
                        }
                    );
                    break;
            }
        }
        
        
        public bool IsWorld(string path) {
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
                if (file.Name.Equals("level.dat")) // check if contains level.dat therefore = world file
                    return true;
            return false;
        }

        private void WorldListKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                RemoveWorld(null, null);
            }
        }

        public void WorldListLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is RenderItem item && item.IsSelected) {
                SetFocusWorld(item);
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

        public List<RenderItem> Renders {
            get {
                List<RenderItem> result = new List<RenderItem>();
                foreach (RenderItem item in worlds.Items)
                    result.Add(item);
                return result;
            }
            set {
                worlds.Items.Clear();
                foreach (RenderItem item in value)
                    worlds.Items.Add(item);
            }
        }

        public RenderItem SelectedRender {
            get { return (RenderItem)worlds.SelectedItem; }
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
                return null;
            }
        }

        public List<RegionItem> RegionList {
            get {
                ItemCollection items = regions.Items;
                List<RegionItem> result = new List<RegionItem>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    result.Add((RegionItem) items.GetItemAt(i));
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


    }
}
