using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using CommonLibrary;
using DirRX.HRManagement.VacationShiftTask;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;
using DocKind = DirRX.HRManagement.Constants.Module.DocumentKind;

namespace DirRX.HRManagement.Server
{
  partial class VacationShiftTaskFunctions
  {
    /// <summary>
    /// Перераспределить значения, чтобы не было пустых строк.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.SortFields().
    /// </summary>
    [Remote]
    public void SortFields()
    {
      if (_obj.Vacation1 == null)
      {
        if (_obj.Vacation2 != null)
        {
          _obj.Vacation1 = _obj.Vacation2;
          _obj.Vacation2 = null;
        }
        else if (_obj.Vacation3 != null)
        {
          _obj.Vacation1 = _obj.Vacation3;
          _obj.Vacation3 = null;
        }
      }
      
      if (_obj.Vacation2 == null && _obj.Vacation3 != null)
      {
        _obj.Vacation2 = _obj.Vacation3;
        _obj.Vacation3 = null;
      }

      if (this.IsEmptyFirstNewVacationDate())
      {
        if (!this.IsEmptySecondNewVacationDate())
        {
          _obj.DataBegin1 = _obj.DataBegin2;
          _obj.Duration1 = _obj.Duration2;
          _obj.VacationKind1 = _obj.VacationKind2;
          _obj.DataBegin2 = null;
          _obj.Duration2 = null;
          _obj.VacationKind2 = null;
        }
        else if (!this.IsEmptyThirdNewVacationDate())
        {
          _obj.DataBegin1 = _obj.DataBegin3;
          _obj.Duration1 = _obj.Duration3;
          _obj.VacationKind1 = _obj.VacationKind3;
          _obj.Duration3 = null;
          _obj.DataBegin3 = null;
          _obj.VacationKind3 = null;
        }
      }
      
      if (this.IsEmptySecondNewVacationDate() && !this.IsEmptyThirdNewVacationDate())
      {
        _obj.DataBegin2 = _obj.DataBegin3;
        _obj.Duration2 = _obj.Duration3;
        _obj.VacationKind2 = _obj.VacationKind3;
        _obj.Duration3 = null;
        _obj.DataBegin3 = null;
        _obj.VacationKind3 = null;
      }
    }
    
    /// <summary>
    /// Привести в соответствие выбранные и новые даты отпуска по виду отпуска.
    /// Если при изменении переносимых отпусков определенный вид отпуска стал недоступен для указания в новых датах,
    ///  то очистить строки новых дат у которых указан этот вид.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.VacationKindsCheckUp().
    /// </summary>
    [Remote]
    public void VacationKindsCheckUp()
    {
      var selectedKindList = GetVacationKindList();
      if (_obj.VacationKind1 != null && !selectedKindList.Contains(_obj.VacationKind1))
        this.CleanFirstNewVacationDate();
      if (_obj.VacationKind2 != null && !selectedKindList.Contains(_obj.VacationKind2))
        this.CleanSecondNewVacationDate();
      if (_obj.VacationKind3 != null && !selectedKindList.Contains(_obj.VacationKind3))
        this.CleanThirdNewVacationDate();
    }
    
    /// <summary>
    /// Получить список видов отпусков, выбранных для переноса.
    /// </summary>
    /// <returns>Список видов отпусков.</returns>
    [Remote]
    public List<IVacationKind> GetVacationKindList()
    {
      return GetVacationKindList(_obj.Vacation1, _obj.Vacation2, _obj.Vacation3);
    }
    
    /// <summary>
    /// Получить список видов отпусков выбранных для переноса.
    /// </summary>
    /// <param name="vacation1">Первый вид отпуска.</param>
    /// <param name="vacation2">Воторой вид отпуска.</param>
    /// <param name="vacation3">Третий вид отпуска.</param>
    /// <returns>Список видов отпусков.</returns>
    [Remote]
    public static List<IVacationKind> GetVacationKindList(IVacation vacation1, IVacation vacation2, IVacation vacation3)
    {
      var kindList = new List<IVacationKind>();
      if (vacation1 != null)
        kindList.Add(vacation1.VacationKind);
      if (vacation2 != null)
        kindList.Add(vacation2.VacationKind);
      if (vacation3 != null)
        kindList.Add(vacation3.VacationKind);
      return kindList.Distinct().ToList();
    }
    
    #region Расчет доступных дней.
    /// <summary>
    /// Пересчитать сумму запланированных и исполненных на дату отзыва дней отпуска.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.FillVacationPlanDurations().
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    public void FillVacationPlanDurations(IVacation vacation)
    {
      var newDaysUsed = 0;
      if (_obj.RecallDate != null && _obj.RecallDate.Value > vacation.StartDate.Value && _obj.RecallDate.Value <= vacation.FinDate.Value)
        newDaysUsed = Functions.Module.DurationInCalendarDaysWihtOutHoliday(vacation.StartDate.Value, _obj.RecallDate.Value.AddDays(-1), vacation.Employee);

      var vacDurationKind = _obj.VacationDurations.Where(v => Equals(v.VacationKind, vacation.VacationKind)).FirstOrDefault();
      if (vacDurationKind != null)
      {
        vacDurationKind.DaysPlaned += vacation.VacationDuration;
        vacDurationKind.DaysUsed += newDaysUsed;
      }
      else
      {
        var kindDuration = _obj.VacationDurations.AddNew();
        kindDuration.VacationKind = vacation.VacationKind;
        kindDuration.DaysPlaned = vacation.VacationDuration - newDaysUsed;
        kindDuration.DaysUsed = newDaysUsed;
        kindDuration.DaysAvailComp = 0;
        kindDuration.DaysCanceled = 0;
        kindDuration.DaysShifted = 0;
        kindDuration.DaysComped = 0;
      }
      if (vacation != null)
        _obj.Year = vacation.Year;
    }
    
