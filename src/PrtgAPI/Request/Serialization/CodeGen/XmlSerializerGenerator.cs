using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PrtgAPI.Attributes;
using PrtgAPI.Linq.Expressions.Pretty;
using PrtgAPI.Request.Serialization;
using PrtgAPI.Reflection.Cache;
using PrtgAPI.Request.Serialization.CodeGen;
using PrtgAPI.Utilities;
using XmlMapping = PrtgAPI.Request.Serialization.XmlMapping;

namespace PrtgAPI.Linq.Expressions.Serialization
{
    class XmlSerializerGenerator
    {
        internal delegate bool ProcessMappingValueCallback<T>(XmlMapping mapping, int i, ref int mappingNameIndex, ref List<T> result);

        public List<XmlMapping> Mappings { get; }

        Type delegateType;
        Type trueType;
        internal bool update;

        internal ParameterExpression Target { get; }
        internal ParameterExpression DelegateTarget { get; }

#if DEBUG && DEBUG_SERIALIZATION
        private static Dictionary<Type, Tuple<Delegate, TypeBuilder>> fullLambdaCache = new Dictionary<Type, Tuple<Delegate, TypeBuilder>>();
        private static Dictionary<Type, Tuple<Delegate, TypeBuilder>> updateLambdaCache = new Dictionary<Type, Tuple<Delegate, TypeBuilder>>();
#else
        private static Dictionary<Type, Delegate> fullLambdaCache = new Dictionary<Type, Delegate>();
        private static Dictionary<Type, Delegate> updateLambdaCache = new Dictionary<Type, Delegate>();
#endif
        private static object lockObj = new object();
        private static object buildLock = new object();

