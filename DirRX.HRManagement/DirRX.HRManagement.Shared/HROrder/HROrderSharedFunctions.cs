using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HROrder;

namespace DirRX.HRManagement.Shared
{
  partial class HROrderFunctions
  {
    /// <summary>
    /// Установить обязательность свойств.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      _obj.State.Properties.Subject.IsRequired = false;
    }

    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == HRDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      
      // Имя в формате: <Вид документа> <Сотрудник> от <Дата создания документа>.
      using (TenantInfo.Culture.SwitchTo())
      {
        name += _obj.DocumentKind.ShortName + " ";
        name += _obj.Employee != null ? Sungero.Company.PublicFunctions.Employee.GetShortName(_obj.Employee, DeclensionCase.Genitive, false) : HRDocuments.Resources.EmployeeTemplate;
        
        if (_obj.RegistrationDate != null)
          name += HRDocuments.Resources.DateFrom + _obj.RegistrationDate.Value.ToString("d");
      }
      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }
    
  }
}