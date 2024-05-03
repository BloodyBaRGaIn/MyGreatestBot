using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses
{
    internal sealed class GenericDataValueDescriptor(GenericDataValueKey key)
    {
        public Guid Id { get; set; }

        [DisallowNull]
        public GenericDataValueKey Key { get; } = key;

        [AllowNull]
        public string HyperText { get; set; }
    }

    internal sealed class GenericDataValueKey(ulong giuldId, ApiIntents apiType, string genericId)
    {
        [DisallowNull]
        public ulong GiuldId { get; } = giuldId;

        [DisallowNull]
        public ApiIntents ApiType { get; } = apiType;

        [DisallowNull]
        public string GenericId { get; } = genericId;

        public bool Equals(GenericDataValueKey other)
        {
            return GiuldId == other.GiuldId && ApiType == other.ApiType && GenericId == other.GenericId;
        }
    }
}
