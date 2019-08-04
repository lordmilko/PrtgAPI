using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PrtgAPI.Linq.Expressions.Pretty;

namespace PrtgAPI.Request.Serialization.CodeGen
{
    //TODO: this doesnt work when you specify an internal type and you havent granted internals visible to to the generated assembly. seems to have something to do with
    //creating the new object with the internal type arguments

#if DEBUG && DEBUG_SERIALIZATION
    /// <summary>
    /// Generates a lazy <see cref="MethodBuilder"/> invocation proxy for transparent F11 debugging.
    /// </summary>
    class ProxyBuilder
    {
        TypeBuilder typeBuilder;
        Type delegateType;
        Type[] genericArguments;

        FieldBuilder typeBuilderField;
        FieldBuilder methodNameField;
        FieldBuilder actionField;

        MethodBuilder invokeMethod;
        MethodBuilder lambdaMethod;

        Type dynamicLazyType;
        Type dynamicFuncType;

        private ProxyBuilder(ModuleBuilder moduleBuilder, Type delegateType, bool @return)
        {
            var cleanTypeName = CSharpWriter.CleanGenericName(delegateType.Name);

            var suffix = string.Empty;

            if (delegateType.IsGenericType)
                suffix = "`" + delegateType.GetGenericArguments().Length;

            typeBuilder = moduleBuilder.DefineType(
                $"{cleanTypeName}Proxy{suffix}",   //name
                TypeAttributes.Public | //attributes
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit
            );

            this.delegateType = delegateType;

            if (delegateType.IsGenericType)
            {
                var argNames = delegateType.GetGenericTypeDefinition().GetGenericArguments().Select(a => a.Name).ToArray();

                genericArguments = typeBuilder.DefineGenericParameters(argNames);
            }
            else
                genericArguments = Type.EmptyTypes;

            GenerateFields();

            GenerateInvoke(@return);
            GenerateLambda();
            GenerateConstructor();
        }

        internal static Type Build(ModuleBuilder moduleBuilder, Type delegateType, bool @return)
        {
            var builder = new ProxyBuilder(moduleBuilder, delegateType, @return);

            return builder.typeBuilder.CreateType();
        }

        /// <summary>
        /// Generate fields like<para/>
        ///     private TypeBuilder typeBuilder;<para/>
        ///     private string methodName;<para/>
        ///     private Lazy&lt;Func&lt;string, bool&gt;&gt; action;<para/>
        /// </summary>
        void GenerateFields()
        {
            typeBuilderField = GenerateField("typeBuilder", typeof(TypeBuilder));     //private TypeBuilder typeBuilder;
            methodNameField = GenerateField("methodName", typeof(string));            //private string methodName;

            if (delegateType.IsGenericType)
                dynamicFuncType = delegateType.MakeGenericType(genericArguments);     //Func<string, bool>
            else
            {
                //If the delegate type wasn't generic (i.e. Action) then use the original delgate type
                dynamicFuncType = delegateType;
            }
            
            dynamicLazyType = typeof(Lazy<>).MakeGenericType(dynamicFuncType);        //Lazy<Func<string, bool>>

            actionField = GenerateField("action", dynamicLazyType);                   //private Lazy<Func<string, bool>> action;
        }

        /// <summary>
        /// Generate a method like<para/>
        ///     public bool Invoke (string arg) => action.Value(arg);
        /// </summary>
        /// <param name="return">Whether this method should return a value.</param>
        private void GenerateInvoke(bool @return)
        {
            var takeLength = @return ? 1 : 0;

            var parameters = genericArguments.Take(genericArguments.Length - takeLength).ToArray();
            var returnType = @return ? genericArguments.Last() : null;

            invokeMethod = typeBuilder.DefineMethod(
                "Invoke",                                             //name
                MethodAttributes.Public | MethodAttributes.HideBySig, //attributes
                returnType,                                           //return type
                parameters                                            //parameterTypes
            );

            var lazyGetValue = typeof(Lazy<>).GetProperty("Value").GetGetMethod();
            var delegateInvoke = delegateType.GetMethod("Invoke");

            MethodInfo dynamicLazyGetValue;
            MethodInfo dynamicFuncInvoke;

            if (delegateType.IsGenericType)
            {
                //Get Lazy<Func<string, bool>>.get_Value
                dynamicLazyGetValue = TypeBuilder.GetMethod(dynamicLazyType, lazyGetValue);

                //Get Func<string, bool>.Invoke()
                dynamicFuncInvoke = TypeBuilder.GetMethod(dynamicFuncType, delegateInvoke);
            }
            else
            {
                //Get Lazy<Action>.get_Value
                dynamicLazyGetValue = typeof(Lazy<>).MakeGenericType(delegateType).GetProperty("Value").GetGetMethod();

                //Get Action.Invoke()
                dynamicFuncInvoke = delegateInvoke;
            }

            var ilg = invokeMethod.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_0);                       //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldfld, actionField);            //Load this.action            
            ilg.Emit(OpCodes.Callvirt, dynamicLazyGetValue); //Call action.get_Value();

