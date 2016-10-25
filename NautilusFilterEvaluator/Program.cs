using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new NautilusDatabase(args[0]);
            var startingFilterId = args.Length > 1 ? int.Parse(args[1]) : 1;
            var filters = db.GetAllFiltersDictionary(startingFilterId);
            var filterArgs = db.GetAllFilterArgsDictionary();
            var tables = db.GetAllTablesDictionary();
            var sqlConstructor = new SqlConstructor(tables, filterArgs);
            var dataSource = new OracleConnectionStringBuilder(args[0]).DataSource;
            var comments = args.Length > 2 ? args[2] : string.Empty;

            foreach(var filter in filters)
            {
                Console.Write($"{filter.Value.FilterId} {filter.Value.Name}: querying");
                filter.Value.Sql = sqlConstructor.GetSql(filter.Value);
                var stopwatch = Stopwatch.StartNew();
                filter.Value.Status = db.ExecuteFilterSql(filter.Value.Sql);
                stopwatch.Stop();
                filter.Value.MillisecondsToComplete = stopwatch.ElapsedMilliseconds;
                LogToDatabase(filter.Value, dataSource, comments);
                Console.WriteLine($"\b\b\b\b\b\b\b\b{filter.Value.Status.PadRight(8)} {filter.Value.MillisecondsToComplete}");
            }
        }

        private static void LogToDatabase(Filter filter, string dataSource, string comments)
        {
            using (var connection = new OracleConnection("Data Source=limsdev93rac;User Id=lims_sys;Password=qOrco20"))
            {
                connection.Open();
                var sql = @"insert into filter_evaluation values (sq_filter_evaluation.nextval, :data_source, :filter_id, :name, :status, :time, :sql_text, sysdate, :comments)";
                var cmd = new OracleCommand(sql, connection);
                cmd.Parameters.AddWithValue(":data_source", dataSource);
                cmd.Parameters.AddWithValue(":filter_id", filter.FilterId);
                cmd.Parameters.AddWithValue(":name", filter.Name);
                cmd.Parameters.AddWithValue(":status", filter.Status);
                cmd.Parameters.AddWithValue(":time", filter.MillisecondsToComplete);
                cmd.Parameters.AddWithValue(":sql_text", filter.Sql);
                cmd.Parameters.AddWithValue(":comments", comments);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
