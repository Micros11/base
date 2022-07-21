using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.LNALists;

namespace DirRX.HRManagement
{
  partial class LNAListsDepartmentsDepartmentPropertyFilteringServerHandler<T>
  {

    public virtual IQueryable<T> DepartmentsDepartmentFiltering(IQueryable<T> query, Sungero.Domain.PropertyFilteringEventArgs e)
    {
      if (_root.BusinessUnit != null)
        query = query.Where(l => l.BusinessUnit.Equals(_root.BusinessUnit));
      return query;
    }
  }

  partial class LNAListsServerHandlers
  {

    public override void Created(Sungero.Domain.CreatedEventArgs e)
    {
      _obj.Name = LNAListses.Resources.LNAListNameFormat(_obj.Id);
    }
    
    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      // Проверить наличие документов.
      if (!_obj.LNADocs.Any())
        e.AddError(LNAListses.Resources.NoDocumentsInRecord);

      var isDepatmentsAny = _obj.Departments.Any();
      var isJobTitlesAny = _obj.JobTitles.Any();
      
      // Обработать названия должностей для отображения в списке.
      if (isJobTitlesAny)
        _obj.JobNames = string.Join(";", _obj.JobTitles.OrderBy(l => l.JobTitle.Name).Select(l => l.JobTitle.Name));
      else
        _obj.JobNames = string.Empty;
      
      // Обработать названия подразделений для отображения в списке.
      if (isDepatmentsAny)
        _obj.DepatmentNames = string.Join(";", _obj.Departments.OrderBy(l => l.Department.Name).Select(l => l.Department.Name));
      else
        _obj.DepatmentNames = string.Empty;

      // Проверить уникальность записи.
      var anyRecord = false;
      switch (isDepatmentsAny.ToString() + "|" + isJobTitlesAny.ToString())
      {
        case "False|False":
          anyRecord = LNAListses.GetAll(l =>
                                        l.Id != _obj.Id &&
                                        l.BusinessUnit.Equals(_obj.BusinessUnit) &&
                                        !l.Departments.Any() &&
                                        !l.JobTitles.Any()).Any();
          if (anyRecord)
            e.AddError(LNAListses.Resources.ExistedEmptyRecordErrorText);
          break;
        case "True|False":
          foreach (var dep in _obj.Departments)
          {
            anyRecord = LNAListses.GetAll(l =>
                                          l.Id != _obj.Id &&
                                          l.BusinessUnit.Equals(_obj.BusinessUnit) &&
                                          l.Departments.Any(k => k.Department.Equals(dep.Department)) &&
                                          !l.JobTitles.Any()).Any();
            if (anyRecord)
              e.AddError(LNAListses.Resources.ExistedDepartmentRecordErrorTextFormat(dep.Department.Name));
          }
          break;
        case "False|True":
          e.AddError(LNAListses.Resources.OnlyJobRecordErrorText);
          break;
        case "True|True":
          foreach (var dep in _obj.Departments)
          {
            foreach (var job in _obj.JobTitles)
            {
              anyRecord = LNAListses.GetAll(l => l.Id != _obj.Id &&
                                            l.BusinessUnit.Equals(_obj.BusinessUnit) &&
                                            l.JobTitles.Any(k => k.JobTitle.Equals(job.JobTitle)) &&
                                            l.Departments.Any(k => k.Department.Equals(dep.Department))).Any();
              if (anyRecord)
                e.AddError(LNAListses.Resources.ExistedPairRecordErrorTextFormat(job.JobTitle.Name, dep.Department.Name));
            }
          }
          break;
      }
    }
  }
}