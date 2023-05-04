using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class MigrationChecker
    {
        private Version _version;

        public MigrationChecker(string version)
        {
            try
            {
                _version = Version.Parse(version);
            }
            catch (Exception e)
            {
                _version = new Version();
            }
        }


        /// <summary>
        /// Check to see if eligible for 'Reset verse mappings to default versification'
        ///
        /// Anything less than version 1.0.5 should be updated
        /// </summary>
        /// <returns></returns>
        public bool CheckForResetVerseMappings()
        {
            if (_version is null)
            {
                return false;
            }

            if (_version <= Version.Parse("1.0.5"))
            {
                return true;
            }
            
            return false;
        }
    }
}
