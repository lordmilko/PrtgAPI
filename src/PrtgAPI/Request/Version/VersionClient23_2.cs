using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    internal partial class VersionClient23_2 : VersionClient18_1
    {
        private const string AntiCsrfToken = "anti-csrf-token";

        internal VersionClient23_2(PrtgClient client) : base(client)
        {
        }

        protected override void FixAuth(ICommandParameters internalParams, CancellationToken token)
        {
            var newAntiCsrf = client.GetAntiCsrfToken(token);
            UpdateCsrfToken(internalParams, newAntiCsrf);
        }

        protected override async Task FixAuthAsync(ICommandParameters internalParams, CancellationToken token)
        {
            var newAntiCsrf = await client.GetAntiCsrfTokenAsync(token).ConfigureAwait(false);
            UpdateCsrfToken(internalParams, newAntiCsrf);
        }

        private void UpdateCsrfToken(ICommandParameters internalParams, string newAntiCsrf)
        {
            object custom;

            if (internalParams.GetParameters().TryGetValue(Parameter.Custom, out custom))
            {
                var list = custom as List<CustomParameter>;

                if (list != null)
                {
                    var oldAntiCsrf = list.FirstOrDefault(v => v.Name == AntiCsrfToken);

                    if (oldAntiCsrf != null)
                        oldAntiCsrf.Value = newAntiCsrf;
                    else
                        list.Add(new CustomParameter(AntiCsrfToken, newAntiCsrf));
                }
            }
        }
    }
}
