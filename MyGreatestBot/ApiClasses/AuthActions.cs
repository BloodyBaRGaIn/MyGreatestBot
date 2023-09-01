using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MyGreatestBot.ApiClasses
{
    public sealed class AuthActions
    {
        public readonly Action InitAction;
        public readonly Action DeinitAction;

        public AuthActions(Action initAction, Action deinitAction)
        {
            InitAction = initAction;
            DeinitAction = deinitAction;
        }

        public static IEnumerable<ApiIntents> ApiOrder { get; private set; } = Enumerable.Empty<ApiIntents>();

        private static readonly Dictionary<ApiIntents, AuthActions> AuthActionsDictionary = new();

        public static void SetApiOrder(params ApiIntents[] apis)
        {
            ApiOrder = apis;
        }

        public static void AddOrReplace(ApiIntents key, AuthActions value)
        {
            if (!AuthActionsDictionary.TryAdd(key, value))
            {
                AuthActionsDictionary[key] = value;
            }
        }

        public static void AddOrReplace(ApiIntents key, Action initAction, Action deinitAction)
        {
            AddOrReplace(key, new AuthActions(initAction, deinitAction));
        }

        public static bool TryGetValue(ApiIntents key, [MaybeNullWhen(false)] out AuthActions value)
        {
            return AuthActionsDictionary.TryGetValue(key, out value);
        }
    }
}
