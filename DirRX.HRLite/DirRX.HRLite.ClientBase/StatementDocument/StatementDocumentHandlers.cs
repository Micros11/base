using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.StatementDocument;

namespace DirRX.HRLite
{
  partial class StatementDocumentClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      // HACK: Событие добавлено для того, чтобы отключить событие Обновление формы базового типа.
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      // Для Соглашения на КЭДО скрыть поля, относящиеся только к заявлениям.
      if (Equals(_obj.DocumentKind, Sungero.Docflow.PublicFunctions.DocumentKind.GetNativeDocumentKind(Constants.Module.DocumentKind.DocumentManagementAgreementKind)))
      {
        _obj.State.Properties.StatementDate.IsVisible = false;
        _obj.State.Properties.Department.IsVisible = false;
      }
      else
      {
        // Для остальных Заявлений сделать Дату заявления обязательной.
        _obj.State.Properties.StatementDate.IsRequired = true;
      }
    }
  }

}