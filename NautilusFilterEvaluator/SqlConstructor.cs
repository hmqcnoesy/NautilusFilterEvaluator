using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NautilusFilterEvaluator
{
    class SqlConstructor
    {
        private Dictionary<int, Table> _tables;
        private Dictionary<int, FilterArg> _args;


        public SqlConstructor(Dictionary<int, Table> tables, Dictionary<int, FilterArg> args)
        {
            _tables = tables;
            _args = args;
        }


        public string GetSql(Filter filter)
        {
            var table = _tables[filter.TableId];
            var sqlSelect = "select 'ok' from dual union select distinct 'ok' ";
            var sqlFrom = "\r\nfrom lims_sys." + table.DatabaseName;
            var sqlWhere = "\r\nwhere ";

            if (table.ExtraTableId.HasValue)
            {
                var extraTable = _tables[table.ExtraTableId.Value];
                sqlFrom += ", lims_sys." + extraTable.DatabaseName;
                sqlWhere += $"lims_sys.{table.DatabaseName}.{table.UniqueKey} = lims_sys.{extraTable.DatabaseName}.{table.UniqueKey} and ";
            }

            sqlWhere += $"(\r\n{filter.WhereStatement}\r\n)";

            var sql = $"{sqlSelect}\r\n{sqlFrom}\r\n{sqlWhere}";

            foreach (var arg in _args.Where(a => a.Value.FilterId == filter.FilterId).Select(a => a.Value))
            {
                sql = sql.Replace($"#{arg.Name}#", arg.Value);
            }

            return sql;
        }
    }
}
