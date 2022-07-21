using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.LocalRegulationDocument;

namespace DirRX.HRLite.Shared
{
  partial class LocalRegulationDocumentFunctions
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
    /// Получить подписывающего по умолчанию.
    /// </summary>
    /// <param name="signatories">Список подписывающих с приоритетом.</param>
    /// <returns>Подписывающий по умолчанию.</returns>
    public override Sungero.Company.IEmployee GetDefaultSignatory(List<Sungero.Docflow.Structures.SignatureSetting.Signatory> signatories)
    {
      return base.GetDefaultSignatory(signatories);
    }
  }
}