using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace DbViewer.Model
{
    public class Db
    {
        private static readonly string CON_STR = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"C:\\Users\\Mihay\\Documents\\MIET\\5 semester\\БД\\Lab2.accdb\"";
        private static readonly OleDbConnection cn = new OleDbConnection(CON_STR);

        public static List<string> GetTables()
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

        public static List<KeyValuePair<string, Type>> GetColumns(string tableName)
        {
            List<KeyValuePair<string, Type>> columns = new List<KeyValuePair<string, Type>>();
            cn.Open();
            OleDbDataAdapter dbAdapter = new OleDbDataAdapter($"SELECT * FROM [{tableName}]", cn);
            DataTable dataTable = new DataTable();
            dbAdapter.Fill(dataTable);
            cn.Close();
            foreach (DataColumn item in dataTable.Columns)
            {
                columns.Add(new KeyValuePair<string, Type>(item.ColumnName, item.DataType));
            }

            return columns;
        }

        public static List<List<string>> GetValuseFromTable(string tableName)
        {
            List<List<string>> result = new List<List<string>>();
            List<KeyValuePair<string, Type>> columns = GetColumns(tableName);
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM [{tableName}]";
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

        public static List<string> GetValuesFromColumn(string tableName, string columnName)
        {
            List<string> result = new List<string>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT [{columnName}] FROM [{tableName}]";
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

        public static string AddValue(string tableName, List<string> values)
        {
            List<string> columns = GetColumns(tableName).Select(x => x.Key).ToList();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"INSERT INTO [{tableName}](";
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
            catch (Exception e)
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

        public static List<string> RetrievePrimaryKeyInfo(string tableName)
        {
            List<string> primaryKey = new List<string>();
            cn.Open();
            using (DataTable dtPrimaryKeys = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] { null, null, tableName}))
            {
                for (int i = 0; i < dtPrimaryKeys.Rows.Count; i++)
                {
                    primaryKey.Add((string)dtPrimaryKeys.Rows[i][3]);
                }
            }
            cn.Close();
            return primaryKey;
        }

        public static string DeleteValue(string tableName, object[] values)
        {
            List<KeyValuePair<string, Type>> columns = GetColumns(tableName);
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"DELETE FROM [{tableName}] WHERE ";
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
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                cn.Close();
            }
        }

        public static string UpdateValue(string tableName, List<string> columns, List<string> newValues, object[] oldValues)
        {
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"UPDATE [{tableName}] SET ";
                for (int i = 0; i < columns.Count; i++)
                {
                    command += $"[{columns[i]}] = @{i}";
                    if (i < columns.Count - 1)
                    {
                        command += ", ";
                    }
                }
                command += " WHERE ";
                for (int i = 0; i < columns.Count; i++)
                {
                    command += $"[{columns[i]}] = @{i + columns.Count}";
                    if (i < columns.Count - 1)
                    {
                        command += " AND ";
                    }
                }
                cmd.CommandText = command;

                for (int i = 0; i < columns.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"{i}", newValues[i]);
                }
                for (int i = 0; i < columns.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"{i + columns.Count}", oldValues[i].ToString());
                }
                cmd.ExecuteNonQuery();
                return "201";
            }
            catch (Exception e)
            {
                return e.Message;
            }

            finally
            {
                cn.Close();
            }
        }

        public static List<KeyValuePair<string, KeyValuePair<string, string>>> RetrieveViewsInfo()
        {
            List<KeyValuePair<string, KeyValuePair<string, string>>> views = new List<KeyValuePair<string, KeyValuePair<string, string>>>();
            cn.Open();
            using (DataTable dtProcedures = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, null))
            {
                foreach (DataRow row in dtProcedures.Rows)
                {
                    views.Add(new KeyValuePair<string, KeyValuePair<string, string>>(row[2].ToString(), new KeyValuePair<string, string>(row[3].ToString(), "VIEW")));
                }
            }
            cn.Close();
            return views;
        }

        public static List<KeyValuePair<string, KeyValuePair<string, string>>> RetrieveProveduresInfo()
        {
            List<KeyValuePair<string, KeyValuePair<string, string>>> views = new List<KeyValuePair<string, KeyValuePair<string, string>>>();
            cn.Open();
            using (DataTable dtProcedures = cn.GetOleDbSchemaTable(OleDbSchemaGuid.Procedures, null))
            {
                foreach (DataRow row in dtProcedures.Rows)
                {
                    if ((short)row["PROCEDURE_TYPE"] == 3)
                    {
                        views.Add(new KeyValuePair<string, KeyValuePair<string, string>>(row["PROCEDURE_NAME"].ToString(),
                            new KeyValuePair<string, string>(row["PROCEDURE_DEFINITION"].ToString(), "FUNCTION")));
                    }
                    else if ((short)row["PROCEDURE_TYPE"] == 2 && !((string)row["PROCEDURE_NAME"]).Contains("~"))
                    {
                        if ((row["PROCEDURE_DEFINITION"] as string).Contains("UNION"))
                        {
                            views.Add(new KeyValuePair<string, KeyValuePair<string, string>>(row["PROCEDURE_NAME"].ToString(),
                            new KeyValuePair<string, string>(row["PROCEDURE_DEFINITION"].ToString(), "VIEW")));
                        }
                        else
                        {
                            views.Add(new KeyValuePair<string, KeyValuePair<string, string>>(row["PROCEDURE_NAME"].ToString(),
                                new KeyValuePair<string, string>(row["PROCEDURE_DEFINITION"].ToString(), "PROCEDURE")));
                        }
                    }
                }
            }
            cn.Close();
            return views;
        }

        public static List<List<string>> ExecuteView(string viewName)
        {
            List<List<string>> result = new List<List<string>>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM [{viewName}]";
                OleDbDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        result.Add(new List<string>());
                        for (int i = 0; i < rd.FieldCount; i++)
                        {
                            result[result.Count - 1].Add(rd[i].ToString());
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

        public static List<string> GetColumnNameFromView(string viewName)
        {
            List<string> columns = new List<string>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM [{viewName}]";
                OleDbDataReader rd = cmd.ExecuteReader();

                for (int i = 0; i < rd.FieldCount; i++)
                {
                    columns.Add(rd.GetName(i));
                }
                rd.Close();
                return columns;
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

        public static List<List<string>> ExecuteFunction(string funcName, List<string> valuesName, List<string> values)
        {
            List<List<string>> result = new List<List<string>>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM [{funcName}]";
                for (int i = 0; i < valuesName.Count; i++)
                {
                    cmd.Parameters.AddWithValue(valuesName[i], values[i]);
                }
                OleDbDataReader rd = cmd.ExecuteReader();
                if (rd.HasRows)
                {
                    while (rd.Read())
                    {
                        result.Add(new List<string>());
                        for (int i = 0; i < rd.FieldCount; i++)
                        {
                            result[result.Count - 1].Add(rd[i].ToString());
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
        public static List<string> GetColumnNameFromFunction(string funcName, List<string> valuesName, List<string> values)
        {
            List<string> columns = new List<string>();
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                cmd.CommandText = $"SELECT * FROM [{funcName}]";
                for (int i = 0; i < valuesName.Count; i++)
                {
                    cmd.Parameters.AddWithValue(valuesName[i], values[i]);
                }
                OleDbDataReader rd = cmd.ExecuteReader();

                for (int i = 0; i < rd.FieldCount; i++)
                {
                    columns.Add(rd.GetName(i));
                }
                rd.Close();
                return columns;
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

        public static string ExecuteProcedure(string procedureName, List<string> values)
        {
            cn.Open();
            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cn;
                string command = $"EXEC [{procedureName}] ";
                for (int i = 0; i < values.Count; i++)
                {
                    string value = values[i];
                    if (int.TryParse(value, out int res))
                    {
                        command += res;
                    }
                    else
                    {
                        command += $"\'{value}\'";
                    }
                    if (i < values.Count - 1)
                    {
                        command += ", ";
                    }
                }
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
                return "200";
            }
            catch (Exception e)
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
