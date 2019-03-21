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

        private const bool RENDER = false;

        private bool running = false;
        
        private MainWindow main;
        private RenderProfile profile;
        private string ovPath;

        private Thread ovThread;
        private Process ovProcess;

        private Stopwatch watch;

        public Launcher(MainWindow main) {
            this.main = main;

        }

        public string Launch(RenderProfile prof) {
            this.profile = prof;
            this.ovPath = UserSettings.OverviewerPath;

            main.console.AppendMsg("Launching... this may take a while, especially if you have many big worlds");

            watch = new Stopwatch();
            watch.Start();
            running = true;

            Directory.CreateDirectory(prof.OutPath);

            ovThread = new Thread(new ThreadStart(Run));
            ovThread.Start();
            main.console.Append("Starting overviewer thread", ColorCode.MSG);
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
                
                //create config file for overviewer to use
                string configFile = CreateConfig();

                main.Dispatcher.Invoke(() => {
                    main.console.AppendMsg("Created config file in " + ovPath + "\\" + configFile);
                });

                //create overviewer process
                ovProcess = new Process {
                    StartInfo = new ProcessStartInfo() {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "cmd.exe",
                        WorkingDirectory = ovPath,
                        Arguments = "/C overviewer.exe --config=" + configFile,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                if (RENDER) {
                    main.Dispatcher.Invoke(() => {
                        main.console.AppendMsg("Starting Overviewer process using "
                            + (profile.ThreadCount < 1 ? "all" : "" + profile.ThreadCount)
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
                            if (int.TryParse(line.Split('.')[1].Substring(2).Split('%')[0],
                                out int perc))
                                main.SetProgress(perc);

                            if (line.Contains("100% complete."))
                                main.FinishProgress();
                        }
                    });

                }

            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                main.Dispatcher.Invoke(() => {
                    main.console.AppendError("Exception during Launch: " + e.Message);
                });
            } finally {
                if(running) {
                    Finish();
                }
            }

        }

        public void Finish() {
            running = false;

            string finishMsg = "";
            if (watch != null) {
                //end timer
                watch.Stop();
                TimeSpan time = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
                finishMsg = "Done in : " + time.ToString(@"hh\:mm\:ss\:fff");
                watch.Reset();
            }
            main.Dispatcher.Invoke(() => {
                main.Finish(finishMsg);
            });
        }


        private string CreateConfig() {
            List<string> worldDeclarations = new List<string>();
            List<string> renderDeclarations = new List<string>();

            int wrldCount = 0;

            foreach (RenderItem wrld in profile.Renders) {
                string name = "w" + wrldCount;
                int renderCount = 0;
                worldDeclarations.Add(ConfigParams.WorldParam(name, wrld.Path));
                foreach (string mode in wrld.RenderModes) {
                    string renderName = name + ".r" + renderCount;
                    string renderTitle = ConfigParams.FormatRenderName(wrld.RenderName, mode);
                    string renderDimension = ConfigParams.DimensionForRenderType(mode);
                    renderDeclarations.Add(ConfigParams.RenderParam(
                        renderName, name, renderTitle, renderDimension, mode, wrld.RegionItems
                    ));
                    renderCount++;
                }
                wrldCount++;
            }

            string cfg = "";

            foreach (string w in worldDeclarations)
                cfg += w;
            cfg += "\n";
            foreach (string r in renderDeclarations)
                cfg += r;
            cfg += "\n";

            cfg += ConfigParams.OutputParam(profile.OutPath);

            if (profile.JarPath != null)
                cfg += ConfigParams.TextureParam(profile.JarPath);

            if (profile.ThreadCount > 0)
                cfg += ConfigParams.ThreadsParam(profile.ThreadCount);

            try {
                string configName = "ovconfig" + DateTime.Now.ToFileTime() + ".txt";
                File.WriteAllText(
                    ovPath + "\\" + configName, cfg);
                return configName;
            } catch (IOException e) {
                throw;
            }
        }
        
    }
}
