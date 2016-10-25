using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class NautilusDatabase
    {
        private static string _connectionString;


        public NautilusDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string ExecuteFilterSql(string sql)
        {
            var queryResult = string.Empty;
            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var cmd = new OracleCommand(sql, connection) { CommandTimeout = 1 };
                try
                {
                    queryResult = (string)cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    queryResult = ex.Message;
                }
            }
            return queryResult;
        }


        public List<Filter> GetAllFilters(int startingFilterId)
        {
            var filters = new List<Filter>();

            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var sql = @"select filter_id, name, schema_table_id, where_statement 
                            from lims_sys.filter 
                            where schema_table_id is not null 
                            and filter_id >= :startingFilterId
                            order by filter_id";
                var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.AddWithValue(":startingFilterId", startingFilterId);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    filters.Add(new Filter
                    {
                        FilterId = reader.GetInt32(0),
                        Name = reader[1].ToString(),
                        TableId = reader.GetInt32(2),
                        WhereStatement = reader[3].ToString()
                    });
                }
            }

            return filters;
        }


        public Dictionary<int, Filter> GetAllFiltersDictionary(int startingFilterId = 1)
        {
            var dictionary = new Dictionary<int, Filter>();
            var filters = GetAllFilters(startingFilterId);
            foreach (var filter in filters)
            {
                dictionary.Add(filter.FilterId, filter);
            }
            return dictionary;
        }


        public List<FilterArg> GetAllFilterArgs()
        {
            var filterArgs = new List<FilterArg>();

            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var sql = @"select filter_argument_id, filter_id, argument_type, name,
                            case 
                              when argument_type in ('L', 'N', 'E') then nvl(to_char(value_number), '-1')
                              when argument_type in ('B', 'T', 'P') then nvl(value_text, 'XXX')
                              when argument_type = 'D' then 'sysdate'
                              else null
                            end
                            from lims_sys.filter_argument";
                var cmd = new OracleCommand(sql, connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    filterArgs.Add(new FilterArg
                    {
                        FilterArgId = reader.GetInt32(0),
                        FilterId = reader.GetInt32(1),
                        ArgType = reader[2].ToString(),
                        Name = reader[3].ToString(),
                        Value = reader[4].ToString()
                    });
                }
            }

            return filterArgs;
        }


        public Dictionary<int, FilterArg> GetAllFilterArgsDictionary()
        {
            var dictionary = new Dictionary<int, FilterArg>();
            var filterArgs = GetAllFilterArgs();
            foreach (var filterArg in filterArgs)
            {
                dictionary.Add(filterArg.FilterArgId, filterArg);
            }
            return dictionary;
        }


        public List<Table> GetAllTables()
        {
            var tables = new List<Table>();

            using (var connection = new OracleConnection(_connectionString))
            {
                connection.Open();
                var sql = @"select schema_table_id, database_name, extra_table_id,
                            case
                                when extra_table_id is not null then(select database_name from schema_field where schema_table_id = schema_table.schema_table_id and unique_key = 'T')
                                else null
                            end 
                            from lims_sys.schema_table";
                var cmd = new OracleCommand(sql, connection);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    tables.Add(new Table
                    {
                        TableId = reader.GetInt32(0),
                        DatabaseName = reader[1].ToString(),
                        ExtraTableId = reader.IsDBNull(2) ? null : (int?)reader.GetInt32(2),
                        UniqueKey = reader[3].ToString()
                    });
                }
            }

            return tables;
        }


        public Dictionary<int, Table> GetAllTablesDictionary()
        {
            var dictionary = new Dictionary<int, Table>();
            var tables = GetAllTables();
            foreach (var table in tables)
            {
                dictionary.Add(table.TableId, table);
            }
            return dictionary;
        }
    }
}
