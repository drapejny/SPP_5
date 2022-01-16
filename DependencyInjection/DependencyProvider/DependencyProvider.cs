using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DependencyInjection.DependencyConfiguration;
using DependencyInjection.DependencyConfiguration.ImplementationData;

namespace DependencyInjection.DependencyProvider
{
    public class DependencyProvider
    {
        static object locker = new object();
        private readonly DependencyConfig _configuration;
        public readonly Dictionary<Type, List<SingletonContainer>> _singletons;
        private readonly Stack<Type> _recursionStack = new Stack<Type>();
        private Dictionary<Type, Type> nullDictionary = new Dictionary<Type, Type>();

        public DependencyProvider(DependencyConfig configuration)
        {
            ConfigValidator configValidator = new ConfigValidator(configuration);
            if (!configValidator.Validate())
            {
                throw new ArgumentException("Wrong configuration");
            }

            this._singletons = new Dictionary<Type, List<SingletonContainer>>();
            this._configuration = configuration;
        }

        public TDependency Resolve<TDependency>(ImplNumber number = ImplNumber.Any)
            where TDependency : class
        {
            if (typeof(TDependency).IsClass)
            {
                return (TDependency)ResolveClass(typeof(TDependency), number);
            }
            return (TDependency)Resolve(typeof(TDependency), number);
        }

        private object ResolveClass(Type classType , ImplNumber number = ImplNumber.Any)
        {
            Type[] interfaces = classType.GetInterfaces();
            foreach(Type classInterface in interfaces)
            {
                if (this._configuration.DependenciesDictionary.ContainsKey(classInterface))
                {
                    Type implType = this._configuration.DependenciesDictionary[classInterface]
                        .FindLast(container => number.HasFlag(container.ImplNumber)).ImplementationsType;
                    if (implType.Equals(classType))
                    {
                       return Resolve(classInterface);
                    }
                }

            }
            throw new ArgumentException("Wrong class type, check configuration");
           
        }

        public object Resolve(Type dependencyType, ImplNumber number = ImplNumber.Any)
        {

            if (_recursionStack.Contains(dependencyType))
            {
                Console.WriteLine("Return null");
                return null;
            }

            Console.WriteLine("RECURSION PUSH: " + dependencyType);
            _recursionStack.Push(dependencyType);

            object result;
            if (this.IsIEnumerable(dependencyType))
            {
                result = this.CreateEnumerable(dependencyType.GetGenericArguments()[0]);
            }
            else
            {
                ImplContainer container = GetImplContainerByDependencyType(dependencyType, number);
                Type requiredType = GetGeneratedType(dependencyType, container.ImplementationsType);
                result = this.ResolveNonIEnumerable(requiredType, container.TimeToLive, dependencyType, container.ImplNumber);
            }
            Console.WriteLine("RECURSION POP: " + _recursionStack.Peek());
            _recursionStack.Pop();
            return result;
        }

        private object ResolveNonIEnumerable(Type implType, LifeCycle ttl, Type dependencyType,
            ImplNumber number)
        {
            if (ttl != LifeCycle.Singleton)
            {
                return CreateInstance(dependencyType,implType);
            }

            if (!IsInSingletons(dependencyType, implType, number))
            {
                lock (locker)
                {
                    if (!IsInSingletons(dependencyType, implType, number))
                    {
                        var result = CreateInstance(dependencyType,implType);
                        this.AddToSingletons(dependencyType, result, number);
                        Console.WriteLine("Solve: " + dependencyType);
                        solveNullObjects(dependencyType, number);
                    }
                }
            }
            Console.WriteLine("Finding in singletones: " + dependencyType);
            return this._singletons[dependencyType]
                   .Find(singletonContainer => number.HasFlag(singletonContainer.ImplNumber)).Instance;
        }

