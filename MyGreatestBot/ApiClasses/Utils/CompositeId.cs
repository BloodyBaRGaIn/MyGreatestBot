using System;

namespace MyGreatestBot.ApiClasses.Utils
{
    /// <summary>
    /// Represents a stringified identifier with corresponded API
    /// </summary>
    /// <param name="id">ID string</param>
    /// <param name="intents">API flag</param>
    public sealed class CompositeId(string id = "", ApiIntents intents = ApiIntents.None) : IEquatable<CompositeId>
    {
        /// <summary>
        /// ID string
        /// </summary>
        public string Id { get; } = id;

        /// <summary>
        /// API flag
        /// </summary>
        public ApiIntents Api => intents;

        public bool Equals(CompositeId? other)
        {
            return Id == other?.Id && Api == other.Api;
        }

        public override bool Equals([AllowNull] object obj)
        {
            return Equals(obj as CompositeId);
        }

        public override string ToString()
        {
            return Id;
        }

        public static implicit operator string(CompositeId instance)
        {
            return instance.ToString();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, intents);
        }
    }
}
