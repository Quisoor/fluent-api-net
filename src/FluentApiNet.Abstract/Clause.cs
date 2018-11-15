namespace FluentApiNet.Abstract
{
    /// <summary>
    /// Clause class
    /// </summary>
    public class Clause
    {
        /// <summary>
        /// Gets or sets the member.
        /// </summary>
        /// <value>
        /// The member.
        /// </value>
        public string Member { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the evaluator.
        /// </summary>
        /// <value>
        /// The evaluator.
        /// </value>
        public Evaluator Evaluator { get; set; }
    }
}