        private void solveNullObjects(Type replaceType, ImplNumber number)
        {
            foreach(KeyValuePair<Type, Type> keyValuePair in nullDictionary)
            {
                Console.WriteLine("[" + keyValuePair.Key + " " + keyValuePair.Value + "]");
                if (replaceType.Equals(keyValuePair.Value))
                {
                    Console.WriteLine("IN IF");
                    object objectWithNull = Resolve(keyValuePair.Key, ImplNumber.First);
                    PropertyInfo[] propertyInfos = objectWithNull.GetType().GetProperties();
                    for(int i = 0; i < propertyInfos.Length; i++)
                    {
                        Console.WriteLine("PROP: " + propertyInfos[i].PropertyType);
                        if (propertyInfos[i].PropertyType.Equals(keyValuePair.Value)){
                            object replaceObject = this._singletons[replaceType]
                   .Find(singletonContainer => number.HasFlag(singletonContainer.ImplNumber)).Instance; //Resolve(replaceType, ImplNumber.First);
                            Console.WriteLine(replaceObject == null);
                            objectWithNull.GetType().GetProperty(propertyInfos[i].Name).SetValue(objectWithNull, replaceObject);
                            break;
                        }
                    }
                }
            }
        }

        private object CreateInstance( Type dependecyType, Type implementationType)
        {
            var constructors = implementationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();
                var generatedParams = new List<dynamic>();
                foreach (var parameterInfo in constructorParams)
                {
                    dynamic parameter;
                    if (parameterInfo.ParameterType.IsInterface)
                    {
                        var number = parameterInfo.GetCustomAttribute<DependencyKeyAttribute>()?.ImplNumber ?? ImplNumber.First;
                        parameter = Resolve(parameterInfo.ParameterType, number);
                        if (parameter == null)
                        {
                            //if (!nullDictionary.ContainsKey(dependecyType))
                            //{
                                Console.WriteLine("Added to nullable: " + dependecyType + " " + parameterInfo.ParameterType);
                                nullDictionary.Add(dependecyType, parameterInfo.ParameterType);
                           // }
                        }
                    }
                    else
                    {
                        break;
                    }

                    generatedParams.Add(parameter);
                }

                Console.WriteLine("Created instance of :" + implementationType);
                return constructor.Invoke(generatedParams.ToArray());
            }

            throw new ArgumentException("Cannot create instance of class");
        }

        private ImplContainer GetImplContainerByDependencyType(Type dependencyType, ImplNumber number)
        {
            ImplContainer container;
            if (dependencyType.IsGenericType)
            {
                container = GetImplementationsContainerLast(dependencyType, number);
                container ??= GetImplementationsContainerLast(dependencyType.GetGenericTypeDefinition(), number);
            }
            else
            {
                container = GetImplementationsContainerLast(dependencyType, number);
            }

            return container;
        }

        private bool IsIEnumerable(Type dependencyType)
        {
            return dependencyType.GetInterfaces().Any(i => i.Name == "IEnumerable");
        }



        private Type GetGeneratedType(Type dependencyType, Type implementationType)
        {
            if (dependencyType.IsGenericType && implementationType.IsGenericTypeDefinition)
            {
                return implementationType.MakeGenericType(dependencyType.GetGenericArguments());
            }

            return implementationType;
        }

        private IList CreateEnumerable(Type dependencyType)
        {
            var implementationList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(dependencyType));
            var implementationsContainers = this._configuration.DependenciesDictionary[dependencyType];
            foreach (var implementationContainer in implementationsContainers)
            {
                var instance = this.ResolveNonIEnumerable(implementationContainer.ImplementationsType,
                    implementationContainer.TimeToLive, dependencyType, implementationContainer.ImplNumber);
                implementationList.Add(instance);
            }

            return implementationList;
        }

        private ImplContainer GetImplementationsContainerLast(Type dependencyType, ImplNumber number)
        {
            if (this._configuration.DependenciesDictionary.ContainsKey(dependencyType))
            {
                return this._configuration.DependenciesDictionary[dependencyType]
                    .FindLast(container => number.HasFlag(container.ImplNumber));
            }

            return null;
        }

        private void AddToSingletons(Type dependencyType, object implementation, ImplNumber number)
        {
            if (this._singletons.ContainsKey(dependencyType))
            {
                this._singletons[dependencyType].Add(new SingletonContainer(implementation, number));
            }
            else
            {
                Console.WriteLine("Added to singletones:" + implementation);
                this._singletons.Add(dependencyType, new List<SingletonContainer>()
                {
                    new SingletonContainer(implementation, number)
                });
            }
        }

        private bool IsInSingletons(Type dependencyType, Type implType, ImplNumber number)
        {
            var lst = this._singletons.ContainsKey(dependencyType) ? this._singletons[dependencyType] : null;
            return lst?.Find(container => number.HasFlag(container.ImplNumber) && container.Instance.GetType() == implType) is not null;
        }
    }
}