    /// <summary>
    /// Пересчитать сумму распределенных дней отпуска.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.FillVacationShiftDurations().
    /// </summary>
    /// <param name="vacKind">Вид отпуска.</param>
    /// <param name="duration">Продолжительность.</param>
    public void FillVacationShiftDurations(IVacationKind vacKind, int duration)
    {
      var vacDurationKind = _obj.VacationDurations.Where(v => Equals(v.VacationKind, vacKind)).FirstOrDefault();
      if (vacDurationKind != null)
        vacDurationKind.DaysShifted += duration;
    }
    
    /// <summary>
    /// Установить количество оставшихся дней отпуска.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.SetLeftDays().
    /// </summary>
    [Remote]
    public void SetLeftDays()
    {
      _obj.VacationDurations.Clear();
      
      if (_obj.Vacation1 != null)
        this.FillVacationPlanDurations(_obj.Vacation1);
      if (_obj.Vacation2 != null)
        this.FillVacationPlanDurations(_obj.Vacation2);
      if (_obj.Vacation3 != null)
        this.FillVacationPlanDurations(_obj.Vacation3);
      
      if (_obj.VacationKind1 != null && _obj.Duration1 != null)
        this.FillVacationShiftDurations(_obj.VacationKind1, _obj.Duration1.Value);
      if (_obj.VacationKind2 != null && _obj.Duration2 != null)
        this.FillVacationShiftDurations(_obj.VacationKind2, _obj.Duration2.Value);
      if (_obj.VacationKind3 != null && _obj.Duration3 != null)
        this.FillVacationShiftDurations(_obj.VacationKind3, _obj.Duration3.Value);
      
      foreach (var vacDurations in _obj.VacationDurations)
        if (vacDurations.VacationKind.Paid.Value && !vacDurations.VacationKind.Required.Value)
          vacDurations.DaysAvailComp += Functions.Module.GetAvailableDaysToCompensation(_obj.Employee, vacDurations.VacationKind, _obj.Year.Value.Year, vacDurations.DaysPlaned.Value);
      
      if (_obj.Compensation.Value)
        this.SetLeftDays(_obj.Compensation.Value);
      
      if (!_obj.VacationDurations.Any() || _obj.VacationDurations.All(v => v.DaysPlaned == 0))
        _obj.Year = Calendar.SqlMinValue;
      
      this.ChangeInfoLabel();
    }
    
    /// <summary>
    /// Установить количество отмененных дней отпуска при простановке признака Компенсировать/Отменить.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.SetLeftDays().
    /// </summary>
    /// <param name="isCompensation">True, если отменить/компенсировать не распределенные дни отпуска.</param>
    [Remote]
    public void SetLeftDays(bool isCompensation)
    {
      foreach (var vacDuration in _obj.VacationDurations)
      {
        if (!vacDuration.VacationKind.Paid.Value)
        {
          if (isCompensation)
          {
            vacDuration.DaysCanceled = vacDuration.DaysPlaned - vacDuration.DaysShifted;
            vacDuration.DaysShifted += vacDuration.DaysCanceled;
          }
          else
          {
            vacDuration.DaysShifted -= vacDuration.DaysCanceled;
            vacDuration.DaysCanceled = 0;
          }
        }
        
        if (vacDuration.VacationKind.Paid.Value && !vacDuration.VacationKind.Required.Value)
        {
          if (isCompensation)
          {
            vacDuration.DaysComped = vacDuration.DaysAvailComp > vacDuration.DaysPlaned - vacDuration.DaysShifted ? vacDuration.DaysPlaned - vacDuration.DaysShifted : vacDuration.DaysAvailComp;
            vacDuration.DaysShifted += vacDuration.DaysComped;
            vacDuration.DaysAvailComp = 0;
          }
          else
          {
            vacDuration.DaysShifted -= vacDuration.DaysComped;
            vacDuration.DaysAvailComp += vacDuration.DaysComped;
            vacDuration.DaysComped = 0;
          }
        }
      }
      
      this.ChangeInfoLabel();
    }
    
