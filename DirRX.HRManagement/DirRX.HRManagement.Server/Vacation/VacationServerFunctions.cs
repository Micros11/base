using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.Vacation;
using DateStatuses = DirRX.HRManagement.Constants.Module.DateStatus;
using HRRoles = DirRX.HRManagement.Constants.Module.HRRoles;
using ParamKeys = DirRX.HRManagement.Constants.Module.ParamKey;

namespace DirRX.HRManagement.Server
{
  partial class VacationFunctions
  {
    /// <summary>
    /// Найти все документы связанные с отпуском.
    /// </summary>
    /// <returns>Список документов.</returns>
    [Remote]
    public virtual List<Sungero.Docflow.IInternalDocumentBase> GetAllDocumentsForVacation()
    {
      var docs = new List<Sungero.Docflow.IInternalDocumentBase>();
      var vacationStartDate = _obj.StartDate;
      var vacationEndDate = _obj.FinDate;
      var vacationEmployee = _obj.Employee;
      var vacationKind = _obj.VacationKind;
      if (vacationKind.Paid.Value)
      {
        var schedulingAssignments = VacationEmployeeSchedulingAssignments.GetAll(l => l.Performer.Equals(vacationEmployee) &&
                                                                                 l.Vacations.Any(x => Equals(x.DateBegin, vacationStartDate) &&
                                                                                                 Equals(x.DateEnd, vacationEndDate)) &&
                                                                                 l.Task.Status == Sungero.Workflow.Task.Status.Completed);
        
        var departmentSchedulingTask = VacationDepartmentSchedulingTasks.As(schedulingAssignments.Select(l => l.Task).FirstOrDefault());
        if (departmentSchedulingTask != null)
        {
          var schedulingTask = VacationSchedulingTasks.As(departmentSchedulingTask.MainSchedulingTask);
          var doc = schedulingTask.VacationScheduleGroup.HRDocuments.FirstOrDefault();
          if (doc != null)
            docs.Add(doc);
        }
      }
      
      var singleSchedulingTask = VacationSingleSchedulingTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                                                      l.Vacations.Any(x => Equals(x.DateBegin, vacationStartDate) &&
                                                                                      Equals(x.DateEnd, vacationEndDate) &&
                                                                                      Equals(x.VacationKind, vacationKind)) &&
                                                                      l.Status == Sungero.Workflow.Task.Status.Completed).FirstOrDefault();
      if (singleSchedulingTask != null && singleSchedulingTask.Order != null)
        docs.Add(singleSchedulingTask.Order);
      
      var approvalTask = VacationApprovalTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                                      Equals(l.DateBegin, vacationStartDate) &&
                                                      Equals(l.VacationKind, vacationKind) &&
                                                      Equals(l.DateEnd, vacationEndDate) &&
                                                      l.Status == Sungero.Workflow.Task.Status.Completed).FirstOrDefault();
      if (approvalTask != null && approvalTask.Order != null)
        docs.Add(approvalTask.Order);
      