        /// <summary>
        /// Retrieves the dynamic lambda expression for deserializing the specified type, generating it if it
        /// does not already exist.
        /// </summary>
        /// <param name="type">The type to deserialize.</param>
        /// <returns>A lambda expression capable of deserializing the specified type.</returns>
#if DEBUG && DEBUG_SERIALIZATION
        public static Tuple<Delegate, TypeBuilder> Get(Type type)
#else
        public static Delegate Get(Type type)
#endif
        {
            return GetInternal(type, null, fullLambdaCache, (t1, t2) => MakeLambda(t1, t2), false);
        }

#if DEBUG && DEBUG_SERIALIZATION
        public static Tuple<Delegate, TypeBuilder> Update(Type delegateType, Type trueType)
#else
        public static Delegate Update(Type delegateType, Type trueType)
#endif
        {
            return GetInternal(delegateType, trueType, updateLambdaCache, (t1, t2) => MakeLambda(t1, t2, true), true);
        }

#if DEBUG && DEBUG_SERIALIZATION
        private static Tuple<Delegate, TypeBuilder> GetInternal(
            Type delegateType,
            Type trueType,
            Dictionary<Type, Tuple<Delegate, TypeBuilder>> map,
            Func<Type, Type, LambdaExpression> build, bool updater)
        {
            Tuple<Delegate, TypeBuilder> lambda;
#else
        public static Delegate GetInternal(
            Type delegateType,
            Type trueType,
            Dictionary<Type, Delegate> map,
            Func<Type, Type, LambdaExpression> build, bool updater
            )
        {
            Delegate lambda;
#endif

            if (trueType == null)
                trueType = delegateType;

            lock (lockObj)
            {
                if (map.TryGetValue(trueType, out lambda))
                    return lambda;
            }

            var method = build(delegateType, trueType);

#if DEBUG && DEBUG_SERIALIZATION
            TypeBuilder typeBuilder;

            //todo: this lock still isnt locking properly
            lock(buildLock)
            {
                lock (lockObj)
                {
                    if (map.TryGetValue(trueType, out lambda))
                        return lambda;
                }

                var @delegate = DynamicAssembly.Generate(GetDynamicTypeName(trueType, updater), method, out typeBuilder);

                lambda = Tuple.Create(@delegate, typeBuilder);

                lock (lockObj)
                {
                    map[trueType] = lambda;
                }
            }
#else
            lambda = method.Compile();

            lock (lockObj)
            {
                map[trueType] = lambda;
            }
#endif

            return lambda;
        }

#if DEBUG && DEBUG_SERIALIZATION
        private static string GetDynamicTypeName(Type type, bool updater)
        {
            var builder = new StringBuilder();
            builder.Append("DynamicXmlExpressionSerializer_");

            GetDynamicTypeNameInternal(type, builder);

            var str = builder.ToString();

            lock(lockObj)
            {
                if (!fullLambdaCache.Any(v => v.Value.Item2.Name == str))
                {
                    if (updater)
                        str += "_Updater";

                    return str;
                }

                var index = 1;

                do
                {
                    str += index;
                    index++;
                } while (fullLambdaCache.Any(v => v.Value.Item2.Name == str));

                if (updater)
                    str += "_Updater";

                return str;
            }
        }

        private static string GetDynamicTypeNameInternal(Type type)
        {
            var builder = new StringBuilder();
            GetDynamicTypeNameInternal(type, builder);
            return builder.ToString();
        }

        private static void GetDynamicTypeNameInternal(Type type, StringBuilder builder)
        {
            builder.Append(CSharpWriter.CleanGenericName(type.Name));

            if (type.IsGenericType)
            {
                builder.Append("_");

                foreach (var t in type.GetGenericArguments())
                    GetDynamicTypeNameInternal(t, builder);
            }
        }
#else
        private static string GetDynamicTypeNameInternal(Type type)
        {
            return type.Name;
        }
#endif

        /// <summary>
        /// Construct an expression like
        /// 
        /// if (reader.NodeType == XmlNodeType.Element
        /// {
        ///     return ProcessTableDataInternal();
        /// }
        /// else
        /// {
        ///     SkipUnknownNode();
        ///     return null;
        /// }
        /// </summary>
        /// <param name="delegateType">The type of object that will be returned by the delegate.</param>
        /// <param name="trueType">The type of object the delegate will actually manipulate. If this object is null, <paramref name="delegateType"/> is used.</param>
        /// <param name="update">Whether this delegate will be used for constructing a new object or updating an existing one.</param>
        /// <returns>A lambda expression that deserializes a value of the specified type.</returns>
        internal static LambdaExpression MakeLambda(Type delegateType, Type trueType, bool update = false)
        {
            if (trueType == null)
                trueType = delegateType;

            if (trueType == typeof(ObjectProperty) || trueType == typeof(ObjectPropertyInternal))
            {
                //A single ObjectProperty doesn't actually bother with XML; it simply skips straight
                //to deserializing the value
                return MakeObjectPropertyLambda(trueType);
            }

            var generator = new XmlSerializerGenerator(delegateType, trueType, update);
            var processInternal = generator.MakeReadElement(true, null);        //ReadTableData            

            var invokeInternal = Expression.Invoke(                             //ReadTableData(serializer, reader, validateValueTypes);
                processInternal,
                generator.GetReadElementParameters(Expression.Convert(generator.DelegateTarget, trueType), true)
            );

            var moveToContent = XmlExpressionConstants.XmlReader_MoveToContent; //reader.MoveToContent();

            //Construct the test of the if statement
            var nodeType = XmlExpressionConstants.XmlReader_NodeType;           //reader.NodeType
            var element = Expression.Constant(XmlNodeType.Element);             //XmlNodeType.Element
            var equal = Expression.Equal(nodeType, element);                    //reader.NodeType == XmlNodeType.Element

            var list = new List<Expression>();
            list.Add(XmlExpressionConstants.Serializer_SkipUnknownNode);        //reader.SkipUnknownNode();

            if (!update)
                list.Add(Expression.Constant(null, delegateType));                      //return null;

            //Construct the ifFalse block of the if statement
            var skipBlock = Expression.Block(                                   //{
                update ? typeof(void) : delegateType,                           //    reader.SkipUnknownNode();
                list.ToArray()                                                  //    return null; }
            );

            //Construct the if statement
            var ifElement = Expression.Condition(                               //if (reader.NodeType == XmlNodeType.Element)
                equal,                                                          //    ReadTableData();
                Expression.Block(invokeInternal),                               //else {
                skipBlock                                                       //   reader.SkipUnknownNode();
            );                                                                  //   return null; }

            //Construct the entry point to the deserializer, ReadTableDataOuter
            return Expression.Lambda(                                           //TableData<Sensor> ReadTableDataOuter() {
                Expression.Block(                                               //    reader.MoveToContent();
                    moveToContent,                                              //    if (reader.NodeType == XmlNodeType.Element)
                    ifElement                                                   //        reader.ReadTableData();
                ),                                                              //    else {
                $"Read{GetDynamicTypeNameInternal(trueType)}Outer",             //        reader.SkipUnknownNode();
                generator.GetReadElementParameters(generator.DelegateTarget, false, false).Cast<ParameterExpression>().ToArray() //return null; }
            );                                                                  //}
        }

        internal Expression[] GetReadElementParameters(Expression targetOverride, bool invoke, bool lambdaNeedsValidator = true)
        {
            if (!update)
                return XmlExpressionConstants.ReadElementParameters;
            else
            {
                if (invoke)
                    return new[]
                    {
                        XmlExpressionConstants.Serializer,
                        XmlExpressionConstants.XmlReader,
                        Expression.Constant(true),
                        targetOverride ?? Target
                    };

                var list = new List<Expression>();
                list.Add(XmlExpressionConstants.Serializer);
                list.Add(XmlExpressionConstants.XmlReader);

                if (lambdaNeedsValidator)
                    list.Add(XmlExpressionConstants.ValidateValueTypes);

                list.Add(targetOverride ?? Target);

                return list.ToArray();
            }
        }

        internal Expression[] GetInnerReadElementParameters(bool parentUpdate, bool innerInvoke)
        {
            if ((!update || !innerInvoke) && !parentUpdate)
                return XmlExpressionConstants.ReadElementParameters;
            else
                return new Expression[] { XmlExpressionConstants.Serializer, XmlExpressionConstants.XmlReader, Expression.Constant(true) };
        }

        private static LambdaExpression MakeObjectPropertyLambda(Type trueType)
        {
            var builder = new ObjectPropertyBuilder(trueType);

            return builder.BuildDeserializer();
        }

        public XmlSerializerGenerator(Type delegateType, Type trueType, bool update)
        {
            if (trueType == null)
                trueType = delegateType;

            this.delegateType = delegateType;
            this.trueType = trueType;
            this.update = update;
            Mappings = XmlMapping.GetMappings(trueType).OrderBy(m => m.AttributeValue.FirstOrDefault() == "message").ToList();

            if (update)
            {
                Mappings = Mappings.Where(m =>
                {
                    var attrib = m.PropertyCache.GetAttribute<PropertyParameterAttribute>();

                    if (attrib != null && attrib.Property != null)
                    {
                        if (attrib.Property.Equals(Property.Name) || attrib.Property.Equals(Property.Id))
                            return false;
                    }

                    return true;
                }).ToList();
            }

            Target = Expression.Variable(trueType, "obj"); //var obj;
            DelegateTarget = Expression.Parameter(delegateType, "obj");
        }

        /// <summary>
        /// Construct an expression like
        /// 
        /// var obj = new Obj();
        /// 
        /// while(ReadAttribute())
        /// {
        ///     ...
        /// }
        /// 
        /// while(ReadElement())
        /// {
        ///     ...
        /// }
        /// 
        /// return obj;
        /// </summary>
        /// <param name="first">Whether this is the outer object being deserialized.</param>
        /// <param name="parentProperty">The property on our parent object this object will be deserialized to.</param>
        /// <returns>A lambda that reads the members of an object to completion.</returns>
        internal Expression MakeReadElement(bool first = false, PropertyCache parentProperty = null)
        {
            Expression moveToContent = null;
            ParameterExpression itemPointer = Expression.Variable(typeof(object), "item");

            if (first)
                moveToContent = XmlExpressionConstants.XmlReader_MoveToContent;

            var makeNew = Expression.Assign(Target, Expression.New(trueType));             //obj = new TableData<Sensor>();

            var initArrays = InitializeArrays();                                           //if (obj.Items == null) obj.Items = new List<Sensor>();

            var newFlags = Expression.NewArrayBounds(                                      //new bool[40]
                typeof(bool),
                Expression.Constant(Mappings.Count)
            );
            var flagsAssignment = XmlExpressionConstants.SerializerFlags.Assign(newFlags); //var flagsArray = new bool[40]

            var newName = Expression.NewArrayBounds(typeof(object),                        //new object[42]
                Expression.Constant(Mappings.Sum(m => m.AttributeValue.Length))
            );
            var nameAssignment = XmlExpressionConstants.SerializerNames.Assign(newName);   //var nameArray = new object[42]

            var populateNameTable = PopulateNameTable(parentProperty);                     //InitTableData(nameArray)

            var processAttributes = ProcessAttributes();
            var skipOrProcessElement = SkipOrProcessElement();

            var blockExpressions = new List<Expression>();

            if (first)
                blockExpressions.Add(moveToContent);                                       //reader.MoveToContent();

            if (!update)
                blockExpressions.Add(makeNew);                                             //var obj = new TObj()
            else
            {
                var nameTable = XmlExpressionConstants.XmlReader_NameTable;
                var add = XmlExpressionConstants.ListAdd(nameTable, Expression.Constant("item"));

                blockExpressions.Add(itemPointer.Assign(add));
                blockExpressions.Add(XmlExpressionConstants.XmlReader_ReadToFollowing(Expression.Convert(itemPointer, typeof(string))));
            }

            if (initArrays.Count > 0)
                blockExpressions.AddRange(initArrays);                                     //obj.Items = new List<Sensor>();

            blockExpressions.Add(flagsAssignment);                                         //var flagsArray = new bool[40]
            blockExpressions.Add(nameAssignment);                                          //var nameArray = new object[42]

            if (populateNameTable != null)
                blockExpressions.Add(populateNameTable);                                   //InitTableData(nameArray)

            blockExpressions.Add(processAttributes);                                       //if (!flagsArray[0] && (object)reader.Name == nameArray[0])
            blockExpressions.Add(XmlExpressionConstants.XmlReader_MoveToContent);
            blockExpressions.Add(skipOrProcessElement);

            var variables = new List<ParameterExpression>();

            if (!update)
                variables.Add(Target);                                                     //TableData<Sensor> obj;

            variables.Add(XmlExpressionConstants.SerializerFlags);                         //bool[] flagArray;
            variables.Add(XmlExpressionConstants.SerializerNames);                         //object[] nameArray;

            if (update)
                variables.Add(itemPointer);

            var block = Expression.Block(
                variables.ToArray(),
                blockExpressions
            );

            var lambda = Expression.Lambda(
                block,
                $"Read{GetDynamicTypeNameInternal(trueType)}",
                GetReadElementParameters(null, false).Cast<ParameterExpression>().ToArray()
            );

            return LambdaOrDelegate(lambda);
        }

        private List<ConditionalExpression> InitializeArrays()
        {
            var lists = Mappings.Where(m => LadderCondition.MappingIsList(m));

            var expressions = lists.Select(l => LadderCondition.InitializeListIfNull(Target, l).Item2).ToList();

            return expressions;
        }

        /// <summary>
        /// Generate an expression like
        /// 
        /// if (reader.IsEmptyElement)
        /// {
        ///     reader.Skip();
        ///     return obj;
        /// }
        /// 
        /// [Process Element]
        /// 
        /// return obj;
        /// </summary>
        /// <returns>An expression that skips or processes an element.</returns>
        private Expression SkipOrProcessElement()
        {
            var skip = SkipElement();
            var process = ProcessElement();

            var isEmptyElement = XmlExpressionConstants.XmlReader_IsEmptyElement;

            return Expression.Condition(isEmptyElement, skip, process);
        }

        /// <summary>
        /// Generate an expression like
        /// 
        /// if (reader.IsEmptyElement)
        /// {
        ///     reader.Skip();
        ///     return obj;
        /// }
        /// </summary>
        /// <returns>An expression that skips an element.</returns>
        private Expression SkipElement()
        {
            var list = new List<Expression>
            {
                XmlExpressionConstants.XmlReader_Skip
            };

            var validator = MakeValidateValueTypes();

            if (validator != null)
                list.Add(validator);

            if (!update)
                list.Add(Target);

            return Expression.Block(list.ToArray());
        }

        private Expression ProcessElement()
        {
            var nodeType = XmlExpressionConstants.XmlReader_NodeType;
            var element = Expression.Constant(XmlNodeType.Element);
            var endElement = Expression.Constant(XmlNodeType.EndElement);
            var none = Expression.Constant(XmlNodeType.None);

            var loopCondition = Expression.AndAlso(
                nodeType.NotEqual(endElement), //reader.NodeType != XmlNodeType.EndElement
                nodeType.NotEqual(none)        //reader.NodeType != XmlNodeType.none
            );

            var builder = new LadderBuilder(this, XmlAttributeType.Element);

            var isElement = nodeType.Equal(element);
            var elementLadder = builder.GetLadder();
            var skipUnknown = XmlExpressionConstants.Serializer_SkipUnknownNode;

            Expression loopBody;

            var textMappings = Mappings.Where(m => m.AttributeType == XmlAttributeType.Text).ToList();

            if (textMappings.Count > 0)
            {
                if (textMappings.Count > 1)
                {
                    var str = string.Join(", ", textMappings.Select(m => m.PropertyCache.Property.Name));

                    throw new InvalidOperationException($"Canot deserialize type {trueType.Name} as multiple properties contain a {nameof(XmlTextAttribute)} ({str})."); //todo: complain about the name of this type and the properties that have the two xmltextattributes
                }

                var isText = nodeType.Equal(Expression.Constant(XmlNodeType.Text));
                var textBlock = GetTextBlock(textMappings.Single());

                loopBody = new ElseIfExpression(
                    new[]
                    {
                        Expression.IfThen(isElement, elementLadder), //if (reader.NodeType == XmlNodeType.Element) [Ladder]
                        Expression.IfThen(isText, textBlock)         //else if (reader.NodeType == XmlNodeType.Text) [TextBlock]
                    },        
                    skipUnknown                                      //else SkipUnknownNode()
                );
            }
            else
            {
                loopBody = Expression.IfThenElse(                    //if (reader.NodeType == XmlNodeType.Element)
                    isElement,                                       //    [Ladder]
                    elementLadder,                                   //else
                    skipUnknown                                      //    SkipUnknownNode()
                );
            }

            var @break = Expression.Label("break");

            var loop = Expression.Loop(
                Expression.IfThenElse(loopCondition, loopBody, Expression.Break(@break)),
                @break
            );

            var body = new List<Expression>();

            body.Add(XmlExpressionConstants.XmlReader_ReadStartElement);
            body.Add(XmlExpressionConstants.XmlReader_MoveToContent);
            body.Add(loop);
            body.Add(XmlExpressionConstants.XmlReader_ReadEndElement);

            var validator = MakeValidateValueTypes();

            if (validator != null)
                body.Add(validator);

            if (!update)
                body.Add(Target);

            return Expression.Block(body.ToArray());
        }

        private bool BuildTextConditions(XmlMapping mapping, int i, ref int mappingNameIndex, ref List<ConditionalExpression> result)
        {
            if (mapping.AttributeType == XmlAttributeType.Text)
            {
                var blockBuilder = new LadderCondition(mapping, i, this);

                var condition = blockBuilder.GetCondition(ref mappingNameIndex);

                result.Add(condition);
                return true;
            }

            return false;
        }

        private Expression GetTextBlock(XmlMapping mapping)
        {
            var body = ForEachMappingValue<ConditionalExpression>(BuildTextConditions).Single().IfTrue;

            return body;
        }

        private Expression ProcessAttributes()
        {
            var builder = new LadderBuilder(this, XmlAttributeType.Attribute);
            var processAttributesBody = builder.GetLadder();

            var @break = Expression.Label("break");

            var condition = Expression.IfThenElse(
                XmlExpressionConstants.XmlReader_MoveToNextAttribute,
                processAttributesBody,
                Expression.Break(@break)
            );

            var loop = Expression.Loop(condition, @break);

            return loop;
        }

        /// <summary>
        /// Generate an expression like
        /// 
        /// void InitSensor(object[] nameArray, XmlReader reader)
        /// {
        ///     nameArray[0] = reader.NameTable.Add("name");
        ///     nameArray[1] = reader.NameTable.Add("objid");
        /// }
        /// </summary>
        /// <returns>The invocation of the generated lambda.</returns>
        private Expression PopulateNameTable(PropertyCache parentProperty)
        {
            var nameTable = XmlExpressionConstants.XmlReader_NameTable;

            var names = Mappings.SelectMany(m => m.AttributeValue).ToArray();

            var assignments = new List<BinaryExpression>();

            //We don't have any deserializable elements (e.g. GetTotalObjects which just asks for a TableData<object>)
            if (names.Length == 0)
                return null;

            for(var i = 0; i < names.Length; i++)
            {
                //We're probably an XmlTextAttribute; get our parent's name
                if (names[i] == null)
                    names[i] = parentProperty.GetAttribute<XmlElementAttribute>().ElementName;

                //XmlTextAttribute items don't have an AttributeValue
                if (names[i] != null)
                {
                    var name = Expression.ArrayAccess(                                                                //nameArray[0]
                        XmlExpressionConstants.SerializerNames,
                        Expression.Constant(i)
                    );

                    var add = Expression.Call(                                                                        //reader.NameTable.Add("name")
                        nameTable,
                        XmlSerializerMembers.XmlNameTable_Add,
                        Expression.Constant(names[i])
                    );

                    var assign = Expression.Assign(name, add);                                                        //nameArray[0] = reader.NameTable.Add("name")

                    assignments.Add(assign);
                }
            }

            var lambda = Expression.Lambda(
                Expression.Block(typeof(void), assignments.ToArray()),                                                //Body
                $"Init{trueType.Name}",                                                                               //Name
                new[] { XmlExpressionConstants.SerializerNames, XmlExpressionConstants.XmlReader }                    //Parameters
            );

            var expr = LambdaOrDelegate(lambda);

            return Expression.Invoke(expr, XmlExpressionConstants.SerializerNames, XmlExpressionConstants.XmlReader); //InitSensor(nameArray, serializer.reader)
        }       

        private Expression MakeValidateValueTypes()
        {
            var list = ForEachMappingValue<ConditionalExpression>(BuildConditions);
            
            if (list.Count > 0)
            {
                var block = Expression.Block(list.ToArray());

                var args = new[] { XmlExpressionConstants.SerializerFlags, XmlExpressionConstants.SerializerNames, XmlExpressionConstants.Serializer };

                var lambda = Expression.Lambda(
                    block, $"Validate{GetDynamicTypeNameInternal(trueType)}",
                    args
                );

                var invocation = Expression.Invoke(
                    lambda,
                    args
                );

                return Expression.IfThen(
                    XmlExpressionConstants.ValidateValueTypes,
                    invocation
                );
            }

            return null;
        }

        private bool BuildConditions(XmlMapping mapping, int i, ref int mappingNameIndex, ref List<ConditionalExpression> result)
        {
            var type = Mappings[i].PropertyCache.Property.PropertyType;

            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
            {
                var flagIndex = Expression.Constant(i);
                var flag = Expression.ArrayIndex(XmlExpressionConstants.SerializerFlags, flagIndex);    //flagArray[0]

                var nameIndex = Expression.Constant(mappingNameIndex);
                var name = Expression.ArrayIndex(XmlExpressionConstants.SerializerNames, nameIndex);

                var assignName = XmlExpressionConstants.Serializer_Name(mapping.AttributeType).Assign(name);

                var throwNull = XmlExpressionConstants.Fail(null, type).Throw();                        //throw Fail(null, null, typeof(int));

                var block = Expression.Block(
                    assignName,
                    throwNull
                );

                var condition = flag.Not().IfThen(block);

                result.Add(condition);
            }

            return false;
        }

        internal List<T> ForEachMappingValue<T>(ProcessMappingValueCallback<T> processMapping)
        {
            var mappingNameIndex = 0;
            var list = new List<T>();

            for (var i = 0; i < Mappings.Count; i++)
            {
                if (!processMapping(Mappings[i], i, ref mappingNameIndex, ref list))
                    mappingNameIndex += Mappings[i].AttributeValue.Length;
            }

            return list;
        }

        internal static Expression LambdaOrDelegate(LambdaExpression lambda)
        {
#if DEBUG
            return lambda;
#else
            var d = lambda.Compile();
            return Expression.Constant(d);
#endif
        }
    }
}
