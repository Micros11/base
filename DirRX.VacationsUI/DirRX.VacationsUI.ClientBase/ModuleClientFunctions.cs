using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.VacationsUI.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Сформировать отчет для проверки настроек системы.
    /// </summary>
    public virtual void CreateSettingsReport()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateSettingsReport();
    }
    
    /// <summary>
    /// Настроить исполнителей ролей по НОР.
    /// </summary>
    public virtual void ConfigureRolesByCompany()
    {
      DirRX.HRManagement.PublicFunctions.Module.ConfigureRolesByCompany();
    }

    /// <summary>
    /// Создать задачу на планирование по сотруднику.
    /// </summary>
    public virtual void CreateSingleScheduling()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateSingleScheduling();
    }

    /// <summary>
    /// Создать задачу на формирование и утверждение графика отпусков.
    /// </summary>
    public virtual void CreateNewVacationScheduleTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewVacationScheduleTask();
    }
    
    /// <summary>
    /// Построить актуальный график отпусков.
    /// </summary>
    public virtual void CreateActualVacationReport()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateActualVacationReport();
    }

    /// <summary>
    /// Выгрузить документы по отпускам.
    /// </summary>
    public virtual void ExportVacationDocuments()
    {
      DirRX.HRManagement.PublicFunctions.Module.ExportVacationDocuments();
    }

    /// <summary>
    /// Построить график отпусков за год.
    /// </summary>
    public virtual void ShowVacationScheduleChart()
    {
      DirRX.HRManagement.PublicFunctions.Module.ShowVacationScheduleChart();
    }

    /// <summary>
    /// Создать задачу на отзыв из отпуска.
    /// </summary>
    public virtual void CreateNewRecallVacationTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewRecallVacationTask();
    }

    /// <summary>
    /// Создать задачу на перенос отпуска.
    /// </summary>
    public virtual void CreateNewShiftVacationTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewShiftVacationTask();
    }

    /// <summary>
    /// Создать задачу на согласование отпуска.
    /// </summary>
    public virtual void CreateNewVacationApprovalTask()
    {
      DirRX.HRManagement.PublicFunctions.Module.CreateNewVacationApprovalTask();
    }

    /// <summary>
    /// Показать отпуска сотрудника.
    /// </summary>
    public virtual void ShowEmployeeVacation()
    {
      DirRX.HRManagement.PublicFunctions.Module.ShowEmployeeVacation();
    }

  }
}