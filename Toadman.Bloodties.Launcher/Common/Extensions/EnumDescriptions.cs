using System;
using System.Linq;
using System.Windows.Markup;

namespace Toadman.Bloodties.Launcher
{
    public class EnumDescriptions : MarkupExtension
    {
        public class EnumValue<T>
        {
            public T Value { get; private set; }
            public string Description { get; private set; }

            public EnumValue(T val)
            {
                this.Value = val;
                this.Description = ((Enum)(object)val).GetDescription();
            }

            public override string ToString()
            {
                return Description;
            }

            public static implicit operator T(EnumValue<T> val)
            {
                return val.Value;
            }
        }

        private readonly Type type;
        private readonly Type enumValueType;

        public EnumDescriptions(Type type)
        {
            this.type = type;
            this.enumValueType = typeof(EnumValue<>).MakeGenericType(new Type[] { type });
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(type).Cast<object>().Select(e => Activator.CreateInstance(enumValueType, e));
        }
    }
}