    /// <summary>
    /// Изменить сообщение на форме о нераспределенных днях отпуска.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.ChangeInfoLabel().
    /// </summary>
    public void ChangeInfoLabel()
    {
      var notDistributedVacationDays = new List<string>();
      var cancelInfo = new List<string>();
      var compensationInfo = new List<string>();
      
      _obj.VacationInfo = string.Empty;
      _obj.LiquidationInfo = string.Empty;
      foreach (var vacDuration in _obj.VacationDurations)
      {
        var vacKind = vacDuration.VacationKind;
        
        if (vacDuration.DaysPlaned > vacDuration.DaysShifted)
        {
          var notDistributedVacationDay = string.Format("{0}: {1}.", vacKind.Name, vacDuration.DaysPlaned.Value - vacDuration.DaysShifted.Value);
          if (vacDuration.DaysPlaned.Value - vacDuration.DaysShifted.Value - vacDuration.DaysAvailComp.Value > 0 && vacDuration.VacationKind.Paid.Value)
            notDistributedVacationDay += string.Format(" ({0}: {1})", VacationShiftTasks.Resources.VacationDaysRequiredToDistribute,
                                                       vacDuration.DaysPlaned.Value - vacDuration.DaysShifted.Value - vacDuration.DaysAvailComp.Value);
          notDistributedVacationDays.Add(notDistributedVacationDay);
        }
        
        if (vacDuration.DaysCanceled != 0)
          cancelInfo.Add(vacKind.Name + ": " + vacDuration.DaysCanceled);
        if (vacDuration.DaysComped != 0)
          compensationInfo.Add(vacKind.Name + ": " + vacDuration.DaysComped);
      }
      
      if (_obj.Vacation1 != null)
        if (!notDistributedVacationDays.Any())
          _obj.VacationInfo = VacationShiftTasks.Resources.AllDistributedVacationDays;
        else
          _obj.VacationInfo += VacationShiftTasks.Resources.NotDistributedVacationDaysFormat(string.Join(Environment.NewLine, notDistributedVacationDays) + Environment.NewLine + Environment.NewLine);
      
      if (cancelInfo.Any())
        _obj.LiquidationInfo += VacationShiftTasks.Resources.VacationDaysCanceledFormat(string.Join(Environment.NewLine, cancelInfo)) + Environment.NewLine;
      if (compensationInfo.Any())
        _obj.LiquidationInfo += VacationShiftTasks.Resources.VacationDaysCompensationedFormat(string.Join(Environment.NewLine, compensationInfo)) + Environment.NewLine;
      
      Functions.VacationShiftTask.SetVisibleProperties(_obj);
    }
    #endregion
    
    #region Проверки по отпускам.

    /// <summary>
    /// Проверить правильность всех указанных дат отпуска.
    /// </summary>
    /// <returns>Ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckAllVacationDates()
    {
      var year = _obj.Year.Value;
      var employee = _obj.Employee;
      List<string> errorList = new List<string>();

      // перевести в структуру новые планируемые части отпуска
      var newVacationsParties = this.CreateVacationDateStructure();
      
      // проверить, что распределено дней не больше и не меньше, чем в выбранных для изменения отпусках
      foreach (var vd in _obj.VacationDurations) {
        // разница между тем, что выбрали и тем, что распределили
        var delta = vd.DaysPlaned - vd.DaysShifted;
        if (delta > 0)
          errorList.Add(Resources.VacationDurationUnderLimitFormat(vd.VacationKind.Name, delta));
        if (delta < 0)
          errorList.Add(Resources.VacationDurationOverLimitFormat(vd.VacationKind.Name, -1 * delta));
        if (vd.DaysCanceled < 0)
          errorList.Add(Resources.VacationDurationOverLimitFormat(vd.VacationKind.Name, -1 * vd.DaysCanceled));
        if (vd.DaysComped < 0)
          errorList.Add(Resources.VacationDurationOverLimitFormat(vd.VacationKind.Name, -1 * vd.DaysComped));
      }
      
      // Проверить, что даты новых отпусков не выйдут за границы года отпуска.
      var vacationInYear = Functions.Module.CheckVacationDateInCalendarYear(newVacationsParties, employee, year, Resources.VacationEndOverBorderCalendarYearForShift);
      if (!string.IsNullOrWhiteSpace(vacationInYear))
        errorList.Add(vacationInYear);
      
      // Проверить отпуска на пересечения
      // Достать отпуска по сотруднику за исключением выбранных в задаче
      var previousYear = year.Year - 1;
      var nextYear = year.Year + 1;
      var vacationList = Vacations.GetAll(v => Equals(v.Employee, employee) &&
                                          (v.Year.Value.Year == previousYear || v.Year.Value.Year == nextYear || v.Year.Value.Year == year.Year) &&
                                          !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation2) && !Equals(v, _obj.Vacation3) &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Closed &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Canceled &&
                                          v.Status != DirRX.HRManagement.Vacation.Status.Shifted);
      var vacationDatesForIntersections = Functions.Module.FormVacationDateStructure(vacationList);
      // добавить новые части отпуска
      vacationDatesForIntersections.AddRange(newVacationsParties);
      var vacationIntersections = Functions.Module.CheckVacationIntersections(vacationDatesForIntersections);
      if (vacationIntersections.Any())
        errorList.AddRange(vacationIntersections);

      // Проверить интервал от текущей даты до следующего отпуска.
      var vacationsInterval = Functions.Module.CheckVacationsInterval(newVacationsParties, employee);
      if (vacationsInterval.Any())
        errorList.AddRange(vacationsInterval);
      
