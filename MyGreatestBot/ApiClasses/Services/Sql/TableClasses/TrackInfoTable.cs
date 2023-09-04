using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Sql.TableClasses
{
    internal class TrackInfoTable : GenericTable
    {
        internal TrackInfoTable(string database) : base("SavedTracks", database)
        {

        }
    }
}
