using System;

namespace PrtgAPI.Parameters
{
    class LoadConfigFilesParameters : BaseParameters, ICommandParameters
    {
        CommandFunction ICommandParameters.Function
        {
            get
            {
                if (fileType == ConfigFileType.General)
                    return CommandFunction.ReloadFileLists;
                if (fileType == ConfigFileType.Lookups)
                    return CommandFunction.LoadLookups;

                throw new NotImplementedException($"Don't know how to handle file type '{fileType}'.");
            }
        }

        ConfigFileType fileType;

        public LoadConfigFilesParameters(ConfigFileType fileType)
        {
            this.fileType = fileType;
        }
    }
}
