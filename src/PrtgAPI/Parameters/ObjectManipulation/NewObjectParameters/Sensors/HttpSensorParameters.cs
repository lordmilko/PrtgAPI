using System;
using PrtgAPI.Attributes;
using PrtgAPI.Request;

namespace PrtgAPI.Parameters
{
    /// <summary>
    /// Represents parameters used to construct a <see cref="PrtgRequestMessage"/> for creating a new HTTP sensor.
    /// </summary>
    public class HttpSensorParameters : SensorParametersInternal
    {
        internal override string[] DefaultTags => new[] { "httpsensor" };

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSensorParameters"/> class.
        /// </summary>
        /// <param name="url">The URL to monitor.</param>
        /// <param name="sensorName">The name to use for the sensor.</param>
        /// <param name="requestMethod">The HTTP request method to use.</param>
        /// <param name="timeout">The duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.</param>
        /// <param name="postData">Data to include in requests when the HTTP request method is <see cref="PrtgAPI.HttpRequestMethod.POST"/>.</param>
        /// <param name="useCustomPostContent">Whether to use a custom content type in POST requests.</param>
        /// <param name="postContentType">The custom content type to use in POST requests.</param>
        /// <param name="useSNIFromUrl">Whether the Server Name Indication should be derived from the specified <see cref="Url"/>, or from the parent device.</param>
        public HttpSensorParameters(string url = "http://", string sensorName = "HTTP", HttpRequestMethod requestMethod = HttpRequestMethod.GET, int timeout = 60,
            string postData = null, bool useCustomPostContent = false, string postContentType = null, bool useSNIFromUrl = false) : base(sensorName, SensorType.Http)
        {
            Url = url;
            HttpRequestMethod = requestMethod;
            Timeout = timeout;
            PostData = postData;
            UseCustomPostContent = useCustomPostContent;
            PostContentType = postContentType;
            UseSNIFromUrl = useSNIFromUrl;

            //todo: add a unit test for this to our existing test files that test on wmi service/exexml sensors
            //update about_sensorsettings to document the new objectproperty types
            //make a note to update wiki to say this type is now nataively supported
        }

        /// <summary>
        /// Gets or sets the URL to monitor. If a protocol is not specified, HTTP is used.
        /// </summary>
        [RequireValue(true)]
        [PropertyParameter(ObjectProperty.Url)]
        public string Url
        {
            get { return (string)GetCustomParameter(ObjectProperty.Url); }
            set { SetCustomParameter(ObjectProperty.Url, value); }
        }

        /// <summary>
        /// Gets or sets the duration (in seconds) this sensor can run for before timing out. This value must be between 1-900.
        /// </summary>
        [PropertyParameter(ObjectProperty.Timeout)]
        public int Timeout
        {
            get { return (int)GetCustomParameter(ObjectProperty.Timeout); }
            set
            {
                if (value > 900)
                    throw new ArgumentException("Timeout must be less than 900.");

                if (value < 1)
                    throw new ArgumentException("Timeout must be greater than or equal to 1.");

                SetCustomParameter(ObjectProperty.Timeout, value);
            }
        }

        /// <summary>
        /// Gets or sets the HTTP Request Method to use for requesting the <see cref="Url"/>.
        /// </summary>
        [PropertyParameter(ObjectProperty.HttpRequestMethod)]
        public HttpRequestMethod HttpRequestMethod
        {
            get { return (HttpRequestMethod)GetCustomParameterEnumXml<HttpRequestMethod>(ObjectProperty.HttpRequestMethod); }
            set { SetCustomParameterEnumXml(ObjectProperty.HttpRequestMethod, value); }
        }

        /// <summary>
        /// Gets or sets the data to include in POST requests. Applies when <see cref="HttpRequestMethod"/> is <see cref="PrtgAPI.HttpRequestMethod.POST"/>.
        /// </summary>
        [PropertyParameter(ObjectProperty.PostData)]
        public string PostData
        {
            get { return (string)GetCustomParameter(ObjectProperty.PostData); }
            set { SetCustomParameter(ObjectProperty.PostData, value); }
        }

        /// <summary>
        /// Gets or sets whether POST requests should use a custom content type. If false, content type "application/x-www-form-urlencoded" will be used.
        /// </summary>
        [PropertyParameter(ObjectProperty.UseCustomPostContent)]
        public bool UseCustomPostContent
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.UseCustomPostContent); }
            set { SetCustomParameterBool(ObjectProperty.UseCustomPostContent, value); }
        }

        /// <summary>
        /// Gets or sets a custom content type to use for POST requests.
        /// </summary>
        [PropertyParameter(ObjectProperty.PostContentType)]
        public string PostContentType
        {
            get { return (string)GetCustomParameter(ObjectProperty.PostContentType); }
            set { SetCustomParameter(ObjectProperty.PostContentType, value); }
        }

        /// <summary>
        /// Gets or sets whether the Server Name Indication is inherited from the parent device, or derived from the specified <see cref="Url"/>.
        /// </summary>
        [PropertyParameter(ObjectProperty.UseSNIFromUrl)]
        public bool UseSNIFromUrl
        {
            get { return (bool) GetCustomParameterBool(ObjectProperty.UseSNIFromUrl); }
            set { SetCustomParameterBool(ObjectProperty.UseSNIFromUrl, value); }
        }
    }
}
