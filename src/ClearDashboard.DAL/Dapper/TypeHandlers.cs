using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ClearDashboard.Common;

namespace ClearDashboard.DAL
{
    /// <summary>
    /// Used by Dapper to convert datatypes between the db and POCOs.
    /// </summary>
    public class EsDateTimeOffsetConverter : SqlMapper.TypeHandler<DateTimeOffset>
    {
        public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToUnixTimeSeconds().ToString();
        }

        public override DateTimeOffset Parse(object value)
        {
            long unixSeconds = long.Parse((string)value);
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        }
    }

    /// <summary>
    /// Used by Dapper to convert datatypes between the db and POCOs.
    /// </summary>
    public class ListOfStringsConverter : SqlMapper.TypeHandler<List<string>>
    {
        public override void SetValue(IDbDataParameter parameter, List<string> value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = string.Join(",", value);
        }

        public override List<string> Parse(object value)
        {
            return new List<string>(value.ToString().Split(','));
        }
    }

    //public class VerseInfoDataObjectConverter : SqlMapper.TypeHandler<List<Models.VerseInfoDataObject>>
    //{
    //    public override void SetValue(IDbDataParameter parameter, List<Models.VerseInfoDataObject> value)
    //    {
    //        parameter.DbType = DbType.String;
    //        parameter.Value = string.Join(",", value.Select(s => s.VerseId));
    //    }

    //    public override List<Models.VerseInfoDataObject> Parse(object value)
    //    {
    //        List<Models.VerseInfoDataObject> verseInfos = new();
    //        string[] verseInfoesStringArray = value.ToString().Split(',');

    //        foreach (string v in verseInfoesStringArray)
    //        {
    //            verseInfos.Add(new Models.VerseInfoDataObject(v));
    //        }

    //        return verseInfos;
    //    }
    //}
}
