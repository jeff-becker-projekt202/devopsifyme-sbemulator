using System.Linq.Expressions;
using System.Reflection;

namespace ServiceBusEmulator.Abstractions.Configuration;

public class SwitchMapBuilder<TOptions> where TOptions : class
{
    private readonly List<IMapSwitches> _options;
    private readonly string _prefix;
    private readonly Lazy<AggregateSwitchMapper> _mapper;
    private SwitchMapBuilder(string prefix, List<IMapSwitches> options)
    {
        _prefix = prefix.TrimEnd(':');
        _options = options;
        _mapper = new Lazy<AggregateSwitchMapper>(() => new AggregateSwitchMapper(_options));

    }
    private SwitchMapBuilder() { }
    public static SwitchMapBuilder<TOptions> Create(string prefix = "")
    {
        prefix = prefix.Trim(':');
        if (!prefix.StartsWith("Emulator"))
        {
            prefix = $"Emulator:{prefix}";
        }
        return new(prefix, new ());
    }
    public SwitchMapBuilder<TOptions> Child<TChildProperty>(Expression<Func<TOptions, TChildProperty>> expression, Action<SwitchMapBuilder<TChildProperty>> build) where TChildProperty : class
    {
        var info = GetPropertyInfo(expression);

        var child = new SwitchMapBuilder<TChildProperty>(_prefix + ":" + info.Name, _options);
        build(child);
        return this;
    }


    public SwitchMapBuilder<TOptions> Add<TProperty>(string arg, Expression<Func<TOptions, TProperty>> expression)
    {
        if (!arg.StartsWith("--"))
        {
            arg = $"--{arg.TrimStart('/')}";
        }
        var info = GetPropertyInfo(expression);
        if (IsList(info))
        {
            _options.Add(new ToListOfValuesSwitchMapper(arg, $"{_prefix}:{info.Name}"));
        }
        else
        {
            _options.Add(new ToValueSwitchMapper(arg, $"{_prefix}:{info.Name}"));
        }

        return this;
    }

    private bool IsList(PropertyInfo info)
    {
        var t = info.PropertyType;
        return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
    }

    private static PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TOptions, TProperty>> property)
    {
        LambdaExpression lambda = property;
        var memberExpression = lambda.Body is UnaryExpression expression
            ? (MemberExpression)expression.Operand
            : (MemberExpression)lambda.Body;

        return (PropertyInfo)memberExpression.Member;
    }

    public IMapSwitches Mapper => _mapper.Value;

    private record ToValueSwitchMapper(string Key, string Value) : IMapSwitches
    {
        public bool CanHandle(string arg) => String.Compare(arg, Key, StringComparison.OrdinalIgnoreCase) == 0;

        public string Transform(string arg) => Value;
    }
    public record ToListOfValuesSwitchMapper(string Key, string Value) : IMapSwitches
    {
        private int _index = 0;
        public bool CanHandle(string arg) => String.Compare(arg, Key, StringComparison.OrdinalIgnoreCase) == 0;

        public string Transform(string arg)
        {
            var result = Value + ":" + _index;
            _index++; 
            return result;
        }
            
    }
}
