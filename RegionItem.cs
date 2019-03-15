using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp1 {
    public class RegionItem : ListViewItem {

        public int x1, z1, x2, z2;

        public RegionItem(int x1, int z1, int x2, int z2, BlockAreaType type) : base() {

            switch (type) {
                case BlockAreaType.region:
                    x1 = Utils.Region2Block(x1);
                    z1 = Utils.Region2Block(z1);
                    x2 = Utils.Region2Block(x2);
                    z2 = Utils.Region2Block(z2);
                    break;
                case BlockAreaType.chunk:
                    x1 = Utils.Chunk2Block(x1);
                    z1 = Utils.Chunk2Block(z1);
                    x2 = Utils.Chunk2Block(x2);
                    z2 = Utils.Chunk2Block(z2);
                    break;
                case BlockAreaType.block:
                default: break;
            }

            this.x1 = x1;
            this.z1 = z1;
            this.x2 = x2;
            this.z2 = z2;

            Content = "x: " + x1 + " z: " + z1 + " -> x: " + x2 + " z: " + z2;
        }
    }
}
