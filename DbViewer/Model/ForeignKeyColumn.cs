using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbViewer.Model
{
    public class ForeignKeyColumn
    {
        public ForeignKeyColumn()
        {
        }

        public string DetailColumnName { get; set; }
        public string MasterColumnName { get; set; }
    }
}