      // Проверить, что Дата начала отпуска не больше Даты окончания.
      var vacationDatesRangeError = Functions.Module.CheckVacationDatesRange(newVacationsParties);
      if (vacationDatesRangeError != null)
        errorList.AddRange(vacationDatesRangeError);
      
      // Проверить, что Дата начала + Длительность = Дата окончания с учетом календаря рабочего времени.
      var vacationDatesNotMatchDuration = Functions.Module.CheckVacatinDatesMatchDuration(newVacationsParties, _obj.Employee);
      if (vacationDatesNotMatchDuration.Any())
        errorList.AddRange(vacationDatesNotMatchDuration);
      
      return errorList;
    }

    /// <summary>
    /// Создать список отпусков, которые будут созданы в результате задачи.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.CreateVacationDateStructure().
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      // три новых части отпуска (если они заполнены)
      if (_obj.DataBegin1.HasValue && _obj.DataEnd1.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin1.Value, _obj.DataEnd1.Value, _obj.Duration1.Value, DateStatuses.New, _obj.VacationKind1));
      if (_obj.DataBegin2.HasValue && _obj.DataEnd2.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin2.Value, _obj.DataEnd2.Value, _obj.Duration2.Value, DateStatuses.New, _obj.VacationKind2));
      if (_obj.DataBegin3.HasValue && _obj.DataEnd3.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin3.Value, _obj.DataEnd3.Value, _obj.Duration3.Value, DateStatuses.New, _obj.VacationKind3));
      // компенсированные отпуска
      //      if (_obj.Compensation.Value)
      //        foreach (var vacDuration in _obj.VacationDurations.Where(v => v.DaysComped != 0))
      //          vacationDateList.Add(Structures.Vacation.VacationDates.Create(DateTime.MinValue, DateTime.MinValue, vacDuration.DaysComped.Value, DateStatuses.New, vacDuration.VacationKind));
      // отмененные отпуска
      //      if (_obj.RecallDate != null)
      //        foreach (var vacDuration in _obj.VacationDurations.Where(v => v.DaysUsed != 0))
      //          vacationDateList.Add(Structures.Vacation.VacationDates.Create(DateTime.MinValue, DateTime.MinValue, vacDuration.DaysUsed.Value, DateStatuses.New, vacDuration.VacationKind));
      return vacationDateList;
    }

    /// <summary>
    /// Проверить правильность указанных дат отпуска на не критичные ошибки.
    /// </summary>
    /// <param name="vacationDate">Дата отпуска.</param>
    /// <returns>Не критичные ошибки в датах отпуска.</returns>
    [Public, Remote]
    public virtual List<string> CheckNotCriticalVacationsDates(Structures.Vacation.IVacationDates vacationDate)
    {
      List<string> errorList = new List<string>();
      
      // Проверить, что отпуск не заканчивается перед выходным или праздничным днем.
      var vacationEndBeforeWeekend = Functions.Module.CheckVacationEndBeforeWeekendOrHoliday(vacationDate, _obj.Employee);
      if (!string.IsNullOrWhiteSpace(vacationEndBeforeWeekend))
        errorList.Add(vacationEndBeforeWeekend);
      
      return errorList;
    }

    /// <summary>
    /// Проверить, что есть часть отпуска с необходимой продолжительностью.
    /// </summary>
    /// <returns>Текст сообщения об отсутствии части основного отпуска с необходимой продолжительностью.</returns>
    [Remote]
    public virtual string CheckRequiredDuration()
    {
      var requiredDurationError = string.Empty;

      var vacationDates = this.CreateVacationDateStructure();

      // Добавить к новым датам те, что есть в справочнике отпусков, за исключением тех, что уже выбраны в карточке
      // Причем с точки проверки минимальной длительности порции оснонового вида отпуска нас интересуют отпуска только определенного вида
      var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
      var paidVacationList = Functions.Vacation.GetPaidVacations(_obj.Employee, _obj.Year.Value)
        .Where(v => !v.Equals(_obj.Vacation1) && !v.Equals(_obj.Vacation2) && !v.Equals(_obj.Vacation3) && Equals(v.VacationKind, mainPaidVacationKind));
      vacationDates.AddRange(Functions.Module.FormVacationDateStructure(paidVacationList));

      // для отзыва добавим в проверки уже использованную часть отпуска Vacation1 -а то она не попадет в анализ
      if (_obj.IsVacationRecall.Value && _obj.Vacation1 != null && _obj.Vacation1.VacationKind == mainPaidVacationKind) {
        var vd = _obj.VacationDurations.Where(v => v.VacationKind == mainPaidVacationKind).FirstOrDefault();
        var vacationPart = Structures.Vacation.VacationDates.Create();
        vacationPart.VacationKind = mainPaidVacationKind;
        if (vd.DaysUsed > 0) {
          vacationPart.Duration = vd.DaysUsed.Value;
          vacationDates.Add(vacationPart);
        }
      }
      
      // Проверить, что есть часть отпуска с необходимой продолжительностью.
      if (vacationDates.Any())
        requiredDurationError = Functions.Module.CheckVacationRequiredDuration(vacationDates, _obj.Employee, _obj.Year.Value);
      
      // Для отзыва, если нарушение правила 14-ти дней произошло из-за выбранной даты отзыва, а не из-за неправильного указания новых дат, то
      //     - если стартует сотрудник сам по себе, то сделать вид, что ошибки не было
      //     - если по другому сотруднику, то подменить сообщение
      if (_obj.IsVacationRecall.Value && !string.IsNullOrEmpty(requiredDurationError) && _obj.IsRequiredDurationBroken.Value)
      {
        var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
        if (isFromEssAccount || (!isFromEssAccount && Equals(Employees.Current, _obj.Employee)))
          requiredDurationError = string.Empty;
        else
          requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationError;
      }
      return requiredDurationError;
    }

    /// <summary>
    /// Проверить, что дата отзыва не нарушит правило 14 дней
    /// Правило может быть нарушено только если в Vacation1 будет часть отпуска с основным видом
    /// Если обнаружено нарушение правила, то галочка "Игнорировать..." становится доступной, если это руководитель.
    /// </summary>
    /// <returns> True - если найдено нарушение правила 14 дней, false - если нет.</returns>
    [Remote]
    public virtual bool CheckRequiredDurationonVacation1()
    {
      var requiredDurationError = string.Empty;
      var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
      
      // проверку выполнять только если отзыв и Vacation1 заполнено основным видом отпуска
      if (_obj.IsVacationRecall.Value && _obj.Vacation1 != null && _obj.Vacation1.VacationKind == mainPaidVacationKind) {
        // Достанем все части по основному виду отпуска, кроме того, что упоминается в Vacation1
        var mainPaidVacationList = Functions.Vacation.GetPaidVacations(_obj.Employee, _obj.Year.Value)
          .Where(v => !v.Equals(_obj.Vacation1) && Equals(v.VacationKind, mainPaidVacationKind));
        var vacationDates = Functions.Module.FormVacationDateStructure(mainPaidVacationList);
        // добавим часть отпуска из Vacation1 с учетом того, что часть возможно была использовано - т.е. разделим на две части
        var vd = _obj.VacationDurations.Where(v => v.VacationKind == mainPaidVacationKind).FirstOrDefault();
        if (vd != null && vd.DaysPlaned > 0) {
          var vacationPlaned  = Structures.Vacation.VacationDates.Create();
          vacationPlaned.VacationKind = mainPaidVacationKind;
          vacationPlaned.Duration = vd.DaysPlaned.Value;
          vacationDates.Add(vacationPlaned);
        }
        if (vd != null && vd.DaysUsed > 0) {
          var vacationUsed  = Structures.Vacation.VacationDates.Create();
          vacationUsed.VacationKind = mainPaidVacationKind;
          vacationUsed.Duration = vd.DaysUsed.Value;
          vacationDates.Add(vacationUsed);
        }
        // Проверить, что есть часть отпуска с необходимой продолжительностью.
        if (vacationDates.Any())
          requiredDurationError = Functions.Module.CheckVacationRequiredDuration(vacationDates, _obj.Employee, _obj.Year.Value);
      }
      return !string.IsNullOrEmpty(requiredDurationError);
    }
    
    #endregion
    
    #region Синхронизация данных Задача-Задание
    /// <summary>
    /// Перенести данные о датах отпуска из задачи в задание.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public virtual void FillVacationDataFromTaskToAssignment(IShiftStatementApprovalAssignment assignment)
    {
      assignment.Employee = _obj.Employee;
      assignment.RecallDate = _obj.RecallDate;
      assignment.Year = _obj.Year;

      assignment.Vacation1 = _obj.Vacation1;
      assignment.Vacation2 = _obj.Vacation2;
      assignment.Vacation3 = _obj.Vacation3;

      assignment.VacationKind1 = _obj.VacationKind1;
      assignment.DataBegin1 = _obj.DataBegin1;
      assignment.Duration1 = _obj.Duration1;
      assignment.DataEnd1 = _obj.DataEnd1;

      assignment.VacationKind2 = _obj.VacationKind2;
      assignment.DataBegin2 = _obj.DataBegin2;
      assignment.Duration2 = _obj.Duration2;
      assignment.DataEnd2 = _obj.DataEnd2;

      assignment.VacationKind3 = _obj.VacationKind3;
      assignment.DataBegin3 = _obj.DataBegin3;
      assignment.Duration3 = _obj.Duration3;
      assignment.DataEnd3 = _obj.DataEnd3;

      assignment.Compensation = _obj.Compensation;
      assignment.IgnoreRequiredDuration = _obj.IgnoreRequiredDuration;
      assignment.IsRequiredDurationBroken = _obj.IsRequiredDurationBroken;

      assignment.ShiftReason = _obj.ShiftReason;
    }
    
    /// <summary>
    /// Перенести данные о датах отпуска из задания в задачу.
    /// </summary>
    /// <param name="assignment">Задание.</param>
    public virtual void FillVacationDataFromAssignmentToTask(IShiftStatementApprovalAssignment assignment)
    {
      _obj.Employee = assignment.Employee;
      _obj.RecallDate = assignment.RecallDate;
      _obj.Year = assignment.Year;

      _obj.Vacation1 = assignment.Vacation1;
      _obj.Vacation2 = assignment.Vacation2;
      _obj.Vacation3 = assignment.Vacation3;

      _obj.VacationKind1 = assignment.VacationKind1;
      _obj.DataBegin1 = assignment.DataBegin1;
      _obj.Duration1 = assignment.Duration1;
      _obj.DataEnd1 = assignment.DataEnd1;

      _obj.VacationKind2 = assignment.VacationKind2;
      _obj.DataBegin2 = assignment.DataBegin2;
      _obj.Duration2 = assignment.Duration2;
      _obj.DataEnd2 = assignment.DataEnd2;

      _obj.VacationKind3 = assignment.VacationKind3;
      _obj.DataBegin3 = assignment.DataBegin3;
      _obj.Duration3 = assignment.Duration3;
      _obj.DataEnd3 = assignment.DataEnd3;

      _obj.ShiftReason = assignment.ShiftReason;

      _obj.IgnoreRequiredDuration = assignment.IgnoreRequiredDuration;
      _obj.IsRequiredDurationBroken = assignment.IsRequiredDurationBroken;
      _obj.Compensation = assignment.Compensation;
    }
    #endregion
    
    #region Работа с документами
    
    /// <summary>
    /// Получить заявление о переносе отпуска.
    /// </summary>
    /// <returns>Заявление.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHRDocument GetEmployeeStatement()
    {
      return _obj.Statement;
    }
    
    /// <summary>
    /// Получить приказ о переносе отпуска.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Public, Remote(IsPure = true)]
    public virtual IHROrder GetEmployeeOrder()
    {
      return _obj.Order;
    }
    
    /// <summary>
    /// Сформировать текст с датами отпусков для переноса.
    /// </summary>
    /// <returns>Периоды отпуска.</returns>
    public List<string> GetCurrentVacationDates()
    {
      var currentVacation = new List<IVacation>();
      if (_obj.Vacation1 != null)
        currentVacation.Add(_obj.Vacation1);
      if (_obj.Vacation2 != null)
        currentVacation.Add(_obj.Vacation2);
      if (_obj.Vacation3 != null)
        currentVacation.Add(_obj.Vacation3);
      
      var currentDatesText = new List<string>();
      foreach (var curVacation in currentVacation)
      {
        if (_obj.VacationDurations.Any(n => Equals(curVacation.VacationKind, n.VacationKind) && (n.DaysComped != n.DaysPlaned || n.DaysCanceled != n.DaysPlaned)))
          currentDatesText.Add(Resources.VacationStatementBodyPartFormat(curVacation.VacationKind.Name.ToLower(),
                                                                         curVacation.VacationDuration.Value,
                                                                         curVacation.StartDate.Value.ToLongDateString()));
      }
      return currentDatesText;
      
    }
    
    /// <summary>
    /// Сформировать текст с новыми датами отпуска.
    /// </summary>
    /// <returns>Новые даты отпуска.</returns>
    public List<string> GetNewVacationDates()
    {
      var newDatesText = new List<string>();
      var newDates = Functions.VacationShiftTask.CreateVacationDateStructure(_obj).Where(l => !Equals(l.BeginDate, DateTime.MinValue));
      foreach (var newDate in newDates)
        newDatesText.Add(VacationShiftTasks.Resources.ShiftTemplateNewDatesFormat(newDate.BeginDate.ToLongDateString(),
                                                                                  newDate.EndDate.ToLongDateString(),
                                                                                  newDate.VacationKind.Name.ToLower()));
      return newDatesText;
    }
    
    /// <summary>
    /// Сформировать текст с информацией о компенсации отпуска.
    /// </summary>
    /// <returns>Информация о компенсации отпуска.</returns>
    public List<string> GetCompensationInfo()
    {
      var compensatedPart = new List<string>();
      var compensatedVacations = _obj.VacationDurations.Where(l => l.DaysComped.HasValue && l.DaysComped.Value > 0);
      if (compensatedVacations.Any())
      {
        foreach (var vacationDuration in compensatedVacations)
          compensatedPart.Add(VacationShiftTasks.Resources.StatementCompensationPartFormat(vacationDuration.VacationKind.Name.ToLower(),
                                                                                           vacationDuration.DaysComped.Value));
      }
      return compensatedPart;
    }
    
    /// <summary>
    /// Сформировать текст с информацией об отмене отпуска.
    /// </summary>
    /// <returns>Информация об отмене отпуска.</returns>
    public List<string> GetCanceledInfo()
    {
      var currentVacation = new List<IVacation>();
      if (_obj.Vacation1 != null)
        currentVacation.Add(_obj.Vacation1);
      if (_obj.Vacation2 != null)
        currentVacation.Add(_obj.Vacation2);
      if (_obj.Vacation3 != null)
        currentVacation.Add(_obj.Vacation3);
      
      var canceledPart = new List<string>();
      var canceledVacations = _obj.VacationDurations.Where(l => l.DaysCanceled.HasValue && l.DaysCanceled.Value > 0);
      if (canceledVacations.Any())
      {
        foreach (var vacationDuration in canceledVacations)
        {
          var cancelVac = currentVacation.Where(n => Equals(n.VacationKind, vacationDuration.VacationKind)).FirstOrDefault();
          canceledPart.Add(VacationShiftTasks.Resources.ShiftTemplateNewDatesFormat(vacationDuration.VacationKind.Name.ToLower(),
                                                                                    cancelVac.StartDate.Value.ToLongDateString(),
                                                                                    Functions.Module.GetVacationEndDate(cancelVac.StartDate.Value,
                                                                                                                        vacationDuration.DaysCanceled.Value,
                                                                                                                        _obj.Employee).ToLongDateString()));
        }
      }
      return canceledPart;
    }
    
    /// <summary>
    /// Создать заявление на перенос или отзыв отпуска.
    /// Аналогичный методу создания приказов на перенос и отзыв из отпуска.
    /// </summary>
    /// <returns>Заявление.</returns>
    public IHRDocument CreateShiftVacationStatement()
    {
      var document = HRDocuments.Null;
      
      if (_obj.IsVacationRecall.Value)
      {
        // Отзыв.
        document = Functions.Module.CreateStatement(DocKind.VacationRecallStatementKind, _obj.Employee, _obj.Started.Value, _obj.Year.Value);
        document.WorkDate = _obj.RecallDate;
      }
      else
      {
        // Перенос.
        document = Functions.Module.CreateStatement(DocKind.VacationShiftStatementKind, _obj.Employee, _obj.Started.Value, _obj.Year.Value);
      }
      
      document.Employee = _obj.Employee;
      
      // Информация о переносимых датах отпусках, если их не отменили или компенсировали полностью.
      if (_obj.VacationDurations.All(n => n.DaysPlaned == n.DaysComped || n.DaysPlaned == n.DaysCanceled))
      {
        document.VacationShift = string.Empty;
      }
      else
      {
        // Текущие даты отпуска.
        if (!_obj.IsVacationRecall.Value)
          document.BodyText = string.Join(", ", this.GetCurrentVacationDates());
        
        // Новые даты отпуска.
        document.NewVacationDatesText = string.Join(", ", this.GetNewVacationDates());
      }
      
      if (_obj.Compensation.Value)
      {
        // Компенсация.
        if (this.GetCompensationInfo().Any())
        {
          document.VacationCompensationText = string.Join(", ", this.GetCompensationInfo());
          document.VacationYear = _obj.Year.Value.Year.ToString();
        }
        else
          document.VacationCompensation = string.Empty;
        
        // Отмена.
        if (this.GetCanceledInfo().Any())
          document.VacationCancelText = string.Join(", ", this.GetCanceledInfo());
        else
          document.VacationCancel = string.Empty;
        
      }
      else
      {
        document.VacationCompensation = string.Empty;
        document.VacationCancel = string.Empty;
      }
      document.Save();
      return document;
    }
    
    /// <summary>
    /// Создать приказ о переносе или отзыве из отпуска.
    /// Аналогичный методу создания заявления на перенос и отзыв из отпуска.
    /// </summary>
    /// <returns>Приказ.</returns>
    [Remote]
    public IHROrder CreateShiftVacationOrder()
    {
      var document = HROrders.Null;
      if (_obj.IsVacationRecall.Value)
      {
        // Отзыв
        document = Functions.Module.CreateOrder(DocKind.VacationRecallOrderKind, _obj.Employee, _obj.Statement, _obj.Year.Value);
        document.EventDate = _obj.RecallDate;
      }
      else
        document = Functions.Module.CreateOrder(DocKind.VacationShiftOrderKind, _obj.Employee, _obj.Statement, _obj.Year.Value);

      document.Employee = _obj.Employee;
      
      // Текущие даты отпуска.
      document.BodyText = string.Join(";" + Environment.NewLine, this.GetCurrentVacationDates());
      
      // Информация о переносимых датах отпусках, если их не отменили или компенсировали полностью.
      if (_obj.VacationDurations.All(n => n.DaysPlaned == n.DaysComped || n.DaysPlaned == n.DaysCanceled))
      {
        document.VacationShift = string.Empty;
      }
      else
      {
        // Новые даты отпуска.
        document.NewVacationDatesText = string.Join(";" + Environment.NewLine, this.GetNewVacationDates());
      }
      
      if (_obj.Compensation.Value)
      {
        // Компенсация.
        if (this.GetCompensationInfo().Any())
        {
          document.VacationCompensationText = string.Join(";" + Environment.NewLine, this.GetCompensationInfo());
          document.VacationYear = _obj.Year.Value.Year.ToString();
        }
        else
          document.VacationCompensation = string.Empty;
        
        // Отмена.
        if (this.GetCanceledInfo().Any())
          document.VacationCancelText = string.Join(";" + Environment.NewLine, this.GetCanceledInfo());
        else
          document.VacationCancel = string.Empty;
        
      }
      else
      {
        document.VacationCompensation = string.Empty;
        document.VacationCancel = string.Empty;
      }
      document.Save();
      return document;
    }

    /// <summary>
    /// Переформировать приказ о переносе отпуска.
    /// </summary>
    [Remote]
    public void ReCreateShiftVacationOrder()
    {
      Functions.Module.RegenerateOrder(_obj.Order);
    }
    #endregion

    /// <summary>
    /// Отфильтровать доступные для выбора отпуска на перенос или отзыв.
    /// Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.VacationShiftFiltering().
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    /// <returns>Список доступных отпусков.</returns>
    [Remote]
    public IQueryable<IVacation> VacationShiftFiltering(IVacation vacation)
    {
      var query = this.VacationShiftFilter(vacation);
      return query.Where(v => !Equals(v, _obj.Vacation1) && !Equals(v, _obj.Vacation2) && !Equals(v, _obj.Vacation3));
    }
    
    /// <summary>
    /// Отфильтровать доступные для выбора отпуска на перенос или отзыв.
    ///   Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.VacationShiftFilter().
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    /// <returns>Список доступных отпусков.</returns>
    [Remote]
    public IQueryable<IVacation> VacationShiftFilter(IVacation vacation)
    {
      var year = vacation != null ? vacation.Year.Value : _obj.Year.Value;
      if (!_obj.IsVacationRecall.Value)
        return Functions.Vacation.GetPlanVacations(_obj.Employee, year);
      else
        return Functions.Vacation.GetRecallVacations(_obj.Employee, _obj.RecallDate);
    }
    
    /// <summary>
    /// Отфильтровать доступные для переноса виды отпуска.
    /// Возвращает виды отпуска в выбранных для изменения частях отпуска и еще не полностю распределены,
    /// а также вид отпуска из currentVacationKind.
    /// </summary>
    /// <param name="currentVacationKind">Текущий вид отпуска.</param>
    /// <returns>Список доступных видов отпусков.</returns>
    // TODO Описать.
    [Remote]
    public IQueryable<IVacationKind> VacationKindFilter(IVacationKind currentVacationKind)
    {
      var kindList = new List<IVacationKind>();
      if (currentVacationKind != null)
        kindList.Add(currentVacationKind);
      foreach (var vd in _obj.VacationDurations) {
        if (vd.DaysPlaned > vd.DaysShifted)
          kindList.Add(vd.VacationKind);
      }
      return VacationKinds.GetAll().Where(l => kindList.Contains(l));
    }
    
    /// <summary>
    /// Получить дату окончания отпуска и пересчитать количество доступных дней.
    ///   Аналогичный метод в задании Functions.ShiftStatementApprovalAssignment.FillEndDateAndSetDays().
    /// </summary>
    /// <param name="date">Дата.</param>
    /// <param name="duration">Продолжительность.</param>
    /// <returns>Дата окончания отпуска.</returns>
    [Remote]
    public DateTime? FillEndDateAndSetDays(DateTime? date, int? duration)
    {
      var dateEnd = new DateTime?();
      if (date.HasValue && duration.HasValue)
        dateEnd = Functions.Module.GetVacationEndDate(date.Value, duration.Value, _obj.Employee);
      else
        dateEnd = null;
      Functions.VacationShiftTask.SetLeftDays(_obj);
      return dateEnd;
    }
    
    /// <summary>
    /// Проверить, заполнена ли строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <param name="dateBegin">Дата начала.</param>
    /// <param name="duration">Продолжительность.</param>
    /// <param name="dateEnd">Дата окончания.</param>
    /// <param name="vacationKind">Вид отпуска.</param>
    /// <returns>True, если строка пустая.</returns>
    public static bool IsEmptyNewVacationDates(DateTime? dateBegin, int? duration, DateTime? dateEnd, IVacationKind vacationKind)
    {
      return dateBegin == null && duration == null && dateEnd == null && vacationKind == null;
    }
    
    /// <summary>
    /// Проверить, заполнена ли первая строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptyFirstNewVacationDate()
    {
      return IsEmptyNewVacationDates(_obj.DataBegin1, _obj.Duration1, _obj.DataEnd1, _obj.VacationKind1);
    }
    
    /// <summary>
    /// Проверить, заполнена ли вторая строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptySecondNewVacationDate()
    {
      return IsEmptyNewVacationDates(_obj.DataBegin2, _obj.Duration2, _obj.DataEnd2, _obj.VacationKind2);
    }
    
    /// <summary>
    /// Проверить, заполнена ли третяя строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptyThirdNewVacationDate()
    {
      return IsEmptyNewVacationDates(_obj.DataBegin3, _obj.Duration3, _obj.DataEnd3, _obj.VacationKind3);
    }
    
    /// <summary>
    /// Очистить первую строку в разделе "Новые даты отпуска".
    /// </summary>
    [Remote]
    public void CleanFirstNewVacationDate()
    {
      _obj.VacationKind1 = null;
      _obj.DataBegin1 = null;
      _obj.Duration1 = null;
    }
    
    /// <summary>
    /// Очистить вторую строку в разделе "Новые даты отпуска".
    /// </summary>
    [Remote]
    public void CleanSecondNewVacationDate()
    {
      _obj.VacationKind2 = null;
      _obj.DataBegin2 = null;
      _obj.Duration2 = null;
    }
    
    /// <summary>
    /// Очистить третью строку в разделе "Новые даты отпуска".
    /// </summary>
    [Remote]
    public void CleanThirdNewVacationDate()
    {
      _obj.VacationKind3 = null;
      _obj.DataBegin3 = null;
      _obj.Duration3 = null;
    }
  }
}