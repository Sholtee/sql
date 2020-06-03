/********************************************************************************
*  Order.cs                                                                     *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents the order of a selection.
    /// </summary>
    public enum Order
    {
        /// <summary>
        /// Not specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Ascending.
        /// </summary>
        Ascending = 1,

        /// <summary>
        /// Descending.
        /// </summary>
        Descending = 2
    }
}