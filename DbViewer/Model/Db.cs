using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbViewer.Model
{
    public class Db
    {
        private static string CON_STR = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"S:\\Documents\\Миша\\МИЭТ\\5 семестр\\БД\\Lab2.mdb\"";
        private static OleDbConnection cn = new OleDbConnection(CON_STR);

        public static List<string> GetTable()
        {
            List<string> tablesName = new List<string>();
            cn.Open();

            DataTable tables = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            foreach (DataRow row in tables.Rows)
            {
                tablesName.Add(row[2].ToString());
            }

            cn.Close();

            return tablesName;
        }

        public static List<KeyValuePair<string, Type>> GetColumn(string tableName)
        {
            List<KeyValuePair<string, Type>> columns = new List<KeyValuePair<string, Type>>();
            cn.Open();
            OleDbDataAdapter dbAdapter = new OleDbDataAdapter(@"SELECT * FROM " + tableName, cn);
            DataTable dataTable = new DataTable();
            dbAdapter.Fill(dataTable);
            cn.Close();
            foreach (DataColumn item in dataTable.Columns)
            {
                columns.Add(new KeyValuePair<string, Type>(item.ColumnName, item.DataType));
            }

            return columns;
        }

        public static List<List<string>> GetAllFromTable(string tableName)
        {
            List<List<string>> result = new List<List<string>>();
            List<KeyValuePair<string, Type>> columns = GetColumn(tableName);
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM {tableName}";
                OleDbDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        result.Add(new List<string>());
                        foreach (var column in columns)
                        {
                            result[result.Count - 1].Add(rd[$"{column.Key}"].ToString());
                        }
                    }

                }
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                cn.Close();
            }
        }

        public static List<string> GetColumnFromTable(string tableName, string columnName)
        {
            List<string> result = new List<string>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT [{columnName}] FROM {tableName}";
                OleDbDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        result.Add(rd[$"{columnName}"].ToString());
                    }

                }
                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                cn.Close();
            }
        }

        public static List<KeyValuePair<string, Type>> GetColumnWithoutAutoIncrement(string tableName)
        {
            List<KeyValuePair<string, Type>> columns = new List<KeyValuePair<string, Type>>();
            cn.Open();
            OleDbDataAdapter dbAdapter = new OleDbDataAdapter(@"SELECT * FROM " + tableName, cn);
            DataTable dataTable = new DataTable();
            dbAdapter.Fill(dataTable);
            cn.Close();
            foreach (DataColumn item in dataTable.Columns)
            {
                if (item.AutoIncrement == false)
                {
                    columns.Add(new KeyValuePair<string, Type>(item.ColumnName, item.DataType));
                }
            }

            return columns;
        }

        public static string AddValues(string tableName, List<string> columns, List<string> values)
        {
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"INSERT INTO {tableName}(";
                for (int i = 0; i < columns.Count; i++)
                {
                    command += $"[{columns[i]}]";
                    if (i < columns.Count - 1)
                    {
                        command += ", ";
                    }
                }
                command += ") VALUES (";
                for (int i = 0; i < columns.Count; i++)
                {
                    command += "?";
                    if (i < columns.Count - 1)
                    {
                        command += ", ";
                    }
                }
                command += ")";
                cmd.CommandText = command;

                for (int i = 0; i < columns.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"{i}", values[i]);
                }
                cmd.ExecuteNonQuery();
                return "201";
            }
            catch(Exception e)
            {
                return e.Message;
            }

            finally
            {
                cn.Close();
            }
        }
        public static List<ForeignKey> RetrieveForeignKeyInfo(string tableName)
        {
            string[] fkRestrictions = new string[] { null, null, null, null, null, tableName };
            List<ForeignKey> foreignKeys = new List<ForeignKey>();
            ForeignKey foreignKey = null;
            cn.Open();
            using (DataTable dtForeignKeys = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, fkRestrictions))
            {
                string constraintName = "";
                foreach (DataRow row in dtForeignKeys.Rows)
                {
                    string newConstraintName = (string)row["FK_NAME"];
                    if (newConstraintName != constraintName)
                    {
                        if (foreignKey != null)
                        {
                            foreignKeys.Add(foreignKey);
                        }
                        constraintName = newConstraintName;
                        foreignKey = new ForeignKey
                        {
                            MasterTableName = (string)row["PK_TABLE_NAME"]
                        };
                    }
                    ForeignKeyColumn foreignKeyColumn = new ForeignKeyColumn
                    {
                        DetailColumnName = (string)row["FK_COLUMN_NAME"],
                        MasterColumnName = (string)row["PK_COLUMN_NAME"]
                    };
                    foreignKey.Columns.Add(foreignKeyColumn);
                }
                if (foreignKey != null)
                {
                    foreignKeys.Add(foreignKey);
                }
            }
            cn.Close();
            return foreignKeys;
        }

        public static string DeleteValue(string tableName, object[] values)
        {
            List<KeyValuePair<string, Type>> columns = GetColumn(tableName);
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"DELETE FROM {tableName} WHERE ";
                for (int i = 0; i < values.Length; i++)
                {
                    command += $"[{columns[i].Key}] = @{i} ";
                    if (i != values.Length - 1)
                    {
                        command += "AND ";
                    }
                    cmd.Parameters.AddWithValue($"@{i}", values[i]);
                }
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
                return "200";
            }
            catch(Exception e)
            {
                return e.Message;
            }
            finally
            {
                cn.Close();
            }
        }
    }
}
