using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.BilaterialDocument;

namespace DirRX.HRLite.Shared
{
  partial class BilaterialDocumentFunctions
  {
    /// <summary>
    /// Сделать доступными рег. номер и рег. дату.
    /// </summary>
    public override void EnableRegistrationNumberAndDate()
    {
      base.EnableRegistrationNumberAndDate();
      var properties = _obj.State.Properties;
      properties.RegistrationNumber.IsEnabled = true;
      properties.RegistrationDate.IsEnabled = true;
    }
    
    /// <summary>
    /// Установить обязательность свойств.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      _obj.State.Properties.RegistrationDate.IsRequired = true;
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
      
      if (documentKind != null && !documentKind.GenerateDocumentName.Value && _obj.Name == Sungero.Docflow.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (documentKind == null || !documentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      
      /* Имя в формате:
        <Вид документа> №<номер> <Сотрудники> от <дата>.
       */
      using (TenantInfo.Culture.SwitchTo())
      {
        if (!string.IsNullOrWhiteSpace(_obj.RegistrationNumber))
          name += Sungero.Docflow.OfficialDocuments.Resources.Number + _obj.RegistrationNumber;
        
        var nameList = new List<string>();
        foreach (var row in _obj.Employees)
          nameList.Add(row.Employee.Person.ShortName);
        if (nameList.Any())
          name += " " + string.Join(", ", nameList);
        
        if (_obj.RegistrationDate != null)
          name += Sungero.Docflow.OfficialDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      }
      
      if (string.IsNullOrWhiteSpace(name))
      {
        if (_obj.VerificationState == null)
          name = Sungero.Docflow.Resources.DocumentNameAutotext;
        else
          name = _obj.DocumentKind.ShortName;
      }
      else if (documentKind != null)
      {
        name = documentKind.ShortName + name;
      }
      
      name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
      
      _obj.Name = Sungero.Docflow.PublicFunctions.OfficialDocument.AddClosingQuote(name, _obj);
    }
    
    /// <summary>
    /// Пполучить подписывающего по умолчанию.
    /// </summary>
    /// <param name="signatories">Список подписывающих с приоритетом.</param>
    /// <returns>Подписывающий по умолчанию.</returns>
    public override Sungero.Company.IEmployee GetDefaultSignatory(List<Sungero.Docflow.Structures.SignatureSetting.Signatory> signatories)
    {
      return base.GetDefaultSignatory(signatories);
    }
  }
}