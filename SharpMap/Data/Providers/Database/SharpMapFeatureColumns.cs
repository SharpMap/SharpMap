using System;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Text;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Collection of <see cref="SharpMapFeatureColumn"/>s used to create the SQL statement
    /// </summary>
    public class SharpMapFeatureColumns : Collection<SharpMapFeatureColumn>
    {
        private readonly SpatialDbProvider _provider;

        private readonly SpatialDbUtility _spatialDbUtility;

        /// <summary>
        /// Event raised when the Feature column
        /// </summary>
        public event EventHandler FeatureColumnsChanged;

        private void OnFeatureColumnsChanged(EventArgs eventArgs)
        {
            if (FeatureColumnsChanged != null)
                FeatureColumnsChanged(this, eventArgs);
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="dbUtility">The db utility object</param>
        public SharpMapFeatureColumns(SpatialDbProvider provider,
                                      SpatialDbUtility dbUtility)
        {
            _provider = provider;
            _spatialDbUtility = dbUtility;
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnFeatureColumnsChanged(EventArgs.Empty);
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            //var item = this[index];
            base.RemoveItem(index);

            // unsubscribe from event
            //item.OrdinalChanged -= HandleOrdinalChanged;
            OnFeatureColumnsChanged(EventArgs.Empty);
        }

        //private void HandleOrdinalChanged(object sender, EventArgs e)
        //{
        //    OnFeatureColumnsChanged(EventArgs.Empty);
        //}

        /// <inheritdoc/>
        protected override void InsertItem(int index, SharpMapFeatureColumn item)
        {
            item.Ordinal = index;
            base.InsertItem(index, item);
            OnFeatureColumnsChanged(EventArgs.Empty);
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, SharpMapFeatureColumn item)
        {
            item.Ordinal = index;
            base.SetItem(index, item);
            OnFeatureColumnsChanged(EventArgs.Empty);
        }

        private string _selectClause;

        /// <summary>
        /// Gets the main select clause
        /// </summary>
        /// <returns>The plain select clause without any constraints or order clauses</returns>
        public string GetSelectClause()
        {
            return GetSelectClause(null);
        }

        /// <summary>
        /// Gets the main select clause
        /// </summary>
        /// <param name="from">The from clause to use (if it is not schema.table)</param>
        /// <returns>The plain select clause without any constraints or order clauses</returns>
        public string GetSelectClause(string from)
        {
            if (string.IsNullOrEmpty(from))
                from = _spatialDbUtility.DecorateTable(_provider.Schema, _provider.Table);

            if (string.IsNullOrEmpty(_selectClause))
            {
                var sqlBuilder = new StringBuilder("SELECT ");
                sqlBuilder.Append(_spatialDbUtility.DecorateAs(_provider.ObjectIdColumn));

                foreach (var fc in this)
                {
                    if (!fc.Display) continue;

                    sqlBuilder.AppendFormat(", {0}", _spatialDbUtility.DecorateAs(
                        !string.IsNullOrEmpty(fc.Function) ? GetFunctionColumn(fc) : fc.Column,
                        fc.As));
                }
                sqlBuilder.AppendFormat(", {0}", GetGeometryColumn(true));

                sqlBuilder.AppendFormat(" FROM {0}", from);

                _selectClause = sqlBuilder.ToString();
            }

            return _selectClause;
        }

        /// <summary>
        /// Gets a select clause for querying a column (mainly the geometry column is our focus)
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="column">The column</param>
        /// <returns>The sql string to select the column</returns>
        public string GetSelectColumnClause(DbCommand command, SharpMapFeatureColumn column)
        {
            return GetSelectColumnClause(command, column, 0xffffffff, null);
        }

        /// <summary>
        /// Gets a select clause for querying a column (mainly the geometry column is our focus)
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="column">The column</param>
        /// <param name="oid">The (optional) object id constraint</param>
        /// <returns>The sql string to select the column</returns>
        public string GetSelectColumnClause(DbCommand command, SharpMapFeatureColumn column, uint oid)
        {
            return GetSelectColumnClause(command, column, oid, null);
        }

        /// <summary>
        /// Gets a select clause for querying a column (mainly the geometry column is our focus)
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="column">The column</param>
        /// <param name="spatialWhere">The (optional) spatial constraint</param>
        /// <returns>The sql string to select the column</returns>
        public string GetSelectColumnClause(DbCommand command, SharpMapFeatureColumn column, string spatialWhere)
        {
            return GetSelectColumnClause(command, column, 0xffffffff, spatialWhere);
        }

        /// <summary>
        /// Gets a select clause for querying a column (mainly the geometry column is our focus)
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="column">The column</param>
        /// <param name="oid">The (optional) object id constraint</param>
        /// <param name="spatialWhere">The (optional) spatial constraint</param>
        /// <returns>The sql string to select the column</returns>
        public string GetSelectColumnClause(DbCommand command, SharpMapFeatureColumn column, uint oid, string spatialWhere)
        {
            var sqlBuilder = new StringBuilder("SELECT ");
            sqlBuilder.Append(_spatialDbUtility.DecorateColumn(column.Column));
            sqlBuilder.AppendFormat(" FROM {0}", _spatialDbUtility.DecorateTable(_provider.Schema, _provider.Table));
            if (oid != 0xffffffff)
            {
                if (column.DbType != System.Data.DbType.UInt32)
                {
                    sqlBuilder.AppendFormat(" WHERE {0}",
                        _spatialDbUtility.DecorateEntityConstraintWithParameter(command, _provider._oidColumn, "= {0}", _spatialDbUtility.ToDbType(oid, column.DbType)));
                }
            }
            else
                sqlBuilder.Append(GetWhereClause(spatialWhere));
            sqlBuilder.AppendFormat(" {0};", GetGroupByClause());

            return sqlBuilder.ToString();
        }

        private string _whereClause;

        /// <summary>
        /// Gets the where clause
        /// </summary>
        public string GetWhereClause()
        {
            return GetWhereClause(null);
        }

        // <param name="command">A command object to add parameters to</param>

        /// <summary>
        /// Gets the where clause
        /// </summary>
        /// <param name="spatialWhere">An optional spatial constraint</param>
        /// <returns>The </returns>
        public string GetWhereClause(/*DbCommand command,*/ string spatialWhere)
        {
            if (string.IsNullOrEmpty(_whereClause))
            {
                var sqlBuilder = new StringBuilder();
                foreach (var fc in this)
                {
                    if (!string.IsNullOrEmpty(fc.Constraint))
                    {
                        if (sqlBuilder.Length > 0)
                            sqlBuilder.Append(" AND ");

                        var column = string.IsNullOrEmpty(fc.Function)
                                            ? fc.Column
                                            : GetFunctionColumn(fc);

                        sqlBuilder.AppendFormat("{0} {1}", column, fc.Constraint);
                    }
                }
                _whereClause = sqlBuilder.ToString().Trim();
            }

            var res = _whereClause;
            if (res.Length > 0 && !string.IsNullOrEmpty(spatialWhere))
                res += " AND ";
            if (!string.IsNullOrEmpty(spatialWhere))
                res += spatialWhere;

            return res.Length > 0 ? " WHERE " + res : "";
        }

        /// <summary>
        /// Gets the SQL ORDER BY clause.
        /// </summary>
        /// <returns>The order by</returns>
        public string GetOrderByClause()
        {
            var orderBy = string.Empty;

            foreach (var featureColumn in Items)
            {
                switch (featureColumn.OrderBy)
                {
                    case null:
                        continue;

                    case "ASC":
                        orderBy += string.Format(", {0}", _spatialDbUtility.DecorateEntity(featureColumn.As));
                        break;

                    case "DESC":
                        orderBy += string.Format(", {0} DESC", _spatialDbUtility.DecorateEntity(featureColumn.As));
                        break;
                }
            }

            return orderBy.Length > 0 ? orderBy.Substring(2) : null;
        }

        /// <summary>
        /// Gets the SQL GROUP BY clause
        /// </summary>
        /// <returns>The group by clause</returns>
        public string GetGroupByClause()
        {
            var groupBy = string.Empty;

            foreach (var featureColumn in Items)
            {
                if (featureColumn.GroupBy)
                    groupBy += string.Format(", {0}", _spatialDbUtility.DecorateEntity(featureColumn.As));
            }

            return groupBy.Length > 0 ? groupBy.Substring(2) : null;
        }

        private string GetFunctionColumn(SharpMapFeatureColumn fc)
        {
            if (string.IsNullOrEmpty(fc.Function))
                return string.Empty;

            var par = new object[1 + (fc.FunctionParameters != null ? fc.FunctionParameters.Length : 0)];
            par[0] = _spatialDbUtility.DecorateEntity(fc.Column);
            if (fc.FunctionParameters != null)
                Array.Copy(fc.FunctionParameters, 0, par, 1, par.Length - 2);

            return string.Format(fc.Function, par);
        }

        /// <summary>
        /// Gets the geometry column
        /// </summary>
        /// <returns>The geometry column.</returns>
        public string GetGeometryColumn()
        {
            return GetGeometryColumn(false);
        }

        /// <summary>
        /// Gets the geometry column
        /// </summary>
        /// <param name="usAs">Uses AS</param>
        /// <returns>The geometry column.</returns>
        public string GetGeometryColumn(bool usAs)
        {
            var res = _provider.NeedsTransform
                          ? string.Format(_spatialDbUtility.ToGeometryDecoratorFormat, _provider.GeometryColumn, _provider.TargetSRID)
                          : _spatialDbUtility.DecorateEntity(_provider.GeometryColumn);

            res = string.Format(_spatialDbUtility.ToGeometryDecoratorFormat, res);

            if (usAs)
                res = string.Format("{0} AS {1}", res, _spatialDbUtility.DecorateEntity("_smGeom_"));

            return res;
        }
    }
}