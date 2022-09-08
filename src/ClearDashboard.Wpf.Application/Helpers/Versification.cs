using System;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Models;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public static class Versification
    {
        public static List<VersificationList> GetVersificationFromOriginal(List<VersificationList> list,
            ParatextProject paratextProject)
        {

            ScrVers projVersification = new ScrVers(paratextProject.ScrVerseType);

            foreach (var verse in list)
            {
                try
                {
                    // we are converting from an original versification type to whatever
                    // versification type the project is
                    var reference = new VerseRef(Convert.ToInt32(verse.SourceBBBCCCVV), ScrVers.Original);
                    reference.ChangeVersification(projVersification);
                    verse.TargetBBBCCCVV = reference.BBBCCCVVV.ToString().PadLeft(9, '0');
                }
                catch (Exception)
                {
                    verse.TargetBBBCCCVV = verse.SourceBBBCCCVV;
                }
            }

            //string path = "";
            //using (TextReader reader = File.OpenText(path))
            //{
            //    SIL.Scripture.Versification.Table.Implementation.Load(reader, path);

            //}

            return list;
        }

    }
}
