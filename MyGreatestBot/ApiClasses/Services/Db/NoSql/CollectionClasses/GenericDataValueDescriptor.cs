using System;
using System.Diagnostics.CodeAnalysis;

namespace MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses
{
    internal sealed class GenericDataValueDescriptor(GenericDataValueKey key)
    {
        private static readonly byte[] GuidConvert = new byte[16];

        public Guid Id { get; set; }

        [DisallowNull]
        public GenericDataValueKey Key { get; } = key;

        [AllowNull]
        public string HyperText { get; set; }

        public GenericDataValueDescriptor(UInt128 id, GenericDataValueKey key) : this(key)
        {
            for (int i = 0; i < GuidConvert.Length; i++)
            {
                GuidConvert[i] = (byte)(id & 0xFF);
                id >>= 8;
            }
            Id = new(GuidConvert);
        }
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
