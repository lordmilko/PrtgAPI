using System;
using System.Collections.Generic;
using System.Linq;

namespace PrtgAPI.Linq.Expressions
{
    class ConsecutiveCallManager
    {
        private Dictionary<string, Tuple<CallCounter, LinqExpression>> counter = new Dictionary<string, Tuple<CallCounter, LinqExpression>>();

        private List<LinqExpression> GetExpressions() => counter.Select(c => c.Value.Item2).ToList();

        public bool HasOtherConditions(Type thisType) => HasType<ConditionLinqExpression>(thisType);

        public bool HasCountLimiter(Type thisType) => HasType<ICountLimitLinqExpression>(thisType);

        public bool HasCountOffset(Type thisType) => HasType<ICountOffsetLinqExpression>(thisType);

        private bool HasType<T>(Type thisType) => GetExpressions().Any(e => e is T && e.GetType() != thisType);

        public bool HasUnknownMethod() => GetExpressions().Any(e => e == null);

        public CallCounter LastMethod { get; private set; }

        public bool Call(string method, LinqExpression expr)
        {
            Tuple<CallCounter, LinqExpression> methodCounter;
            
            if (counter.TryGetValue(method, out methodCounter))
            {
                return CompareWithPrevious(methodCounter.Item1);
            }
            else
            {
                methodCounter = Tuple.Create(new CallCounter(), expr);
                counter[method] = methodCounter;
                return CompareWithPrevious(methodCounter.Item1);
            }
        }

        private bool CompareWithPrevious(CallCounter methodCounter)
        {
            //We've already called this method before (and were interrupted by another method).
            //Additional calls must be formatted as IEnumerable
            if (methodCounter.ChainCompleted)
                return false;

            if (LastMethod != null)
            {
                //Same method as last time
                if (methodCounter == LastMethod)
                {
                    methodCounter.ConsecutiveCalls++;
                    return true;
                }
                else
                {
                    //We're being replaced by another method
                    LastMethod.ChainCompleted = true;
                    return CallNewMethod(methodCounter);
                }
            }
            else
            {
                //First time calling a method
                return CallNewMethod(methodCounter);
            }
        }

        private bool CallNewMethod(CallCounter methodCounter)
        {
            LastMethod = methodCounter;
            methodCounter.ConsecutiveCalls++;
            return true;
        }
    }
}