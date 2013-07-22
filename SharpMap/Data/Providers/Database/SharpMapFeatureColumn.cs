using System;
using System.Collections.Generic;
using System.Data;

namespace SharpMap.Data.Providers
{
    /// <summary>
    /// Feature column class
    /// </summary>
    [Serializable]
    public class SharpMapFeatureColumn
    {
        private string _orderBy;
        private int _ordinal;

        /// <summary>
        /// Default constructor, forcing <seealso cref="Display"/> to be <value>true</value> by default
        /// </summary>
        public SharpMapFeatureColumn()
        {
            Display = true;
        }

        ///// <summary>
        ///// Event raised when the ordinal of a feature column changed.
        ///// </summary>
        //public event EventHandler OrdinalChanged;

        //protected virtual void OnOrdinalChanged(EventArgs e)
        //{
        //    var handler = OrdinalChanged;
        //    if (handler != null) handler(this, e);
        //}

        /// <summary>
        /// Gets or sets the name of the column to get from the table
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// Gets or sets the ordinal (index)
        /// </summary>
        public int Ordinal
        {
            get { return _ordinal; }
            set
            {
                if (value != _ordinal)
                {
                    _ordinal = value;
                    //OnOrdinalChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name the <see cref="Column"/> should have in the output<br/>
        /// If this is <c>null</c> or <c>string.Empty</c>, <see cref="Column"/> remains unchanged.
        /// </summary>
        public string As { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Display { get; set; }

        /// <summary>
        /// The name of the function to apply to <see cref="Column"/>
        /// </summary>
        public string Function { get; set; }
        
        /// <summary>
        /// The additional parameters
        /// </summary>
        public string[] FunctionParameters { get; set; }

        /// <summary>
        /// Gets or sets whether this column should appear in the group by section
        /// </summary>
        public bool GroupBy { get; set; }

        /// <summary>
        /// Gets or sets the order applied to the <see cref="Column"/>
        /// </summary>
        public string OrderBy
        {
            get { return _orderBy; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (!(value.Equals("ASC", StringComparison.InvariantCultureIgnoreCase)) || 
                        value.Equals("DESC", StringComparison.InvariantCultureIgnoreCase))
                        return;
                }
                _orderBy = value;
            }
        }

        /// <summary>
        /// A constraint for the column
        /// </summary>
        /// <remarks>
        /// The constraint must be rhs of the column name, since it is added
        /// <code><see cref="SpatialDbUtility.DecorateEntity"/>(<see cref="Column"/>) + <see cref="Constraint"/></code>
        /// <para/>
        /// If a <see cref="Function"/> is applied to <see cref="Column"/> that is included.
        /// </remarks>
        public string Constraint { get; set; }

        /// <summary>
        /// Gets or sets the type used in database
        /// </summary>
        public DbType DbType { get; set; }
    }

}