using System;
using System.Collections.Generic;

namespace Dapper.Builder.Builder.Processes.Configuration {
    public class ExcludeZone {
        private List<Type> _excludedTypes;

        public ExcludeZone (List<Type> excludeTypes) {
            this._excludedTypes = excludeTypes;
        }
        public void Dispose () {
            _excludedTypes.Clear ();
        }
    }
}