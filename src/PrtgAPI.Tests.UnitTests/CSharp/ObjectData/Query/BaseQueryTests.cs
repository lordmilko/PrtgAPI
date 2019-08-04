using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrtgAPI.Tests.UnitTests.Support;
using PrtgAPI.Tests.UnitTests.Support.TestResponses;
using PrtgAPI.Tests.UnitTests.Support.TestItems;

namespace PrtgAPI.Tests.UnitTests.ObjectData.Query
{
    public abstract class BaseQueryTests : BaseTest
    {
        protected void ExecuteQuery(Func<IQueryable<Sensor>, IQueryable<Sensor>> func, string filter, Action<List<Sensor>> validator)
        {
            ExecuteQuery(func, new[] { filter }, validator);
        }

        protected void ExecuteQuery(Func<IQueryable<Sensor>, IQueryable<Sensor>> func, string[] filters,
            Action<List<Sensor>> validator)
        {
            Func<string, string> getDelim = f => f != string.Empty ? "&" : "";

            var urls = filters.Select(f => UnitRequest.Sensors($"count=500{getDelim(f)}{f}", UrlFlag.Columns)).ToArray();

            Execute(func, urls, validator);
        }

        protected void ExecuteFilter(System.Linq.Expressions.Expression<Func<Sensor, bool>> predicate, string filter, Action<List<Sensor>> validator)
        {
            ExecuteFilter(predicate, new[] { filter }, validator);
        }

        protected void ExecuteFilter(System.Linq.Expressions.Expression<Func<Sensor, bool>> predicate, string[] filters, Action<List<Sensor>> validator)
        {
            ExecuteQuery(q => q.Where(predicate), filters, validator);
        }

        protected void Execute<TResult>(Func<IQueryable<Sensor>, IQueryable<TResult>> func, string url, Action<List<TResult>> validator)
        {
            Execute(func, new[] { url }, validator);
        }

        protected void Execute<TResult>(Func<IQueryable<Sensor>, IQueryable<TResult>> func, string[] url, Action<List<TResult>> validator, int count = 3, Dictionary<Content, Action<BaseItem>> propertyManipulator = null)
        {
            var urls = url.SelectMany(u =>
            {
                if (u.StartsWith("https"))
                    return new[] {u};

                return new[]
                {
                    $"https://prtg.example.com/api/table.xml?{u}&username=username&passhash=12345678"
                };
            });

            var client = GetClient(urls.ToArray(), propertyManipulator: propertyManipulator);

            var result = func(client.Item1.QuerySensors()).ToList();

            validator(result);

            client.Item2.AssertFinished();
        }

        protected void ExecuteNow<TResult>(Func<IQueryable<Sensor>, TResult> func, string url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            if ((flags & UrlFlag.Count) == UrlFlag.Count)
            {
                url = $"count=500" + (string.IsNullOrEmpty(url) ? url : $"&{url}");
                flags = flags & ~UrlFlag.Count;
            }

            ExecuteNow(func, new[] { url }, validator, flags, count);
        }

        protected void ExecuteNow<TResult>(Func<IQueryable<Sensor>, TResult> func, string[] url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            url = url.Select(f =>
            {
                var innerFlags = flags;

                if ((flags & UrlFlag.Count) == UrlFlag.Count)
                {
                    f = $"count=500" + (string.IsNullOrEmpty(f) ? f : $"&{f}");
                    innerFlags = flags & ~UrlFlag.Count;
                }

                return UnitRequest.Sensors(f, innerFlags);
            }).ToArray();

            var client = GetClient(url, count);

            var result = func(client.Item1.QuerySensors());

            validator(result);

            client.Item2.AssertFinished();
        }

        protected void ExecuteSkip<TResult>(Func<IQueryable<Sensor>, TResult> func, string url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            ExecuteSkip(func, new[] { url }, validator, flags, count);
        }

        protected void ExecuteSkip<TResult>(Func<IQueryable<Sensor>, TResult> func, string[] url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            var list = new List<string>();
            list.Add(UnitRequest.SensorCount);
            list.AddRange(url.Select(f => UnitRequest.Sensors(f, flags)));

            var client = GetClient(list.ToArray(), count);

            var result = func(client.Item1.QuerySensors());

            validator(result);

            client.Item2.AssertFinished();
        }

        protected void ExecuteClient<TResult>(Func<PrtgClient, TResult> action, string[] urls, Action<TResult> validator)
        {
            var client = GetClient(urls);

            var result = action(client.Item1);

            validator(result);

            client.Item2.AssertFinished();
        }

        protected void ExecuteNullable<TResult>(Func<IQueryable<Sensor>, IQueryable<TResult>> func, string url, Action<SensorItem> manipulator)
        {
            var u = "content=sensors&";

            if (!url.Contains("columns"))
                u += $"columns={UnitRequest.DefaultSensorProperties()}";
            else
                u += url;

            if (url.StartsWith("filter_"))
                u += "&count=500&" + url;
            else
                u += "&count=500";

            Execute(func, new[] {u}, s =>
            {
                if (s.Count > 0)
                {
                    Assert.AreEqual(3, s.Count);
                    Assert.AreEqual(null, s[0]);
                    Assert.AreEqual(null, s[1]);
                    Assert.AreEqual(null, s[2]);
                }
                else
                    Assert.AreEqual(0, s.Count);
            }, propertyManipulator: new Dictionary<Content, Action<BaseItem>>
            {
                [Content.Sensors] = i => manipulator(((SensorItem)i))
            });
        }

        protected Tuple<PrtgClient, AddressValidatorResponse> GetClient(string[] urls, int sensorCount = 3, int deviceCount = 4, int logCount = 5, Dictionary<Content, Action<BaseItem>> propertyManipulator = null)
        {
#pragma warning disable 618
            var response = new AddressValidatorResponse(urls.Cast<object>().ToArray())
            {
                CountOverride = new Dictionary<Content, int>
                {
                    [Content.Sensors] = sensorCount,
                    [Content.Devices] = deviceCount,
                    [Content.Logs] = logCount,
                },
                PropertyManipulator = propertyManipulator
            };

            var client = Initialize_Client(response);
#pragma warning restore 618

            return Tuple.Create(client, response);
        }

        public static string Cast(string value, string type)
        {
#if NETFRAMEWORK
            return $"Convert({value})";
#else
            return $"Convert({value}, {type})";
#endif
        }
    }
}
