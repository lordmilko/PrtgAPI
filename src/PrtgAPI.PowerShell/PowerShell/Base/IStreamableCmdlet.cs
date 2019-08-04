using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrtgAPI.Parameters;

namespace PrtgAPI.PowerShell.Base
{
    interface IStreamableCmdlet<TCmdlet, TObject, TParam>
        where TCmdlet : PrtgProgressCmdlet, IStreamableCmdlet<TCmdlet, TObject, TParam>
        where TParam : PageableParameters, IXmlParameters
    {
        StreamableCmdletProvider<TCmdlet, TObject, TParam> StreamProvider { get; set; }

        Tuple<List<TObject>, int> GetStreamObjects(TParam parameters);

        Task<List<TObject>> GetStreamObjectsAsync(TParam parameters);

        int GetStreamTotalObjects(TParam parameters);
    }
}