            CallDelegate(ilg, parameters);                   //Load all the parameters to pass to action.Value.Invoke()

            ilg.Emit(OpCodes.Callvirt, dynamicFuncInvoke);   //Call action.Value.Invoke()
            ilg.Emit(OpCodes.Ret);                           //Return from the method
        }

        /// <summary>
        /// Generate a method like<para/>
        ///     private void Lambda()<para/>
        ///     {<para/>
        ///         return (Func&lt;string, bool>) Delegate.CreateDelegate(
        ///             typeof (Func&lt;string, bool&gt;),
        ///             this.typeBuilder.CreateType().GetMethod(this.methodName, BindingFlags.Static | BindingFlags.Public)
        ///         );<para/>
        ///     }
        /// </summary>
        private void GenerateLambda()
        {
            lambdaMethod = typeBuilder.DefineMethod(
                "Lambda",                                              //name
                MethodAttributes.Private | MethodAttributes.HideBySig, //attributes
                dynamicFuncType,                                       //return type
                Type.EmptyTypes                                        //parameterTypes (none - just uses this)
            );

            var createType = typeof(TypeBuilder).GetMethod("CreateType");
            var getMethod = typeof(Type).GetMethod("GetMethod", new[] { typeof(string), typeof(BindingFlags) });
            var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var createDelegate = typeof(Delegate).GetMethod("CreateDelegate", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(MethodInfo) }, null);

            var ilg = lambdaMethod.GetILGenerator();

            var methodLocal = ilg.DeclareLocal(typeof(MethodInfo));    //MethodInfo methodInfo;
            var funcLocal = ilg.DeclareLocal(dynamicFuncType);         //Func<string, bool> result;

