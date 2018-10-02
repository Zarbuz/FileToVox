using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace fNbt.Serialization.Compiled {
    internal abstract class CodeEmitter {
        [NotNull]
        public abstract ParameterExpression ReturnValue { get; }


        [NotNull]
        public abstract Expression GetPreamble();


        [NotNull]
        public abstract Expression HandlePrimitiveOrEnum([NotNull] string tagName, [NotNull] PropertyInfo property);


        [NotNull]
        public abstract Expression HandleDirectlyMappedType([NotNull] string tagName, [NotNull] PropertyInfo property,
                                                            NullPolicy selfPolicy);


        [NotNull]
        public abstract Expression HandleINbtSerializable([NotNull] string tagName, [NotNull] PropertyInfo property, NullPolicy selfPolicy);


        [NotNull]
        public abstract Expression HandleNbtTag([NotNull] string tagName, [NotNull] PropertyInfo property,
                                                NullPolicy selfPolicy);


        [NotNull]
        public abstract Expression HandleNbtFile([NotNull] string tagName, [NotNull] PropertyInfo property,
                                                 NullPolicy selfPolicy);


        [NotNull]
        public abstract Expression HandleIList([NotNull] string tagName, [NotNull] PropertyInfo property,
                                               [NotNull] Type iListImpl,
                                               NullPolicy selfPolicy, NullPolicy elementPolicy);


        [NotNull]
        public abstract Expression HandleStringIDictionary([NotNull] string tagName, [NotNull] PropertyInfo property,
                                                           [NotNull] Type iDictImpl,
                                                           NullPolicy selfPolicy, NullPolicy elementPolicy);


        [NotNull]
        public abstract Expression HandleCompoundObject([NotNull] string tagName, [NotNull] PropertyInfo property,
                                                        NullPolicy selfPolicy);
    }
}