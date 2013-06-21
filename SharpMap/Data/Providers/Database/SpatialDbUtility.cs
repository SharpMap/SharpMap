using System;
using System.Data;
using System.Data.Common;
using System.Text;
using GeoAPI.IO;
using NetTopologySuite.IO;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Database entity decorator object
    /// </summary>
    [Serializable]
    public class SpatialDbUtility
    {
        /// <summary>
        /// Creates an instance of this class using the default decorator strings
        /// </summary>
        public SpatialDbUtility()
            : this("\"{0}\"", "'{0}'", "@P{0}")
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided decorator strings
        /// </summary>
        /// <param name="entityDecoratorFormat">The format string to decorate database entities</param>
        /// <param name="literalDecoratorFormat">The format string to decorate literals (strings)</param>
        /// <param name="parameterDecoratorFormat">The format string to decorate parameters</param>
        public SpatialDbUtility(string entityDecoratorFormat, string literalDecoratorFormat, string parameterDecoratorFormat)
            : this(entityDecoratorFormat, literalDecoratorFormat, parameterDecoratorFormat,
            new WKBReader(), new WKBWriter())
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided decorator strings
        /// </summary>
        /// <param name="entityDecoratorFormat">The format string to decorate database entities</param>
        /// <param name="literalDecoratorFormat">The format string to decorate literals (strings)</param>
        /// <param name="parameterDecoratorFormat">The format string to decorate parameters</param>
        /// <param name="reader"> </param>
        /// <param name="writer"> </param>
        public SpatialDbUtility(string entityDecoratorFormat, string literalDecoratorFormat, string parameterDecoratorFormat,
            IBinaryGeometryReader reader, IBinaryGeometryWriter writer)
        {
            EntityDecoratorFormat = entityDecoratorFormat;
            LiteralDecoratorFormat = literalDecoratorFormat;
            ParameterDecoratorFormat = parameterDecoratorFormat;

            //This won't do anything to the geometry
            ToEnvelopeDecoratorFormat = "{0}";
            ToGeometryDecoratorFormat = "{0}";
            SetSridDecoratorFormat = "{0}";
            TransformDecoratorFormat = "{0}";

            Reader = reader;
            Writer = writer;
        }

        /// <summary>
        /// Gets the database entity decorator format.
        /// <para/>
        /// For e.g. PostgreSQL that would be "\"{0}\"", so that a table named smTable would be decorated to "smTable"
        /// </summary>
        public string EntityDecoratorFormat { get; private set; }

        /// <summary>
        /// Gets the database literal (string) decorator
        /// </summary>
        public string LiteralDecoratorFormat { get; private set; }

        /// <summary>
        /// Gets the database parameter decorator
        /// </summary>
        public string ParameterDecoratorFormat { get; private set; }

        /// <summary>
        /// Function to decorate a database entity
        /// </summary>
        /// <param name="entity">The name of the entity</param>
        /// <returns>The decorated database entity</returns>
        public string DecorateEntity(string entity)
        {
            return string.Format(EntityDecoratorFormat, entity);
        }

        /// <summary>
        /// Decorates a constraint with parameters
        /// </summary>
        /// <param name="command">The command object to add the parameters to.</param>
        /// <param name="entity">The entity</param>
        /// <param name="constraint"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string DecorateEntityConstraintWithParameter(DbCommand command, SharpMapFeatureColumn entity, string constraint, params object[] parameters)
        {
            var sb = new StringBuilder();
            var pc = command.Parameters;
            var pNr = pc.Count;

            foreach (var o in parameters)
            {
                var p = command.CreateParameter();
                p.DbType = entity.DbType;
                p.ParameterName = string.Format(ParameterDecoratorFormat, pNr++);
                p.Value = o;
                pc.Add(p);
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.AppendFormat(p.ParameterName);
            }

            var formattedConstraint = string.Format(constraint, sb);

            return string.Format("{0} {1}", DecorateEntity(entity.Column), formattedConstraint);
        }

        /// <summary>
        /// Decorates the table name
        /// </summary>
        /// <returns>The decorated table name</returns>
        public string DecorateTable(string schema, string table)
        {
            return DecorateTable(schema, table, null);
        }

        /// <summary>
        /// Decorates the table name
        /// </summary>
        /// <returns>The decorated table name</returns>
        public string DecorateTable(string schema, string table, string asSuffix)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(schema))
                sb.Append(DecorateEntity(schema));

            if (sb.Length > 0)
                sb.Append(".");

            sb.Append(DecorateEntity(table));

            if (!string.IsNullOrEmpty(asSuffix))
                sb.AppendFormat("AS {0}", DecorateEntity(asSuffix));

            return sb.ToString();
        }

        /// <summary>
        /// Decorates a column name, optionally with a prefix
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The decorated column name</returns>
        public string DecorateColumn(string columnName)
        {
            return DecorateColumn(columnName, null);
        }

        /// <summary>
        /// Decorates a column name, optionally with a prefix
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="prefix">The (optional) prefix</param>
        /// <returns>The decorated column name</returns>
        public string DecorateColumn(string columnName, string prefix)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(prefix))
                sb.AppendFormat("{0}.", DecorateEntity(prefix));
            sb.Append(DecorateEntity(columnName));
            return sb.ToString();
        }

        /// <summary>
        /// Renames an already decorated entity using the SQL AS statement
        /// </summary>
        /// <param name="decoratedEntity">The decorated entity</param>
        /// <returns>The decorated entity</returns>
        public string DecorateAs(string decoratedEntity)
        {
            return DecorateAs(decoratedEntity, null);
        }

        /// <summary>
        /// Renames an already decorated entity using the SQL AS statement
        /// </summary>
        /// <param name="decoratedEntity">The decorated entity</param>
        /// <param name="asSuffix">The suffix</param>
        /// <returns>The decorated entity</returns>
        public string DecorateAs(string decoratedEntity, string asSuffix)
        {
            var sb = new StringBuilder();
            sb.Append(decoratedEntity);
            if (!string.IsNullOrEmpty(asSuffix))
                sb.AppendFormat(" AS {0}", DecorateEntity(asSuffix));
            return sb.ToString();
        }

        #region SpatialFunctions

        /// <summary>
        /// Decorator for the function to assign a specific SRID value to a geometry
        /// </summary>
        /// <remarks>
        /// The format must have
        /// <list type="bullet">
        /// <item>a placeholder for the geometry ({0})</item>
        /// <item>a placeholder for the srid ({1})</item>
        /// </list>
        /// <example language="C#">
        /// //e.g. Postgis
        /// this.SetSridDecoratorFormat = "ST_SetSrid({0}, {1})";
        /// </example>
        /// </remarks>
        public string SetSridDecoratorFormat { get; set; }

        /// <summary>
        /// Decorator for the format to transform a geometry to a specified
        /// </summary>
        /// <remarks>
        /// The format must have
        /// <list type="bullet">
        /// <item>a placeholder for the geometry ({0})</item>
        /// <item>a placeholder for the target srid ({1})</item>
        /// </list>
        /// <example language="C#">
        /// //e.g. Postgis
        /// this.TransformDecoratorFormat = "ST_Transform({0}, {1})";
        /// </example>
        /// </remarks>
        public string TransformDecoratorFormat { get; set; }

        /// <summary>
        /// Decorator for the transformation of the geometry data, in case the
        /// <see cref="Writer"/> produces a specific format (e.g. WKB) that does
        /// not match the backend's format.
        /// </summary>
        /// <remarks>
        /// The format must have
        /// <list type="bullet">
        /// <item>a placeholder for the geometry ({0})</item>
        /// </list>
        /// </remarks>
        public string ToGeometryDecoratorFormat { get; set; }

        /// <summary>
        /// Decorator for the transformation of the envelope data, in case the
        /// <see cref="Reader"/> expects a specific format (e.g. WKB).
        /// </summary>
        /// <remarks>
        /// The format must have
        /// <list type="bullet">
        /// <item>a placeholder for the envelope ({0})</item>
        /// </list>
        /// </remarks>
        public string ToEnvelopeDecoratorFormat { get; set; }

        public virtual object ToDbType(object obj, DbType type)
        {
            switch (type)
            {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return Convert.ToString(obj);
                case DbType.Binary:
                    return obj as byte[];
                case DbType.Boolean:
                    return Convert.ToBoolean(obj);
                case DbType.Byte:
                    return Convert.ToByte(obj);

                case DbType.Time:
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return Convert.ToDateTime(obj);
                
                case DbType.Decimal:
                    return Convert.ToDecimal(obj);
                case DbType.Double:
                    return Convert.ToDouble(obj);
                case DbType.Guid:
                    return Convert.ToString(obj);

                case DbType.Int16:
                    return Convert.ToInt16(obj);
                case DbType.Int32:
                    return Convert.ToInt32(obj);
                case DbType.Int64:
                    return Convert.ToInt64(obj);

                case DbType.UInt16:
                    return Convert.ToUInt16(obj);
                case DbType.UInt32:
                    return Convert.ToUInt32(obj);
                case DbType.UInt64:
                    return Convert.ToUInt64(obj);

                case DbType.SByte:
                    return Convert.ToSByte(obj);

                case DbType.Single:
                    return Convert.ToSingle(obj);
                
                case DbType.Currency:
                    return Convert.ToDecimal(obj);

            }
            return obj;
        }

        /// <summary>
        /// Decorator for the transformation of the geometry data, in case the
        /// <see cref="Reader"/> can only read a format (e.g. WKB) that does
        /// not match the backend's format.
        /// </summary>
        /// <remarks>
        /// The format must have
        /// <list type="bullet">
        /// <item>a placeholder for the geometry ({0})</item>
        /// </list>
        /// </remarks>
        public string FromGeometryDecoratorFormat { get; set; }

        /// <summary>
        /// Reader for geometry data
        /// </summary>
        public IBinaryGeometryReader Reader { get; private set; }

        /// <summary>
        /// Writer for geometry
        /// </summary>
        public IBinaryGeometryWriter Writer { get; private set; }

        #endregion SpatialFunctions
    }
}