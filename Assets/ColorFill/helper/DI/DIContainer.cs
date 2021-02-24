using System;
using System.Collections.Generic;
using System.Reflection;
using ColorFill.helper.Exception;

namespace ColorFill.helper.DI
{
    public static class DIContainer
    {
        private static Dictionary<Type,object> _singletonInstances = new Dictionary<Type, object>();

        public static TRegistryType RegisterSingle<TRegistryType, TActualType>(Type[] constructorTypes,object[] constructorParams) 
            where TRegistryType: class 
            where TActualType : class
        {
            ConstructorInfo constructorInfo = typeof(TActualType).GetConstructor(constructorTypes);
            if (constructorInfo == null)
            {
                string errorMessage = $"No matching constructor found for given types.\n{nameof(TActualType)}";
                foreach(var type in constructorTypes)
                {
                    errorMessage += type.Name + ", ";
                }
                throw new ConstructorException(errorMessage);
            }
            object instance = constructorInfo.Invoke(constructorParams) as TRegistryType;
            if (instance == null)
            {
                throw new DIRegistryException($"{nameof(TActualType)} is not a {nameof(TRegistryType)}");
            }

            _singletonInstances[typeof(TRegistryType)] = instance;
            return instance as TRegistryType;
        }

        public static TActualType RegisterSingle<TActualType>(Type[] constructorTypes, object[] constructorParams)
            where TActualType : class
        {
            return RegisterSingle<TActualType,TActualType>(constructorTypes,constructorParams);
        }
        
        public static TActualType RegisterSingle<TActualType>()
            where TActualType : class
        {
            return RegisterSingle<TActualType,TActualType>(new Type[] {},new object [] {});
        }

        public static T GetSingle<T>() 
            where T: class
        {
            return _singletonInstances[typeof(T)] as T;
        }
    }
}