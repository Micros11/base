using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.Aspirant;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class AspirantFunctions
  {

    /// <summary>
    /// Получить список документов по кандидату.
    /// </summary>
    /// <returns>Документы по кандидату.</returns>
    [Remote(IsPure = true)]
    public IQueryable<IAspirantDocument> GetAspirantDocuments()
    {
      return AspirantDocuments.GetAll(d => Equals(d.Aspirant, _obj));
    }
    
    /// <summary>
    /// Создать отчет по кандидату.
    /// </summary>
    /// <returns>Отчет по кандидату.</returns>
    [Remote]
    public IAspirantDocument CreateReport()
    {
      var employee = Employees.Current;
      var businessUnit = employee != null ? Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee) : BusinessUnits.Null;
      var document = AspirantDocuments.Null;
      var docKind = Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(DocKind.AspirantReportKindName);
      var template = Functions.Module.GetTemplateByKind(docKind, businessUnit);
      if (template != null)
        document = AspirantDocuments.CreateFrom(template);
      else
        document = AspirantDocuments.Create();
      
      document.Aspirant = _obj;
      document.DocumentKind = docKind;
      document.Author = Users.Current;
      document.Save();
      return document;                        
    }
            
    /// <summary>
    /// Получить кандидата, в контексте которого создается документ.
    /// </summary>
    /// <returns>Кандидат.</returns>
    [Public]
    public static IAspirant GetContextAspirant()
    {
      if (CallContext.CalledFrom(Aspirants.Info))
      {
        var aspirantId = CallContext.GetCallerEntityId(Aspirants.Info);
        return Aspirants.GetAll(m => Equals(m.Id, aspirantId)).SingleOrDefault();
      }
      return null;
    }
  }
}