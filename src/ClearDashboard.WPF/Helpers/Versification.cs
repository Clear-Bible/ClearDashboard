using ClearDashboard.DataAccessLayer.Models;
using SIL.Scripture;
using System;
using System.Collections.Generic;

namespace ClearDashboard.Wpf.Helpers
{
    public static class Versification
    {
        public static List<VersificationList> GetVersificationFromOriginal(List<VersificationList> list,
            Project paratextProject)
        {

            ScrVers projVersification = new ScrVers(paratextProject.ScrVersType);

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
