using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace netcore6demo.Utis
{
    public interface IQueryManager
    {
        T As<T>();

        IEnumerable<T> AsEnumerable<T>();

        T AsScalar<T>();

        DataTable GetDataTable();

        DataSet GetDataSet();

        int Execute();
    }

    public class QueryManager : BaseQueryManager, IQueryManager, IDisposable
    {
        public QueryManager(DbConnection dbConnection)
        {
            this.DbProviderFactory = DbProviderFactories.GetFactory(dbConnection);
            this.ConnectionString = dbConnection.ConnectionString;
        }

        public T As<T>() => this.GetDataTable().BindData<T>().FirstOrDefault();

        public IEnumerable<T> AsEnumerable<T>() => this.GetDataTable().BindData<T>();

        public T AsScalar<T>()
        {
            using (var connection = this.DbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = this.ConnectionString;
                using (var command = this.DbProviderFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = this.CommandType;
                    command.CommandText = this.CommandText;
                    command.CommandTimeout = this.CommandTimeout;
                    SetParameters(command);

                    connection.Open();
                    T ret = (T)(command.ExecuteScalar());
                    return ret;
                }
            }
        }

        public DataTable GetDataTable()
        {
            using (var connection = this.DbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = this.ConnectionString;
                using (var dataAdapter = this.DbProviderFactory.CreateDataAdapter())
                {
                    using (var command = this.DbProviderFactory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = this.CommandType;
                        command.CommandText = this.CommandText;
                        command.CommandTimeout = this.CommandTimeout;
                        SetParameters(command);

                        var dataTable = new DataTable();
                        dataAdapter.SelectCommand = command;
                        dataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        public DataSet GetDataSet()
        {
            using (var connection = this.DbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = this.ConnectionString;
                using (var dataAdapter = this.DbProviderFactory.CreateDataAdapter())
                {
                    using (var command = this.DbProviderFactory.CreateCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = this.CommandType;
                        command.CommandText = this.CommandText;
                        command.CommandTimeout = this.CommandTimeout;
                        SetParameters(command);

                        var dataSet = new DataSet();
                        dataAdapter.SelectCommand = command;
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        public int Execute()
        {
            using (var connection = this.DbProviderFactory.CreateConnection())
            {
                connection.ConnectionString = this.ConnectionString;
                using (var command = this.DbProviderFactory.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandType = this.CommandType;
                    command.CommandText = this.CommandText;
                    command.CommandTimeout = this.CommandTimeout;
                    SetParameters(command);

                    connection.Open();
                    int intCount = command.ExecuteNonQuery();
                    return intCount;
                }
            }
        }

        public void SetParameters(DbCommand command)
        {
            if (this.Parameters == null || this.Parameters.Count == 0) return;

            foreach (var parameter in this.Parameters)
            {
                var dbParameter = this.DbProviderFactory.CreateParameter();
                dbParameter.ParameterName = parameter.Name;
                dbParameter.Value = parameter.Value;
                command.Parameters.Add(dbParameter);
            }
        }

        public void Dispose()
        {
        }
    }

    public class BaseQueryManager
    {
        protected DbProviderFactory DbProviderFactory { get; set; }

        public CommandType CommandType { get; set; } = CommandType.Text;

        public int CommandTimeout { get; set; } = 60;

        protected string ConnectionString { get; set; }

        public string CommandText { get; set; }

        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public Parameter(string name, object value)
        {
            this.Name = name;
            this.Value = value ?? DBNull.Value;
        }
    }

    public static class DbManagerExtensions
    {
        public static void Show(this DataTable dataTable)
        {
            var stringBuilder = new StringBuilder();

            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    string rowContent = string.Join(" | ", dataRow.ItemArray.Select(x => x.Justify()));
                    stringBuilder.Append($"{rowContent}\n");
                }
            }

            Console.WriteLine(stringBuilder.ToString());
        }

        public static void Show(this DataSet dataSet)
        {
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    dataTable.Show();
                }
            }
        }

        public static string Justify(this object obj, int length = 14)
        {
            if (length <= 6) throw new Exception("function Justify require length greater than 6");

            if (obj == DBNull.Value)
                return "(null)" + new string(' ', length - 6);

            var objS = obj.ToString();

            if (objS.Length == length)
            {
                return objS;
            }
            else if (objS.Length > length)
            {
                return objS.Substring(0, length - 3) + "...";
            }
            else
            {
                return objS + new string(' ', length - objS.Length);
            }
        }

        private static PropertyInfo[] ValidateColumns<T>(List<string> columns)
        {
            var properties = typeof(T).GetProperties();

            var propetyNames = properties.Select(p => p.Name).ToList();

            var missColumns = columns.Where(p => !propetyNames.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();

            if (missColumns.Count > 0)
                throw new Exception($"Columns {string.Join(',', missColumns)} not map to {typeof(T).ToString()}");

            return properties;
        }

        public static IEnumerable<T> BindData<T>(this DataTable dataTable)
        {
            var columns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();

            var properties = ValidateColumns<T>(columns);

            DataRow[] rows = dataTable.Select();
            foreach (var row in rows)
            {
                var obj = Activator.CreateInstance<T>();

                foreach (var property in properties)
                {
                    if (columns.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        if (row[property.Name] != DBNull.Value)
                        {
                            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            var value = Convert.ChangeType(row[property.Name], type);
                            property.SetValue(obj, value);
                        }
                    }
                }

                yield return obj;
            }
        }
    }
}
