using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Packaging;

namespace UFormKit.Migrations
{
    public class PackageMigrationPlan : AutomaticPackageMigrationPlan
    {
        public PackageMigrationPlan() : base("UFormKit")
        {
        }
    }
}
