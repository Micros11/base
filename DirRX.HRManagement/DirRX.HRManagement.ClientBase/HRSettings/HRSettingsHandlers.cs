using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRSettings;
using ParamKeys = DirRX.HRManagement.Constants.HRSettings.ParamKey;

namespace DirRX.HRManagement
{
  partial class HRSettingsClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      var isVisibleOriginalDocSettings = true;
      // Если настройка общая для всех Наших организаций, видимость необходимости подписания в бумаге определяется подключением к ЛК в справочнике Настройки подключения к сервисам личного кабинета:
      //   - Если ЛК подключен, то считаем что работа идет в электронном виде 
      //   - Если ЛК не подключен, то настройка задается администратором
      if (_obj.BusinessUnit == null)
        isVisibleOriginalDocSettings = !EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected();
      // Если настройка для конкретной Нашей организации, видимость необходимости подписания в бумаге определяется подключением к ЛК в перекрыии Нашей организации:
      //   - Если ЛК подключен, то считаем что работа идет в электронном виде 
      //   - Если ЛК не подключен, то настройка задается администратором
      else
      {
        var businessUnitO = DirRX.EssPlatformSolution.BusinessUnits.As(_obj.BusinessUnit);
        isVisibleOriginalDocSettings = !(businessUnitO.UseESSDirRX.HasValue && businessUnitO.UseESSDirRX.Value);
      }
      _obj.State.Properties.OriginalDocInStaff.IsVisible = isVisibleOriginalDocSettings;
      _obj.State.Properties.OriginalDocInVacation.IsVisible = isVisibleOriginalDocSettings;
    }

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      if (DirRX.EssPlatform.PublicFunctions.EssSetting.Remote.SettingsConnected())
        _obj.State.Properties.NeedAdvancedSign.IsVisible = false;
    }

    public override void Closing(Sungero.Presentation.FormClosingEventArgs e)
    {
      e.Params.Remove(ParamKeys.OldBusinessUnit);
    }

    public virtual void PeoplePercentageValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && (e.NewValue.Value <= 0 || e.NewValue.Value > 100))
        e.AddError(HRSettingses.Resources.PercentageError);
    }

    public virtual void RequiredOnePartDurationValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && e.NewValue.Value <= 0)
        e.AddError(HRSettingses.Resources.NegativeValueError);
    }

    public virtual void DaysCountVacationPayValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && e.NewValue.Value <= 0)
        e.AddError(HRSettingses.Resources.NegativeValueError);
    }

    public virtual void DaysCountBeforeVacationAlertValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && e.NewValue.Value <= 0)
        e.AddError(HRSettingses.Resources.NegativeValueError);
    }

    public virtual void MinDaysForVacationApprovalValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue.HasValue && e.NewValue != e.OldValue && e.NewValue.Value <= 0)
        e.AddError(HRSettingses.Resources.NegativeValueError);
    }
  }

}