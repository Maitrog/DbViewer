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
