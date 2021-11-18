using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbViewer.Model
{
    public class ForeignKey
    {
        public string MasterTableName { get; internal set; }
        public List<ForeignKeyColumn> Columns { get; set; } = new List<ForeignKeyColumn>();
    }
}
