using System;
using System.Reflection;
using System.Linq.Expressions;
using Common.Log;

namespace Common.Utility
{
    public static class Constructor
    {
        public delegate T Delegate<T>(params object[] args);
        public static Delegate<T> Compile<T>(System.Type type)
        {
            // Get constructor information?
            ConstructorInfo[] constructors = type.GetConstructors();

            // Is there at least 1?
            if (constructors.Length > 0)
            {
                // Get our one constructor.
                ConstructorInfo constructor = constructors[0];

                // Yes, does this constructor take some parameters?
                ParameterInfo[] paramsInfo = constructor.GetParameters();

                if (paramsInfo.Length > 0)
                {
                    // Create a single param of type object[].
                    ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

                    // Pick each arg from the params array and create a typed expression of them.
                    Expression[] argsExpressions = new Expression[paramsInfo.Length];

                    for (int i = 0; i < paramsInfo.Length; i++)
                    {
                        Expression index = Expression.Constant(i);
                        Type paramType = paramsInfo[i].ParameterType;
                        Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                        Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                        argsExpressions[i] = paramCastExp;
                    }

                    // Make a NewExpression that calls the constructor with the args we just created.
                    NewExpression newExpression = Expression.New(constructor, argsExpressions);

                    // Create a lambda with the NewExpression as body and our param object[] as arg.
                    LambdaExpression lambda = Expression.Lambda(typeof(Delegate<T>), newExpression, param);

                    // Compile it
                    Delegate<T> compiled = (Delegate<T>)lambda.Compile();
                    return compiled;
                }
            }
            Logger.Fatal($"[Constructor::Compile] No Have Constructors. DelegateType : {typeof(Delegate<T>).ToString()}, Instance Type : {type.ToString()}");
            return null;
        }

        public static Delegate<T> CompileDefaultConstructor<T>(System.Type type)
        {
            // Get constructor information?
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                Logger.Fatal(
                    $"[Constructor::Compile] No Have Default Constructor. DelegateType : {typeof(Delegate<T>).ToString()}, Instance Type : {type.ToString()}");
            }


            // Yes, does this constructor take some parameters?
            ParameterInfo[] paramsInfo = constructor.GetParameters();

            if (paramsInfo.Length > 0)
            {
                // Create a single param of type object[].
                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

                // Pick each arg from the params array and create a typed expression of them.
                Expression[] argsExpressions = new Expression[paramsInfo.Length];

                for (int i = 0; i < paramsInfo.Length; i++)
                {
                    Expression index = Expression.Constant(i);
                    Type paramType = paramsInfo[i].ParameterType;
                    Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                    Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                    argsExpressions[i] = paramCastExp;
                }

                // Make a NewExpression that calls the constructor with the args we just created.
                NewExpression newExpression = Expression.New(constructor, argsExpressions);

                // Create a lambda with the NewExpression as body and our param object[] as arg.
                LambdaExpression lambda = Expression.Lambda(typeof(Delegate<T>), newExpression, param);

                // Compile it
                Delegate<T> compiled = (Delegate<T>)lambda.Compile();
                return compiled;
            }
            else
            {
                return delegate(object[] args) { return (T)constructor.Invoke(args); };
            }
        }
    }
    

    
}
