﻿/*******************************************************************************************
 * This code was generated by a tool.                                                      *
 * Please do not modify this file directly - modify VersionClient18_1.Generated.tt instead *
 *******************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.Request
{
    internal partial class VersionClient18_1
    {
        //######################################
        // AddSensorInternal
        //######################################

        internal override void AddSensorInternal(ICommandParameters internalParams, int index, CancellationToken token)
        {
            var deviceId = (int) internalParams[Parameter.Id];

            int? tmpId = (int?) internalParams[Parameter.TmpId];

            //If it's our first request (not the case when adding excesive values)
            if (index == 0)
            {
                if (tmpId == null)
                    tmpId = GetTmpId(deviceId, internalParams, token);

                internalParams[Parameter.TmpId] = tmpId.Value;
                ((BaseParameters)internalParams).Cookie = true;
            }

            FixAuth(internalParams, token);
            client.AddObjectInternalDefault(internalParams, token);

            //If it's our first request (not the case when adding excesive values)
            if (index == 0)
            {
                var progressParameters = new AddSensorProgressParameters(deviceId, tmpId.Value, true);
                var progress = client.ObjectEngine.GetObject<AddSensorProgress>(progressParameters, token: token);

                client.ValidateAddSensorProgressResult(deviceId, progress, true, token);
            }         
        }

        internal override async Task AddSensorInternalAsync(ICommandParameters internalParams, int index, CancellationToken token)
        {
            var deviceId = (int) internalParams[Parameter.Id];

            int? tmpId = (int?) internalParams[Parameter.TmpId];

            //If it's our first request (not the case when adding excesive values)
            if (index == 0)
            {
                if (tmpId == null)
                    tmpId = await GetTmpIdAsync(deviceId, internalParams, token).ConfigureAwait(false);

                internalParams[Parameter.TmpId] = tmpId.Value;
                ((BaseParameters)internalParams).Cookie = true;
            }

            await FixAuthAsync(internalParams, token).ConfigureAwait(false);
            await client.AddObjectInternalDefaultAsync(internalParams, token).ConfigureAwait(false);

            //If it's our first request (not the case when adding excesive values)
            if (index == 0)
            {
                var progressParameters = new AddSensorProgressParameters(deviceId, tmpId.Value, true);
                var progress = await client.ObjectEngine.GetObjectAsync<AddSensorProgress>(progressParameters, token: token).ConfigureAwait(false);

                await client.ValidateAddSensorProgressResultAsync(deviceId, progress, true, token).ConfigureAwait(false);
            }         
        }
    }
}
