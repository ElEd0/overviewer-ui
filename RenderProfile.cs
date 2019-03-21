using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    public class RenderProfile {

        public string OutPath { get; set; }
        public string JarPath { get; set; }
        public int ThreadCount { get; set; }

        public List<RenderItem> Renders { get; set; }

        public RenderProfile(string outPath, string jarPath, int threadCount, 
            List<RenderItem> renders) {
            this.OutPath = outPath;
            this.JarPath = jarPath;
            this.ThreadCount = threadCount;
            this.Renders = renders;
        }

    }
}
