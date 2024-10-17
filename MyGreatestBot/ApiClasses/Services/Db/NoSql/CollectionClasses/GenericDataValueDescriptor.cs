using System;

namespace MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses
{
    internal sealed class GenericDataValueDescriptor(GenericDataValueKey key)
    {
        public Guid Id { get; set; }
        [DisallowNull] public GenericDataValueKey Key { get; } = key;
        [AllowNull] public string HyperText { get; set; }

        public GenericDataValueDescriptor(UInt128 id, GenericDataValueKey key) : this(key)
        {
            byte[] GuidConvert = new byte[16];

            for (int i = 0; i < GuidConvert.Length; i++)
            {
                GuidConvert[i] = (byte)(id & 0xFF);
                id >>= 8;
            }

            Id = new(GuidConvert);
        }
    }
}
