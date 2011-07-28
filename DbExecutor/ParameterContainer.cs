using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Codeplex.Data
{
    public sealed class ParameterContainer
    {
        Dictionary<string, Parameter> parameters = new Dictionary<string, Parameter>();

        public void Add(string parameterName, object value)
        {
            var p = new Parameter
            {
                Name = parameterName,
                Value = value,
                Direction = ParameterDirection.Input,
            };
            parameters.Add(parameterName, p);
        }

        public void Add(string parameterName, ParameterDirection direction, DbType dbType, int? size = null, object value = null)
        {
            var p = new Parameter
            {
                Direction = direction,
                Name = parameterName,
                DbType = dbType,
                Size = size,
                Value = value
            };
            parameters.Add(parameterName, p);
        }

        public T GetValue<T>(string parameterName)
        {
            return (T)parameters[parameterName].DbParameter.Value;
        }

        public void AttachParameters(IDbCommand command)
        {
            foreach (var item in parameters.Values)
            {
                var p = command.CreateParameter();
                p.ParameterName = item.Name;
                p.Value = item.Value;
                p.Direction = item.Direction;
                if (item.DbType != null) p.DbType = item.DbType.Value;
                if (item.Size != null) p.Size = item.Size.Value;

                item.DbParameter = p;
                command.Parameters.Add(p);
            }
        }

        class Parameter
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ParameterDirection Direction { get; set; }
            public DbType? DbType { get; set; }
            public int? Size { get; set; }

            public IDbDataParameter DbParameter { get; set; }
        }
    }
}