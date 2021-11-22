using System.Collections.Generic;

namespace DbViewer.Model
{
    public class ForeignKey
    {
        public string MasterTableName { get; internal set; }
        public List<ForeignKeyColumn> Columns { get; set; } = new List<ForeignKeyColumn>();
    }
}
