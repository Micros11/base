using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite.Shared
{
  partial class StatementDocumentFunctions
  {

    /// <summary>
    /// Сделать доступной рег. дату.
    /// </summary>
    public override void EnableRegistrationNumberAndDate()
    {
      base.EnableRegistrationNumberAndDate();
      _obj.State.Properties.RegistrationDate.IsEnabled = true;
    }
    
    /// <summary>
    /// Поведение панели по умолчанию.
    /// </summary>
    /// <returns>True, если панель должна быть отображена при создании документа.</returns>
    public override bool DefaultRegistrationPaneVisibility()
    {
      return true;
    }
    
    /// <summary>
    /// Заполнить имя документа.
    /// </summary>
    [Public]
    public override void FillName()
    {
      var documentKind = _obj.DocumentKind;
      var name = string.Empty;
      if (documentKind != null)
      {
        var currentEmployee = HRSolution.Employees.As(_obj.Author);
        if (currentEmployee != null)
        {
          var employeeSex = CaseConverter.DefineGender(currentEmployee.Person.MiddleName);
          var employeeName = CaseConverter.ConvertPersonFullNameToTargetDeclension(currentEmployee.Person.LastName, currentEmployee.Person.FirstName,
                                                                                   currentEmployee.Person.MiddleName, DeclensionCase.Genitive, employeeSex);
          if (Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.DocumentManagementAgreementKind)))
            name = string.Format("{0} {1}", documentKind.Name, employeeName);
          else
            name = string.Format("{0} {1} от {2}", documentKind.Name, employeeName, Calendar.Today.ToShortDateString());
        }
      }
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = Sungero.Docflow.PublicFunctions.OfficialDocument.AddClosingQuote(name, _obj);
    }

  }
}