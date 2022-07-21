using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.ShiftStatementApprovalAssignment;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;

namespace DirRX.HRManagement.Server
{
  partial class ShiftStatementApprovalAssignmentFunctions
  {
    /// <summary>
    /// Перераспределить значения, чтобы не было пустых строк.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.SortFields().
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
    /// Если при изменении переносимых отпусков определенный вид отпуска стал недоступен для указания в новых датах, то очистить строки новых дат у которых указан этот вид.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.VacationKindsCheckUp().
    /// </summary>
    [Remote]
    public void VacationKindsCheckUp()
    {
      var selectedKindList = this.GetVacationKindList();
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
      return Functions.VacationShiftTask.GetVacationKindList(_obj.Vacation1, _obj.Vacation2, _obj.Vacation3);
    }
    
    #region Расчет доступных дней.
    /// <summary>
    /// Пересчитать сумму запланированных и исполненных на дату отзыва дней отпуска.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.FillVacationPlanDurations().
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
    /// Аналогичный метод в задаче Functions.VacationShiftTask.FillVacationShiftDurations().
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
    /// Аналогичный метод в задаче Functions.VacationShiftTask.SetLeftDays().
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
          vacDurations.DaysAvailComp += Functions.Module.GetAvailableDaysToCompensation(VacationShiftTasks.As(_obj.Task).Employee, vacDurations.VacationKind, _obj.Year.Value.Year, vacDurations.DaysPlaned.Value);
      
      if (_obj.Compensation.Value)
        this.SetLeftDays(_obj.Compensation.Value);
      
      if (!_obj.VacationDurations.Any() || _obj.VacationDurations.All(v => v.DaysPlaned == 0))
        _obj.Year = Calendar.SqlMinValue;
      
     this.ChangeInfoLabel();
    }
    
    /// <summary>
    /// Установить количество отмененных дней отпуска при простановке признака Компенсировать/Отменить.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.SetLeftDays().
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
    /// Аналогичный метод в задаче Functions.VacationShiftTask.ChangeInfoLabel().
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
      
      Functions.ShiftStatementApprovalAssignment.SetVisibleProperties(_obj);
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
      
      var vacationShiftTask = VacationShiftTasks.As(_obj.Task);
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
      var vacationDatesNotMatchDuration = Functions.Module.CheckVacatinDatesMatchDuration(newVacationsParties, employee);
      if (vacationDatesNotMatchDuration.Any())
        errorList.AddRange(vacationDatesNotMatchDuration);
      
