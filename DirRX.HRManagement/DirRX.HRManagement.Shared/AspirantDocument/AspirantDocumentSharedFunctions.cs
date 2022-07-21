using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.AspirantDocument;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Shared
{
  partial class AspirantDocumentFunctions
  {
    /// <summary>
    /// Заполнить имя.
    /// </summary>
    public override void FillName()
    {
      if (_obj.DocumentKind != null && !_obj.DocumentKind.GenerateDocumentName.Value && _obj.Name == AspirantDocuments.Resources.DocumentNameAutotext)
        _obj.Name = string.Empty;
      
      if (_obj.DocumentKind == null || !_obj.DocumentKind.GenerateDocumentName.Value)
        return;
      
      var name = string.Empty;
      
      // Имя в формате: <Вид документа> <Кандидат>.
      using (TenantInfo.Culture.SwitchTo())
      {
        name += _obj.DocumentKind.ShortName + " ";
        if (_obj.Aspirant != null)
        {
          var person = _obj.Aspirant.Person;
          name += Sungero.Parties.PublicFunctions.Person.GetSurnameAndInitialsInTenantCulture(person.FirstName, person.MiddleName, person.LastName); 
        }
        else
          name += AspirantDocuments.Resources.AspirantTemplate;
      }

      _obj.Name = Sungero.Docflow.PublicFunctions.Module.TrimSpecialSymbols(name);
    }
    
    /// <summary>
    /// Установить обязательность свойств.
    /// </summary>
    public override void SetRequiredProperties()
    {
      base.SetRequiredProperties();
      _obj.State.Properties.Subject.IsRequired = false;
    }
    
    /// <summary>
    /// Признак необходимости отображения панели регистрации.
    /// </summary>
    /// <param name="additionalCondition">Дополнительное условие при наследовании.</param>
    /// <returns>True, если надо показать панель.</returns>
    public override bool NeedShowRegistrationPane(bool additionalCondition)
    {
      return false;
    }
    
  }
}