            ilg.Emit(OpCodes.Nop);                                     //Allignment (probably)

            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldfld, typeBuilderField);                 //Load this.typeBuilder
            ilg.Emit(OpCodes.Callvirt, createType);                    //Call this.typeBuilder.CreateType();

            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldfld, methodNameField);                  //Load this.methodName
            ilg.Emit(OpCodes.Ldc_I4_S, (byte)24);                      //Load BindingFlags.Public (16) | BindingFlags.Static (8)
            ilg.Emit(OpCodes.Callvirt, getMethod);                     //Call GetMethod(methodName, BindingFlags.Public | BindingFlags.Static) on the value returned from this.typeBuilder.CreateType()
            ilg.Emit(OpCodes.Stloc_0);                                 //Store the result of GetMethod() in varible methodInfo

            ilg.Emit(OpCodes.Ldtoken, dynamicFuncType);                //typeof(Func<string, bool> results in two opcodes: https://blog.scooletz.com/2016/03/31/opcodes-ldtoken/
            ilg.Emit(OpCodes.Call, getTypeFromHandle);
            ilg.Emit(OpCodes.Ldloc_0);                                 //Load the variable methodInfo
            ilg.Emit(OpCodes.Call, createDelegate);                    //Call Delegate.CreateDelegate(typeof(Func<string, bool>), methodInfo);
            ilg.Emit(OpCodes.Castclass, dynamicFuncType);              //Cast the result of CreateDelegate to Func<string, bool>
            ilg.Emit(OpCodes.Stloc_1);                                 //Store the delegate result in variable "result"
            ilg.Emit(OpCodes.Ldloc_1);                                 //Load the delegate stored in variable "result"
            ilg.Emit(OpCodes.Ret);                                     //Return the delegate stored in variable "result" from the method
        }

        /// <summary>
        /// Generate a method like<para/>
        ///     public FuncProxy(TypeBuilder typeBuilder, string methodName)<para/>
        ///     {<para/>
        ///         this.typeBuilder = typeBuilder;<para/>
        ///         this.methodName = methodName;<para/>
        ///         this.action = new Lazy&lt;Func&lt;string, bool&gt;&gt;(new Func&lt;Func&lt;string, bool&gt;&gt;(this.Lambda));<para/>
        ///     }
        /// </summary>
        private void GenerateConstructor()
        {
            var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, new[] { typeof(TypeBuilder), typeof(string) });

            var objectCtor = typeof(object).GetConstructor(Type.EmptyTypes);
            var funcFuncCtor = typeof(Func<>).GetGenericTypeDefinition().GetConstructor(new[] { typeof(object), typeof(IntPtr) });
            var dynamicFuncFunc = typeof(Func<>).MakeGenericType(dynamicFuncType); //The parameter type expected by Lazy's constructor

            var dynamicFuncFuncCtor = MakeConstructor(dynamicFuncFunc, funcFuncCtor);
            var typedLazyCtor = typeof(Lazy<int>).GetConstructor(new[] { typeof(Func<int>) });
            var lazyCtor = typeof(Lazy<>).GetConstructors().First(c => c.MetadataToken == typedLazyCtor.MetadataToken);
            var dynamicLazyTypeCtor = MakeConstructor(dynamicLazyType, lazyCtor);

            var ilg = ctor.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Call, objectCtor);                        //Call the base class constructor (on type "object")
            ilg.Emit(OpCodes.Nop);                                     //Allignment (probably)

            ilg.Emit(OpCodes.Nop);                                     //Allignment (probably)

            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldarg_1);                                 //Load argument "typeBuilder"
            ilg.Emit(OpCodes.Stfld, typeBuilderField);                 //this.typeBuilder = typeBuilder;

            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldarg_2);                                 //Load argument "methodName"
            ilg.Emit(OpCodes.Stfld, methodNameField);                  //this.methodName = methodName;

            ilg.Emit(OpCodes.Ldarg_0);                                 //?
            ilg.Emit(OpCodes.Ldarg_0);                                 //Load the proxy onto the evaluation stack
            ilg.Emit(OpCodes.Ldftn, lambdaMethod);                     //Load this.lambda onto the stack
            ilg.Emit(OpCodes.Newobj, dynamicFuncFuncCtor);             //Create a new Func containing our lambda
            ilg.Emit(OpCodes.Newobj, dynamicLazyTypeCtor);             //Create a new Lazy containing the Func containing our lambda
            ilg.Emit(OpCodes.Stfld, actionField);                      //this.action = Lazy<Func<string, bool>>(...);
            ilg.Emit(OpCodes.Ret);                                     //Return from the constructor
        }

        private ConstructorInfo MakeConstructor(Type dynamicType, ConstructorInfo originalInfo)
        {
            if (delegateType.IsGenericType)
            {
                //We've had to create Func<string, bool> and in turn Lazy<Func<string, bool>>.
                //Whatever we're dealing with, its dynamic
                return TypeBuilder.GetConstructor(dynamicType, originalInfo);
            }
            else
            {
                var type = dynamicType.GetGenericTypeDefinition().MakeGenericType(delegateType);

                var ctor = type.GetConstructors().First(c => c.MetadataToken == originalInfo.MetadataToken);

                return ctor;
            }
        }

        private void CallDelegate(ILGenerator ilg, Type[] parameters)
        {
            /* A delegate could have a variable number of parameters
             *     Action  - 0
             *     Func<>  - 0
             *     Func<,> - 1
             * etc.
             * 
             * We use the normal Ldarg_x instructions for the first three parameters,
             * and Ldarg_S for all additional parameters
             */

            if (parameters.Length >= 1)
                ilg.Emit(OpCodes.Ldarg_1);           //T1

            if (parameters.Length >= 2)
                ilg.Emit(OpCodes.Ldarg_2);           //T2

            if (parameters.Length >= 3)
                ilg.Emit(OpCodes.Ldarg_3);           //T3

            for (var i = 4; i <= parameters.Length; i++)
                ilg.Emit(OpCodes.Ldarg_S, (short)i); //T4-T16
        }

        FieldBuilder GenerateField(string name, Type fieldType)
        {
            return typeBuilder.DefineField(name, fieldType, FieldAttributes.Private);
        }
    }
#endif
}
