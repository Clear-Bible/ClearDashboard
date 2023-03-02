using System;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Model;

public class ModelGroup<G> where G: notnull
{
    public GeneralListModel<GeneralModel<G>> Items = new();
}