      var shiftTask = VacationShiftTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                                (Equals(l.Vacation1, _obj) ||
                                                 Equals(l.Vacation2, _obj) ||
                                                 Equals(l.Vacation3, _obj)) &&
                                                l.Status == Sungero.Workflow.Task.Status.Completed).FirstOrDefault();
      if (shiftTask != null && shiftTask.Order != null)
        docs.Add(shiftTask.Order);

      shiftTask = VacationShiftTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                            (Equals(l.DataBegin1.Value, vacationStartDate) && Equals(l.DataEnd1.Value, vacationEndDate) && Equals(l.VacationKind1, vacationKind) ||
                                             Equals(l.DataBegin2.Value, vacationStartDate) && Equals(l.DataEnd2.Value, vacationEndDate) && Equals(l.VacationKind2, vacationKind) ||
                                             Equals(l.DataBegin3.Value, vacationStartDate) && Equals(l.DataEnd3.Value, vacationEndDate) && Equals(l.VacationKind3, vacationKind)) &&
                                            l.Status == Sungero.Workflow.Task.Status.Completed).FirstOrDefault();
      if (shiftTask != null && shiftTask.Order != null)
        docs.Add(shiftTask.Order);
      
      return docs;
    }
    
    /// <summary>
    /// Найти все задачи связанные с отпуском.
    /// </summary>
    /// <returns>Список задач.</returns>
    [Remote]
    public virtual List<Sungero.Workflow.ITask> GetAllTasksForVacation()
    {
      var tasks = new List<Sungero.Workflow.ITask>();
      var vacationStartDate = _obj.StartDate;
      var vacationEndDate = _obj.FinDate;
      var vacationEmployee = _obj.Employee;
      var vacationKind = _obj.VacationKind;
      
      var schedulingAssignments = VacationEmployeeSchedulingAssignments.GetAll(l => l.Performer.Equals(vacationEmployee) &&
                                                                               l.Vacations.Any(x => Equals(x.DateBegin, vacationStartDate) &&
                                                                                               Equals(x.DateEnd, vacationEndDate) &&
                                                                                               Equals(x.VacationKind, vacationKind)));
      if (schedulingAssignments.Any())
        tasks.AddRange(schedulingAssignments.Select(l => l.Task));
      
      tasks.AddRange(VacationSingleSchedulingTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                                          l.Vacations.Any(x => Equals(x.DateBegin, vacationStartDate) &&
                                                                          Equals(x.DateEnd, vacationEndDate) &&
                                                                          Equals(x.VacationKind, vacationKind))));
      
      tasks.AddRange(VacationApprovalTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                                  Equals(l.DateBegin, vacationStartDate) &&
                                                  Equals(l.VacationKind, vacationKind) &&
                                                  Equals(l.DateEnd, vacationEndDate)));
      
      tasks.AddRange(VacationShiftTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                               (Equals(l.Vacation1, _obj) ||
                                                Equals(l.Vacation2, _obj) ||
                                                Equals(l.Vacation3, _obj))));
      
      tasks.AddRange(VacationShiftTasks.GetAll(l => Equals(l.Employee, vacationEmployee) &&
                                               (Equals(l.DataBegin1.Value, vacationStartDate) && Equals(l.DataEnd1.Value, vacationEndDate) ||
                                                Equals(l.DataBegin2.Value, vacationStartDate) && Equals(l.DataEnd2.Value, vacationEndDate) ||
                                                Equals(l.DataBegin3.Value, vacationStartDate) && Equals(l.DataEnd3.Value, vacationEndDate))));
      
      tasks.AddRange(VacationAlertTasks.GetAll(l => Equals(l.Vacation, _obj)));
      return tasks;
    }

    /// <summary>
    /// Создать структуру для хранения периодов отпусков.
    /// </summary>
    /// <returns>Список дат отпусков.</returns>
    [Remote]
    public virtual List<Structures.Vacation.IVacationDates> CreateVacationDateStructure()
    {
      var vacationDateList = new List<Structures.Vacation.IVacationDates>();
      vacationDateList.Add(Structures.Vacation.VacationDates.Create(_obj.StartDate.Value,
                                                                    _obj.FinDate.Value,
                                                                    _obj.VacationDuration.Value,
                                                                    DateStatuses.New,
                                                                    _obj.VacationKind));
      var vacationList = Functions.Vacation.GetPaidVacations(_obj.Employee, _obj.Year.Value).Where(l => !l.Equals(_obj));
      vacationDateList.AddRange(Functions.Module.FormVacationDateStructure(vacationList));
      
      return vacationDateList;
    }
    
    /// <summary>
    /// Проверить правильность указанных дат отпуска на не критичные ошибки.
    /// </summary>
    /// <param name="vacationDates">Дата отпуска.</param>
    /// <returns>Не критичные ошибки в датах отпуска.</returns>
    [Remote]
    public virtual List<string> CheckNotCriticalVacationsDates(List<Structures.Vacation.IVacationDates> vacationDates)
    {
      List<string> errorList = new List<string>();
      var employee = _obj.Employee;
      var year = _obj.Year.Value;
      // Проверить, что отпуск не заканчивается перед выходным или праздничным днем.
      if (_obj.VacationKind.Paid.Value)
      {
        var newVacationDate = vacationDates.Where(l => l.Status == DateStatuses.New).FirstOrDefault();
        var vacationEndBeforeWeekend = Functions.Module.CheckVacationEndBeforeWeekendOrHoliday(newVacationDate, employee);
        if (!string.IsNullOrWhiteSpace(vacationEndBeforeWeekend))
          errorList.Add(vacationEndBeforeWeekend);
      }
      
      // Проверить, что есть часть отпуска с необходимой продолжительностью.
      var vacationDuration = Functions.Module.CheckVacationRequiredDuration(vacationDates, employee, year);
      if (!string.IsNullOrWhiteSpace(vacationDuration))
        errorList.Add(vacationDuration);

      // Проверить общую продолжительность отпуска.
      var employeePersonalDuration = Functions.Module.GetEmployeeVacationDuration(employee, year);
      if (employeePersonalDuration != null)
      {
        var kindDuration = employeePersonalDuration.AvailableDuration.Where(l => Equals(l.VacationKind, _obj.VacationKind)).FirstOrDefault();
        if (kindDuration != null)
        {
          var vacationsTotalDuration = Functions.Module.CheckVacationsTotalDuration(vacationDates,
                                                                                    kindDuration.VacationKind,
                                                                                    kindDuration.DaysCount.Value);
          if (!string.IsNullOrWhiteSpace(vacationsTotalDuration))
            errorList.Add(vacationsTotalDuration);
        }
      }
      return errorList;
    }

    /// <summary>
    /// Проверить правильность указанных дат отпуска на критичные ошибки.
    /// </summary>
    /// <param name="vacationDates">Список дат отпусков.</param>
    /// <returns>Критичные ошибки в датах отпуска.</returns>
    [Remote]
    public virtual List<string> CheckCriticalVacationsDates(List<Structures.Vacation.IVacationDates> vacationDates)
    {
      List<string> errorList = new List<string>();

      // Проверить, что отпуск не пересекается.
      // К проверяемым датам добавить даты отпусков предыдущего года.
      var vacationDatesForIntersections = Functions.Module.CreateVacationDateStructureForIntersection(_obj.Employee, _obj.Year.Value, false);
      if (_obj.VacationKind.Paid.Value)
        vacationDatesForIntersections.AddRange(vacationDates);
      var vacationIntersections = Functions.Module.CheckVacationIntersections(vacationDatesForIntersections);
      if (vacationIntersections.Any())
        errorList.AddRange(vacationIntersections);

      // Проверить что даты не выходят за границы года.
      var newVacationDate = vacationDates.Where(l => l.Status == DateStatuses.New).ToList();
      CommonLibrary.LocalizedString hint;
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      var isFullAccess = Functions.Module.IsHRAdministrator(users) || Functions.Module.IsDebugEnabled();
      var isVacationSchedulingInBusinessUnit = Functions.Module.IsVacationSchedulingInBusinessUnit(users);
      if (isFullAccess && !isVacationSchedulingInBusinessUnit)
        hint = Resources.VacationEndOverBorderCalendarYearForAdmin;
      else
        hint = Resources.VacationEndOverBorderCalendarYear;
      var vacationDatеInCalendarYear = Functions.Module.CheckVacationDateInCalendarYear(newVacationDate, _obj.Employee, _obj.Year.Value, hint);
      if (!string.IsNullOrWhiteSpace(vacationDatеInCalendarYear))
        errorList.Add(vacationDatеInCalendarYear);
      
      return errorList;
    }
    
    #region Функции, относящиеся к получению прав доступа к записи справочника Отпуска.
    
    /// <summary>
    /// Получить тип прав пользователя на текущую запись отпуска с учетом замещений.
    /// </summary>
    /// <param name="users">Пользователь и его замещаемые.</param>
    /// <returns>Режим доступа к записи справочника Отпуска:
    /// - Полные права: на любые записи в режиме отладки и для роли Администратор HR-процессов и сервисным пользователям;
    /// - Редактирование: рядовому пользователю на этапе планирвоания в подразделении, руководителям и ответственным за отпуска в подразделениях на этапе планирования в организации;
    /// - Ограниченное редактирование: после завершения планирования, ответственным за отпуска в организации;
    /// - Просмотр: прочие ситуации.
    /// </returns>
    [Remote(IsPure = true), Public]
    public string GetVacationAccess(List<IUser> users)
    {
      // Полные права, если:
      //  - Пользователь включен в роль Администратор HR-процессов
      //  - Пользователь служебный
      //  - Пользователь включен в роль для отладки
      if (Functions.Module.IsHRAdministrator(users) || Functions.Module.IsServiceUser(users) || Functions.Module.IsDebugEnabled())
        return DirRX.HRManagement.Constants.Vacation.VacationAccess.FullAccess;
      
      if (_obj.State.IsInserted || _obj.State.IsCopied)
      {
        // Создание отпусков потенциально возможно только во время планирования в организации.
        if (Functions.Module.IsVacationSchedulingInBusinessUnit(users))
          // Создавать записи во время планирования в подразделении может любой пользователь, а после окончания - только ответственные.
          if (Functions.Module.IsVacationSchedulingInDepartment(users) || this.IsDepartmentVacationResponsible(users) || this.IsBusinessUnitVacationResponsible(users))
            return DirRX.HRManagement.Constants.Vacation.VacationAccess.Change;
      }
      else
      {
        // Редактирование отпусков потенциально возможно только во время планирования в организации для записей на следующий учетный период.
        // После окончания планирования в организации возможно ограниченное редактирование только ответственным за отпуска в организации.
        if (this.IsVacationSchedulingInBusinessUnit() && this.IsVacationInNextYear())
        {
          // Личные записи можно редактировать в период планирования внутри подразделения.
          if (users.Any(l => Equals(_obj.Employee, l)) && this.IsVacationSchedulingInDepartment())
            return DirRX.HRManagement.Constants.Vacation.VacationAccess.Change;
          // Если планирование в подразделении уже закончилось, то записи могут редактировать ответственные за отпуска в подразделениях и по организации.
          if (this.IsDepartmentVacationResponsible(users) || this.IsBusinessUnitVacationResponsible(users))
            return DirRX.HRManagement.Constants.Vacation.VacationAccess.Change;
        }
        else
          if (this.IsBusinessUnitVacationResponsible(users))
            return DirRX.HRManagement.Constants.Vacation.VacationAccess.LimitedChange;
      }
      return DirRX.HRManagement.Constants.Vacation.VacationAccess.Read;
    }
    
    /// <summary>
    /// Получить тип прав текущего пользователя на текущую запись отпуска с учетом замещений.
    /// </summary>
    /// <returns>Режим доступа к записи справочника Отпуска:
    /// - Полные права: на любые записи в режиме отладки и для роли Администратор HR-процессов;
    /// - Редактирование: рядовому пользователю на этапе планирования в подразделении, руководителям и ответственным за отпуска в подразделениях на этапе планирования в организации;
    /// - Ограниченное редактирование: после завершения планирования, ответственным за отпуска в организации;
    /// - Просмотр: прочие ситуации. </returns>
    [Remote(IsPure = true)]
    public string GetVacationAccess()
    {
      // Получить всех замещаемых текущего пользователя.
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      return this.GetVacationAccess(users);
    }
    
    /// <summary>
    /// Проверить, что пользователь является Ответственным за отпуска в организации.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей входит в Ответственные за отпуска в организации сотрудника, указанного в записи.</returns>
    [Remote(IsPure = true), Public]
    public bool IsBusinessUnitVacationResponsible(List<IUser> users)
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee);
      var responsibleRoleRecipient = Functions.Module.GetVacationResponsible(businessUnit);
      if (users.Any(v => v.Equals(responsibleRoleRecipient)))
        return true;
      return false;
    }
    
    /// <summary>
    /// Проверить, что пользователь является Ответственным за отпуска в подразделении.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>True, если один из пользователей является руководителем или входит в Ответственные за отпуска подразделения сотрудника, указанного в записи.</returns>
    [Remote(IsPure = true), Public]
    public bool IsDepartmentVacationResponsible(List<IUser> users)
    {
      if (_obj.State.IsInserted)
      {
        // Проверить, является ли руководителем одного из подразделений.
        if (users.Any(v => Departments.GetAll(d => v.Equals(Users.As(d.Manager))).Any()))
          return true;
        // Проверить, входит ли в ответственные по подразделениям.
        if (Functions.Module.IsVacationManager(users))
          return true;
      }
      else
      {
        // Проверить, является ли один из пользователей руководителем подразделения сотрудника из записи.
        var departmentManager = Functions.Module.GetManager(_obj.Employee.Department);
        if (users.Any(v => v.Equals(Users.As(departmentManager))))
          return true;
        // Проверить, входит ли в ответственные по подразделению сотрудника из записи.
        var vacationManagerDepartments = GetVacationManagerDepartment(users);
        if (vacationManagerDepartments != null)
          if (vacationManagerDepartments.Any(d => d.Equals(_obj.Employee.Department)))
            return true;
      }
      return false;
    }
    
    /// <summary>
    /// Получить подразделения сотрудников, входящих в роль Ответственные за график отпусков в подразделении.
    /// </summary>
    /// <param name="users">Список работников.</param>
    /// <returns>Подразделения.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IDepartment> GetVacationManagerDepartment(List<IEmployee> users)
    {
      var vacationManagerRole = Functions.Module.GetRole(HRRoles.VacationManagers);
      var managerUser = users.Where(u => u.IncludedIn(vacationManagerRole)).ToList();
      if (managerUser.Any())
      {
        var managerDep = managerUser.Select(m => m.Department).ToList();
        var departmentsList = Departments.GetAll().ToList();
        var departmentsQuery = departmentsList.Where(d => managerUser.Any(m => m.Equals(d.Manager)) || managerDep.Any(md => md.Equals(d))).AsQueryable();
        return departmentsQuery;
      }
      else
        return (IQueryable<IDepartment>)Departments.Null;
    }
    
    /// <summary>
    /// Получить подразделения сотрудников, входящих в роль Ответственные за график отпусков в подразделении.
    /// </summary>
    /// <param name="users">Список пользователей.</param>
    /// <returns>Подразделения.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IDepartment> GetVacationManagerDepartment(List<IUser> users)
    {
      var employees = new List<IEmployee>();
      employees = users.Select(u => Employees.As(u)).ToList();
      return GetVacationManagerDepartment(employees);
    }
    
    /// <summary>
    /// Проверить, что в подразделении сотрудника, указанного в записи, идет процесс планирования отпусков.
    /// </summary>
    /// <returns>True, если в подразделении идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInDepartment()
    {
      return Functions.Module.IsVacationSchedulingInDepartment(_obj.Employee.Department);
    }
    
    /// <summary>
    /// Проверить, что в организации сотрудника, указанного в записи, идет процесс планирования отпусков.
    /// </summary>
    /// <returns>True, если в организации идет процесс планирования отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationSchedulingInBusinessUnit()
    {
      var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(_obj.Employee);
      return Functions.Module.IsVacationSchedulingInBusinessUnit(businessUnit);
    }
    
    /// <summary>
    /// Проверить, что даты, указанные в отпуске, входят следующий учетный год отпусков.
    /// </summary>
    /// <returns>True, если отпуск входит в следующий учетный год отпусков.</returns>
    [Remote(IsPure = true), Public]
    public bool IsVacationInNextYear()
    {
      return Calendar.Today.NextYear() == _obj.Year.Value;
    }
    
    /// <summary>
    /// Получить ИД всех сотрудников, которых текущий пользователь может указывать в записи Отпуска.
    /// </summary>
    /// <returns>Список ИД сотрудников.</returns>
    public static IQueryable<int> GetEmployeesForVacationFill()    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      var availableEmployees = Employees.GetAll();
      if (!Functions.Module.IsHRAdministrator(users) && !Functions.Module.IsDebugEnabled())
      {
        // Пользователь и его замещаемые из подразделений, в которых сейчас идет планирование.
        var userEmployees = users.Select(u => Employees.As(u)).Where(v => Functions.Module.IsVacationSchedulingInDepartment(v.Department));
        // Подразделения, где пользователь или его замещаемые явлюятся руководителями или ответственными за отпуска.
        var userManagerInDepartments = Departments.GetAll(d => users.Contains(Users.As(d.Manager)));
        var vacationManagerRole = Functions.Module.GetRole(HRRoles.VacationManagers);
        var userResponsibleInDepartments = users.Where(v => v.IncludedIn(vacationManagerRole)).Select(u => Employees.As(u).Department);
        // Организации, где пользователь и его замещаемые ответственны за работу с графиком отпусков.
        var responsibleRoleRecipients = Functions.Module.GetRoleRecipients(Constants.Module.HRRoles.VacationResponsible);
        var userResponsibleInBusinessUnits = users.Where(v => responsibleRoleRecipients.Any(r => Equals(r, v))).Select(u => Employees.As(u).Department.BusinessUnit);
        
        // Во всех организациях, которые попали в фильтр, должно идти планирование.
        var schedulingInBusinessUnits = BusinessUnits.GetAll().ToList().Where(b => Functions.Module.IsVacationSchedulingInBusinessUnit(b));
        availableEmployees = availableEmployees.Where(u => (userEmployees.Contains(u) ||
                                                            userManagerInDepartments.Contains(u.Department) ||
                                                            userResponsibleInDepartments.Contains(u.Department) ||
                                                            userResponsibleInBusinessUnits.Contains(u.Department.BusinessUnit)) &&
                                                      schedulingInBusinessUnits.Contains(u.Department.BusinessUnit));
      }
      return availableEmployees.Select(u => u.Id);
    }
    
    /// <summary>
    /// Получить ИД всех подразделений, которые текущий пользователь может указывать в записи Отпуска.
    /// </summary>
    /// <returns>Список ИД подразделений.</returns>
    public static IQueryable<int> GetDepartmentsForVacationFill()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsersWithoutSystem.ToList();
      users.Add(Users.Current);
      var availableDepartments = Departments.GetAll();
      if (!Functions.Module.IsHRAdministrator(users) && !Functions.Module.IsDebugEnabled())
      {
        // Подразделения, где пользователь и его замещаемые являются руководителями или ответственными за отпуска в подразделении.
        var userDepartments = users.Select(v => Employees.As(v).Department).Where(d => Functions.Module.IsVacationSchedulingInDepartment(d));
        // Подразделения, где пользователь и его замещаемые явлюятся руководителями или ответственными за отпуска в подразделении.
        var userManagerInDepartments = Departments.GetAll(d => users.Contains(Users.As(d.Manager)));
        var vacationManagerRole = Functions.Module.GetRole(HRRoles.VacationManagers);
        var userResponsibleInDepartments = users.Where(v => v.IncludedIn(vacationManagerRole)).Select(u => Employees.As(u).Department);
        // Организации, где пользователь и его замещаемые ответственны за работу с графиком отпусков.
        var responsibleRoleRecipients = Functions.Module.GetRoleRecipients(Constants.Module.HRRoles.VacationResponsible);
        var userResponsibleInBusinessUnits = users.Where(v => responsibleRoleRecipients.Any(r => Equals(r, v))).Select(u => Employees.As(u).Department.BusinessUnit);
        
        // Во всех организациях подразделений, которые попадут в фильтр, должно идти планирование.
        var schedulingInBusinessUnits = BusinessUnits.GetAll().ToList().Where(b => Functions.Module.IsVacationSchedulingInBusinessUnit(b));
        availableDepartments = availableDepartments.Where(d => (userDepartments.Contains(d) ||
                                                                userManagerInDepartments.Contains(d) ||
                                                                userResponsibleInDepartments.Contains(d) ||
                                                                userResponsibleInBusinessUnits.Contains(d.BusinessUnit)) &&
                                                          schedulingInBusinessUnits.Contains(d.BusinessUnit));
      }
      return availableDepartments.Select(d => d.Id);
    }
    #endregion
    
    /// <summary>
    /// Получить все оплачиваемые отпуска по сотруднику.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetPaidVacations(IEmployee employee, DateTime year)
    {
      return Vacations.GetAll(v => Equals(v.Employee, employee) &&
                              (v.Year.Value.Year == year.Year) &&
                              (!(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled)) &&
                              (v.VacationKind.Paid == true)).OrderBy(v => v.StartDate);
    }
    
    /// <summary>
    /// Получить все оплачиваемые отпуска по списку подразделений.
    /// </summary>
    /// <param name="departments">Список подразделений.</param>
    /// <param name="year">Год.</param>
    /// <param name="activeOnly">True, если в список должны попасть только действующие сотрудники.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetPaidVacations(List<IDepartment> departments, DateTime year, bool activeOnly)
    {
      var vacations = Vacations.GetAll(v => departments.Contains(v.Department) &&
                                       (v.Year.Value.Year == year.Year) &&
                                       (!(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled)) &&
                                       (v.VacationKind.Paid == true));
      if (activeOnly)
        vacations = vacations.Where(v => v.Employee.Status == Sungero.Company.Employee.Status.Active);
      return vacations;
    }
    
    /// <summary>
    /// Получить все оплачиваемые отпуска по НОР.
    /// </summary>
    /// <param name="businessUnit">НОР.</param>
    /// <param name="year">Год.</param>
    /// <param name="activeOnly">True, если в список должны попасть только действующие сотрудники.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetPaidVacations(IBusinessUnit businessUnit, DateTime year, bool activeOnly)
    {
      var vacations = Vacations.GetAll(v => Equals(v.BusinessUnit, businessUnit) &&
                                       (v.Year.Value.Year == year.Year) &&
                                       (!(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled)) &&
                                       (v.VacationKind.Paid == true));
      if (activeOnly)
        vacations = vacations.Where(v => v.Employee.Status == Sungero.Company.Employee.Status.Active);
      return vacations.OrderBy(v => v.Department.Name).ThenBy(v => v.Employee.Name).ThenBy(v => v.StartDate);
    }
    
    /// <summary>
    /// Получить все оплачиваемые отпуска по НОР, включить отпуска выходящие за границы учетного год.
    /// </summary>
    /// <param name="businessUnit">Список бизнес-единиц.</param>
    /// <param name="year">Год.</param>
    /// <param name="activeOnly">True, если в список должны попасть только действующие сотрудники.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetBorderPaidVacations(IBusinessUnit businessUnit, DateTime year, bool activeOnly)
    {
      var borderDate = Functions.Module.GetBorderVacationYear(businessUnit, year);
      var beginingYear = year.BeginningOfYear().BeginningOfWeek();
      
      var vacations = Vacations
        .GetAll(v => Equals(v.BusinessUnit, businessUnit) &&
                (v.FinDate.Value >= beginingYear) &&
                (v.StartDate.Value <= borderDate) &&
                (!(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled)) &&
                (v.VacationKind.Paid == true));
      
      if (activeOnly)
        vacations = vacations.Where(v => v.Employee.Status == Sungero.Company.Employee.Status.Active);
      return vacations.OrderBy(v => v.Department.Name).ThenBy(v => v.Employee.Name).ThenBy(v => v.StartDate);
    }
    
    /// <summary>
    /// Получить все оплачиваемые отпуска по списку подразделений из одной НОР, включить отпуска выходящие за границы учетного год.
    /// </summary>
    /// <param name="departments">Список подразделений.</param>
    /// <param name="year">Год.</param>
    /// <param name="activeOnly">True, если в список должны попасть только действующие сотрудники.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetBorderPaidVacations(List<IDepartment> departments, DateTime year, bool activeOnly)
    {
      var vacationsList = GetBorderPaidVacations(departments.FirstOrDefault().BusinessUnit, year, activeOnly).ToList();
      var vacationsQuery = vacationsList.Where(v => departments.Any(d => d.Equals(v.Department))).AsQueryable();
      return vacationsQuery;
    }
    
    /// <summary>
    /// Получить все отпуска сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetVacations(IEmployee employee, DateTime year)
    {
      return Vacations.GetAll(v => Equals(v.Employee, employee) &&
                              (v.Year.Value.Year == year.Year) &&
                              (!(v.Status == DirRX.HRManagement.Vacation.Status.Shifted || v.Status == DirRX.HRManagement.Vacation.Status.Canceled)))
        .OrderBy(v => v.StartDate);
    }

    /// <summary>
    /// Получить запланированные отпуска сотрудника.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="year">Год.</param>
    /// <returns>Список запланированных отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetPlanVacations(IEmployee employee, DateTime year)
    {
      return Vacations.GetAll(v => Equals(v.Employee, employee) &&
                              (v.Year.Value.Year == year.Year || year == Calendar.SqlMinValue) &&
                              (v.Status == DirRX.HRManagement.Vacation.Status.Active))
        .OrderBy(v => v.StartDate);
    }
    
    /// <summary>
    /// Получить все отпуска сотрудника для отзыва. Для отзыва нас интересует непрерывная цепочка исполняющихся и/или подтвержденных отпусков.
    /// Для первого отпуска из цепочки должно выполняться условие: дата отзыва попадает в период отпуска.
    /// </summary>
    /// <param name="employee">Сотрудник.</param>
    /// <param name="recallDate">Дата отзыва.</param>
    /// <returns>Список отпусков.</returns>
    [Remote(IsPure = true)]
    public static IQueryable<IVacation> GetRecallVacations(IEmployee employee, DateTime? recallDate)
    {
      if (recallDate.HasValue) {
        recallDate = recallDate.Value;
        var vacationForRecall = Vacations.GetAll().Where(v => Equals(v.Employee, employee) &&
                                                         (v.Status == DirRX.HRManagement.Vacation.Status.Approved  ||
                                                          v.Status == DirRX.HRManagement.Vacation.Status.Performing));
        // Найдем первую порцию отпуска, для которого верно условие: дата отзыва попадает внутрь отпуска
        var v1 = vacationForRecall.Where(v => v.StartDate <= recallDate && recallDate <= v.FinDate).OrderBy(v => v.StartDate);
        if (v1.FirstOrDefault() != null)
        {
          // начало цепочки нашли.
          // достанем всю цепочку
          // TODO по хорошему надо найти конец цепочки. Пока исходим из гипотезы, что:
          //        в один момент времени есть одна и только одна цепочка с исполяющимися/подтвержденными отпусками
          //        это непрерывная цепочка
          var d = v1.FirstOrDefault().StartDate.Value;
          var v2 = vacationForRecall.Where(v => v.StartDate >= d).OrderBy(v => v.StartDate);
          return v2;
        }
        else
        {
          // Не нашли подходящие отпуска, вернем пустой список
          return v1;
        }
      }
      else {
        // нет даты - нет подходящих отпусков
        return null;
      }
    }
    
    /// <summary>
    /// Получить тип видимости пользователя на отпуска.
    /// </summary>
    /// <param name="user">Пользователь.</param>
    /// <returns>Режим видимости к записи справочника Отпуска:
    /// - Все записи: Служебный пользователь, Администратор HR-процессов, Пользователи HR-процессов, для которых включен режим отладки;
    /// - Записи подчиненных НОР: руководитель НОР, Директор по персоналу, Ответственный за график отпусков по НОР;
    /// - Записи подчиненных подразделений: руководитель подразделения, Ответственный за график отпусков по подразделению;
    /// - Записи своего подразделения: сотрудник.
    /// </returns>
    public static string GetVacationsVisibilty(IUser user)
    {
      var users = new List<IUser>() { user };
      
      if (Functions.Module.IsServiceUser(user) ||
          Functions.Module.IsHRAdministrator(users) ||
          Functions.Module.IsIncludedInRole(users, Constants.Module.HRDebugUsers))
        return Constants.Vacation.VacationVisible.All;
      
      if (BusinessUnits.GetAll().ToList().Where(bu => users.Any(u => u.Equals(bu.CEO))).Any() ||
          Functions.Module.IsStaffChief(users) ||
          Functions.Module.IsVacationResponsible(users))
        return Constants.Vacation.VacationVisible.SubordinateBusinessUnits;
      
      if (Departments.GetAll().ToList().Where(d => users.Any(u => u.Equals(d.Manager))).Any() ||
          Functions.Module.IsVacationManager(users))
        return Constants.Vacation.VacationVisible.SubordinateDepartments;
      return Constants.Vacation.VacationVisible.Department;
    }
    
    /// <summary>
    /// Получить доступные пользователям по правам видимости отпусков НОР.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>
    /// Список НОР у которых сотрудникам видны отпуска.
    /// </returns>
    public static List<IBusinessUnit> GetVisibleBusinessUnits(List<IUser> users)
    {
      var headBusinessUnits = new List<IBusinessUnit>();
      
      foreach (var user in users)
      {
        var visibility = Functions.Vacation.GetVacationsVisibilty(user);
        
        // Администраторам HR-процессов, Пользователям, для которых включен режим отладки, Системным пользователям доступны записи всех НОР.
        if (visibility == Constants.Vacation.VacationVisible.All)
          return BusinessUnits.GetAll().ToList();

        var employee = Employees.As(user);
        // Руководителям НОР, Директорам по персоналу, Ответственным за график отпусков по НОР доступны записи подчиненных НОР.
        if (visibility == Constants.Vacation.VacationVisible.SubordinateBusinessUnits)
        {
          var businessUnit = Sungero.Company.PublicFunctions.BusinessUnit.Remote.GetBusinessUnit(employee);
          if (businessUnit != null)
            headBusinessUnits.Add(businessUnit);
          headBusinessUnits.AddRange(BusinessUnits.GetAll(bu => employee == bu.CEO).ToList());
        }
      }
      
      return headBusinessUnits.Concat(Functions.Module.GetSubBusinessUnits(headBusinessUnits)).Distinct().ToList();
    }
    
    /// <summary>
    /// Получить доступные текущему пользователю по правам видимости отпусков подразделения.
    /// </summary>
    /// <returns>
    /// Подразделения у которых сотрудникам видны отпуска.
    /// </returns>
    [Remote]
    public static List<IDepartment> GetVisibleDepartments()
    {
      var users = Sungero.CoreEntities.Substitutions.ActiveSubstitutedUsers.ToList();
      users.Add(Users.Current);
      return GetVisibleDepartments(users);
    }
    
    /// <summary>
    /// Получить доступные пользователям по правам видимости отпусков подразделения.
    /// </summary>
    /// <param name="users">Пользователи.</param>
    /// <returns>
    /// Подразделения у которых сотрудникам видны отпуска.
    /// </returns>
    [Remote]
    public static List<IDepartment> GetVisibleDepartments(List<IUser> users)
    {
      var headDepartments = new List<IDepartment>();
      var employeeDepartments = new List<IDepartment>();
      
      foreach (var user in users)
      {
        var visibility = Functions.Vacation.GetVacationsVisibilty(user);
        
        // Администраторам HR-процессов, Пользователям, для которых включен режим отладки, Системным пользователям доступны записи всех подразделений.
        if (visibility == Constants.Vacation.VacationVisible.All)
          return Departments.GetAll().ToList();
        
        var employee = Employees.As(user);
        // Руководителям подразделений, Ответственным за график отпусков в подразделении доступны записи подчиненных подразделений.
        if (visibility == Constants.Vacation.VacationVisible.SubordinateDepartments)
        {
          headDepartments.Add(employee.Department);
          headDepartments.AddRange(Departments.GetAll(d => d.Manager == employee).ToList());
        }
        
        // Сотруднику доступны записи своего подразделения.
        if (visibility == Constants.Vacation.VacationVisible.Department)
          employeeDepartments.Add(employee.Department);
      }
      
      // Руководителям НОР, Директорам по персоналу, Ответственным за график отпусков по НОР доступны записи подразделений из подчиненных НОР.
      var visibleBusinessUnits = GetVisibleBusinessUnits(users);
      var departmentsFiltered = Departments.GetAll().ToList().Where(d => visibleBusinessUnits.Any(vb => vb.Equals(d.BusinessUnit)));
      headDepartments.AddRange(departmentsFiltered);
      
      return headDepartments
        .Concat(Functions.Module.GetSubDepartments(headDepartments))
        .Concat(employeeDepartments)
        .Distinct()
        .ToList();
    }
  }

}
