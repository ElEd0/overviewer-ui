using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1 {
    class RenderProfile {

        public string OutPath { get; set; }
        public string JarPath { get; set; }
        public int ThreadCount { get; set; }

        public List<RenderItem> Renders { get; set; }

        public GroupRenderByType GroupBy { get; set; }
        public OrderRenderByType OrderBy { get; set; }
        public bool OrderReversed { get; set; }

        public RenderProfile() {

        }

    }
}
