using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Insight360.Util
{
    public enum DataProcessingState
    {
        UnProcessed = 0,
        Processing,
        Processed,
        KeywordsNotFound,
        DocumentSourceNotFound,
        UnProcessed_DocumentUpdate=100,
        Refresh=1
    }

	public enum DataPersistenceMode
	{
		SingleDataSource ,
		MultipleDataSources
	}
}
