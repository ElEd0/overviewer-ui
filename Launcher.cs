using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfApp1 {
    class Launcher {

        //Hardcoded limit for renders, use -1 for no limit
        private const int LIMIT = -1;

        private const bool RENDER = true;

        private bool running = false;

        private string ovPath, outPath, workPath;
        private int threads;

        private string[] regions;
        private List<WorldItem> worlds;
        private MainWindow main;

        private Thread ovThread;
        private Process ovProcess;

        private Stopwatch watch;

        public Launcher(MainWindow main) {
            this.main = main;

        }

        public string Launch(string ovPath, string outPath, int threads) {
            if(ovPath == null || outPath == null ||
                ovPath.Equals("") || outPath.Equals("")) 
                return ErrorCode.PARAM_ERROR;

            if (!outPath.IsValidPath()) 
                return ErrorCode.PARAM_ERROR;
            
            this.threads = threads;
            this.ovPath = ovPath;
            this.outPath = outPath;
            this.workPath = main.WorkPath;

            regions = main.GenRegions();//get regions file names
            worlds = main.Worlds;// get worlds items

            watch = new Stopwatch();
            watch.Start();
            running = true;
            main.console.AppendMsg("Starting... this may take a while, especially if you have many worlds and/or regions");
            

            Directory.CreateDirectory(outPath);

            Console.WriteLine("directory : "+outPath+" created");

            if (!Directory.Exists(ovPath) || !Directory.Exists(outPath)) {
                Console.WriteLine("dirs dont exist");
                return ErrorCode.PARAM_ERROR;
            }                        

            ovThread = new Thread(new ThreadStart(Run));
            ovThread.Start();
            main.console.Append("Started overviewer thread", ColorCode.MSG);
            return ErrorCode.CORRECT;
        }

        public bool Stop() {

            main.console.AppendMsg("Stopping Overviewer process");

            int c = 0;
            try {
                if (ovProcess != null)
                    ovProcess.Close();
                c++;
            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                main.console.AppendError("Could not close overviewer process");
            }
            try {
                if (ovThread != null)
                    ovThread.Interrupt();
                c++;
            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                main.console.AppendError("Could not close overviewer thread");
            }

            Finish();

            if (c == 2)
                return true;
            return false;

        }


        public void Run() {
            
            try {

                if (worlds.Count < 1)
                    throw new Exception("No worlds");

                for (int i = 0; i < worlds.Count; i++)
                    if (!CreateWorld(worlds[i], i))
                        throw new Exception("Couldnt create one or more worlds");
                //create config file for overviewer to use
                CreateConfig();

                //create overviewer process
                ovProcess = new System.Diagnostics.Process {
                    StartInfo = new System.Diagnostics.ProcessStartInfo() {
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        WorkingDirectory = ovPath,
                        Arguments = "/C overviewer.exe --config=cfg.temp",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                if (RENDER) {
                    main.Dispatcher.Invoke(() => {
                        main.console.AppendMsg("Starting Overviewer process using "
                            + (threads == 0 ? "all" : ""+threads)
                            + " threads");
                    });

                    ovProcess.Start();
                }


                // print output and update progress bar
                while (!ovProcess.StandardOutput.EndOfStream) {
                    string line = ovProcess.StandardOutput.ReadLine();

                    main.Dispatcher.Invoke(() => {
                        main.console.Append("\t" + line, ColorCode.OV);
                        //set progress
                        if (line.Contains("complete")) {
                            int.TryParse(line.Split('.')[1].Substring(2).Split('%')[0], out int perc);

                            main.SetProgress(perc);

                            if (line.Contains("100% complete."))
                                main.FinishProgress();
                        }
                    });

                }

                //modifiy overviewerConfig.js to display all renders as one world
                string code = "";
                if (!(code = ModifiyOVConfig()).Equals(ErrorCode.CORRECT))
                    main.Dispatcher.Invoke(() => {
                        main.console.AppendError(code);
                    });


            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                main.Dispatcher.Invoke(() => {
                    main.console.AppendError("Exception during Launch");
                });
            } finally {
                if(running)
                    main.Dispatcher.Invoke(() => {
                        Finish();
                    });
            }

        }

        //This will always be executed in main thread
        public void Finish() {
            //always try to delete working dir, if this fails bad luck..
            try {
                Directory.Delete(workPath, true);
            } catch (DirectoryNotFoundException e) {
            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                main.console.AppendError("Failed to delete temp files, you can try to delete the directory "
                    + workPath + " manually");
            }

            main.ToggleLaunchStop(false);
            main.ResetProgress();

            if (watch == null)
                return;
            //end timer
            watch.Stop();
            running = false;
            TimeSpan time = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
            main.console.AppendMsg("Done in : " + time.ToString(@"hh\:mm\:ss\:fff"));
            watch.Reset();
        }


        private string ModifiyOVConfig() {
            try {
                string text = File.ReadAllText(outPath + "\\overviewerConfig.js");

                // sort world items in descending order by length of index
                // for example:
                //   3   world.012
                //   2   world.18
                //   2   world.34
                //   1   world.1
                //   0   world
                // This way longer strings will be replaced before than shorter ones
                // so that the shorter ones dont interfere
                // ('world.12' would replace 'world.123' into 'xxx3')

                worlds.Sort(
                    delegate (WorldItem i1, WorldItem i2) {
                        string s1, s2;
                        try {
                            s1 = i1.NameWIndex.Split(new char[] { '.' })[1];
                        } catch (Exception e) {
                            s1 = "";
                        }
                        try {
                            s2 = i2.NameWIndex.Split(new char[] { '.' })[1];
                        } catch (Exception e) {
                            s2 = "";
                        }

                        return s1.Length > s2.Length ? -1 : s1.Length < s2.Length ? 1 : 0;
                    }
                );
                //replace world names by wrld
                foreach (WorldItem world in worlds)
                    text = text.Replace(world.NameWIndex, "wrld");

                //remove all world definitions in 'worlds' array except last one
                bool done = false;
                string op = "";
                foreach (string line in text.Split('\n')) {
                    if (!done) {
                        if (!line.Contains("\"wrld\","))
                            op += line + "\n";
                    } else {
                        op += line + "\n";
                        continue;
                    }
                    if (line.Contains("],"))
                        done = true;
                    
                }

                //write all text into file
                File.WriteAllText(outPath + "\\overviewerConfig.js", op);
                return ErrorCode.CORRECT;
            } catch (Exception ex) when (ex is DirectoryNotFoundException || ex is FileNotFoundException) {
                return "overviewerConfig.js not found! Did render end correctly?";
            } catch (IOException e) {
                Console.WriteLine(e.StackTrace);
                return "Error creating overviewerConfig.js";
            }

        }

        private void CreateConfig() {
            int count = 0;
            //create config file
            string cfg = "";
            foreach (WorldItem world in worlds) {
                count++;
                if (LIMIT != -1 && count > LIMIT)
                    break;
                cfg += ConfigParameters.WorldParam(world.NameWIndex, world.WorkPath);
                cfg += ConfigParameters.RenderParam(world.DateWIndex, world.NameWIndex, world.Date);
            }

            cfg += ConfigParameters.OutputParam(outPath);
            //TODO texture param

            cfg += ConfigParameters.TextureParam("1.8.jar");

            if (threads != 0)
                cfg += ConfigParameters.ThreadsParam(threads);

            try {
                File.WriteAllText(ovPath + "\\cfg.temp", cfg);
            } catch (IOException e) {
                throw;
            }
        }

        /// <summary>
        /// Creates the working files needed to render the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="index"></param>
        /// <returns>true if </returns>
        public bool CreateWorld(WorldItem world, int index) {
            try {

                world.Index = index;

                // set working path
                world.WorkPath = workPath + "\\" + world.NameWIndex;

                //create region folder which will create all non existing parent folders (including world folder)
                DirectoryInfo regionFolder = new DirectoryInfo(world.WorkPath + "\\region");
                regionFolder.Create();

                foreach (FileInfo region in regionFolder.EnumerateFiles())
                    region.Delete();

                

                //copy level.dat
                try {
                    File.Delete(world.WorkPath + "\\level.dat");
                } catch (DirectoryNotFoundException) { }
                File.Copy(world.Path + "\\level.dat",
                    world.WorkPath + "\\level.dat");

                
                string[] regs = regions;

                //if no regions in list copy all of them
                if(regions.Length == 0) {
                    FileInfo[] files = new DirectoryInfo(world.Path + "\\region").GetFiles();
                    regs = new string[files.Length];
                    for (int i=0; i<regs.Length; i++)
                        regs[i] = files[i].Name;
                }

                //copy regions
                foreach (string r in regs) {
                    try {
                        try {
                            File.Delete(world.WorkPath + "\\region\\" + r);
                        } catch (DirectoryNotFoundException) { }

                        File.Copy(world.Path + "\\region\\" + r,
                            world.WorkPath + "\\region\\" + r);

                    } catch (FileNotFoundException) {
                        continue;
                    }
                }

                return true;

            } catch (System.IO.IOException e) {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }
        
    }
}