      return errorList;
    }
    
    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.CreateVacationDateStructure().
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Public, Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      if (_obj.DataBegin1.HasValue && _obj.DataEnd1.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin1.Value, _obj.DataEnd1.Value, _obj.Duration1.Value, DateStatuses.New, _obj.VacationKind1));
      if (_obj.DataBegin2.HasValue && _obj.DataEnd2.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin2.Value, _obj.DataEnd2.Value, _obj.Duration2.Value, DateStatuses.New, _obj.VacationKind2));
      if (_obj.DataBegin3.HasValue && _obj.DataEnd3.HasValue)
        vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.DataBegin3.Value, _obj.DataEnd3.Value, _obj.Duration3.Value, DateStatuses.New, _obj.VacationKind3));
      return vacationDateList;
    }
    
    /// <summary>
    /// Проверить, что есть часть отпуска с необходимой продолжительностью.
    /// </summary>
    /// <returns>Текст сообщения об отсутствии части основного отпуска с необходимой продолжительностью.</returns>
    [Remote]
    public virtual string CheckRequiredDuration()
    {
      var requiredDurationError = string.Empty;
      var task = VacationShiftTasks.As(_obj.Task);

      var vacationDates = this.CreateVacationDateStructure();

      // Добавить к новым датам те, что есть в справочнике отпусков, за исключением тех, что уже выбраны в карточке
      // Причем с точки проверки минимальной длительности порции оснонового вида отпуска нас интересуют отпуска только определенного вида
      var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
      var paidVacationList = Functions.Vacation.GetPaidVacations(_obj.Employee, _obj.Year.Value)
        .Where(v => !v.Equals(_obj.Vacation1) && !v.Equals(_obj.Vacation2) && !v.Equals(_obj.Vacation3) && Equals(v.VacationKind, mainPaidVacationKind));
      vacationDates.AddRange(Functions.Module.FormVacationDateStructure(paidVacationList));

      // для отзыва добавим в проверки уже использованную часть отпуска Vacation1 -а то она не попадет в анализ
      if (task.IsVacationRecall.Value && _obj.Vacation1 != null && _obj.Vacation1.VacationKind == mainPaidVacationKind) {
        var vd = _obj.VacationDurations.Where(v => v.VacationKind == mainPaidVacationKind).FirstOrDefault();
        var vacationPart = Structures.Vacation.VacationDates.Create();
        vacationPart.VacationKind = mainPaidVacationKind;
        if (vd.DaysUsed > 0) {
          vacationPart.Duration = vd.DaysUsed.Value;
          vacationDates.Add(vacationPart);
        }
      }

      // Проверить, что есть часть отпуска с необходимой продолжительностью.
      if (vacationDates.Any()) {
        requiredDurationError = Functions.Module.CheckVacationRequiredDuration(vacationDates, _obj.Employee, _obj.Year.Value);
      }
      
      if (task.IsVacationRecall.Value && !string.IsNullOrEmpty(requiredDurationError))
      {
        var responsibleRoleRecipients = Functions.Module.GetRoleRecipients(Constants.Module.HRRoles.VacationResponsible);
        var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
        if (!isFromEssAccount && !Equals(Employees.Current, _obj.Employee) &&
            Equals(Employees.Current, Functions.Module.GetManager(_obj.Employee)))
        {
          // галочку "Отключить проверку на 14 дней" делать доступной только:
          // если задачу создал руководитель подраздления по своему сотруднику, а не по себе
          requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationError;
          _obj.State.Properties.IgnoreRequiredDuration.IsVisible = true;
          
        }
      }
      else
      {
        _obj.IgnoreRequiredDuration = false;
        _obj.State.Properties.IgnoreRequiredDuration.IsVisible = false;
      }
      return requiredDurationError;
    }
    
    /// <summary>
    /// Проверить, что дата отзыва не нарушит правило 14 дней
    /// Правило может быть нарушено только если в Vacation1 будет часть отпуска с основным видом.
    /// </summary>
    /// <returns>Текст сообщения об отсутствии части основного отпуска с необходимой продолжительностью.</returns>
    [Remote]
    public virtual string CheckRequiredDurationonVacation1()
    {
      var requiredDurationError = string.Empty;
      var mainPaidVacationKind = Functions.Module.GetPaidVacationKind(_obj.Employee);
      var task = VacationShiftTasks.As(_obj.Task);
      
      // проверку выполнять только если отзыв и Vacation1 заполнено основным видом отпуска
      if (task.IsVacationRecall.Value && _obj.Vacation1 != null && _obj.Vacation1.VacationKind == mainPaidVacationKind) {
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
        if (vacationDates.Any()) {
          requiredDurationError = Functions.Module.CheckVacationRequiredDuration(vacationDates, _obj.Employee, _obj.Year.Value);
          // подменить сообщение - в этом случае текст другой будет
          if (!string.IsNullOrEmpty(requiredDurationError))
          {
            // Текст сообщения зависит от того, кто и по кому стартует задачу - кто-то сам по себе или руководитель по сотруднику.
            var isFromEssAccount = _obj.IsFromEssAccount.HasValue && _obj.IsFromEssAccount.Value;
            if (!isFromEssAccount && !Equals(Employees.Current, _obj.Employee) &&
                Equals(Employees.Current, Functions.Module.GetManager(_obj.Employee)))
              requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationError;
            else
              requiredDurationError = VacationShiftTasks.Resources.VacationRequiredDurationWarning;
          }
        }
      }
      return requiredDurationError;
    }
    
    #endregion
    
    /// <summary>
    /// Отфильтровать доступные для выбора отпуска на перенос и отзыв.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.VacationShiftFilter().
    /// </summary>
    /// <param name="vacation">Отпуск.</param>
    /// <returns>Список доступных отпусков.</returns>
    [Remote]
    public IQueryable<IVacation> VacationShiftFilter(IVacation vacation)
    {
      var year = vacation != null ? vacation.Year.Value : _obj.Year.Value;
      if (!VacationShiftTasks.As(_obj.Task).IsVacationRecall.Value)
        return Functions.Vacation.GetPlanVacations(_obj.Employee, year);
      else
        return Functions.Vacation.GetRecallVacations(_obj.Employee, _obj.RecallDate);
    }
    
    /// <summary>
    /// Отфильтровать доступные для переноса виды отпуска.
    /// Возвращает виды отпуска в выбранных для изменения частях отпуска и еще не полностю распределены, а также вид отпуска из currentVacationKind.
    /// </summary>
    /// <param name="currentVacationKind">Вид отпуска.</param>
    /// <returns>Список доступных видов отпусков.</returns>
    // TODO описать.
    [Remote]
    public IQueryable<IVacationKind> VacationKindFilter(IVacationKind currentVacationKind)
    {
      var kindList = new List<IVacationKind>();
      if (currentVacationKind != null)
        kindList.Add(currentVacationKind);
      foreach (var vd in _obj.VacationDurations) {
        if (vd.DaysPlaned > vd.DaysShifted) {
          kindList.Add(vd.VacationKind);
        }
      }
      return VacationKinds.GetAll().Where(l => kindList.Contains(l));
    }
    
    /// <summary>
    /// Получить дату окончания отпуска и пересчитать количество доступных дней.
    /// Аналогичный метод в задаче Functions.VacationShiftTask.FillEndDateAndSetDays().
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
      Functions.ShiftStatementApprovalAssignment.SetLeftDays(_obj);
      return dateEnd;
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
    /// Проверить, заполнена ли первая строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptyFirstNewVacationDate()
    {
      return Functions.VacationShiftTask.IsEmptyNewVacationDates(_obj.DataBegin1, _obj.Duration1, _obj.DataEnd1, _obj.VacationKind1);
    }
    
    /// <summary>
    /// Проверить, заполнена ли вторая строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptySecondNewVacationDate()
    {
      return Functions.VacationShiftTask.IsEmptyNewVacationDates(_obj.DataBegin2, _obj.Duration2, _obj.DataEnd2, _obj.VacationKind2);
    }
    
    /// <summary>
    /// Проверить, заполнена ли третяя строка в разделе "Новые даты отпуска".
    /// </summary>
    /// <returns>True, если строка пустая.</returns>
    [Remote]
    public bool IsEmptyThirdNewVacationDate()
    {
      return Functions.VacationShiftTask.IsEmptyNewVacationDates(_obj.DataBegin3, _obj.Duration3, _obj.DataEnd3, _obj.VacationKind3);
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