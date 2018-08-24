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

            var urls = filters.Select(f => $"content=sensors&columns={TestHelpers.DefaultSensorProperties()}&count=500{getDelim(f)}{f}").ToArray();

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
            var urls = url.SelectMany(u => new[]
            {
                $"https://prtg.example.com/api/table.xml?{u}&username=username&passhash=12345678"
            });

            var client = GetClient(urls.ToArray(), propertyManipulator: propertyManipulator);

            var result = func(client.QuerySensors()).ToList();

            validator(result);
        }

        protected void ExecuteNow<TResult>(Func<IQueryable<Sensor>, TResult> func, string url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            ExecuteNow(func, new[] { url }, validator, flags, count);
        }

        protected void ExecuteNow<TResult>(Func<IQueryable<Sensor>, TResult> func, string[] url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            url = url.Select(f => TestHelpers.RequestSensor(f, flags)).ToArray();

            var client = GetClient(url, count);

            var result = func(client.QuerySensors());

            validator(result);
        }

        protected void ExecuteSkip<TResult>(Func<IQueryable<Sensor>, TResult> func, string url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            ExecuteSkip(func, new[] { url }, validator, flags, count);
        }

        protected void ExecuteSkip<TResult>(Func<IQueryable<Sensor>, TResult> func, string[] url, Action<TResult> validator, UrlFlag flags = UrlFlag.Columns | UrlFlag.Count, int count = 3)
        {
            var list = new List<string>();
            list.Add(TestHelpers.RequestSensorCount);
            list.AddRange(url.Select(f => TestHelpers.RequestSensor(f, flags)));

            var client = GetClient(list.ToArray(), count);

            var result = func(client.QuerySensors());

            validator(result);
        }

        protected void ExecuteClient<TResult>(Func<PrtgClient, TResult> action, string[] urls, Action<TResult> validator)
        {
            var client = GetClient(urls);

            var result = action(client);

            validator(result);
        }

        protected void ExecuteNullable<TResult>(Func<IQueryable<Sensor>, IQueryable<TResult>> func, string url, Action<SensorItem> manipulator)
        {
            var u = "content=sensors&";

            if (!url.Contains("columns"))
                u += $"columns={TestHelpers.DefaultSensorProperties()}";
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

        protected PrtgClient GetClient(string[] urls, int sensorCount = 3, int deviceCount = 4, int logCount = 5, Dictionary<Content, Action<BaseItem>> propertyManipulator = null)
        {
            var client = Initialize_Client(new AddressValidatorResponse(urls.Cast<object>().ToArray())
            {
                CountOverride = new Dictionary<Content, int>
                {
                    [Content.Sensors] = sensorCount,
                    [Content.Devices] = deviceCount,
                    [Content.Logs] = logCount,
                },
                PropertyManipulator = propertyManipulator
            });

            return client;
        }
    }
}
